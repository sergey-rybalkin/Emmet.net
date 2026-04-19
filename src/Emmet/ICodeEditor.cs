namespace Emmet
{
    /// <summary>
    /// Defines API for working with Visual Studio editor.
    /// </summary>
    public interface ICodeEditor
    {
        /// <summary>
        /// Gets prefix that is expected to present before abbreviations. This is required to support mixed
        /// content files like JSX and avoid collisions with non-html/css languages. See issue #5.
        /// </summary>
        string AbbreviationPrefix { get; }

        /// <summary>
        /// Returns content of the line under caret position.
        /// </summary>
        string GetCurrentLine();

        /// <summary>
        /// Gets column number that caret is in.
        /// </summary>
        int GetCaretPosColumn();

        /// <summary>
        /// Gets currently selected text.
        /// </summary>
        string GetSelection();

        /// <summary>
        /// Gets content type (html, css, scss etc.) of the text buffer that has the caret.
        /// </summary>
        string GetContentTypeInActiveBuffer();

        /// <summary>
        /// Replaces line that contains caret with the specified content.
        /// </summary>
        /// <param name="newContent">New content to replace current line with.</param>
        /// <param name="formatNewCode">
        /// Indicates whether to call Visual Studio formatting for the new content.
        /// </param>
        void ReplaceCurrentLine(string newContent, bool formatNewCode = true);

        /// <summary>
        /// Replaces currently selected text with the specified content.
        /// </summary>
        /// <param name="newContent">New content to replace selection with.</param>
        /// <param name="formatNewCode">
        /// Indicates whether to call Visual Studio formatting for the new content.
        /// </param>
        void ReplaceSelection(string newContent, bool formatNewCode = true);

        /// <summary>
        /// Displays dialog box that prompts user for input.
        /// </summary>
        string Prompt();
    }
}
