using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System.Collections.Generic;

namespace Emmet.Mnemonics.Tests
{
    [TestClass()]
    public class MnemonicParserTests
    {
        private static readonly Dictionary<string, string> _accessibilityLevels =
            new Dictionary<string, string>
        {
            { "_", "private" },
            { "p", "public" },
            { "P", "protected" },
            { "i", "internal" },
            { "pi", "protected internal" }
        };

        private static readonly Dictionary<string, string> _modifiers =
            new Dictionary<string, string>
        {
            { "c", "const" },
            { "s", "static" },
            { "v", "virtual" },
            { "a", "abstract" },
            { "r", "readonly" }
        };

        private static readonly Dictionary<string, string> _types =
            new Dictionary<string, string>
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

        [TestMethod()]
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
                                      .Should()
                                      .BeTrue($"Mnemonic {mnemonic} is valid.");

                        result.Should().Be($"{visibility} {mod} {returns} _;");
                    }
                }
            }
        }

        [TestMethod()]
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
                                      .Should()
                                      .BeTrue($"Mnemonic {mnemonic} is valid.");

                        result.Should().Be($"{visibility} {mod} {returns}  {{ get; set; }}");
                    }
                }
            }
        }

        [TestMethod()]
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
                                      .Should()
                                      .BeTrue($"Mnemonic {mnemonic} is valid.");

                        result.Should().StartWith($"{visibility} {mod} {returns} ()");
                    }
                }
            }
        }
    }
}