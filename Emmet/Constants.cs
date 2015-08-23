using System;

namespace Emmet
{
    /// <summary>
    /// Registers and runs menu command handlers.
    /// </summary>
    internal static class Constants
    {
        /// <summary>
        /// Identifier for the expand abbreviation command.
        /// </summary>
        public const int ExpandAbbreviationCommandId = 0x0100;

        /// <summary>
        /// Identifier for the wrap with abbreviation command.
        /// </summary>
        public const int WrapWithAbbreviationCommandId = 0x0101;

        /// <summary>
        /// Identifier for the toggle comment command.
        /// </summary>
        public const int ToggleCommentCommandId = 0x0102;

        /// <summary>
        /// Identifier for the merge lines command.
        /// </summary>
        public const int MergeLinesCommandId = 0x0103;

        /// <summary>
        /// Unique identifier of the Emmet command set.
        /// </summary>
        public static readonly Guid CommandSetGuid = new Guid("8e070628-bb22-4983-99e8-ac37608cc5a6");
    }
}