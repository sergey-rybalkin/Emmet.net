using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using FluentAssertions;
using Emmet.Engine;
using V8.Net;

namespace Emmet.Tests.Engine
{
    /// <summary>
    /// Unit tests for EngineWrapper class.
    /// </summary>
    [TestClass]
    [DeploymentItem(@"..\..\..\Emmet\x86")]
    [DeploymentItem(@"..\..\..\Emmet\emmet-min.js")]
    public class EngineWrapperTests
    {
        private static EngineWrapper _engine;

        /// <summary>
        /// Gets or sets the test context which provides information about and functionality for the current
        /// test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            _engine = new EngineWrapper(null);
        }

        [TestMethod]
        public void RunCommand_ExpandsHtmlAbbreviation()
        {
            // Arrange
            string result = string.Empty;
            IEmmetEditor editor = Substitute.For<IEmmetEditor>();
            editor.GetContentTypeInActiveBuffer().Returns("htmlx");
            editor.GetContent().Returns("div");
            editor.GetSelectionRange().Returns(new Range(3, 3));
            editor.GetCurrentLineRange().Returns(new Range(0, 3));
            editor.GetCaretPosition().Returns(3);
            editor.ReplaceContentRange(Arg.Do<string>(s => result = s), Arg.Any<int>(), Arg.Any<int>());

            // Act
            _engine.RunCommand(PackageIds.CmdIDExpandAbbreviation, editor).Should().BeTrue();
            result.Should().Be("<div></div>");
        }

        [TestMethod]
        public void RunCommand_ExpandsCssAbbreviation()
        {
            // Arrange
            string result = string.Empty;
            IEmmetEditor editor = Substitute.For<IEmmetEditor>();
            editor.GetContentTypeInActiveBuffer().Returns("css");
            editor.GetContent().Returns("p10");
            editor.GetSelectionRange().Returns(new Range(3, 3));
            editor.GetCurrentLineRange().Returns(new Range(0, 3));
            editor.GetCaretPosition().Returns(3);
            editor.ReplaceContentRange(Arg.Do<string>(s => result = s), Arg.Any<int>(), Arg.Any<int>());

            // Act
            _engine.RunCommand(PackageIds.CmdIDExpandAbbreviation, editor).Should().BeTrue();
            result.Should().Be("padding: 10px;");
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            _engine.Dispose();
            _engine = null;
        }
    }
}