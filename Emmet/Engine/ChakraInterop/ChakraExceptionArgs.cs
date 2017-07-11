using System.Collections.Generic;
using Emmet.Diagnostics;

namespace Emmet.Engine.ChakraInterop
{
    /// <summary>
    /// Contains additional information about JavaScript runtime exceptions.
    /// </summary>
    public class ChakraExceptionArgs : ExceptionArgs
    {
        private static IDictionary<JavaScriptErrorCode, string> _messages =
            new Dictionary<JavaScriptErrorCode, string>
            {
                { JavaScriptErrorCode.InvalidArgument, "Invalid argument." },
                { JavaScriptErrorCode.NullArgument, "Null argument." },
                { JavaScriptErrorCode.NoCurrentContext, "No current context." },
                { JavaScriptErrorCode.InExceptionState, "Runtime is in exception state." },
                { JavaScriptErrorCode.NotImplemented, "Method is not implemented." },
                { JavaScriptErrorCode.WrongThread, "Runtime is active on another thread." },
                { JavaScriptErrorCode.RuntimeInUse, "Runtime is in use." },
                { JavaScriptErrorCode.BadSerializedScript, "Bad serialized script." },
                { JavaScriptErrorCode.InDisabledState, "Runtime is disabled." },
                { JavaScriptErrorCode.CannotDisableExecution, "Cannot disable execution." },
                { JavaScriptErrorCode.AlreadyDebuggingContext, "Context is already in debug mode." },
                { JavaScriptErrorCode.HeapEnumInProgress, "Heap enumeration is in progress." },
                { JavaScriptErrorCode.ArgumentNotObject, "Argument is not an object." },
                { JavaScriptErrorCode.InProfileCallback, "In a profile callback." },
                { JavaScriptErrorCode.InThreadServiceCallback, "In a thread service callback." },
                { JavaScriptErrorCode.CannotSerializeDebugScript, "Cannot serialize a debug script." },
                { JavaScriptErrorCode.AlreadyProfilingContext, "Already profiling this context." },
                { JavaScriptErrorCode.IdleNotEnabled, "Idle is not enabled." },
                { JavaScriptErrorCode.OutOfMemory, "Out of memory." },
                { JavaScriptErrorCode.ScriptTerminated, "Script was terminated." },
                { JavaScriptErrorCode.ScriptEvalDisabled, "Eval of strings is disabled in this runtime." }
            };

        private JavaScriptErrorCode _error;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChakraExceptionArgs"/> class.
        /// </summary>
        /// <param name="error">JavaScript runtime error to throw as exception.</param>
        public ChakraExceptionArgs(JavaScriptErrorCode error)
        {
            _error = error;
        }

        /// <summary>
        /// Gets the error message specific to the exception.
        /// </summary>
        public override string Message
        {
            get
            {
                if (_messages.TryGetValue(_error, out string retVal))
                    return retVal;

                switch (_error)
                {
                    case JavaScriptErrorCode.ScriptException:
                        {
                            JavaScriptValue errorObject;
                            JavaScriptErrorCode innerError = NativeMethods.JsGetAndClearException(
                                out errorObject);

                            if (JavaScriptErrorCode.NoError != innerError)
                                return "Fatal exception";

                            return $"Script threw an exception: {errorObject.GetProperty("message")}";
                        }

                    case JavaScriptErrorCode.ScriptCompile:
                        {
                            JavaScriptValue errorObject;
                            JavaScriptErrorCode innerError = NativeMethods.JsGetAndClearException(
                                out errorObject);

                            if (JavaScriptErrorCode.NoError != innerError)
                                return "Fatal exception";

                            return $"Script compilation error: {errorObject.GetProperty("message")}";
                        }

                    default:
                        return "Fatal exception";
                }
            }
        }
    }
}