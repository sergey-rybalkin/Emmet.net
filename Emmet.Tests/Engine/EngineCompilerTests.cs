using Emmet.Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.ClearScript.V8;
using FluentAssertions;
using System.IO;
using System.Reflection;

namespace Emmet.Tests.Engine
{
    /// <summary>
    /// Unit tests for EngineCompiler class.
    /// </summary>
    [TestClass]
    public class EngineCompilerTests
    {
        [TestMethod]
        public void Compile_CompilesEmmetEngineScript()
        {
            // Arrange
            var engine = new V8ScriptEngine();
            var compiler = new EngineCompiler();

            // Act
            compiler.CompileCore(engine);

            // Assert
            object obj = engine.Evaluate("replaceAbbreviation('p>a', 3, 'markup')");
            obj.Should().BeOfType<string>();
            engine.Dispose();
        }

        [TestMethod]
        public void LoadExtensions_PointedToPreferencesFile_LoadsCustomSnippets()
        {
            // Arrange
            var engine = new V8ScriptEngine();
            var compiler = new EngineCompiler();
            string baseDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Act
            compiler.CompileCore(engine);
            compiler.LoadExtensions(Path.Combine(baseDirectory, "Resources"), engine);

            // Assert
            string script = "replaceAbbreviation('cst', '3', 'markup')";
            object obj = engine.Evaluate(script);
            obj.Should().BeOfType<string>();

            engine.Dispose();
        }
    }
}