using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Emmet.EditorExtensions
{
    /// <summary>
    /// View creation listener that is responsible for injecting command filters.
    /// </summary>
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType("CSS")]
    [ContentType("LESS")]
    [ContentType("SCSS")]
    [ContentType("HTML")]
    [ContentType("HTMLX")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    public class ViewCreationListener : IVsTextViewCreationListener
    {
        /// <summary>
        /// Gets or sets the editor adapters factory service, injected through MEF.
        /// </summary>
        [Import]
        public IVsEditorAdaptersFactoryService EditorAdaptersFactoryService { get; set; }

        /// <summary>
        /// Gets or sets the completion broker, injected through MEF.
        /// </summary>
        [Import]
        public ICompletionBroker CompletionBroker { get; set; }

        /// <summary>
        /// Called when a IVsTextView adapter has been created and initialized.
        /// </summary>
        /// <param name="textViewAdapter">The newly created and initialized text view adapter.</param>
        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            IWpfTextView textView = EditorAdaptersFactoryService.GetWpfTextView(textViewAdapter);

            ViewContext context = new ViewContext(textView, textViewAdapter);
            textView.Properties.GetOrCreateSingletonProperty(
                "EmmetCommandTarget",
                () => new EmmetCommandTarget(context, CompletionBroker));
        }
    }
}