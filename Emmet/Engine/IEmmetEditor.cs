using System;

namespace Emmet.Engine
{
    /// <summary>
    /// Defines editor interface required by the Emmet engine.
    /// </summary>
    public interface IEmmetEditor
    {
        /// <summary>
        /// Selects the specified text range.
        /// </summary>
        /// <param name="start">First character to select.</param>
        /// <param name="end">Last character to select.</param>
        void CreateSelection(int start, int end);

        /// <summary>
        /// Gets caret position.
        /// </summary>
        int GetCaretPosition();

        /// <summary>
        /// Gets the content of the editor.
        /// </summary>
        string GetContent();

        /// <summary>
        /// Gets content type of the text buffer that has the caret.
        /// </summary>
        string GetContentTypeInActiveBuffer();

        /// <summary>
        /// Gets text on the current line.
        /// </summary>
        string GetCurrentLine();

        /// <summary>
        /// Gets indexes of the first and last characters of the current line in the editor.
        /// </summary>
        Range GetCurrentLineRange();

        /// <summary>
        /// Gets currently selected text.
        /// </summary>
        string GetSelection();

        /// <summary>
        /// Gets current selection range or caret position if no selection exists.
        /// </summary>
        Range GetSelectionRange();

        /// <summary>
        /// Displays dialog box that prompts user for input.
        /// </summary>
        string Prompt();

        /// <summary>
        /// Replaces the specified region in the document with the new content. If region start position is
        /// negative then the whole document will be replaced with the specified content.
        /// </summary>
        /// <param name="newContent">New content to replace the specified range with.</param>
        /// <param name="startPosition">The start position of the range to replace.</param>
        /// <param name="endPosition">The end position of the range to replace.</param>
        void ReplaceContentRange(string newContent, int startPosition = -1, int endPosition = 0);

        /// <summary>
        /// Sets the caret position.
        /// </summary>
        /// <param name="position">New caret position.</param>
        void SetCaretPosition(int position);

        /// <summary>
        /// Starts tracking the specified regions as tab stops.
        /// </summary>
        /// <param name="tabStops">The tab stops array with start and end positions.</param>
        /// <param name="tabStopGroups">Tab stops groups indexes array.</param>
        void TrackTabStops(Range[] tabStops, int[] tabStopGroups);

        /// <summary>
        /// Formats the specified code region. If region position is
        /// negative then the whole document will be formatted.
        /// </summary>
        /// <param name="startPosition">The start position of the region to format.</param>
        /// <param name="endPosition">The end position of the region to format.</param>
        void FormatRegion(int startPosition = -1, int endPosition = 0);
    }
}