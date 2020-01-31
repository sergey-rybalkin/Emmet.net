using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Emmet
{
    /// <summary>
    /// Contains context and provides helper methods for code editor view.
    /// </summary>
    public struct ViewContext : ICodeEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewContext"/> struct.
        /// </summary>
        /// <param name="wpfView">The WPF interface for the editor.</param>
        /// <param name="textView">The text view interface for the editor.</param>
        public ViewContext(IWpfTextView wpfView, IVsTextView textView)
        {
            WpfView = wpfView;
            TextView = textView;
        }

        /// <summary>
        /// Gets the WPF view.
        /// </summary>
        public IWpfTextView WpfView { get; private set; }

        /// <summary>
        /// Gets the text view.
        /// </summary>
        public IVsTextView TextView { get; private set; }

        /// <summary>
        /// Gets the current text buffer.
        /// </summary>
        public ITextBuffer CurrentBuffer
        {
            get { return WpfView.TextBuffer; }
        }

        /// <summary>
        /// Returns content of the line under caret position.
        /// </summary>
        public string GetCurrentLine() => WpfView.Caret.Position.BufferPosition.GetContainingLine().GetText();
    }
}