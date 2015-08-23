using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace Emmet.EditorExtensions
{
    /// <summary>
    /// Tagger provider for tab stops text marker tags.
    /// </summary>
    [Export(typeof(IViewTaggerProvider))]
    [ContentType("css")]
    [ContentType("scss")]
    [ContentType("less")]
    [ContentType("html")]
    [ContentType(@"htmlx")]
    [ContentType("RazorCSharp")]
    [TagType(typeof(TextMarkerTag))]
    public class TabStopsTaggerProvider : IViewTaggerProvider
    {
        /// <summary>
        /// Creates a new tab stops tagger instance.
        /// </summary>
        /// <param name="textView">View to create tagger for.</param>
        /// <param name="buffer">Text buffer to create tagger for.</param>
        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            // Make sure that each view has only one tagger associated with it. If view has several buffers
            // (e.g. html file with css code) only the top level buffer will have a tagger associated with it.
            IProjectionBuffer projectionBuffer = textView.TextBuffer as IProjectionBuffer;

            if (null == projectionBuffer || buffer == projectionBuffer.SourceBuffers[0])
                return new TabStopsTagger(textView, buffer) as ITagger<T>;
            else
                return null;
        }
    }
}