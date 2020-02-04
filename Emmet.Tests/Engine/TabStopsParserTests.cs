using Emmet.Engine;
using Emmet.Tests.Helpers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Emmet.Tests.Engine
{

    [TestClass]
    public class TabStopsParserTests : EngineTestsBase
    {
        [TestMethod]
        public void ParseContent_GivenContentWithTabStops_RemovesThem()
        {
            // Arrange
            string content = GetSourceFromResource(DataHelper.TabStops);

            // Act
            TabStopsParser parser = TabStopsParser.ParseContent(content);

            // Assert
            string gold = GetSourceFromResource(DataHelper.TabStopsGold);
            string result = NormalizeWhiteSpace(parser.Content);
            gold.Should().BeEquivalentTo(result);
        }

        [TestMethod]
        public void ParseContent_GivenContentWithTabStops_CalculatesTabStopsPositions()
        {
            // Arrange
            string content = "<div>{placeholder}</div><p>{}</p>";

            // Act
            TabStopsParser parser = TabStopsParser.ParseContent(content);

            // Assert
            parser.TabStops.Length.Should().Be(2);
            parser.TabStops[0].Start.Should().Be(5);
            parser.TabStops[0].Length.Should().Be(11);
            parser.TabStops[1].Start.Should().Be(25);
            parser.TabStops[1].Length.Should().Be(0);
        }
    }
}
