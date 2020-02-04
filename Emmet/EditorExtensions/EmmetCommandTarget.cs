using System;
using Emmet.Engine;
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
        private readonly ICompletionBroker _completionBroker;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmmetCommandTarget"/> class.
        /// </summary>
        /// <param name="view">Context of the view to operate on.</param>
        /// <param name="completionBroker">The completion broker to control intellisense UI.</param>
        public EmmetCommandTarget(ViewContext view, ICompletionBroker completionBroker)
            : base(view)
        {
            _completionBroker = completionBroker;
            ExpandAbbreviationOnTab = EmmetPackage.Options.InterceptTabs;
        }

        /// <summary>
        /// Event queue for all listeners interested in TabStopsRemoved events which is triggered when tab
        /// stops collection changes.
        /// </summary>
        public event EventHandler TabStopsChanged;

        /// <summary>
        /// Gets or sets a value indicating whether to run expand abbreviation command on TAB.
        /// </summary>
        public bool ExpandAbbreviationOnTab { get; set; }

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

        /// <summary>
        /// Executes the specified command.
        /// </summary>
        /// <param name="pguidCmdGroup">[in,out] The GUID of the command group.</param>
        /// <param name="nCmdID">The command ID.</param>
        /// <param name="nCmdexecopt">
        /// Specifies how the object should execute the command. Possible values are taken from the
        /// <see cref="T:Microsoft.VisualStudio.OLE.Interop.OLECMDEXECOPT" /> and
        /// <see cref="T:Microsoft.VisualStudio.OLE.Interop.OLECMDID_WINDOWSTATE_FLAG" /> enumerations.
        /// </param>
        /// <param name="pvaIn">The input arguments of the command.</param>
        /// <param name="pvaOut">The output arguments of the command.</param>
        public override int Exec(
            ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            // Since VS extensions became async make sure that initialization has finished.
            if (VSConstants.VSStd2K == pguidCmdGroup && EmmetPackage.Instance != null)
            {
                if (InterceptNativeEvents(nCmdID))
                    return VSConstants.S_OK;
            }
            else if (PackageGuids.GuidEmmetPackageCmdSet == pguidCmdGroup && EmmetPackage.Instance != null)
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
                    case PackageIds.CmdIDSortCssProperties:
                        break;
                    default:
                        // Other commands do not require post processing and can be invoked directly.
                        return VSConstants.S_OK;
                }
            }

            return base.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        private bool InterceptNativeEvents(uint cmdID)
        {
            // TAB and Shift+TAB should cycle through all tab stops until ESC or Enter are pressed.
            switch (cmdID)
            {
                case (uint)VSConstants.VSStd2KCmdID.TAB:
                case (uint)VSConstants.VSStd2KCmdID.BACKTAB:
                    uint backTabCode = (uint)VSConstants.VSStd2KCmdID.BACKTAB;

                    if (HasActiveTabStops)
                    {
                        if (CurrentSnippet.TryMoveToNextTabStop(cmdID == backTabCode))
                            return true;

                        // User has moved caret away from tab stops, clear them.
                        CurrentSnippet.EndEditSnippet();
                        HighlightTabStops();

                        return false;
                    }

                    if (cmdID != backTabCode && ExpandAbbreviationOnTab && TryExpandAbbreviation())
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

            _completionBroker.DismissAllSessions(View.WpfView);
            bool retVal = EmmetPackage.Instance.RunCommand(View, PackageIds.CmdIDExpandAbbreviation);

            if (HasActiveTabStops)
            {
                CurrentSnippet.BeginEditSnippet();
                HighlightTabStops();
            }

            return retVal;
        }

        private bool TryWrapAbbreviation()
        {
            // Ensure that we have selection to wrap with abbreviation.
            if (View.WpfView.Selection.IsEmpty)
                return false;

            bool retVal = EmmetPackage.Instance.RunCommand(View, PackageIds.CmdIDWrapWithAbbreviation);

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