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
        private static JavaScriptSourceContext currentSourceContext =
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
                    string code = view.GetCurrentLine();
                    script = $"extractAbbreviation('{code}');";
                    break;
                case PackageIds.CmdIDWrapWithAbbreviation:
                    script = "window.emmet.run('wrap_with_abbreviation', editor);";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(cmdId),
                        cmdId,
                        "Specified command identifier not found.");
            }

            JavaScriptContext.Current = _context;
            JavaScriptValue result = JavaScriptContext.RunScript(script, currentSourceContext++);

            if (!result.ToBoolean())
            {
                Trace("Emmet engine failed to execute command.");
                return false;
            }

            JavaScriptContext.Idle();

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

        private void InitializeEngine()
        {
            Trace("Started initializing Emmet engine.");

            _engine = JavaScriptRuntime.Create(JavaScriptRuntimeAttributes.EnableIdleProcessing);
            _context = _engine.CreateContext();
            JavaScriptContext.Current = _context;
            var compiler = new EngineCompiler();

            compiler.CompileCore(currentSourceContext);
            if (null != _extensionsDir)
                compiler.LoadExtensions(_extensionsDir);

            Trace("Emmet engine successfully initialized.");

            _initialized = true;
        }
    }
}