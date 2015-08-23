using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Text.Editor;

namespace Emmet.EditorExtensions
{
    /// <summary>
    /// Tagger that creates text markers for tab stops after expand abbreviation command is executed.
    /// </summary>
    public class TabStopsTagger : ITagger<TextMarkerTag>
    {
        private readonly ITextView _view;

        private readonly ITextBuffer _buffer;

        /// <summary>
        /// Initializes a new instance of the <see cref="TabStopsTagger"/> class.
        /// </summary>
        /// <param name="view">The view that contains tab stops.</param>
        /// <param name="sourceBuffer">Text buffer that contains tab stops.</param>
        public TabStopsTagger(ITextView view, ITextBuffer sourceBuffer)
        {
            _view = view;
            _buffer = sourceBuffer;

            EmmetCommandTarget filter;
            if(_view.Properties.TryGetProperty("EmmetCommandTarget", out filter))
                filter.TabStopsChanged += TabStopsChanged;
        }

        /// <summary>
        /// Event queue for all listeners interested in TagsChanged events.
        /// </summary>
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        /// <summary>
        /// Gets all the tags that intersect the specified spans.
        /// </summary>
        /// <param name="spans">The spans to visit.</param>
        public IEnumerable<ITagSpan<TextMarkerTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            EmmetCommandTarget filter;
            bool hasTags = _view.Properties.TryGetProperty("EmmetCommandTarget", out filter);
            if (!hasTags || !filter.HasActiveTabStops)
                yield break;

            Span[] expansionSpans = filter.GetTabStopsToHighlight();
            foreach (Span span in expansionSpans)
            {
                SnapshotSpan snapshotSpan;
                try
                {
                    if (span.Length == 0)
                        snapshotSpan = new SnapshotSpan(_buffer.CurrentSnapshot, new Span(span.Start, 1));
                    else
                        snapshotSpan = new SnapshotSpan(_buffer.CurrentSnapshot, span);
                }
                catch (ArgumentOutOfRangeException)
                {
                    // Sometimes happens when quickly pressing Undo etc.
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(snapshotSpan.GetText()) && spans.OverlapsWith(snapshotSpan))
                    yield return new TagSpan<TextMarkerTag>(
                        snapshotSpan, new TextMarkerTag("MarkerFormatDefinition/HighlightedReference"));
            }
        }

        private void TabStopsChanged(object sender, EventArgs e)
        {
            int bufferLen = _buffer.CurrentSnapshot.Length;
            Span affectedSpan = new Span(0, bufferLen);
            TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(
                new SnapshotSpan(_buffer.CurrentSnapshot, affectedSpan)));
        }
    }
}