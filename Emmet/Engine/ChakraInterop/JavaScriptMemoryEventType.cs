namespace Emmet.Engine.ChakraInterop
{
    /// <summary>
    /// Allocation callback event type.
    /// </summary>
    public enum JavaScriptMemoryEventType
    {
        Allocate = 0,

        Free = 1,

        Failure = 2
    }
}