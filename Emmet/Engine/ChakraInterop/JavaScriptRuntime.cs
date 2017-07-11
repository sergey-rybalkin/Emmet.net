using System;

namespace Emmet.Engine.ChakraInterop
{
    /// <summary>
    /// A JavaScript runtime wrapper.
    /// </summary>
    public struct JavaScriptRuntime : IDisposable
    {
        private IntPtr _handle;

        /// <summary>
        /// Gets a value indicating whether the runtime is valid.
        /// </summary>
        public bool IsValid
        {
            get { return IntPtr.Zero != _handle; }
        }

        /// <summary>
        /// Creates a new runtime.
        /// </summary>
        /// <param name="attributes">The attributes of the runtime to be created.</param>
        /// <returns>The runtime created.</returns>
        public static JavaScriptRuntime Create(JavaScriptRuntimeAttributes attributes)
        {
            NativeMethods.JsCreateRuntime(attributes, null, out JavaScriptRuntime handle).ThrowIfError();
            return handle;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        /// resources.
        /// </summary>
        public void Dispose()
        {
            if (IsValid)
            {
                NativeMethods.JsSetCurrentContext(JavaScriptContext.Invalid);
                NativeMethods.JsDisposeRuntime(this).ThrowIfError();
            }

            _handle = IntPtr.Zero;
        }

        /// <summary>
        /// Performs a full garbage collection.
        /// </summary>
        public void CollectGarbage()
        {
            NativeMethods.JsCollectGarbage(this).ThrowIfError();
        }

        /// <summary>
        /// Creates a script context for running scripts.
        /// </summary>
        public JavaScriptContext CreateContext()
        {
            NativeMethods.JsCreateContext(this, out JavaScriptContext reference).ThrowIfError();
            return reference;
        }
    }
}