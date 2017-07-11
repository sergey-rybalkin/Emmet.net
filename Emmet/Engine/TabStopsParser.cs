using Emmet.Engine.ChakraInterop;

namespace Emmet.Engine
{
    /// <summary>
    /// Wrapper over tab stops utility in the Emmet engine. Can be used to prepare expanded abbreviations to
    /// be inserted into the editor and get information about tab stops.
    /// </summary>
    public class TabStopsParser
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TabStopsParser"/> class. Prevents a default
        /// instance of this class from being created.
        /// </summary>
        private TabStopsParser()
        {
        }

        /// <summary>
        /// Gets or sets the content without tab stops markers.
        /// </summary>
        public string Content { get; private set; }

        /// <summary>
        /// Gets or sets tab stops locations in the content.
        /// </summary>
        public Range[] TabStops { get; private set; }

        /// <summary>
        /// Gets or sets the groups indexes array. It has the same length as the tab stops array and each
        /// element represents the number of the group that tab stop with the corresponding index belongs to.
        /// </summary>
        public int[] TabStopGroups { get; private set; }

        /// <summary>
        /// Looks for tab stops in the specified content and returns a processed version with expanded
        /// placeholders and tab stops found.
        /// </summary>
        /// <param name="content">Expanded abbreviation content.</param>
        public static TabStopsParser ParseContent(JavaScriptValue content)
        {
            JavaScriptValue tabStopsUtil =
                JavaScriptValue.GlobalObject
                               .GetProperty("window")
                               .GetProperty("emmet")
                               .GetProperty("tabStops");

            JavaScriptValue extractFunc = tabStopsUtil.GetProperty("extract");

            // 'this' should be the first argument
            JavaScriptValue extractResult = extractFunc.CallFunction(tabStopsUtil, content);

            TabStopsParser retVal = new TabStopsParser();
            retVal.Content = extractResult.GetProperty(@"text").ToString();
            JavaScriptValue tabStopsList = extractResult.GetProperty(@"tabstops");

            // Tab stops should be added before modifying document so that editor can track their position.
            int tabStopsCount = tabStopsList.GetProperty("length").ToInt32();

            if (tabStopsCount > 0)
            {
                retVal.TabStops = new Range[tabStopsCount];
                retVal.TabStopGroups = new int[tabStopsCount];

                for (int i = 0; i < tabStopsCount; i++)
                {
                    var tabStopObj = tabStopsList.GetIndexedProperty(JavaScriptValue.FromInt32(i));
                    int start = tabStopObj.GetProperty("start").ToInt32();
                    int end = tabStopObj.GetProperty("end").ToInt32();
                    string group = tabStopObj.GetProperty("group").ToString();

                    retVal.TabStops[i] = new Range(start, end);
                    retVal.TabStopGroups[i] = int.Parse(group);
                }
            }

            return retVal;
        }
    }
}