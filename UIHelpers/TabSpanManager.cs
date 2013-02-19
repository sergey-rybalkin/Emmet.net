using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace UIHelpers
{
    /// <summary>
    /// Enables caret navigation between tab stops in the generated markup.
    /// </summary>
    internal class TabSpanManager
    {
        private static readonly Regex _placeholders =
            new Regex(@"(\${\d*(?::([^}]+))?})", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly IWpfTextView _view;
        private LinkedList<ITrackingSpan> _tabSpans;

        // Last span that we have selected as a response to TAB or BACKTAB
        private LinkedListNode<ITrackingSpan> _lastNavigatedSpan;

        internal TabSpanManager(IWpfTextView view)
        {
            _view = view;
        }

        /// <summary>
        /// Starts tab stops navigation inside currently selected text.
        /// </summary>
        internal void PostProcessSelection()
        {
            _tabSpans = null;
            _lastNavigatedSpan = null;
            ITextSelection selection = _view.Selection;
            if (selection.SelectedSpans.Count == 0)
                return;

            SnapshotSpan selectionSpan = selection.SelectedSpans[0];
            string insertedText = _view.TextBuffer.CurrentSnapshot.GetText(selectionSpan);
            selection.Clear();

            if (!FindTabSpans(insertedText, selectionSpan))
            {
                _view.Caret.MoveTo(new SnapshotPoint(_view.TextBuffer.CurrentSnapshot, selectionSpan.End));
                return;
            }

            Dispatcher.CurrentDispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                new Action(() => MoveToNextEmptySlot()));
        }

        /// <summary>
        /// Moves caret to the next tab stop if any.
        /// </summary>
        internal bool MoveToNextEmptySlot()
        {
            if (_tabSpans == null || (_tabSpans.Count == 0))
                return false;

            LinkedListNode<ITrackingSpan> node;
            if (null == _lastNavigatedSpan)
            {
                node = _tabSpans.First;
                _lastNavigatedSpan = node;
            }
            else if (null != _lastNavigatedSpan.Next)
            {
                node = _lastNavigatedSpan.Next;
                _lastNavigatedSpan = node;
            }
            else
            {
                _tabSpans = null;
                _lastNavigatedSpan = null;

                return false;
            }

            Span span = node.Value.GetSpan(_view.TextBuffer.CurrentSnapshot);
            MoveTab(span);

            return true;
        }

        /// <summary>
        /// Moves caret to the previous tab stop if any.
        /// </summary>
        internal bool MoveToPreviousEmptySlot()
        {
            if (_tabSpans == null || (_tabSpans.Count == 0))
                return false;

            if (null == _lastNavigatedSpan)
                return false;

            LinkedListNode<ITrackingSpan> node;
            if (null != _lastNavigatedSpan.Previous)
            {
                node = _lastNavigatedSpan.Previous;
                _lastNavigatedSpan = node;
            }
            else
            {
                _tabSpans = null;
                _lastNavigatedSpan = null;

                return false;
            }

            Span span = node.Value.GetSpan(_view.TextBuffer.CurrentSnapshot);
            MoveTab(span);

            return true;
        }

        /// <summary>
        /// Searches through the specified content for tab span markers and cleans up content.
        /// </summary>
        /// <param name="insertedText">Text to search through.</param>
        /// <param name="targetSpan">Span that contains the specified text.</param>
        /// <returns>
        /// <code>true</code> if text contains tab span markers, <code>false</code> otherwise.
        /// </returns>
        private bool FindTabSpans(string insertedText, Span targetSpan)
        {
            MatchCollection matches = _placeholders.Matches(insertedText);
            if (matches.Count == 0)
                return false;

            using (ITextEdit edit = _view.TextBuffer.CreateEdit())
            {
                _tabSpans = new LinkedList<ITrackingSpan>();
                ITextSnapshot currentSnapshot = _view.TextBuffer.CurrentSnapshot;
                foreach (Match match in matches)
                {
                    string defaultContent = match.Groups[2].Value;
                    int tabSpanPosition = targetSpan.Start + match.Groups[1].Index;
                    int tabSpanLen = string.IsNullOrEmpty(defaultContent) ? 0 : defaultContent.Length;
                    ITrackingSpan span = currentSnapshot.CreateTrackingSpan(
                        new Span(tabSpanPosition, tabSpanLen), SpanTrackingMode.EdgeInclusive);
                    _tabSpans.AddLast(span);

                    edit.Delete(tabSpanPosition, match.Groups[1].Value.Length);
                    if (!string.IsNullOrEmpty(defaultContent))
                        edit.Insert(tabSpanPosition, defaultContent);
                }

                // As a last tab span we add last position in the inserted text
                ITrackingSpan lastSpan = currentSnapshot.CreateTrackingSpan(
                    new Span(targetSpan.Start + targetSpan.Length, 0), SpanTrackingMode.EdgeExclusive);
                _tabSpans.AddLast(lastSpan);

                edit.Apply();
            }

            return true;
        }

        private void MoveTab(Span quote)
        {
            _view.Caret.MoveTo(new SnapshotPoint(_view.TextBuffer.CurrentSnapshot, quote.Start));
            SnapshotSpan selectionSpan = new SnapshotSpan(_view.TextBuffer.CurrentSnapshot, quote);
            _view.Selection.Select(selectionSpan, false);
        }
    }
}