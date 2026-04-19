using Emmet.Engine;
using Emmet.Tests.Helpers;
using Microsoft.ClearScript.V8;
using Shouldly;

namespace Emmet.Tests;

public class EngineTests
{
    [Test]
    public void Engine_is_able_to_compile_emmet()
    {
        // Arrange
        using V8ScriptEngine engine = new();
        EngineCompiler compiler = new();

        // Act
        compiler.CompileCore(engine);

        // Assert
        engine.Evaluate("replaceAbbreviation instanceof Function").ShouldBe(true);
    }

    [Test]
    public void Engine_loads_preferences()
    {
        // Arrange
        using EngineWrapper engine =
            new(Path.Combine(AppContext.BaseDirectory, @"Resources\preferences.json"));
        EditorStub editor = EditorStub.BuildFromTemplate("section", "css");

        // Act
        bool retVal = engine.RunCommand(PackageIds.CmdIDExpandAbbreviation, editor);

        // Assert
        retVal.ShouldBe(true);
        editor.Content.ShouldStartWith("/");
    }

    [Test]
    [Arguments("div", "<div>{}</div>", "markup")]
    [Arguments("p10", "padding: 10px;", "css")]
    public void Engine_is_able_to_expand_abbreviation(string abbreviation, string result, string syntax)
    {
        // Arrange
        using EngineWrapper engine = new(null);
        EditorStub editor = EditorStub.BuildFromTemplate(abbreviation, syntax);

        // Act
        bool retVal = engine.RunCommand(PackageIds.CmdIDExpandAbbreviation, editor);

        // Assert
        retVal.ShouldBe(true);
        editor.Content.ShouldBe(result);
    }
}
