using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace UIHelpers
{
    /// <summary>
    /// Base class for view creation listeners that injects our filter command into the newly created view.
    /// </summary>
    public abstract class ViewCreationListenerBase
    {
        [Import]
        internal abstract IVsEditorAdaptersFactoryService EditorAdaptersFactoryService { get; set; }

        /// <summary>
        /// Gets the syntax of the created document that emmet engine should use to perform its actions.
        /// </summary>
        protected internal abstract EmmetSyntax Syntax { get; }

        /// <summary>
        /// Called when a <see cref="T:Microsoft.VisualStudio.TextManager.Interop.IVsTextView" /> adapter has
        /// been created and initialized.
        /// </summary>
        /// <param name="textViewAdapter">The newly created and initialized text view adapter.</param>
        public virtual void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            IWpfTextView textView = EditorAdaptersFactoryService.GetWpfTextView(textViewAdapter);
            textView.Properties.GetOrCreateSingletonProperty(
                () => new ViewCommandFilter(textView, textViewAdapter, Syntax));
        }
    }
}