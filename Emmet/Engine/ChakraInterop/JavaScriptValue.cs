using System;
using System.Runtime.InteropServices;

namespace Emmet.Engine.ChakraInterop
{
    /// <summary>
    /// Represents an instance of an object or primitive JavaScript type.
    /// </summary>
    public struct JavaScriptValue
    {
        private readonly IntPtr _reference;

        private JavaScriptValue(IntPtr reference)
        {
            _reference = reference;
        }

        /// <summary>
        /// Gets an invalid value.
        /// </summary>
        public static JavaScriptValue Invalid
        {
            get { return new JavaScriptValue(IntPtr.Zero); }
        }

        /// <summary>
        /// Gets the value of <c>undefined</c> in the current script context.
        /// </summary>
        public static JavaScriptValue Undefined
        {
            get
            {
                NativeMethods.JsGetUndefinedValue(out JavaScriptValue value);
                return value;
            }
        }

        /// <summary>
        /// Gets the value of <c>null</c> in the current script context.
        /// </summary>
        public static JavaScriptValue Null
        {
            get
            {
                NativeMethods.JsGetNullValue(out JavaScriptValue value);
                return value;
            }
        }

        /// <summary>
        /// Gets the value of <c>true</c> in the current script context.
        /// </summary>
        public static JavaScriptValue True
        {
            get
            {
                NativeMethods.JsGetTrueValue(out JavaScriptValue value);
                return value;
            }
        }

        /// <summary>
        /// Gets the value of <c>false</c> in the current script context.
        /// </summary>
        public static JavaScriptValue False
        {
            get
            {
                NativeMethods.JsGetFalseValue(out JavaScriptValue value);
                return value;
            }
        }

        /// <summary>
        /// Gets the global object in the current script context.
        /// </summary>
        public static JavaScriptValue GlobalObject
        {
            get
            {
                NativeMethods.JsGetGlobalObject(out JavaScriptValue value);
                return value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the value is valid.
        /// </summary>
        public bool IsValid
        {
            get { return IntPtr.Zero != _reference; }
        }

        /// <summary>
        /// Gets the JavaScript type of the value.
        /// </summary>
        public JavaScriptValueType ValueType
        {
            get
            {
                NativeMethods.JsGetValueType(this, out JavaScriptValueType type).ThrowIfError();
                return type;
            }
        }

        /// <summary>
        /// Creates a new <c>Object</c>.
        /// </summary>
        public static JavaScriptValue CreateObject()
        {
            NativeMethods.JsCreateObject(out JavaScriptValue retVal).ThrowIfError();
            return retVal;
        }

        /// <summary>
        /// Creates a new JavaScript function.
        /// </summary>
        /// <param name="function">The method to call when the function is invoked.</param>
        public static JavaScriptValue CreateFunction(JavaScriptNativeFunction function)
        {
            NativeMethods.JsCreateFunction(function, IntPtr.Zero, out JavaScriptValue retVal).ThrowIfError();
            return retVal;
        }

        /// <summary>
        /// Creates a <c>Boolean</c> value from a <c>bool</c> value.
        /// </summary>
        /// <param name="value">The value to be converted.</param>
        public static JavaScriptValue FromBoolean(bool value)
        {
            NativeMethods.JsBoolToBoolean(value, out JavaScriptValue retVal).ThrowIfError();
            return retVal;
        }

        /// <summary>
        /// Creates a <c>Number</c> value from an <c>int</c> value.
        /// </summary>
        /// <param name="value">The value to be converted.</param>
        public static JavaScriptValue FromInt32(int value)
        {
            NativeMethods.JsIntToNumber(value, out JavaScriptValue retVal).ThrowIfError();
            return retVal;
        }

        /// <summary>
        /// Creates a <c>String</c> value from a string pointer.
        /// </summary>
        /// <param name="value">The string  to convert to a <c>String</c> value.</param>
        public static JavaScriptValue FromString(string value)
        {
            NativeMethods.JsPointerToString(
                value, new UIntPtr((uint)value.Length), out JavaScriptValue retVal).ThrowIfError();
            return retVal;
        }

        /// <summary>
        /// Gets an object's property.
        /// </summary>
        /// <param name="id">The ID of the property.</param>
        public JavaScriptValue GetProperty(JavaScriptPropertyId id)
        {
            NativeMethods.JsGetProperty(this, id, out JavaScriptValue propertyReference).ThrowIfError();
            return propertyReference;
        }

        /// <summary>
        /// Gets an object's property.
        /// </summary>
        /// <param name="name">Name of the property to get.</param>
        public JavaScriptValue GetProperty(string name)
        {
            var id = JavaScriptPropertyId.FromString(name);
            NativeMethods.JsGetProperty(this, id, out JavaScriptValue propertyReference).ThrowIfError();
            return propertyReference;
        }

        /// <summary>
        /// Retrieve the value at the specified index of an object.
        /// </summary>
        /// <param name="index">The index to retrieve.</param>
        public JavaScriptValue GetIndexedProperty(JavaScriptValue index)
        {
            NativeMethods.JsGetIndexedProperty(this, index, out JavaScriptValue retVal).ThrowIfError();
            return retVal;
        }

        /// <summary>
        /// Sets an object's property.
        /// </summary>
        /// <param name="id">The ID of the property.</param>
        /// <param name="value">The new value of the property.</param>
        /// <param name="useStrictRules">The property set should follow strict mode rules.</param>
        public void SetProperty(JavaScriptPropertyId id, JavaScriptValue value, bool useStrictRules = true)
        {
            NativeMethods.JsSetProperty(this, id, value, useStrictRules).ThrowIfError();
        }

        /// <summary>
        /// Sets an object's property.
        /// </summary>
        /// <param name="name">Name of the property.</param>
        /// <param name="value">The new value of the property.</param>
        /// <param name="useStrictRules">The property set should follow strict mode rules.</param>
        public void SetProperty(string name, JavaScriptValue value, bool useStrictRules = true)
        {
            var id = JavaScriptPropertyId.FromString(name);
            NativeMethods.JsSetProperty(this, id, value, useStrictRules).ThrowIfError();
        }

        /// <summary>
        /// Invokes a function.
        /// </summary>
        /// <param name="arguments">The arguments to the call.</param>
        public JavaScriptValue CallFunction(params JavaScriptValue[] arguments)
        {
            JavaScriptValue returnReference;

            if (arguments.Length >= ushort.MaxValue)
                throw new ArgumentOutOfRangeException("arguments");

            NativeMethods.JsCallFunction(this, arguments, (ushort)arguments.Length, out returnReference)
                         .ThrowIfError();

            return returnReference;
        }

        /// <summary>
        /// Retrieves the <c>bool</c> value of a <c>Boolean</c> value.
        /// </summary>
        public bool ToBoolean()
        {
            NativeMethods.JsBooleanToBool(this, out bool retVal).ThrowIfError();
            return retVal;
        }

        /// <summary>
        /// Retrieves the <c>int</c> value of a <c>Number</c> value.
        /// </summary>
        public int ToInt32()
        {
            NativeMethods.JsNumberToInt(this, out int retVal).ThrowIfError();
            return retVal;
        }

        /// <summary>
        /// Retrieves the <c>string</c> value.
        /// </summary>
        public override string ToString()
        {
            NativeMethods.JsStringToPointer(this, out IntPtr buffer, out UIntPtr length).ThrowIfError();
            return Marshal.PtrToStringUni(buffer, (int)length);
        }
    }
}