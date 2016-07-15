using Microsoft.VisualStudio.Text;

namespace Emmet.Snippets
{
    /// <summary>
    /// Represents a region of source code in the editor that has some tab stops in it.
    /// </summary>
    public class CodeSnippet
    {
        public const string ViewPropertyName = "Snippet";

        private ViewContext _view;

        private ITrackingSpan[] _tabStops;

        // Each element of array is a group number of the tab stop with the corresponding index in tab stops
        // array.
        private int[] _tabStopGroups;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeSnippet"/> class. Constructor that prevents a
        /// default instance of this class from being created.
        /// </summary>
        private CodeSnippet()
        {
        }

        /// <summary>
        /// Creates code snippet in the specified view.
        /// </summary>
        /// <param name="view">Editor view that contains snippet.</param>
        /// <param name="tabStops">Tab stops collection.</param>
        /// <param name="tabStopGroups">Groups the tab stop belongs to.</param>
        public static CodeSnippet CreateSnippetInView(ViewContext view, Span[] tabStops, int[] tabStopGroups)
        {
            var snapshot = view.CurrentBuffer.CurrentSnapshot;
            var tabStopTrackers = ConvertToTrackingSpans(view, tabStops);

            CodeSnippet retVal = new CodeSnippet();
            retVal._view = view;
            retVal._tabStops = tabStopTrackers;
            retVal._tabStopGroups = tabStopGroups;

            // Cleanup any existing snippets in this view.
            var viewProperties = view.WpfView.Properties;
            if (viewProperties.ContainsProperty(ViewPropertyName))
                viewProperties.RemoveProperty(ViewPropertyName);

            viewProperties.GetOrCreateSingletonProperty(ViewPropertyName, () => retVal);

            return retVal;
        }

        /// <summary>
        /// Gets this snippet tab stops positions in the active text snapshot.
        /// </summary>
        public Span[] GetTabStopsPositions()
        {
            Span[] retVal = new Span[_tabStops.Length];

            for (int index = 0; index < _tabStops.Length; index++)
                retVal[index] = _tabStops[index].GetSpan(_view.WpfView.TextBuffer.CurrentSnapshot);

            return retVal;
        }

        /// <summary>
        /// Moves caret to the first tab stop in the active snippet.
        /// </summary>
        public void BeginEditSnippet()
        {
            int index = GetLastTabStopInGroup(0);
            MoveToSpan(_tabStops[index]);

            // There is no need to track anything if there is only one tab stop.
            if (1 == _tabStops.Length)
                EndEditSnippet();
        }

        /// <summary>
        /// Tries to move caret to next tab stop. Returns true if succeeded; false otherwise. False
        /// indicates that caret is not inside any of the available tab stops which indicates that tab stops
        /// tracking should be finished and this snippet should be detached from the view.
        /// </summary>
        /// <param name="reverse">Indicates whether move should be made in reverse direction.</param>
        public bool TryMoveToNextTabStop(bool reverse = false)
        {
            int caret = _view.WpfView.Caret.Position.BufferPosition.Position;
            int index;
            Span tabStop;

            // Find tab stop that caret is currently in.
            for (index = 0; index < _tabStops.Length; index++)
            {
                tabStop = _tabStops[index].GetSpan(_view.WpfView.TextBuffer.CurrentSnapshot);

                // Cannot use Span.Contains method as it reports false when caret is on the edge of the span.
                if (tabStop.Start > caret || tabStop.End < caret)
                    continue;

                if (!_view.WpfView.Selection.IsEmpty)
                    _view.WpfView.Selection.Clear();

                // If we are at the last tab stop in the list then start from the first one.
                if (!reverse && ++index == _tabStops.Length)
                    index = 0;
                else if (reverse && 0 == index--)
                    index = _tabStops.Length - 1;

                index = GetLastTabStopInGroup(index);
                MoveToSpan(_tabStops[index]);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Detaches this object from the target view.
        /// </summary>
        public void EndEditSnippet()
        {
            _view.WpfView.Properties.RemoveProperty(ViewPropertyName);
        }

        private static ITrackingSpan[] ConvertToTrackingSpans(ViewContext view, Span[] spans)
        {
            ITrackingSpan[] retVal = new ITrackingSpan[spans.Length];
            ITextSnapshot snapshot = view.WpfView.TextSnapshot;

            for (int index = 0; index < spans.Length; index++)
            {
                retVal[index] = snapshot.CreateTrackingSpan(spans[index], SpanTrackingMode.EdgeInclusive);
            }

            return retVal;
        }

        private void MoveToSpan(ITrackingSpan span)
        {
            SnapshotSpan target = span.GetSpan(_view.WpfView.TextBuffer.CurrentSnapshot);
            MoveToSpan(target);
        }

        private void MoveToSpan(SnapshotSpan target)
        {
            _view.WpfView.Caret.MoveTo(target.Start);

            if (target.Length < 1)
                return;

            string content = target.GetText();
            if (!string.IsNullOrWhiteSpace(content))
            {
                SnapshotSpan selection = new SnapshotSpan(_view.WpfView.TextBuffer.CurrentSnapshot, target);
                _view.WpfView.Selection.Select(target, false);
            }
            else if (4 == content.Length)
            {
                // special case for code formatting issues
                _view.WpfView.Caret.MoveTo(target.Start + 2);
            }
            else
            {
                _view.WpfView.Caret.MoveTo(target.End);
            }
        }

        private int GetLastTabStopInGroup(int index = 0)
        {
            // Group zero does not apply here as it is used by default when no grouping is required.
            if (0 == _tabStopGroups[index])
                return index;

            for (int i = index; i < _tabStopGroups.Length; i++)
            {
                if (_tabStopGroups[i] == _tabStopGroups[index])
                    index = i;
            }

            return index;
        }
    }
}