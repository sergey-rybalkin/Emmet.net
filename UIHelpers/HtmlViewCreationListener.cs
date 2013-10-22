using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace UIHelpers
{
    /// <summary>
    /// Listener responsible for injection of our command filters into every newly created HTML editor window.
    /// </summary>
    [TextViewRole(PredefinedTextViewRoles.Document),
     ContentType("HTML"),
     ContentType("HTMLX"),
     Export(typeof(IVsTextViewCreationListener))]
    public class HtmlViewCreationListener : ViewCreationListenerBase, IVsTextViewCreationListener
    {
        [Import]
        internal override ICompletionBroker CompletionBroker { get; set; }

        [Import]
        internal override IVsEditorAdaptersFactoryService EditorAdaptersFactoryService { get; set; }

        /// <summary>
        /// Gets the syntax of the created document.
        /// </summary>
        protected internal override EmmetSyntax Syntax
        {
            get { return EmmetSyntax.Html; }
        }
    }
}