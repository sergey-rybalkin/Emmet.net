using Emmet.Tests.Helpers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Emmet.Tests.Engine
{
    /// <summary>
    /// Unit tests for HTML abbreviations and actions engine in JSX syntax.
    /// </summary>
    [TestClass]
    [DeploymentItem(@"..\..\..\Emmet\lib")]
    [DeploymentItem(@"..\..\..\Emmet\emmet-min.js")]
    public class JsxTests : EngineTestsBase
    {
        [TestMethod]
        public void ExpandAbbreviation_GivenValidAbbreviation_AppliesJsxFilter()
        {
            // Arrange
            string template = GetSourceFromResource(DataHelper.AbbreviationInJsx);
            var editor = EditorStub.BuildFromTemplate(template, "typescript");

            // Act
            bool retVal = _engine.RunCommand(PackageIds.CmdIDExpandAbbreviation, editor);

            // Assert
            retVal.Should().BeTrue();
            string gold = GetSourceFromResource(DataHelper.AbbreviationInJsxGold);
            string result = NormalizeWhiteSpace(editor.Content);
            gold.Should().BeEquivalentTo(result);
        }
    }
}