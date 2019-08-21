using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

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
    [ContentType("CSharp")]
    [ContentType("TypeScript")]
    [ContentType("JavaScript")]
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
            string contentType = textView.TextBuffer.ContentType.TypeName;

            if ("CSharp" != contentType)
            {
                EmmetCommandTarget target = textView.Properties.GetOrCreateSingletonProperty(
                    "EmmetCommandTarget",
                    () => new EmmetCommandTarget(context, CompletionBroker));

                // As of v2019 Visual Studio does not use projection buffer for JSX files and thus we cannot
                // detect JS and HTML buffers. So, in order to prevent unintended JS and Emmet snippets
                // collisions we support only hotkey based commands invocation.
                if (contentType.EndsWith("script", StringComparison.InvariantCultureIgnoreCase))
                    target.ExpandAbbreviationOnTab = false;
            }
            else
            {
                textView.Properties.GetOrCreateSingletonProperty(
                    "ZenSharpCommandTarget",
                    () => new ZenSharpCommandTarget(context, CompletionBroker));
            }
        }
    }
}