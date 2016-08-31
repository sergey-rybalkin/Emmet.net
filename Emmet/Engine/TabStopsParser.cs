using Emmet.Diagnostics;
using V8.Net;

namespace Emmet.Engine
{
    /// <summary>
    /// Wrapper over tab stops utility in the Emmet engine. Can be used to prepare expanded abbreviations to
    /// be inserted into the editor and get information about tab stops.
    /// </summary>
    public class TabStopsParser
    {
        /// <summary>
        /// Constructor that prevents a default instance of this class from being created.
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
        /// <param name="engine">V8 instance with Emmet engine compiled in it.</param>
        /// <param name="content">Expanded abbreviation content.</param>
        /// <exception cref="Exception{EmmetEngineExceptionArgs}">
        /// Indicates that Emmet engine has failed to parse the specified content.
        /// </exception>
        public static TabStopsParser ParseContent(V8Engine engine, string content)
        {
            ObjectHandle tabStopsUtil = engine.DynamicGlobalObject.window.emmet.tabStops;
            Handle extractResult = tabStopsUtil.Call("extract", null, engine.CreateValue(content));

            if (extractResult.IsError)
            {
                var ex = new EmmetEngineExceptionArgs(
                    "Error while trying to extract tab stops.",
                    extractResult);
                throw new Exception<EmmetEngineExceptionArgs>(ex);
            }

            TabStopsParser retVal = new TabStopsParser();
            ObjectHandle tabStopsObj = (ObjectHandle)extractResult;
            retVal.Content = tabStopsObj.GetProperty(@"text").AsString;
            ObjectHandle tabStopsList = tabStopsObj.GetProperty(@"tabstops");

            // Tab stops should be added before modifying document so that editor can track their position.
            int tabStopsCount = tabStopsList.ArrayLength;

            if (tabStopsCount > 0)
            {
                retVal.TabStops = new Range[tabStopsCount];
                retVal.TabStopGroups = new int[tabStopsCount];

                for (int i = 0; i < tabStopsCount; i++)
                {
                    ObjectHandle tabStopObj = tabStopsList.GetProperty(i.ToString());
                    int start = tabStopObj.GetProperty("start").AsInt32;
                    int end = tabStopObj.GetProperty("end").AsInt32;
                    int group = tabStopObj.GetProperty("group").AsInt32;

                    retVal.TabStops[i] = new Range(start, end);
                    retVal.TabStopGroups[i] = group;
                }
            }

            return retVal;
        }
    }
}