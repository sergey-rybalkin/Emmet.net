namespace Emmet
{
    /// <summary>
    /// Defines API for working with Visual Studio editor.
    /// </summary>
    public interface ICodeEditor
    {
        /// <summary>
        /// Returns content of the line under caret position.
        /// </summary>
        string GetCurrentLine();
    }
}
