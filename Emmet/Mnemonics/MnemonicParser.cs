using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Emmet.Mnemonics
{
    /// <summary>
    /// Parses mnemonics and provides its string representation.
    /// </summary>
    public static class MnemonicParser
    {
        private static readonly Regex _mnemonicTemplate = new Regex(
            @"(_|p|P|pi|i)([csvar]?)(\w{1,2})([pmf])",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Dictionary<string, string> _acessibilityLevels =
            new Dictionary<string, string>(5)
        {
            { "_", "private" },
            { "p", "public" },
            { "P", "protected" },
            { "i", "internal" },
            { "pi", "protected internal" }
        };

        private static readonly Dictionary<string, string> _modifiers =
            new Dictionary<string, string>(5)
        {
            { "c", "const" },
            { "s", "static" },
            { "v", "virtual" },
            { "a", "abstract" },
            { "r", "readonly" }
        };

        private static readonly Dictionary<string, string> _types =
            new Dictionary<string, string>(11)
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

        public static bool TryParse(string mnemonic, out string memberDeclaration)
        {
            memberDeclaration = null;

            Match match = _mnemonicTemplate.Match(mnemonic);
            if (!match.Success)
                return false;

            try
            {
                string accessibilityLevel = _acessibilityLevels[match.Groups[1].Value];
                string modifier = match.Groups[2].Success ? _modifiers[match.Groups[2].Value] : null;
                string returnValue = _types[match.Groups[3].Value];
                char memberType = match.Groups[4].Value[0];

                StringBuilder retVal = new StringBuilder(64);
                retVal.Append(accessibilityLevel);
                retVal.Append(' ');

                if (null != modifier)
                {
                    retVal.Append(modifier);
                    retVal.Append(' ');
                }

                retVal.Append(returnValue);

                switch (memberType)
                {
                    case 'f':
                        retVal.Append(" _;");
                        break;
                    case 'm':
                        retVal.Append(" () \r\n{\r\n}");
                        break;
                    case 'p':
                        retVal.Append(" { get; set; }");
                        break;
                }

                memberDeclaration = retVal.ToString();
                return true;
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
        }
    }
}