using System;

namespace Emmet.Engine.ChakraInterop
{
    /// <summary>
    /// Script execution context.
    /// </summary>
    public struct JavaScriptContext
    {
        private readonly IntPtr _reference;

        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptContext"/> structure.
        /// </summary>
        /// <param name="reference">Context reference.</param>
        internal JavaScriptContext(IntPtr reference)
        {
            _reference = reference;
        }

        /// <summary>
        /// Gets an invalid context.
        /// </summary>
        public static JavaScriptContext Invalid
        {
            get { return new JavaScriptContext(IntPtr.Zero); }
        }

        /// <summary>
        /// Gets or sets the current script context on the thread.
        /// </summary>
        public static JavaScriptContext Current
        {
            get
            {
                NativeMethods.JsGetCurrentContext(out JavaScriptContext reference).ThrowIfError();
                return reference;
            }
            set
            {
                NativeMethods.JsSetCurrentContext(value).ThrowIfError();
            }
        }

        /// <summary>
        /// Tells the runtime to do any idle processing like memory cleanup it needs to do.
        /// </summary>
        public static uint Idle()
        {
            uint ticks;
            NativeMethods.JsIdle(out ticks).ThrowIfError();
            return ticks;
        }

        /// <summary>
        /// Executes specified script.
        /// </summary>
        /// <param name="script">The script to run.</param>
        /// <param name="sourceContext">
        /// A cookie identifying the script that can be used by script contexts that have debugging enabled.
        /// </param>
        public static JavaScriptValue RunScript(string script, JavaScriptSourceContext sourceContext)
        {
            NativeMethods.JsRunScript(script, sourceContext, string.Empty, out JavaScriptValue result)
                         .ThrowIfError();
            return result;
        }
    }
}