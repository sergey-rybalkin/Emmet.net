using System;
using System.Windows.Forms;
using Emmet.Engine;
using Emmet.Snippets;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace Emmet
{
    /// <summary>
    /// Contains context and provides helper methods for code editor view.
    /// </summary>
    public struct ViewContext : ICodeEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewContext"/> struct.
        /// </summary>
        /// <param name="wpfView">The WPF interface for the editor.</param>
        /// <param name="textView">The text view interface for the editor.</param>
        public ViewContext(IWpfTextView wpfView, IVsTextView textView)
        {
            WpfView = wpfView;
            TextView = textView;
            AbbreviationPrefix = null;
        }

        /// <summary>
        /// Gets the WPF view.
        /// </summary>
        public IWpfTextView WpfView { get; private set; }

        /// <summary>
        /// Gets the text view.
        /// </summary>
        public IVsTextView TextView { get; private set; }

        /// <summary>
        /// Gets or sets prefix that is expected to present before abbreviations. This is required to
        /// support mixed content files like JSX and avoid collisions with non-html/css languages. See issue
        /// #5.
        /// </summary>
        public string AbbreviationPrefix { get; set; }

        /// <summary>
        /// Gets the current text buffer.
        /// </summary>
        public ITextBuffer CurrentBuffer
        {
            get { return WpfView.TextBuffer; }
        }

        /// <summary>
        /// Gets column number that caret is in.
        /// </summary>
        public int GetCaretPosColumn()
        {
            return GetCaretColumn(WpfView);
        }

        /// <summary>
        /// Returns content of the line under caret position.
        /// </summary>
        public string GetCurrentLine() => WpfView.Caret.Position.BufferPosition.GetContainingLine().GetText();

        /// <summary>
        /// Gets content type (html, css, scss etc.) of the text buffer that has the caret.
        /// </summary>
        public string GetContentTypeInActiveBuffer()
        {
            IContentType retVal = WpfView.TextBuffer.ContentType;
            if (!(WpfView.TextBuffer is IProjectionBuffer projection))
                return retVal?.TypeName?.ToLowerInvariant();

            // Current view has several buffers (e.g. html file with css), get the one that has the caret.
            var caretPosition = WpfView.Caret.Position.BufferPosition;
            var buffers = projection.SourceBuffers;
            var bufferGraph = WpfView.BufferGraph;

            foreach (ITextBuffer buffer in buffers)
            {
                SnapshotPoint? point = bufferGraph.MapDownToBuffer(
                    caretPosition,
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
        /// Replaces line that contains caret with the specified content.
        /// </summary>
        /// <param name="newContent">New content to replace current line with.</param>
        public void ReplaceCurrentLine(string newContent)
        {
            int caretPosition = WpfView.Caret.Position.BufferPosition.Position;
            ITextSnapshotLine line = WpfView.TextSnapshot.GetLineFromPosition(caretPosition);
            Span currentLineSpan = new Span(line.Start.Position, line.End.Position - line.Start.Position);

            ReplaceRegion(currentLineSpan, newContent);
        }

        /// <summary>
        /// Replaces currently selected text with the specified content.
        /// </summary>
        /// <param name="newContent">New content to replace selection with.</param>
        public void ReplaceSelection(string newContent)
        {
            var selection = WpfView.Selection;
            int selectionLength = selection.End.Position - selection.Start.Position;
            Span selectionSpan = new Span(selection.Start.Position, selectionLength);

            ReplaceRegion(selectionSpan, newContent);
        }

        /// <summary>
        /// Displays dialog box that prompts user for input.
        /// </summary>
        public string Prompt()
        {
            var dlg = new AbbreviationPrompt();
            if (DialogResult.OK != dlg.ShowDialog() || string.IsNullOrWhiteSpace(dlg.Abbreviation))
                return null;

            return dlg.Abbreviation;
        }

        /// <summary>
        /// Gets currently selected text.
        /// </summary>
        public string GetSelection()
        {
            var selection = WpfView.Selection;
            if (selection.SelectedSpans.Count is 0)
                return string.Empty;

            SnapshotSpan selectionSpan = selection.SelectedSpans[0];
            string selectedText = WpfView.TextBuffer.CurrentSnapshot.GetText(selectionSpan);
            return selectedText;
        }

        /// <summary>
        /// Given an IWpfTextView, find the position of the caret and report its column
        /// number. The column number is 0-based.
        /// </summary>
        /// <param name="textView">The text view containing the caret</param>
        /// <returns>The column number of the caret's position. When the caret is at the
        /// leftmost column, the return value is zero.</returns>
        private static int GetCaretColumn(IWpfTextView textView)
        {
            // This is the code the editor uses to populate the status bar.
            Microsoft.VisualStudio.Text.Formatting.ITextViewLine caretViewLine =
                textView.Caret.ContainingTextViewLine;
            double columnWidth = textView.FormattedLineSource.ColumnWidth;
            return (int)Math.Round((textView.Caret.Left - caretViewLine.Left) / columnWidth);
        }

        private void ReplaceRegion(Span region, string replacement)
        {
            var parser = TabStopsParser.ParseContent(replacement, region.Start);

            WpfView.TextBuffer.Replace(region, parser.Content);
            if (parser.TabStops?.Length > 0)
                CodeSnippet.CreateSnippetInView(this, parser.TabStops);

            FormatRegion(region.Start, region.Start + parser.Content.Length);
        }

        private void FormatRegion(int startPosition = -1, int endPosition = 0)
        {
            Span span;
            if (startPosition > 0)
                span = new Span(startPosition, endPosition - startPosition);
            else
                span = new Span(0, WpfView.TextBuffer.CurrentSnapshot.Length);

            var snapshotSpan = new SnapshotSpan(WpfView.TextBuffer.CurrentSnapshot, span);
            WpfView.Selection.Select(snapshotSpan, false);

            ExternalCommandsDispatcher.FormatSelection();

            WpfView.Selection.Clear();
        }
    }
}