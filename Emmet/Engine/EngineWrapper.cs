using System;
using Microsoft.ClearScript.V8;
using static Emmet.Diagnostics.Tracer;

namespace Emmet.Engine
{
    /// <summary>
    /// Encapsulates Emmet library compiled using V8 engine and provides command execution functionality.
    /// </summary>
    public class EngineWrapper : IDisposable
    {
        private const string ExpandFunctionName = "replaceAbbreviation";

        private bool _initialized = false;

        private V8ScriptEngine _engine;

        private string _extensionsDir = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineWrapper"/> class.
        /// </summary>
        /// <param name="extensionsDirectory">Pathname of the directory to load Emmet extensions from.</param>
        public EngineWrapper(string extensionsDirectory)
        {
            _extensionsDir = extensionsDirectory;
        }

        /// <summary>
        /// Executes Emmet command with the specified identifier on the specified editor view.
        /// </summary>
        /// <param name="cmdId">Identifier of the command to execute.</param>
        /// <param name="view">Editor to execute command in.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Indicates that the specified command identifier was not found.
        /// </exception>
        public bool RunCommand(int cmdId, ICodeEditor view)
        {
            if (!_initialized)
                InitializeEngine();

            object result;

            switch (cmdId)
            {
                case PackageIds.CmdIDExpandAbbreviation:
                    result = GetExpandAbbreviationScript(view);
                    break;
                case PackageIds.CmdIDWrapWithAbbreviation:
                    result = GetWrapWithAbbreviationScript(view);
                    if (result is null)
                        return false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(cmdId),
                        cmdId,
                        "Specified command identifier not found.");
            }

            if (result is bool)
            {
                Trace("Emmet engine failed to execute command.");
                return false;
            }

            string replacement = result.ToString();
            _engine.CollectGarbage(false);
            if (cmdId is PackageIds.CmdIDExpandAbbreviation)
                view.ReplaceCurrentLine(replacement);
            else
                view.ReplaceSelection(replacement);

            Trace($"Command completed successfully.");

            return true;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        /// resources.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);

            if (_initialized)
            {
                _engine.Dispose();
                _initialized = false;
            }
        }

        private static string JavaScriptEscape(string value)
        {
            return value.Replace("'", "''").Replace("\r", string.Empty).Replace("\n", "\\n");
        }

        private static string ContentTypeToSyntax(string contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType) || !contentType.EndsWith("ss"))
                return "markup";
            else
                return "stylesheet";
        }

        private void InitializeEngine()
        {
            Trace("Started initializing Emmet engine.");

            _engine = new V8ScriptEngine();
            var compiler = new EngineCompiler();

            compiler.CompileCore(_engine);
            if (null != _extensionsDir)
                compiler.LoadExtensions(_extensionsDir, _engine);

            Trace("Emmet engine successfully initialized.");

            _initialized = true;
        }

        private object GetExpandAbbreviationScript(ICodeEditor view)
        {
            string syntax = ContentTypeToSyntax(view.GetContentTypeInActiveBuffer());
            string currentLine = JavaScriptEscape(view.GetCurrentLine());
            int caretPos = view.GetCaretPosColumn();

            Trace($"Trying to expand {currentLine}");
            return _engine.Invoke(
                ExpandFunctionName, currentLine, caretPos, syntax, null, view.AbbreviationPrefix);
        }

        private object GetWrapWithAbbreviationScript(ICodeEditor view)
        {
            string syntax = ContentTypeToSyntax(view.GetContentTypeInActiveBuffer());
            string selection = JavaScriptEscape(view.GetSelection());
            string abbreviation = view.Prompt();
            if (string.IsNullOrWhiteSpace(selection) || string.IsNullOrWhiteSpace(abbreviation))
            {
                Trace("Cannot wrap empty string.");
                return null;
            }

            Trace($"Trying to wrap {selection} with {abbreviation}");
            return _engine.Invoke(
                ExpandFunctionName, abbreviation, abbreviation.Length, syntax, selection, string.Empty);
        }
    }
}