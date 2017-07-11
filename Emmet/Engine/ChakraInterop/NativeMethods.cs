using System;
using System.Runtime.InteropServices;
using Emmet.Diagnostics;

namespace Emmet.Engine.ChakraInterop
{
    /// <summary>
    /// The host can specify a promise continuation callback in <c>JsSetPromiseContinuationCallback</c>. If
    /// a script creates a task to be run later, then the promise continuation callback will be called with
    /// the task and the task should be put in a FIFO queue, to be run when the current script is done
    /// executing.
    /// </summary>
    /// <param name="task">The task, represented as a JavaScript function.</param>
    /// <param name="callbackState">The data argument to be passed to the callback.</param>
    public delegate void JavaScriptPromiseContinuationCallback(JavaScriptValue task, IntPtr callbackState);

    /// <summary>
    /// A native JavaScript function callback.
    /// </summary>
    /// <param name="callee">
    /// A <c>Function</c> object that represents the function being invoked.
    /// </param>
    /// <param name="isConstructCall">Indicates whether this is a regular call or a 'new' call.</param>
    /// <param name="arguments">The arguments to the call.</param>
    /// <param name="argumentCount">The number of arguments.</param>
    /// <param name="callbackData">Callback data, if any.</param>
    /// <returns>The result of the call, if any.</returns>
    public delegate JavaScriptValue JavaScriptNativeFunction(
        JavaScriptValue callee,
        [MarshalAs(UnmanagedType.U1)] bool isConstructCall,
        [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] JavaScriptValue[] arguments,
        ushort argumentCount,
        IntPtr callbackData);

    /// <summary>
    /// User implemented callback routine for memory allocation events
    /// </summary>
    /// <param name="callbackState">The state passed to SetRuntimeMemoryAllocationCallback.</param>
    /// <param name="allocationEvent">The type of type allocation event.</param>
    /// <param name="allocationSize">The size of the allocation.</param>
    public delegate bool JavaScriptMemoryAllocationCallback(
        IntPtr callbackState,
        JavaScriptMemoryEventType allocationEvent,
        UIntPtr allocationSize);

    /// <summary>
    /// A background work item callback.
    /// </summary>
    /// <param name="callbackData">Data argument passed to the thread service.</param>
    public delegate void JavaScriptBackgroundWorkItemCallback(IntPtr callbackData);

    /// <summary>
    /// The host can specify a background thread service when creating a runtime. If specified, then
    /// background work items will be passed to the host using this callback. The host is expected to either
    /// begin executing the background work item immediately and return true or return false and the runtime
    /// will handle the work item in-thread.
    /// </summary>
    /// <param name="callbackFunction">The callback for the background work item.</param>
    /// <param name="callbackData">The data argument to be passed to the callback.</param>
    /// <returns>
    /// Whether the thread service will execute the callback.
    /// </returns>
    public delegate bool JavaScriptThreadServiceCallback(
        JavaScriptBackgroundWorkItemCallback callbackFunction,
        IntPtr callbackData);

    /// <summary>
    /// A general callback delegate.
    /// </summary>
    /// <param name="callbackState">The state passed to the callback.</param>
    public delegate void JavaScriptGenericCallback(IntPtr callbackState);

    /// <summary>
    /// P/Invoke interface for Chakra core.
    /// </summary>
    internal static class NativeMethods
    {
        private const string DllName = "chakracore.dll";

        internal static void ThrowIfError(this JavaScriptErrorCode code)
        {
            if (JavaScriptErrorCode.NoError != code)
            {
                var args = new ChakraExceptionArgs(code);
                throw new Exception<ChakraExceptionArgs>(args);
            }
        }

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsCreateRuntime(
            JavaScriptRuntimeAttributes attributes,
            JavaScriptThreadServiceCallback threadService,
            out JavaScriptRuntime runtime);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsCollectGarbage(JavaScriptRuntime handle);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsDisposeRuntime(JavaScriptRuntime handle);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsGetRuntimeMemoryUsage(
            JavaScriptRuntime runtime,
            out UIntPtr memoryUsage);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsGetRuntimeMemoryLimit(
            JavaScriptRuntime runtime,
            out UIntPtr memoryLimit);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsSetRuntimeMemoryLimit(
            JavaScriptRuntime runtime,
            UIntPtr memoryLimit);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsSetRuntimeMemoryAllocationCallback(
            JavaScriptRuntime runtime,
            IntPtr callbackState,
            JavaScriptMemoryAllocationCallback allocationCallback);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsSetRuntimeBeforeCollectCallback(
            JavaScriptRuntime runtime,
            IntPtr callbackState,
            JavaScriptGenericCallback beforeCollectCallback);

        [DllImport(DllName, EntryPoint = "JsAddRef")]
        internal static extern JavaScriptErrorCode JsContextAddRef(
            JavaScriptContext reference, out uint count);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsAddRef(JavaScriptValue reference, out uint count);

        [DllImport(DllName, EntryPoint = "JsRelease")]
        internal static extern JavaScriptErrorCode JsContextRelease(
            JavaScriptContext reference, out uint count);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsRelease(JavaScriptValue reference, out uint count);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsCreateContext(
            JavaScriptRuntime runtime, out JavaScriptContext newContext);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsGetCurrentContext(out JavaScriptContext currentContext);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsSetCurrentContext(JavaScriptContext context);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsGetRuntime(
            JavaScriptContext context, out JavaScriptRuntime runtime);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsIdle(out uint nextIdleTick);

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        internal static extern JavaScriptErrorCode JsParseScript(
            string script,
            JavaScriptSourceContext sourceContext,
            string sourceUrl,
            out JavaScriptValue result);

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        internal static extern JavaScriptErrorCode JsRunScript(
            string script,
            JavaScriptSourceContext sourceContext,
            string sourceUrl,
            out JavaScriptValue result);

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        internal static extern JavaScriptErrorCode JsSerializeScript(
            string script, byte[] buffer, ref ulong bufferSize);

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        internal static extern JavaScriptErrorCode JsParseSerializedScript(
            string script,
            byte[] buffer,
            JavaScriptSourceContext sourceContext,
            string sourceUrl,
            out JavaScriptValue result);

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        internal static extern JavaScriptErrorCode JsRunSerializedScript(
            string script,
            byte[] buffer,
            JavaScriptSourceContext sourceContext,
            string sourceUrl,
            out JavaScriptValue result);

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        internal static extern JavaScriptErrorCode JsGetPropertyIdFromName(
            string name, out JavaScriptPropertyId propertyId);

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        internal static extern JavaScriptErrorCode JsGetPropertyNameFromId(
            JavaScriptPropertyId propertyId, out string name);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsGetUndefinedValue(out JavaScriptValue undefinedValue);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsGetNullValue(out JavaScriptValue nullValue);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsGetTrueValue(out JavaScriptValue trueValue);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsGetFalseValue(out JavaScriptValue falseValue);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsBoolToBoolean(
            bool value, out JavaScriptValue booleanValue);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsBooleanToBool(
            JavaScriptValue booleanValue, out bool boolValue);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsConvertValueToBoolean(
            JavaScriptValue value, out JavaScriptValue booleanValue);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsGetValueType(
            JavaScriptValue value, out JavaScriptValueType type);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsDoubleToNumber(
            double doubleValue, out JavaScriptValue value);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsIntToNumber(int intValue, out JavaScriptValue value);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsNumberToDouble(
            JavaScriptValue value, out double doubleValue);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsConvertValueToNumber(
            JavaScriptValue value, out JavaScriptValue numberValue);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsGetStringLength(
            JavaScriptValue sringValue, out int length);

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        internal static extern JavaScriptErrorCode JsPointerToString(
            string value, UIntPtr stringLength, out JavaScriptValue stringValue);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsStringToPointer(
            JavaScriptValue value, out IntPtr stringValue, out UIntPtr stringLength);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsConvertValueToString(
            JavaScriptValue value, out JavaScriptValue stringValue);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsGetGlobalObject(out JavaScriptValue globalObject);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsCreateObject(out JavaScriptValue obj);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsCreateExternalObject(
            IntPtr data, JavaScriptGenericCallback finalizeCallback, out JavaScriptValue obj);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsConvertValueToObject(
            JavaScriptValue value, out JavaScriptValue obj);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsGetPrototype(
            JavaScriptValue obj, out JavaScriptValue prototypeObject);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsSetPrototype(
            JavaScriptValue obj, JavaScriptValue prototypeObject);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsGetExtensionAllowed(
            JavaScriptValue obj, out bool value);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsPreventExtension(JavaScriptValue obj);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsGetProperty(
            JavaScriptValue obj, JavaScriptPropertyId propertyId, out JavaScriptValue value);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsGetOwnPropertyDescriptor(
            JavaScriptValue obj, JavaScriptPropertyId propertyId, out JavaScriptValue propertyDescriptor);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsGetOwnPropertyNames(
            JavaScriptValue obj, out JavaScriptValue propertyNames);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsSetProperty(
            JavaScriptValue obj,
            JavaScriptPropertyId propertyId,
            JavaScriptValue value,
            bool useStrictRules);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsHasProperty(
            JavaScriptValue obj,
            JavaScriptPropertyId propertyId,
            out bool hasProperty);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsDeleteProperty(
            JavaScriptValue obj,
            JavaScriptPropertyId propertyId,
            bool useStrictRules,
            out JavaScriptValue result);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsDefineProperty(
            JavaScriptValue obj,
            JavaScriptPropertyId propertyId,
            JavaScriptValue propertyDescriptor,
            out bool result);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsHasIndexedProperty(
            JavaScriptValue obj, JavaScriptValue index, out bool result);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsGetIndexedProperty(
            JavaScriptValue obj, JavaScriptValue index, out JavaScriptValue result);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsSetIndexedProperty(
            JavaScriptValue obj, JavaScriptValue index, JavaScriptValue value);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsDeleteIndexedProperty(
            JavaScriptValue obj, JavaScriptValue index);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsEquals(
            JavaScriptValue obj1, JavaScriptValue obj2, out bool result);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsStrictEquals(
            JavaScriptValue obj1, JavaScriptValue obj2, out bool result);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsHasExternalData(JavaScriptValue obj, out bool value);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsGetExternalData(
            JavaScriptValue obj, out IntPtr externalData);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsSetExternalData(
            JavaScriptValue obj, IntPtr externalData);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsCreateArray(uint length, out JavaScriptValue result);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsCallFunction(
            JavaScriptValue function,
            JavaScriptValue[] arguments,
            ushort argumentCount,
            out JavaScriptValue result);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsConstructObject(
            JavaScriptValue function,
            JavaScriptValue[] arguments,
            ushort argumentCount,
            out JavaScriptValue result);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsCreateFunction(
            JavaScriptNativeFunction nativeFunction, IntPtr externalData, out JavaScriptValue function);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsCreateError(
            JavaScriptValue message, out JavaScriptValue error);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsCreateRangeError(
            JavaScriptValue message, out JavaScriptValue error);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsCreateReferenceError(
            JavaScriptValue message, out JavaScriptValue error);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsCreateSyntaxError(
            JavaScriptValue message, out JavaScriptValue error);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsCreateTypeError(
            JavaScriptValue message, out JavaScriptValue error);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsCreateURIError(
            JavaScriptValue message, out JavaScriptValue error);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsHasException(out bool hasException);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsGetAndClearException(out JavaScriptValue exception);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsSetException(JavaScriptValue exception);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsDisableRuntimeExecution(JavaScriptRuntime runtime);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsEnableRuntimeExecution(JavaScriptRuntime runtime);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsIsRuntimeExecutionDisabled(
            JavaScriptRuntime runtime, out bool isDisabled);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsSetObjectBeforeCollectCallback(
            JavaScriptValue reference,
            IntPtr callbackState,
            JavaScriptGenericCallback beforeCollectCallback);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsCreateNamedFunction(
            JavaScriptValue name,
            JavaScriptNativeFunction nativeFunction,
            IntPtr callbackState,
            out JavaScriptValue function);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsSetPromiseContinuationCallback(
            JavaScriptPromiseContinuationCallback promiseContinuationCallback, IntPtr callbackState);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsCreateArrayBuffer(
            uint byteLength, out JavaScriptValue result);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsCreateDataView(
            JavaScriptValue arrayBuffer,
            uint byteOffset,
            uint byteOffsetLength,
            out JavaScriptValue result);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsGetArrayBufferStorage(
            JavaScriptValue arrayBuffer, out IntPtr buffer, out uint bufferLength);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsGetDataViewStorage(
            JavaScriptValue dataView, out IntPtr buffer, out uint bufferLength);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsGetPropertyIdType(
            JavaScriptPropertyId propertyId, out JavaScriptPropertyIdType propertyIdType);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsCreateSymbol(
            JavaScriptValue description, out JavaScriptValue symbol);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsGetSymbolFromPropertyId(
            JavaScriptPropertyId propertyId, out JavaScriptValue symbol);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsGetPropertyIdFromSymbol(
            JavaScriptValue symbol, out JavaScriptPropertyId propertyId);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsGetOwnPropertySymbols(
            JavaScriptValue obj, out JavaScriptValue propertySymbols);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsNumberToInt(JavaScriptValue value, out int intValue);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsHasIndexedPropertiesExternalData(
            JavaScriptValue obj, out bool value);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsInstanceOf(
            JavaScriptValue obj, JavaScriptValue constructor, out bool result);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsGetContextOfObject(
            JavaScriptValue obj, out JavaScriptContext context);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsGetContextData(
            JavaScriptContext context, out IntPtr data);

        [DllImport(DllName)]
        internal static extern JavaScriptErrorCode JsSetContextData(JavaScriptContext context, IntPtr data);
    }
}