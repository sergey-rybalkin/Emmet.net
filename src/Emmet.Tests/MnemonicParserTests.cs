using Emmet.Mnemonics;
using Shouldly;

namespace Emmet.Tests;

public class MnemonicParserTests
{
    private static readonly Dictionary<string, string> _accessibilityLevels =
        new()
        {
        { "_", "private" },
        { "p", "public" },
        { "P", "protected" },
        { "i", "internal" },
        { "pi", "protected internal" }
    };

    private static readonly Dictionary<string, string> _modifiers =
        new()
        {
        { "c", "const" },
        { "s", "static" },
        { "v", "virtual" },
        { "a", "abstract" },
        { "r", "readonly" }
    };

    private static readonly Dictionary<string, string> _types =
        new()
        {
        { "s", "string" },
        { "sh", "short" },
        { "by", "byte" },
        { "b", "bool" },
        { "dt", "DateTime" },
        { "d", "double" },
        { "i", "int" },
        { "u", "uint" },
        { "g", "Guid" },
        { "de", "decimal" },
        { "v", "void" }
    };

    [Test]
    public void TryParseGeneratesField()
    {
        string result;
        int caretPosition;

        foreach (string accessibilityLevel in _accessibilityLevels.Keys)
        {
            foreach (string modifier in _modifiers.Keys)
            {
                foreach (string type in _types.Keys)
                {
                    string visibility = _accessibilityLevels[accessibilityLevel];
                    string mod = _modifiers[modifier];
                    string returns = _types[type];
                    string mnemonic = accessibilityLevel + modifier + type + "f";

                    MnemonicParser.TryParse(mnemonic, "    ", out result, out caretPosition)
                                  .ShouldBeTrue($"Mnemonic {mnemonic} is valid.");

                    result.ShouldBe($"{visibility} {mod} {returns} _;");
                }
            }
        }
    }

    [Test]
    public void TryParseGeneratesProperty()
    {
        string result;
        int caretPosition;

        foreach (string accessibilityLevel in _accessibilityLevels.Keys)
        {
            foreach (string modifier in _modifiers.Keys)
            {
                foreach (string type in _types.Keys)
                {
                    string visibility = _accessibilityLevels[accessibilityLevel];
                    string mod = _modifiers[modifier];
                    string returns = _types[type];
                    string mnemonic = accessibilityLevel + modifier + type + "p";

                    MnemonicParser.TryParse(mnemonic, "    ", out result, out caretPosition)
                                  .ShouldBeTrue($"Mnemonic {mnemonic} is valid.");

                    result.ShouldBe($"{visibility} {mod} {returns}  {{ get; set; }}");
                }
            }
        }
    }

    [Test]
    public void TryParseGeneratesMethod()
    {
        string result;
        int caretPosition;

        foreach (string accessibilityLevel in _accessibilityLevels.Keys)
        {
            foreach (string modifier in _modifiers.Keys)
            {
                foreach (string type in _types.Keys)
                {
                    string visibility = _accessibilityLevels[accessibilityLevel];
                    string mod = _modifiers[modifier];
                    string returns = _types[type];
                    string mnemonic = accessibilityLevel + modifier + type + "m";

                    MnemonicParser.TryParse(mnemonic, "    ", out result, out caretPosition)
                                  .ShouldBeTrue($"Mnemonic {mnemonic} is valid.");

                    result.ShouldStartWith($"{visibility} {mod} {returns} ()");
                }
            }
        }
    }
}
