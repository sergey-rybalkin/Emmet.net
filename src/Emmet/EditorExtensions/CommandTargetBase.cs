using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;

namespace Emmet.EditorExtensions
{
    /// <summary>
    /// Base class for Emmet command targets.
    /// </summary>
    public abstract class CommandTargetBase : IOleCommandTarget
    {
        private bool _reloadedWithHighPriority = false;

        private IOleCommandTarget _nextTarget;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandTargetBase"/> class.
        /// </summary>
        /// <param name="view">Context of the view to operate on.</param>
        public CommandTargetBase(ViewContext view)
        {
            View = view;
            View.TextView.AddCommandFilter(this, out _nextTarget);
        }

        protected ViewContext View { get; private set; }

        protected IOleCommandTarget NextTarget
        {
            get { return _nextTarget; }
        }

        public virtual int Exec(
            ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            // Put this command target instance at the end of the chain in order to be able to handle TAB key
            // before the intellisense system.
            if (!_reloadedWithHighPriority && (uint)VSConstants.VSStd2KCmdID.TYPECHAR == nCmdID)
            {
                int retVal = _nextTarget.Exec(pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

                _reloadedWithHighPriority = true;
                View.TextView.RemoveCommandFilter(this);
                View.TextView.AddCommandFilter(this, out _nextTarget);

                return retVal;
            }

            return NextTarget.Exec(pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup != PackageGuids.GuidEmmetPackageCmdSet)
                return NextTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);

            for (uint i = 0; i < cCmds; i++)
                prgCmds[i].cmdf = (uint)GetCommandStatus(prgCmds[i].cmdID);

            return VSConstants.S_OK;
        }

        protected virtual OLECMDF GetCommandStatus(uint commandId)
        {
            return OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED;
        }
    }
}