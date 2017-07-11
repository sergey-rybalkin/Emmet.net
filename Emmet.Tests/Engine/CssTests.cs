using Emmet.Tests.Helpers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Emmet.Tests.Engine
{
    /// <summary>
    /// Unit tests for CSS abbreviations engine.
    /// </summary>
    [TestClass]
    [DeploymentItem(@"..\..\..\Emmet\lib")]
    [DeploymentItem(@"..\..\..\Emmet\emmet-min.js")]
    [DeploymentItem(@"..\..\Resources\", @"Resources\")]
    public class CssTests : EngineTestsBase
    {
        public CssTests() : base(@"Resources")
        {
        }

        [TestMethod]
        public void ExpandAbbreviation_GivenSimpleCssAbbreviation_ExpandsIt()
        {
            // Arrange
            string abbreviation = "p10";
            EditorStub editor = EditorStub.BuildFromTemplate(abbreviation, "css");

            // Act
            bool retVal = _engine.RunCommand(PackageIds.CmdIDExpandAbbreviation, editor);

            // Assert
            retVal.Should().BeTrue();
            "padding: 10px;".Should().BeEquivalentTo(editor.Content);
        }

        [TestMethod]
        public void ExpandAbbreviation_GivenCompositeCssAbbreviation_ExpandsIt()
        {
            // Arrange
            string abbreviation = "p10+m10";
            EditorStub editor = EditorStub.BuildFromTemplate(abbreviation, "css");

            // Act
            bool retVal = _engine.RunCommand(PackageIds.CmdIDExpandAbbreviation, editor);

            // Assert
            retVal.Should().BeTrue();
            "padding: 10px;\nmargin: 10px;".Should().BeEquivalentTo(editor.Content);
        }

        [TestMethod]
        public void ExpandAbbreviation_WithLoadedPreferences_ExpandsCustomSnippets()
        {
            // Arrange
            string abbreviation = "cst";
            EditorStub editor = EditorStub.BuildFromTemplate(abbreviation, "css");

            // Act
            bool retVal = _engine.RunCommand(PackageIds.CmdIDExpandAbbreviation, editor);

            // Assert
            retVal.Should().BeTrue();
            "custom".Should().BeEquivalentTo(editor.Content);
        }        
    }
}