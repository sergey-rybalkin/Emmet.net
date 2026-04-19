using System;
using System.ComponentModel.Design;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.Shell;

namespace Emmet.EditorExtensions
{
    /// <summary>
    /// Alphabetically sorts selected lines in active document.
    /// </summary>
    internal sealed class SortCssPropertiesCommand
    {
        private readonly IServiceProvider _package;

        private SortCssPropertiesCommand(Package package)
        {
            _package = package;
            var commandService = _package.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;

            if (commandService != null)
            {
                var cmdID = new CommandID(
                    PackageGuids.GuidEmmetPackageCmdSet, PackageIds.CmdIDSortCssProperties);
                var menuItem = new OleMenuCommand(Execute, cmdID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets singleton instance of the command.
        /// </summary>
        public static SortCssPropertiesCommand Instance { get; private set; }

        /// <summary>
        /// Registers command in VS instance. Should be called once on package initialization.
        /// </summary>
        /// <param name="package">The package that command belongs to.</param>
        public static void Initialize(Package package)
        {
            Instance = new SortCssPropertiesCommand(package);
        }

        private static void SelectInnerDefinition(TextSelection selection, TextDocument document)
        {
            EditPoint pt = document.CreateEditPoint();

            int startPos = selection.ActivePoint.Line;
            int currentLine = startPos;

            bool IsCssRule(int lineNum)
            {
                string line = pt.GetLines(lineNum, lineNum + 1);

                // We reached the edge of element styles definition.
                if (line.Contains('}') || line.Contains('{'))
                    return false;

                // Current line does not contain any CSS rules, break to support invalid/incomplete files.
                if (!line.Contains(':'))
                    return false;

                return true;
            }

            // Find the start of styles definition block.
            while (IsCssRule(currentLine) && currentLine-- > 0);

            selection.MoveToLineAndOffset(currentLine + 1, 1, false);
            currentLine = startPos;

            // Find the end of styles definition block.
            while (IsCssRule(currentLine) && ++currentLine < document.EndPoint.Line);

            selection.MoveToLineAndOffset(currentLine, 1, true);
        }

        private void OnBeforeQueryStatus(object sender, EventArgs args)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var button = (OleMenuCommand)sender;

            if (_package.GetService(typeof(DTE)) is DTE2 ide)
            {
                var txtDoc = ide.ActiveDocument?.Object("TextDocument") as TextDocument;
                button.Enabled = !txtDoc?.Selection?.IsEmpty ?? false;
            }
            else
                button.Enabled = false;
        }

        private void Execute(object sender, EventArgs args)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var ide = _package.GetService(typeof(DTE)) as DTE2;
            Assumes.Present(ide);
            var document = ide?.ActiveDocument?.Object("TextDocument") as TextDocument;
            Assumes.Present(document);

            TextSelection selection = document.Selection;
            int originalPosition = selection.ActivePoint.AbsoluteCharOffset;

            // Make sure that we have selected lines that need to be sorted.
            if (selection.IsEmpty)
                SelectInnerDefinition(selection, document);

            var start = selection.TopPoint.CreateEditPoint();
            var end = selection.BottomPoint.CreateEditPoint();
            start.StartOfLine();
            end.EndOfLine();
            string selectedText = start.GetText(end);

            // Sort selected lines of text.
            string[] splitText = selectedText.Split(
                new[] { "\r\n", "\n" },
                StringSplitOptions.RemoveEmptyEntries);
            string sortedText = string.Join("\n", splitText.OrderBy(x => x));

            // If the selected and sorted text do not match, delete and insert the replacement.
            if (!selectedText.Equals(sortedText, StringComparison.CurrentCulture))
            {
                if (!ide.UndoContext.IsOpen)
                    ide.UndoContext.Open(Vsix.Name);

                try
                {
                    start.Delete(end);

                    EditPoint insertCursor = start.CreateEditPoint();
                    insertCursor.Insert(sortedText);
                }
                finally
                {
                    ide.UndoContext.Close();
                }
            }

            selection.MoveToAbsoluteOffset(originalPosition);
        }
    }
}
