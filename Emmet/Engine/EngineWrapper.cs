using System;
using V8.Net;
using Emmet.Diagnostics;

namespace Emmet.Engine
{
    /// <summary>
    /// Encapsulates Emmet library compiled using V8 engine and provides command execution functionality.
    /// </summary>
    public class EngineWrapper : IDisposable
    {
        private V8Engine _engine = null;

        private string _extensionsDir = null;

        private EmmetEditorCallbacks _editor = new EmmetEditorCallbacks();

        /// <summary>
        /// Constructor.
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
            if (null == _engine)
                InitializeEngine();

            if (!_editor.Attach(editor))
                return false;

            string script = string.Empty;
            switch (cmdId)
            {
                case Constants.ExpandAbbreviationCommandId:
                    script = "window.emmet.run('expand_abbreviation', editor);";
                    break;
                case Constants.WrapWithAbbreviationCommandId:
                    script = "window.emmet.run('wrap_with_abbreviation', editor);";
                    break;
                case Constants.ToggleCommentCommandId:
                    script = "window.emmet.run('toggle_comment', editor);";
                    break;
                case Constants.MergeLinesCommandId:
                    script = "window.emmet.run('merge_lines', editor);";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(cmdId),
                                                          cmdId,
                                                          "Specified command identifier not found.");
            }

            Handle retVal = _engine.Execute(script);
            _editor.Detach();

            if (retVal.IsError)
            {
                this.TraceError($"Emmet engine error: {retVal.AsString}");
                return false;
            }
            else if (retVal.AsBoolean == false)
            {
                this.Trace($"Command {script} returned false.");
                return false;
            }

            this.Trace($"Command {script} completed successfully.");
            return true;
        }

        private void InitializeEngine()
        {
            this.Trace("Started initializing Emmet engine.");

            _engine = new V8Engine();
            var compiler = new EngineCompiler(_engine);

            compiler.CompileCore();
            compiler.RegisterCallbacks(_editor);
            if (null != _extensionsDir)
                compiler.LoadExtensions(_extensionsDir);

            this.Trace("Emmet engine successfully initialized.");
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        /// resources.
        /// </summary>
        public void Dispose()
        {
            if (null != _engine)
            {
                _engine.Dispose();
                _engine = null;
            }
        }
    }
}