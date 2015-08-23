using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Emmet.Diagnostics
{
    /// <summary>
    /// Implements tracing infrastructure as a set of object extension methods.
    /// </summary>
	internal static class Tracer
	{
        private static IVsOutputWindowPane _logger = null;

        internal static void Initialize(IVsOutputWindowPane pane)
        {
            _logger = pane;
        }

		#region Trace method overloads

		[Conditional("TRACE")]
		internal static void Trace(this object source,
                                   string message,
                                   TraceEventType eventType = TraceEventType.Verbose,
                                   [CallerMemberName] string callerMethodName = "")
        {
            StringBuilder trace = new StringBuilder(message.Length + 128);
            trace.Append('[');
            trace.Append(source.GetType().FullName);
            trace.Append('.');
            trace.Append(callerMethodName);
            trace.Append("] ");
            trace.Append(message);

            _logger.OutputString(trace.ToString());
            _logger.OutputString(Environment.NewLine);
        }

		[Conditional("TRACE")]
        internal static void Trace(this object source,
                                  string message,
                                  object param1,
                                  TraceEventType eventType = TraceEventType.Verbose,
                                  [CallerMemberName] string callerMethodName = "")
        {
            string formattedMessage = string.Format(message, param1);

            Trace(source, formattedMessage, eventType, callerMethodName);
        }

		[Conditional("TRACE")]
        internal static void Trace(this object source,
                                   string message,
                                   object param1,
                                   object param2,
                                   TraceEventType eventType = TraceEventType.Verbose,
                                   [CallerMemberName] string callerMethodName = "")
        {
            string formattedMessage = string.Format(message, param1, param2);

            Trace(source, formattedMessage, eventType, callerMethodName);
        }

		[Conditional("TRACE")]
        internal static void Trace(this object source,
                                   string message,
                                   object param1,
                                   object param2,
                                   object param3,
                                   TraceEventType eventType = TraceEventType.Verbose,
                                   [CallerMemberName] string callerMethodName = "")
        {
            string formattedMessage = string.Format(message, param1, param2, param3);

            Trace(source, formattedMessage, eventType, callerMethodName);
        }

		#endregion

		#region TraceInformation method overloads

		[Conditional("TRACE")]
		internal static void TraceInformation(this object source,
                                              string message,
                                              [CallerMemberName] string callerMethodName = "")
        {
            Trace(source, message, TraceEventType.Information, callerMethodName);
        }

		[Conditional("TRACE")]
        internal static void TraceInformation(this object source,
                                              string message,
                                              object param1,
                                              [CallerMemberName] string callerMethodName = "")
        {
            string formattedMessage = string.Format(message, param1);

            TraceInformation(source, formattedMessage, callerMethodName);
        }

		[Conditional("TRACE")]
        internal static void TraceInformation(this object source,
                                              string message,
                                              object param1,
                                              object param2,
                                              [CallerMemberName] string callerMethodName = "")
        {
            string formattedMessage = string.Format(message, param1, param2);

            TraceInformation(source, formattedMessage, callerMethodName);
        }

		[Conditional("TRACE")]
        internal static void TraceInformation(this object source,
                                              string message,
                                              object param1,
                                              object param2,
                                              object param3,
                                 [CallerMemberName] string callerMethodName = "")
        {
            string formattedMessage = string.Format(message, param1, param2, param3);

            TraceInformation(source, formattedMessage, callerMethodName);
        }

		#endregion

		#region TraceWarning method overloads

		[Conditional("TRACE")]
		internal static void TraceWarning(this object source,
                                          string message,
                                          [CallerMemberName] string callerMethodName = "")
        {
            Trace(source, message, TraceEventType.Warning, callerMethodName);
        }

		[Conditional("TRACE")]
        internal static void TraceWarning(this object source,
                                          string message,
                                          object param1,
                                          [CallerMemberName] string callerMethodName = "")
        {
            string formattedMessage = string.Format(message, param1);

            TraceWarning(source, formattedMessage, callerMethodName);
        }

		[Conditional("TRACE")]
        internal static void TraceWarning(this object source,
                                          string message,
                                          object param1,
                                          object param2,
                                          [CallerMemberName] string callerMethodName = "")
        {
            string formattedMessage = string.Format(message, param1, param2);

            TraceWarning(source, formattedMessage, callerMethodName);
        }

		[Conditional("TRACE")]
        internal static void TraceWarning(this object source,
                                          string message,
                                          object param1,
                                          object param2,
                                          object param3,
                                          [CallerMemberName] string callerMethodName = "")
        {
            string formattedMessage = string.Format(message, param1, param2, param3);

            TraceWarning(source, formattedMessage, callerMethodName);
        }

		#endregion

		#region TraceError method overloads

		[Conditional("TRACE")]
		internal static void TraceError(this object source,
                                        string message,
                                        [CallerMemberName] string callerMethodName = "")
        {
            Trace(source, message, TraceEventType.Error, callerMethodName);
        }

		[Conditional("TRACE")]
        internal static void TraceError(this object source,
                                        string message,
                                        object param1,
                                        [CallerMemberName] string callerMethodName = "")
        {
            string formattedMessage = string.Format(message, param1);

            TraceError(source, formattedMessage, callerMethodName);
        }

		[Conditional("TRACE")]
        internal static void TraceError(this object source,
                                        string message,
                                        object param1,
                                        object param2,
                                        [CallerMemberName] string callerMethodName = "")
        {
            string formattedMessage = string.Format(message, param1, param2);

            TraceError(source, formattedMessage, callerMethodName);
        }

		[Conditional("TRACE")]
        internal static void TraceError(this object source,
                                        string message,
                                        object param1,
                                        object param2,
                                        object param3,
                                        [CallerMemberName] string callerMethodName = "")
        {
            string formattedMessage = string.Format(message, param1, param2, param3);

            TraceError(source, formattedMessage, callerMethodName);
        }

		#endregion		
	}
}