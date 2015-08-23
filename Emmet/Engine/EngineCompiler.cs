using System.IO;
using System.Reflection;
using V8.Net;
using Emmet.Diagnostics;
using System.Collections.Generic;

namespace Emmet.Engine
{
    /// <summary>
    /// Compiles Emmet source files using the specified V8 engine instance.
    /// </summary>
    public class EngineCompiler
    {
        public const string PreferencesFileName = "preferences.json";

        private V8Engine _engine;

        private EmmetFileCallbacks _fileCallbacks;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="engine">The engine to use for compilation.</param>
        public EngineCompiler(V8Engine engine)
        {
            _engine = engine;
        }

        /// <summary>
        /// Finds and compiles Emmet source file.
        /// </summary>
        /// <exception cref="FileNotFoundException">
        /// Indicates that Emmet script was not found.
        /// </exception>
        public void CompileCore()
        {
            string extensionFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string emmetScript = Path.Combine(extensionFolder, @"emmet-min.js");

            if (!File.Exists(emmetScript))
                throw new FileNotFoundException("Emmet script not found.", emmetScript);

            ObjectHandle window = _engine.CreateObject();
            _engine.DynamicGlobalObject.window = window;

            Handle script = _engine.LoadScript(emmetScript, "Emmet");
            if (script.IsError)
            {
                this.TraceError(script.AsString);
                throw new Exception<EmmetEngineExceptionArgs>(
                    new EmmetEngineExceptionArgs("Failed to compile Emmet", script));
            }
            else
                this.Trace("Emmet core compiled successfully.");
        }

        /// <summary>
        /// Registers the callbacks required by Emmet.
        /// </summary>
        /// <param name="editor">Callbacks handler.</param>
        public void RegisterCallbacks(EmmetEditorCallbacks editor)
        {
            ObjectHandle proxy = _engine.CreateObject();

            proxy.SetProperty("getSelectionRange", GetFunctionWrapper(editor.GetSelectionRange));
            proxy.SetProperty("createSelection", GetFunctionWrapper(editor.CreateSelection));
            proxy.SetProperty("getCurrentLineRange", GetFunctionWrapper(editor.GetCurrentLineRange));
            proxy.SetProperty("getCaretPos", GetFunctionWrapper(editor.GetCarretPos));
            proxy.SetProperty("setCaretPos", GetFunctionWrapper(editor.SetCarretPos));
            proxy.SetProperty("getCurrentLine", GetFunctionWrapper(editor.GetCurrentLine));
            proxy.SetProperty("replaceContent", GetFunctionWrapper(editor.ReplaceContent));
            proxy.SetProperty("getContent", GetFunctionWrapper(editor.GetContent));
            proxy.SetProperty("getSyntax", GetFunctionWrapper(editor.GetSyntax));
            proxy.SetProperty("getProfileName", GetFunctionWrapper(editor.GetProfileName));
            proxy.SetProperty("prompt", GetFunctionWrapper(editor.Prompt));
            proxy.SetProperty("getSelection", GetFunctionWrapper(editor.GetSelection));
            proxy.SetProperty("getFilePath", GetFunctionWrapper(editor.GetFilePath));

            _engine.DynamicGlobalObject.editor = proxy;

            this.Trace("IEmmetEditor callbacks successfully registered.");

            _fileCallbacks = new EmmetFileCallbacks();
            ObjectHandle file = _engine.CreateObject();

            file.SetProperty("read", GetFunctionWrapper(_fileCallbacks.Read));
            file.SetProperty("locateFile", GetFunctionWrapper(_fileCallbacks.LocateFile));
            file.SetProperty("createPath", GetFunctionWrapper(_fileCallbacks.CreatePath));
            file.SetProperty("save", GetFunctionWrapper(_fileCallbacks.Save));
            file.SetProperty("getExt", GetFunctionWrapper(_fileCallbacks.GetExtension));

            _engine.DynamicGlobalObject.window.emmet.file = file;

            this.Trace("IEmmetFile callbacks successfully registered.");
        }

        /// <summary>
        /// Loads JavaScript extensions and preferences from the specified directory.
        /// </summary>
        /// <param name="extensionsDirectory">Pathname of the extensions directory.</param>
        public void LoadExtensions(string extensionsDirectory)
        {
            List<InternalHandle> extensions = new List<InternalHandle>();
            var files = Directory.EnumerateFiles(extensionsDirectory, "*.*");
            ObjectHandle emmet = _engine.DynamicGlobalObject.window.emmet;

            foreach (string filePath in files)
            {
                if (0 != string.Compare(Path.GetFileName(filePath), PreferencesFileName, true))
                {
                    extensions.Add(_engine.CreateValue(filePath));
                    continue;
                }

                string content = File.ReadAllText(filePath);
                Handle parameter = _engine.CreateValue(content);
                Handle result = emmet.Call("loadUserData", emmet, parameter);

                if (result.IsError)
                    this.TraceError($"Failed to load Emmet preferences from {filePath}: {result.AsString}");
                else
                    this.Trace($"Successfully loaded Emmet preferences from {filePath}");
            }

            if (extensions.Count > 0)
            {
                var parameter = _engine.CreateArray(extensions.ToArray());
                Handle result = emmet.Call("loadExtensions", emmet, parameter);

                if (result.IsError)
                    this.TraceError($"Failed to load Emmet extensions: {result.AsString}");
                else
                    this.Trace($"Successfully loaded {extensions.Count} Emmet extensions");
            }
        }

        private V8Function GetFunctionWrapper(JSFunction callback)
        {
            return _engine.CreateFunctionTemplate().GetFunctionObject(callback);
        }
    }
}