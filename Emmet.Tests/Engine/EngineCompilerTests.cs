using Emmet.Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using V8.Net;
using FluentAssertions;

namespace Emmet.Tests.Engine
{
    /// <summary>
    /// Unit tests for EngineCompiler class.
    /// </summary>
    [TestClass]
    [DeploymentItem(@"..\..\..\Emmet\x86")]
    [DeploymentItem(@"..\..\..\Emmet\emmet-min.js")]
    public class EngineCompilerTests
    {
        /// <summary>
        /// Gets or sets the test context which provides information about and functionality for the current
        /// test run.
        /// </summary>
        public TestContext TestContext { get; set; }


        [TestMethod]
        public void Compile_CompilesEmmetEngineScript()
        {
            // Arrange
            V8Engine engine = new V8Engine();
            EngineCompiler compiler = new EngineCompiler(engine);

            // Act
            compiler.CompileCore();

            // Assert
            ObjectHandle emmet = engine.DynamicGlobalObject.window.emmet;
            emmet.Should().NotBeNull();
        }

        [TestMethod]
        [DeploymentItem(@"..\..\Resources\preferences.json")]
        public void LoadExtensions_PointedToPreferencesFile_LoadsCustomSnippets()
        {
            // Arrange
            V8Engine engine = new V8Engine();
            EngineCompiler compiler = new EngineCompiler(engine);

            // Act
            compiler.CompileCore();
            compiler.LoadExtensions(TestContext.TestDeploymentDir);

            // Assert
            string script = "window.emmet.resources.fuzzyFindSnippet('css', 'section')";
            Handle result = engine.Execute(script);

            // preferences.json file contains "section" abbreviation for css. If we are able to find it then
            // our extension has been loaded correctly. 
            result.IsObject.Should().BeTrue();
        }
    }
}