namespace Emmet.Engine.ChakraInterop
{
    /// <summary>
    /// The JavaScript type of a JavaScriptValue.
    /// </summary>
    public enum JavaScriptValueType
    {
        Undefined = 0,

        Null = 1,

        Number = 2,

        String = 3,

        Boolean = 4,

        Object = 5,

        Function = 6,

        Error = 7,

        Array = 8,

        Symbol = 9,

        ArrayBuffer = 10,

        TypedArray = 11,

        DataView = 12,
    }
}