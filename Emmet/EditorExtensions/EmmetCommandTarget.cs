using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Emmet.Snippets;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Emmet.EditorExtensions
{
    /// <summary>
    /// Handles Emmet commands passed to the attached view.
    /// </summary>
    public class EmmetCommandTarget : CommandTargetBase
    {
        private List<string> _htmlTags = new List<string>() { "link", "meta", "style", "title", "html", "body", "address", "article", "aside", "footer", "header", "h1", "h2", "h3", "h4", "h5", "h6", "hgroup", "main", "nav", "section", "blockquote", "dd", "dir", "div", "dl", "dt", "figcaption", "figure", "hr", "li", "main", "ol", "p", "pre", "ul", "a", "abbr", "b", "bdi", "bdo", "br", "cite", "code", "data", "dfn", "em", "i", "kbd", "mark", "q", "rb", "rp", "rt", "rtc", "ruby", "s", "samp", "small", "span", "strong", "sub", "sup", "time", "tt", "u", "var", "wbr", "area", "audio", "img", "map", "track", "video", "applet", "embed", "iframe", "noembed", "object", "param", "picture", "source", "canvas", "noscript", "script", "del", "ins", "caption", "col", "colgroup", "table", "tbody", "td", "tfoot", "th", "thead", "tr", "button", "datalist", "fieldset", "form", "input", "label", "legend", "meter", "optgroup", "option", "output", "progress", "select", "textarea", "details", "dialog", "menu", "menuitem", "summary", "content", "element", "shadow", "slot", "template" };
        private List<string> _emmetSpecChars = new List<string>() { ".", "#", ">", "{", "[" };

        private readonly ICompletionBroker _completionBroker;

        private readonly bool _expandAbbreviationOnTab = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmmetCommandTarget"/> class.
        /// </summary>
        /// <param name="view">Context of the view to operate on.</param>
        /// <param name="completionBroker">The completion broker to control intellisense UI.</param>
        public EmmetCommandTarget(ViewContext view, ICompletionBroker completionBroker)
            : base(view)
        {
            _completionBroker = completionBroker;
            _expandAbbreviationOnTab = EmmetPackage.Options.InterceptTabs;
        }

        /// <summary>
        /// Event queue for all listeners interested in TabStopsRemoved events which is triggered when tab
        /// stops collection changes.
        /// </summary>
        public event EventHandler TabStopsChanged;

        /// <summary>
        /// Gets a value indicating whether associated view has active tab stops.
        /// </summary>
        public bool HasActiveTabStops
        {
            get { return null != CurrentSnippet; }
        }

        private CodeSnippet CurrentSnippet
        {
            get
            {
                CodeSnippet snippet;
                if (View.WpfView.Properties.TryGetProperty(CodeSnippet.ViewPropertyName, out snippet))
                    return snippet;
                else
                    return null;
            }
        }

        /// <summary>
        /// Gets tab stops from the associated view that should be highlighted.
        /// </summary>
        public Span[] GetTabStopsToHighlight()
        {
            return CurrentSnippet?.GetTabStopsPositions();
        }

        public override int Exec(
            ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (VSConstants.VSStd2K == pguidCmdGroup)
            {
                if (InterceptNativeEvents(nCmdID))
                    return VSConstants.S_OK;
            }
            else if (PackageGuids.GuidEmmetPackageCmdSet == pguidCmdGroup)
            {
                // Actual Emmet commands handling goes here. All new commands should be added to the switch
                // statement below.
                switch (nCmdID)
                {
                    case PackageIds.CmdIDExpandAbbreviation:
                        TryExpandAbbreviation();
                        return VSConstants.S_OK;

                    case PackageIds.CmdIDWrapWithAbbreviation:
                        TryWrapAbbreviation();
                        return VSConstants.S_OK;

                    default:
                        // Other commands do not require post processing and can be invoked directly.
                        EmmetPackage.Instance.RunCommand(
                            new EmmetEditor(View.WpfView, View.TextView),
                            (int)nCmdID);
                        return VSConstants.S_OK;
                }
            }

            return base.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        private bool InterceptNativeEvents(uint nCmdID)
        {
            // TAB and Shift+TAB should cycle through all tab stops until ESC or Enter are pressed.
            switch (nCmdID)
            {
                case (uint)VSConstants.VSStd2KCmdID.TAB:
                case (uint)VSConstants.VSStd2KCmdID.BACKTAB:
                    uint backTabCode = (uint)VSConstants.VSStd2KCmdID.BACKTAB;

                    if (HasActiveTabStops)
                    {
                        if (CurrentSnippet.TryMoveToNextTabStop(nCmdID == backTabCode))
                            return true;

                        // User has moved caret away from tab stops, clear them.
                        CurrentSnippet.EndEditSnippet();
                        HighlightTabStops();

                        return false;
                    }

                    if (nCmdID != backTabCode && _expandAbbreviationOnTab && TryExpandAbbreviation())
                        return true;

                    break;

                case (uint)VSConstants.VSStd2KCmdID.CANCEL:
                    if (HasActiveTabStops && !_completionBroker.IsCompletionActive(View.WpfView))
                    {
                        CurrentSnippet.EndEditSnippet();
                        HighlightTabStops();

                        if (!View.WpfView.Selection.IsEmpty)
                            View.WpfView.Selection.Clear();

                        return true;
                    }

                    break;
            }

            return false;
        }

        private bool TryExpandAbbreviation()
        {
            // Ensure that the caret is at the end of a non-empty line.
            SnapshotPoint position = View.WpfView.Caret.Position.BufferPosition;
            ITextSnapshotLine line = View.WpfView.Caret.Position.BufferPosition.GetContainingLine();
            string txt = line.GetText();

            if (string.IsNullOrWhiteSpace(txt) || position.Position != line.End)
                return false;

            // remove all white spaces from current line text
            var normalize = string.Join(string.Empty, Regex.Split(txt, @"\s+").Where(c => c != string.Empty));

            // is typed is not HTML and is not Emmet, so it's (probably) code snippet
            // so return false and leave the Tab handle to visual studio
            if (!IsHtmlTag(normalize) && !IsEmmet(normalize))
                return false;

            _completionBroker.DismissAllSessions(View.WpfView);
            var editor = new EmmetEditor(View.WpfView, View.TextView);
            bool retVal = EmmetPackage.Instance.RunCommand(editor, PackageIds.CmdIDExpandAbbreviation);

            if (HasActiveTabStops)
            {
                CurrentSnippet.BeginEditSnippet();
                HighlightTabStops();
            }

            return retVal;
        }

        private bool IsEmmet(string s) => _emmetSpecChars.Any(c => s.Contains(c));

        private bool IsHtmlTag(string s) => _htmlTags.Any(c => c == s);

        private bool TryWrapAbbreviation()
        {
            // Ensure that we have selection to wrap with abbreviation.
            if (View.WpfView.Selection.IsEmpty)
                return false;

            var editor = new EmmetEditor(View.WpfView, View.TextView);
            bool retVal = EmmetPackage.Instance.RunCommand(editor, PackageIds.CmdIDWrapWithAbbreviation);

            if (HasActiveTabStops)
            {
                CurrentSnippet.BeginEditSnippet();
                HighlightTabStops();
            }

            return retVal;
        }

        private void HighlightTabStops() => TabStopsChanged?.Invoke(this, EventArgs.Empty);
    }
}