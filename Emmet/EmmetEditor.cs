using Emmet.Engine;
using Emmet.Snippets;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using System.Windows.Forms;

namespace Emmet
{
    /// <summary>
    /// Implements editor interface required by the Emmet engine based on the Visual Studio editor.
    /// </summary>
    public class EmmetEditor : IEmmetEditor
    {
        private readonly IVsTextView _textView;

        private readonly IWpfTextView _wpfView;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmmetEditor"/> class.
        /// </summary>
        /// <param name="wpfView">The WPF interface for the editor.</param>
        /// <param name="textView">The text view interface for the editor.</param>
        public EmmetEditor(IWpfTextView wpfView, IVsTextView textView)
        {
            _wpfView = wpfView;
            _textView = textView;
        }

        /// <summary>
        /// Selects the specified text range.
        /// </summary>
        /// <param name="start">First character to select.</param>
        /// <param name="end">Last character to select.</param>
        public void CreateSelection(int start, int end)
        {
            if (start == end)
            {
                SetCaretPosition(start);
                return;
            }

            Span range = new Span(start, end - start);
            SnapshotSpan selectionSpan = new SnapshotSpan(_wpfView.TextBuffer.CurrentSnapshot, range);
            _wpfView.Selection.Select(selectionSpan, false);
        }

        /// <summary>
        /// Gets caret position.
        /// </summary>
        public int GetCaretPosition()
        {
            return _wpfView.Caret.Position.BufferPosition.Position;
        }

        /// <summary>
        /// Gets the content of the editor.
        /// </summary>
        public string GetContent()
        {
            return _wpfView.TextSnapshot.GetText();
        }

        /// <summary>
        /// Gets content type of the text buffer that has the caret.
        /// </summary>
        public string GetContentTypeInActiveBuffer()
        {
            IContentType retVal = _wpfView.TextBuffer.ContentType;
            IProjectionBuffer projection = _wpfView.TextBuffer as IProjectionBuffer;
            if (null == projection)
                return retVal?.TypeName?.ToLowerInvariant();

            // Current view has several buffers (e.g. html file with css code), get the one that has the caret
            var caretPosition = _wpfView.Caret.Position.BufferPosition;
            var buffers = projection.SourceBuffers;
            var bufferGraph = _wpfView.BufferGraph;
            
            foreach (ITextBuffer buffer in buffers)
            {
                SnapshotPoint? point = bufferGraph.MapDownToBuffer(caretPosition,
                                                                   PointTrackingMode.Negative,
                                                                   buffer,
                                                                   PositionAffinity.Predecessor);

                if (!point.HasValue)
                    continue;

                // Several matches possible, we are interested in the last one.
                retVal = buffer.ContentType;
            }

            return retVal?.TypeName?.ToLowerInvariant();
        }

        /// <summary>
        /// Gets text on the current line.
        /// </summary>
        public string GetCurrentLine()
        {
            return _wpfView.Caret.Position.BufferPosition.GetContainingLine().GetText();
        }

        /// <summary>
        /// Gets indexes of the first and last characters of the current line in the editor.
        /// </summary>
        public Range GetCurrentLineRange()
        {
            int caretPosition = _wpfView.Caret.Position.BufferPosition.Position;
            var line = _wpfView.TextSnapshot.GetLineFromPosition(caretPosition);

            return new Range(line.Start.Position, line.End.Position);
        }

        /// <summary>
        /// Gets currently selected text.
        /// </summary>
        public string GetSelection()
        {
            var selection = _wpfView.Selection;
            if (0 == selection.SelectedSpans.Count)
                return string.Empty;

            SnapshotSpan selectionSpan = selection.SelectedSpans[0];
            string selectedText = _wpfView.TextBuffer.CurrentSnapshot.GetText(selectionSpan);
            return selectedText;
        }

        /// <summary>
        /// Gets current selection range or caret position if no selection exists.
        /// </summary>
        public Range GetSelectionRange()
        {
            var selection = _wpfView.Selection;

            return new Range(selection.Start.Position, selection.End.Position);
        }

        /// <summary>
        /// Displays dialog box that prompts user for input.
        /// </summary>
        public string Prompt()
        {
            AbbreviationPrompt dlg = new AbbreviationPrompt();
            if (DialogResult.OK != dlg.ShowDialog() || string.IsNullOrWhiteSpace(dlg.Abbreviation))
                return null;

            return dlg.Abbreviation;
        }

        /// <summary>
        /// Replaces the specified region in the document with the new content. If region start position is
        /// negative then the whole document will be replaced with the specified content.
        /// </summary>
        /// <param name="newContent">New content to replace the specified range with.</param>
        /// <param name="startPosition">The start position of the range to replace.</param>
        /// <param name="endPosition">The end position of the range to replace.</param>
        public void ReplaceContentRange(string newContent, int startPosition = -1, int endPosition = 0)
        {
            Span range;
            if (startPosition > 0)
                range = new Span(startPosition, endPosition - startPosition);
            else
                range = new Span(0, _wpfView.TextBuffer.CurrentSnapshot.Length);

            _wpfView.TextBuffer.Replace(range, newContent);
        }

        /// <summary>
        /// Sets the caret position.
        /// </summary>
        /// <param name="position">New caret position.</param>
        public void SetCaretPosition(int position)
        {
            SnapshotPoint point = new SnapshotPoint(_wpfView.TextBuffer.CurrentSnapshot, position);
            _wpfView.Caret.MoveTo(point);
        }

        /// <summary>
        /// Starts tracking the specified regions as tab stops.
        /// </summary>
        /// <param name="tabStops">The tab stops array with start and end positions.</param>
        /// <param name="tabStopGroups">Tab stops groups indexes array.</param>
        public void TrackTabStops(Range[] tabStops, int[] tabStopGroups)
        {
            if (null == tabStops || 0 == tabStops.Length)
                return;

            Span[] tabStopSpans = new Span[tabStops.Length];
            for (int index = 0; index < tabStops.Length; index++)
            {
                Range source = tabStops[index];
                tabStopSpans[index] = new Span(source.Start, source.End - source.Start);
            }

            CodeSnippet.CreateSnippetInView(new ViewContext(_wpfView, _textView),
                                            tabStopSpans,
                                            tabStopGroups);
        }

        /// <summary>
        /// Formats the specified code region. If region position is
        /// negative then the whole document will be formatted.
        /// </summary>
        /// <param name="startPosition">The start position of the region to format.</param>
        /// <param name="endPosition">The end position of the region to format.</param>
        public void FormatRegion(int startPosition = -1, int endPosition = 0)
        {
            Span span;
            if (startPosition > 0)
                span = new Span(startPosition, endPosition - startPosition);
            else
                span = new Span(0, _wpfView.TextBuffer.CurrentSnapshot.Length);

            SnapshotSpan snapshotSpan = new SnapshotSpan(_wpfView.TextBuffer.CurrentSnapshot, span);
            _wpfView.Selection.Select(snapshotSpan, false);

            ExternalCommandsDispatcher.FormatSelection();

            _wpfView.Selection.Clear();
        }
    }
}