using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Text;

namespace Emmet.Engine
{
    /// <summary>
    /// Wrapper over tab stops utility in the Emmet engine. Can be used to prepare expanded abbreviations to
    /// be inserted into the editor and get information about tab stops.
    /// </summary>
    public class TabStopsParser
    {
        private static readonly Regex Parser = new Regex("{(.*?)}", RegexOptions.Compiled);

        /// <summary>
        /// Initializes a new instance of the <see cref="TabStopsParser"/> class. Prevents a default
        /// instance of this class from being created.
        /// </summary>
        private TabStopsParser()
        {
        }

        /// <summary>
        /// Gets the content without tab stops markers.
        /// </summary>
        public string Content { get; private set; }

        /// <summary>
        /// Gets tab stops locations in the content.
        /// </summary>
        public Span[] TabStops { get; private set; }

        /// <summary>
        /// Looks for tab stops in the specified content and returns a processed version with expanded
        /// placeholders and tab stops found.
        /// </summary>
        /// <param name="content">Expanded abbreviation content.</param>
        /// <param name="offset">(Optional) The offset of the content in containing document.</param>
        public static TabStopsParser ParseContent(string content, int offset = 0)
        {
            var tabStops = new List<Span>(5);
            int tabStopOffset = 0;

            var retVal = new TabStopsParser();
            retVal.Content = Parser.Replace(
                content,
                (Match m) =>
                {
                    string replacement = m.Groups[1]?.Value ?? string.Empty;
                    tabStops.Add(new Span(offset + m.Index - tabStopOffset, replacement.Length));
                    tabStopOffset += m.Length - replacement.Length;

                    return replacement;
                });
            retVal.TabStops = tabStops.ToArray();

            return retVal;
        }
    }
}