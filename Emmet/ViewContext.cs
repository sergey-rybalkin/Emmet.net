using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Emmet
{
    /// <summary>
    /// Contains context and provides helper methods for code editor view.
    /// </summary>
    public struct ViewContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewContext"/> class.
        /// </summary>
        /// <param name="wpfView">The WPF interface for the editor.</param>
        /// <param name="textView">The text view interface for the editor.</param>
        public ViewContext(IWpfTextView wpfView, IVsTextView textView)
        {
            WpfView = wpfView;
            TextView = textView;
        }

        /// <summary>
        /// Gets or sets the WPF view.
        /// </summary>
        public IWpfTextView WpfView { get; private set; }

        /// <summary>
        /// Gets or sets the text view.
        /// </summary>
        public IVsTextView TextView { get; private set; }

        /// <summary>
        /// Gets the current text buffer.
        /// </summary>
        public ITextBuffer CurrentBuffer
        {
            get { return WpfView.TextBuffer; }
        }
    }
}