using System;
using Emmet.Engine.ChakraInterop;
using static Emmet.Diagnostics.Tracer;

namespace Emmet.Engine
{
    /// <summary>
    /// Encapsulates Emmet library compiled using V8 engine and provides command execution functionality.
    /// </summary>
    public class EngineWrapper : IDisposable
    {
        private const string ScriptTemplate =
            "replaceAbbreviation('{0}', '{1}', '{2}', {3}, '{4}');";

        private static JavaScriptSourceContext _currentSourceContext =
            JavaScriptSourceContext.FromIntPtr(IntPtr.Zero);

        private bool _initialized = false;

        private JavaScriptRuntime _engine;

        private JavaScriptContext _context;

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

            string script;

            switch (cmdId)
            {
                case PackageIds.CmdIDExpandAbbreviation:
                    script = GetExpandAbbreviationScript(view);
                    break;
                case PackageIds.CmdIDWrapWithAbbreviation:
                    script = GetWrapWithAbbreviationScript(view);
                    if (script is null)
                        return false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(cmdId),
                        cmdId,
                        "Specified command identifier not found.");
            }

            Trace($"Running script: {script}");
            JavaScriptContext.Current = _context;
            JavaScriptValue result = JavaScriptContext.RunScript(script, _currentSourceContext++);

            if (result.ValueType is JavaScriptValueType.Boolean)
            {
                Trace("Emmet engine failed to execute command.");
                return false;
            }

            string replacement = result.ToString();
            JavaScriptContext.Idle();
            if (cmdId is PackageIds.CmdIDExpandAbbreviation)
                view.ReplaceCurrentLine(replacement);
            else
                view.ReplaceSelection(replacement);

            Trace($"Command {script} completed successfully.");
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

        private static string JavaScriptEscape(string value) => value.Replace("'", "''");

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

            _engine = JavaScriptRuntime.Create(JavaScriptRuntimeAttributes.EnableIdleProcessing);
            _context = _engine.CreateContext();
            JavaScriptContext.Current = _context;
            var compiler = new EngineCompiler();

            compiler.CompileCore(_currentSourceContext);
            if (null != _extensionsDir)
                compiler.LoadExtensions(_extensionsDir, _currentSourceContext);

            Trace("Emmet engine successfully initialized.");

            _initialized = true;
        }

        private string GetExpandAbbreviationScript(ICodeEditor view)
        {
            string syntax = ContentTypeToSyntax(view.GetContentTypeInActiveBuffer());
            string currentLine = JavaScriptEscape(view.GetCurrentLine());
            int caretPos = view.GetCaretPosColumn();

            return string.Format(
                ScriptTemplate,
                currentLine,
                caretPos,
                syntax,
                "null",
                view.AbbreviationPrefix);
        }

        private string GetWrapWithAbbreviationScript(ICodeEditor view)
        {
            string syntax = ContentTypeToSyntax(view.GetContentTypeInActiveBuffer());
            string selection = JavaScriptEscape(view.GetSelection());
            string abbreviation = view.Prompt();
            if (string.IsNullOrWhiteSpace(selection) || string.IsNullOrWhiteSpace(abbreviation))
            {
                Trace("Cannot wrap empty string.");
                return null;
            }

            return string.Format(
                ScriptTemplate,
                abbreviation,
                abbreviation.Length,
                syntax,
                "'" + selection + "'",
                string.Empty);
        }
    }
}