using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Emmet.Diagnostics;
using Emmet.EditorExtensions;
using Emmet.Engine;
using Emmet.Mnemonics;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.ClearScript;
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
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.CodeWindow_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionOpening_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideOptionPage(typeof(Options), Vsix.Name, "General", 0, 0, true)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class EmmetPackage : AsyncPackage
    {
        private EngineWrapper _engine;

        /// <summary>
        /// Initializes static members of the <see cref="EmmetPackage"/> class.
        /// </summary>
        static EmmetPackage()
        {
            Options = new Options();
        }

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
        internal bool RunCommand(ViewContext context, int cmdId)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            DTE2 dte = GetService(typeof(DTE)) as DTE2;
            Assumes.Present(dte);

            bool ownUndoContext = false;
            try
            {
                if (!dte.UndoContext.IsOpen)
                {
                    dte.UndoContext.Open(Vsix.Name);
                    ownUndoContext = true;
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

                string msg;
                switch (ex)
                {
                    case ScriptEngineException jsex:
                        msg = $"Unexpected error occurred inside of the Emmet engine. {jsex.Message}";
                        break;
                    default:
                        msg = $"Unexpected error of type {ex.GetType()}: {ex.Message}";
                        break;
                }

                ShowCriticalError(msg);

                return false;
            }
        }

        /// <summary>
        /// Forces reloading cached package options. Is expected to be called when user changes his
        /// preferences through the options dialog.
        /// </summary>
        internal void ReloadOptions()
        {
            Options = GetDialogPage(typeof(Options)) as Options;
        }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this
        /// is the place where you can put all the initialization code that rely on services provided by
        /// Visual Studio.
        /// </summary>
        /// <param name="token">Cancellation token that can signal abort.</param>
        /// <param name="progress">The progress indicator.</param>
        protected override async System.Threading.Tasks.Task InitializeAsync(
            CancellationToken token,
            IProgress<ServiceProgressData> progress)
        {
            Instance = this;

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
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(token);

                ShowCriticalError(
                    $"Failed to load mnemonics configuration from {Options.MnemonicsConfiguration}");
            }

            SortCssPropertiesCommand.Initialize(this);

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(token);

            Options = GetDialogPage(typeof(Options)) as Options;

            if (Options.WriteDebugMessages)
            {
                var pane = GetOutputPane(VSConstants.OutputWindowPaneGuid.DebugPane_guid, Vsix.Name);
                Tracer.Initialize(pane);
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
            return ex is ScriptEngineException || ex is COMException;
        }

        private static void ShowCriticalError(string message)
        {
            VsShellUtilities.ShowMessageBox(
                Instance,
                message,
                $"{Vsix.Name}: Unexpected error.",
                OLEMSGICON.OLEMSGICON_CRITICAL,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}