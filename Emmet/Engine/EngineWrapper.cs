using System;
using System.Collections.Generic;
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

        private static IDictionary<string, JavaScriptNativeFunction> _callbacks;

        private bool _initialized = false;

        private JavaScriptRuntime _engine;

        private JavaScriptContext _context;

        private string _extensionsDir = null;

        private EmmetEditorCallbacks _editor = new EmmetEditorCallbacks();

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
        /// <param name="editor">Editor to execute command in.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Indicates that the specified command identifier was not found.
        /// </exception>
        public bool RunCommand(int cmdId, IEmmetEditor editor)
        {
            if (!_initialized)
                InitializeEngine();

            if (!_editor.Attach(editor))
                return false;

            string script = string.Empty;
            switch (cmdId)
            {
                case PackageIds.CmdIDExpandAbbreviation:
                    script = "window.emmet.run('expand_abbreviation', editor);";
                    break;
                case PackageIds.CmdIDWrapWithAbbreviation:
                    script = "window.emmet.run('wrap_with_abbreviation', editor);";
                    break;
                case PackageIds.CmdIDToggleComment:
                    script = "window.emmet.run('toggle_comment', editor);";
                    break;
                case PackageIds.CmdIDMergeLines:
                    script = "window.emmet.run('merge_lines', editor);";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(cmdId),
                        cmdId,
                        "Specified command identifier not found.");
            }

            JavaScriptContext.Current = _context;
            JavaScriptValue result = JavaScriptContext.RunScript(script, currentSourceContext++);
            _editor.Detach();

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
            var compiler = new EngineCompiler(_engine);

            compiler.CompileCore(currentSourceContext);
            _callbacks = compiler.RegisterCallbacks(_editor, currentSourceContext);
            if (null != _extensionsDir)
                compiler.LoadExtensions(_extensionsDir);

            Trace("Emmet engine successfully initialized.");

            _initialized = true;
        }
    }
}