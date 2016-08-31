using System;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace Emmet
{
    /// <summary>
    /// An external commands dispatcher. Use this class to run commands built into Visual Studio or from
    /// other extensions.
    /// </summary>
    public static class ExternalCommandsDispatcher
    {
        private static DTE2 _dte;

        private static DTE2 DTE
        {
            get
            {
                if (_dte == null)
                    _dte = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE2;

                return _dte;
            }
        }

        /// <summary>
        /// Raises Visual Studio Edit.FormatSelection command.
        /// </summary>
        public static bool FormatSelection()
        {
            string commandName = "Edit.FormatSelection";
            var command = DTE.Commands.Item(commandName);
            if (!command.IsAvailable)
                return false;

            try
            {
                DTE.ExecuteCommand(commandName, string.Empty);
            }
            catch (NullReferenceException)
            {
                // Seems to be thrown even on successful execution, should be ignored.
            }

            return true;
        }
    }
}