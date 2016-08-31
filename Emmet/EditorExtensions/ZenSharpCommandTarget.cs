using System;
using Emmet.Mnemonics;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text;

namespace Emmet.EditorExtensions
{
    /// <summary>
    /// Handles ZenSharp commands passed to the attached view.
    /// </summary>
    public class ZenSharpCommandTarget : CommandTargetBase
    {
        private readonly ICompletionBroker _completionBroker;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZenSharpCommandTarget"/> class.
        /// </summary>
        /// <param name="view">Context of the view to operate on.</param>
        /// <param name="completionBroker">The completion broker to control intellisense UI.</param>
        public ZenSharpCommandTarget(ViewContext view, ICompletionBroker completionBroker)
            : base(view)
        {
            _completionBroker = completionBroker;
        }

        public override int Exec(
            ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (PackageGuids.GuidEmmetPackageCmdSet != pguidCmdGroup ||
                PackageIds.CmdIDExpandMnemonic != nCmdID)
            {
                return base.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            }

            // Get mnemonic content from editor.
            SnapshotPoint caretPosition = View.WpfView.Caret.Position.BufferPosition;
            ITextSnapshotLine line = View.WpfView.Caret.Position.BufferPosition.GetContainingLine();
            string lineText = line.GetText();
            if (caretPosition.Position != line.End || lineText.Length < 3)
                return VSConstants.S_OK;

            string mnemonic = lineText.TrimStart();
            string indent = new string(' ', lineText.Length - mnemonic.Length);
            string snippet = string.Empty;
            int caretOffset;
            if (!MnemonicParser.TryParse(mnemonic, indent, out snippet, out caretOffset))
                return VSConstants.S_OK;

            // Insert generated snippet into the current editor window
            int startPosition = line.End.Position - mnemonic.Length;
            Span targetPosition = new Span(startPosition, mnemonic.Length);
            View.CurrentBuffer.Replace(targetPosition, snippet);

            // Close all intellisense windows
            _completionBroker.DismissAllSessions(View.WpfView);

            // Move caret to the position where user can start typing new member name
            caretPosition = new SnapshotPoint(
                View.CurrentBuffer.CurrentSnapshot,
                caretPosition.Position + caretOffset);
            View.WpfView.Caret.MoveTo(caretPosition);

            return VSConstants.S_OK;
        }

        protected override OLECMDF GetCommandStatus(uint commandId)
        {
            if (PackageIds.CmdIDExpandMnemonic == commandId &&
                "CSharp" == View.WpfView.TextBuffer.ContentType.TypeName)
            {
                return OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED;
            }
            else
            {
                return OLECMDF.OLECMDF_INVISIBLE;
            }
        }
    }
}