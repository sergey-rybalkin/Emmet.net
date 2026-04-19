using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.VisualStudio.Shell.Interop;

namespace Emmet.Diagnostics
{
    /// <summary>
    /// Implements tracing infrastructure as a set of object extension methods.
    /// </summary>
    internal static class Log
    {
        private static IVsOutputWindowPane s_logger = null;

        internal static void Initialize(IVsOutputWindowPane pane)
        {
            s_logger = pane;
        }

        [Conditional("TRACE")]
        internal static void Trace(
            string message,
            [CallerFilePath]string callerFilePath = "",
            [CallerLineNumber]int codeLineNumber = 0,
            [CallerMemberName]string callingMember = "")
        {
            if (null == s_logger)
                return;

            string caller = GenerateCallerName(callerFilePath, callingMember);

            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            s_logger.OutputString($"{caller}({codeLineNumber}): {message}\n");
        }

        private static string GenerateCallerName(string path, string member)
        {
            int fileNameIndex = path.Length - 3;
            while (fileNameIndex > 0)
            {
                if ('\\' == path[--fileNameIndex])
                    break;
            }

            string className = path.Substring(fileNameIndex + 1, path.Length - fileNameIndex - 4);
            StringBuilder retVal = new StringBuilder(className.Length + member.Length + 1);
            retVal.Append(className);
            retVal.Append('.');
            retVal.Append(member);

            return retVal.ToString();
        }
    }
}