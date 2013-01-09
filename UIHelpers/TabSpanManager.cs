using System;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace UIHelpers
{
    /// <summary>
    /// Enables navigation between tab stops in the generated markup.
    /// </summary>
    internal class TabSpanManager
    {
        private static readonly Regex _placeholders =
            new Regex("(\\$\\{[^\\}]*\\})", RegexOptions.IgnoreCase);

        private readonly IWpfTextView _view;
        private ITrackingSpan _trackingSpan;

        internal TabSpanManager(IWpfTextView view)
        {
            _view = view;
        }

        /// <summary>
        /// Moves caret to the next tab stop if any.
        /// </summary>
        internal bool MoveToNextEmptySlot()
        {
            if (_trackingSpan != null)
            {
                int position = _view.Caret.Position.BufferPosition.Position + 1;
                Span span = _trackingSpan.GetSpan(_view.TextBuffer.CurrentSnapshot);
                if (span.Contains(position))
                {
                    Span zenSpan = new Span(position, span.End - position);
                    SetCaret(zenSpan, false);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Moves caret to the previous tab stop if any.
        /// </summary>
        internal bool MoveToPreviousEmptySlot()
        {
            if (_trackingSpan != null)
            {
                int position = _view.Caret.Position.BufferPosition.Position;
                if (position > 0)
                {
                    Span span = _trackingSpan.GetSpan(_view.TextBuffer.CurrentSnapshot);
                    if (span.Contains(position - 1))
                    {
                        Span zenSpan = new Span(span.Start, (position - span.Start) - 1);
                        SetCaret(zenSpan, true);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Starts tab stops navigation inside currently selected text.
        /// </summary>
        /// <param name="originalPosition">
        /// Original position of the caret before generated markup was pasted.
        /// </param>
        internal void PostProcessSelection(int originalPosition)
        {
            ITextSelection selection = _view.Selection;
            if (selection.SelectedSpans.Count == 0)
            {
                _trackingSpan = null;
                return;
            }

            SnapshotSpan selectionSpan = selection.SelectedSpans[0];
            Span insertedText = new Span(originalPosition, selectionSpan.Length);
            _trackingSpan = _view.TextBuffer.CurrentSnapshot.CreateTrackingSpan(
                insertedText, SpanTrackingMode.EdgeExclusive);

            selection.Clear();
            Dispatcher.CurrentDispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                new Action(() => SetCaret(insertedText, false)));
        }

        private static Span FindTabSpan(Span zenSpan, bool isReverse, string text, Regex regex)
        {
            MatchCollection matchs = regex.Matches(text);
            if (!isReverse)
            {
                foreach (Match match in matchs)
                {
                    Group group = match.Groups[1];
                    if (group.Index >= zenSpan.Start)
                        return new Span(group.Index, group.Length);
                }
            }
            else
            {
                for (int i = matchs.Count - 1; i >= 0; i--)
                {
                    Group group = matchs[i].Groups[1];
                    if (group.Index < zenSpan.End)
                        return new Span(group.Index, group.Length);
                }
            }

            return new Span();
        }

        private bool SetCaret(Span zenSpan, bool isReverse)
        {
            string text = _view.TextBuffer.CurrentSnapshot.GetText();
            Span placeholders = FindTabSpan(zenSpan, isReverse, text, _placeholders);

            if (zenSpan.Contains(placeholders.Start))
            {
                MoveTab(placeholders);
                return true;
            }

            if (!isReverse)
            {
                MoveTab(new Span(zenSpan.End, 0));
                return true;
            }

            return false;
        }

        private void MoveTab(Span quote)
        {
            _view.Caret.MoveTo(new SnapshotPoint(_view.TextBuffer.CurrentSnapshot, quote.Start));
            SnapshotSpan selectionSpan = new SnapshotSpan(_view.TextBuffer.CurrentSnapshot, quote);
            _view.Selection.Select(selectionSpan, false);
        }
    }
}