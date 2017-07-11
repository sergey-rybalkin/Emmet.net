using Emmet.Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using Emmet.Engine.ChakraInterop;
using System.IO;

namespace Emmet.Tests.Engine
{
    /// <summary>
    /// Unit tests for EngineCompiler class.
    /// </summary>
    [TestClass]
    [DeploymentItem(@"..\..\..\Emmet\lib")]
    [DeploymentItem(@"..\..\..\Emmet\emmet-min.js")]
    [DeploymentItem(@"..\..\Resources\", @"Resources\")]
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
            JavaScriptRuntime engine = JavaScriptRuntime.Create(JavaScriptRuntimeAttributes.None);
            JavaScriptContext ctx = engine.CreateContext();
            JavaScriptContext.Current = ctx;
            EngineCompiler compiler = new EngineCompiler(engine);

            // Act
            compiler.CompileCore(JavaScriptSourceContext.None);

            // Assert
            JavaScriptValue emmet = JavaScriptValue.GlobalObject.GetProperty("window").GetProperty("emmet");
            emmet.ValueType.Should().Be(JavaScriptValueType.Object);

            engine.Dispose();
        }

        [TestMethod]
        [DeploymentItem(@"..\..\Resources\preferences.json")]
        public void LoadExtensions_PointedToPreferencesFile_LoadsCustomSnippets()
        {
            // Arrange
            JavaScriptRuntime engine = JavaScriptRuntime.Create(JavaScriptRuntimeAttributes.None);
            JavaScriptContext ctx = engine.CreateContext();
            JavaScriptContext.Current = ctx;
            EngineCompiler compiler = new EngineCompiler(engine);

            // Act
            compiler.CompileCore(JavaScriptSourceContext.None);
            compiler.LoadExtensions(Path.Combine(TestContext.DeploymentDirectory, "Resources"));

            // Assert
            string script = "window.emmet.resources.fuzzyFindSnippet('css', 'cst')";
            JavaScriptValue result = JavaScriptContext.RunScript(script, JavaScriptSourceContext.None);

            // preferences.json file contains "cst" abbreviation for css. If we are able to find it then
            // our extension has been loaded correctly. 
            result.IsValid.Should().BeTrue();

            engine.Dispose();
        }
    }
}