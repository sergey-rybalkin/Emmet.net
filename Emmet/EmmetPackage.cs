using System;
using System.IO;
using System.Runtime.InteropServices;
using Emmet.Diagnostics;
using Emmet.Engine;
using Emmet.Engine.ChakraInterop;
using Emmet.Mnemonics;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Emmet
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    [Guid(PackageGuids.GuidEmmetPackageString)]
    [InstalledProductRegistration("#110", "#112", Vsix.Version, IconResourceID = 400)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [ProvideOptionPage(typeof(Options), Vsix.Name, "General", 0, 0, true)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class EmmetPackage : Package
    {
        private EngineWrapper _engine;

        /// <summary>
        /// Gets current configuration settings for the package.
        /// </summary>
        internal static Options Options { get; private set; }

        /// <summary>
        /// Gets the singleton instance of the package.
        /// </summary>
        internal static EmmetPackage Instance { get; private set; }

        /// <summary>
        /// Executes Emmet command in the specified view.
        /// </summary>
        /// <param name="context">The view context to execute command in.</param>
        /// <param name="cmdId">Identifier of the command to execute.</param>
        internal bool RunCommand(EmmetEditor context, int cmdId)
        {
            DTE2 dte = GetService(typeof(DTE)) as DTE2;
            bool ownUndoContext = false;
            try
            {
                if (!dte.UndoContext.IsOpen)
                {
                    ownUndoContext = true;
                    dte.UndoContext.Open(Vsix.Name);
                }

                bool succeeded = _engine.RunCommand(cmdId, context);

                if (ownUndoContext)
                    dte.UndoContext.Close();

                return succeeded;
            }
            catch (Exception ex) when (IsExpectedException(ex))
            {
                if (ownUndoContext)
                    dte.UndoContext.SetAborted();

                string msg = string.Empty;

                switch (ex)
                {
                    case Exception<ChakraExceptionArgs> jsex:
                        msg = $"Unexpected error occurred inside of the Emmet engine. {jsex.Message}";
                        break;
                    case COMException comex:
                        msg = $"Cannot load Microsoft Chakra engine.";
                        break;
                    default:
                        msg = $"Unexpected error of type {ex.GetType()}: {ex.Message}";
                        break;
                }

                VsShellUtilities.ShowMessageBox(
                    this,
                    msg,
                    $"{Vsix.Name}: Unexpected error.",
                    OLEMSGICON.OLEMSGICON_CRITICAL,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

                return false;
            }
        }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this
        /// is the place where you can put all the initialization code that rely on services provided by
        /// Visual Studio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            Instance = this;
            Options = GetDialogPage(typeof(Options)) as Options;

            if (Options.WriteDebugMessages)
            {
                var pane = GetOutputPane(VSConstants.OutputWindowPaneGuid.DebugPane_guid, Vsix.Name);
                Tracer.Initialize(pane);
            }

            if (Directory.Exists(Options.ExtensionsDir))
                _engine = new EngineWrapper(Options.ExtensionsDir);
            else
                _engine = new EngineWrapper(null);

            try
            {
                if (File.Exists(Options.MnemonicsConfiguration))
                    MnemonicParser.MergeConfiguration(Options.MnemonicsConfiguration);
            }
            catch
            {
                VsShellUtilities.ShowMessageBox(
                    this,
                    $"Failed to load mnemonics configuration from {Options.MnemonicsConfiguration}",
                    $"{Vsix.Name}: Unexpected error.",
                    OLEMSGICON.OLEMSGICON_CRITICAL,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the Emmet.EmmetPackage and optionally releases the
        /// managed resources.
        /// </summary>
        /// <param name="disposing">
        /// true to release both managed and unmanaged resources; false to release only unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _engine.Dispose();

            base.Dispose(disposing);
        }

        private static bool IsExpectedException(Exception ex)
        {
            return ex is Exception<ChakraExceptionArgs> || ex is COMException;
        }
    }
}