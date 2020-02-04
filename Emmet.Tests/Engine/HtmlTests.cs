using Emmet.Tests.Helpers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Emmet.Tests.Engine
{
    /// <summary>
    /// Unit tests for HTML abbreviations and actions engine.
    /// </summary>
    [TestClass]
    [DeploymentItem(@"..\..\..\Emmet\lib")]
    [DeploymentItem(@"..\..\..\Emmet\emmet.js")]
    public class HtmlTests : EngineTestsBase
    {
        [TestMethod]
        public void ExpandAbbreviation_GivenValidAbbreviation_ExpandsIt()
        {
            // Arrange
            string template = GetSourceFromResource(DataHelper.Abbreviation);
            var editor = EditorStub.BuildFromTemplate(template, "htmlx");

            // Act
            bool retVal = _engine.RunCommand(PackageIds.CmdIDExpandAbbreviation, editor);

            // Assert
            retVal.Should().BeTrue();
            string gold = GetSourceFromResource(DataHelper.AbbreviationGold);
            string result = NormalizeWhiteSpace(editor.Content);
            gold.Should().BeEquivalentTo(result);
        }        

        [TestMethod]
        public void WrapWithAbbreviation_GivenValidAbbreviation_WrapsSelection()
        {
            // Arrange
            string template = GetSourceFromResource(DataHelper.WrapWithAbbreviation);
            var editor = EditorStub.BuildFromTemplate(template, "htmlx");
            editor.UserInput = "div";

            // Act
            bool retVal = _engine.RunCommand(PackageIds.CmdIDWrapWithAbbreviation, editor);

            // Assert
            retVal.Should().BeTrue();
            string gold = GetSourceFromResource(DataHelper.WrapWithAbbreviationGold);
            string result = NormalizeWhiteSpace(editor.Content);
            gold.Should().BeEquivalentTo(result);
        }
    }
}