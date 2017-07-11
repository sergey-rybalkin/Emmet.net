using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Emmet.Engine.ChakraInterop;
using static Emmet.Diagnostics.Tracer;

namespace Emmet.Engine
{
    /// <summary>
    /// Compiles Emmet source files using the specified V8 engine instance.
    /// </summary>
    public class EngineCompiler
    {
        public const string PreferencesFileName = "preferences.json";

        private JavaScriptRuntime _engine;

        private EmmetFileCallbacks _fileCallbacks;

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineCompiler"/> class.
        /// </summary>
        /// <param name="engine">The engine to use for compilation.</param>
        public EngineCompiler(JavaScriptRuntime engine)
        {
            _engine = engine;
        }

        /// <summary>
        /// Finds and compiles Emmet source file.
        /// </summary>
        /// <param name="sourceContext">Source context to use during compilation.</param>
        /// <exception cref="FileNotFoundException">Indicates that Emmet script was not found.</exception>
        /// <exception cref="Exception{EmmetEngineExceptionArgs}">
        /// Indicates that JavaScript error occured during compilation.
        /// </exception>
        public void CompileCore(JavaScriptSourceContext sourceContext)
        {
            string extensionFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string emmetScriptPath = Path.Combine(extensionFolder, @"emmet-min.js");

            if (!File.Exists(emmetScriptPath))
                throw new FileNotFoundException("Emmet script not found.", emmetScriptPath);

            JavaScriptValue window = JavaScriptValue.CreateObject();
            JavaScriptValue.GlobalObject.SetProperty("window", window, true);

            string script = File.ReadAllText(emmetScriptPath);
            JavaScriptContext.RunScript(script, sourceContext);

            Trace("Emmet core compiled successfully.");
        }

        /// <summary>
        /// Registers the callbacks required by Emmet. Returned collection should not be garbage collected
        /// before the engine itself.
        /// </summary>
        /// <param name="editor">Callbacks handler.</param>
        /// <param name="sourceContext">Source context to use during compilation.</param>
        public IDictionary<string, JavaScriptNativeFunction> RegisterCallbacks(
            EmmetEditorCallbacks editor,
            JavaScriptSourceContext sourceContext)
        {
            var retVal = new Dictionary<string, JavaScriptNativeFunction>();

            _fileCallbacks = new EmmetFileCallbacks();
            JavaScriptValue file = JavaScriptValue.CreateObject();
            JavaScriptValue editorProxy = JavaScriptValue.CreateObject();

            retVal.Add("getSelectionRange", editor.GetSelectionRange);
            retVal.Add("createSelection", editor.CreateSelection);
            retVal.Add("getCurrentLineRange", editor.GetCurrentLineRange);
            retVal.Add("getCaretPos", editor.GetCarretPos);
            retVal.Add("setCaretPos", editor.SetCarretPos);
            retVal.Add("getCurrentLine", editor.GetCurrentLine);
            retVal.Add("replaceContent", editor.ReplaceContent);
            retVal.Add("getContent", editor.GetContent);
            retVal.Add("getSyntax", editor.GetSyntax);
            retVal.Add("getProfileName", editor.GetProfileName);
            retVal.Add("prompt", editor.Prompt);
            retVal.Add("getSelection", editor.GetSelection);
            retVal.Add("getFilePath", editor.GetFilePath);

            retVal.Add("read", _fileCallbacks.Read);
            retVal.Add("locateFile", _fileCallbacks.LocateFile);
            retVal.Add("createPath", _fileCallbacks.CreatePath);
            retVal.Add("save", _fileCallbacks.Save);
            retVal.Add("getExt", _fileCallbacks.GetExtension);

            foreach (var callback in retVal)
            {
                if (callback.Value.Target == editor)
                    RegisterCallback(editorProxy, callback.Key, callback.Value);
                else
                    RegisterCallback(file, callback.Key, callback.Value);
            }

            JavaScriptValue emmet = JavaScriptValue.GlobalObject.GetProperty("window").GetProperty("emmet");
            emmet.SetProperty("file", file, true);
            JavaScriptValue.GlobalObject.SetProperty("editor", editorProxy, true);

            Trace("IEmmetFile and IEmmetEditor callbacks successfully registered.");

            return retVal;
        }

        /// <summary>
        /// Loads JavaScript extensions and preferences from the specified directory.
        /// </summary>
        /// <param name="extensionsDirectory">Pathname of the extensions directory.</param>
        public void LoadExtensions(string extensionsDirectory)
        {
            var files = Directory.EnumerateFiles(extensionsDirectory, "*.*");
            JavaScriptValue emmet = JavaScriptValue.GlobalObject.GetProperty("window").GetProperty("emmet");

            foreach (string filePath in files)
            {
                if (0 != string.Compare(Path.GetFileName(filePath), PreferencesFileName, true))
                    continue;

                string content = File.ReadAllText(filePath);
                var parameter = JavaScriptValue.FromString(content);
                emmet.GetProperty("loadUserData").CallFunction(emmet, parameter);

                Trace($"Successfully loaded Emmet preferences from {filePath}");
            }
        }

        private void RegisterCallback(
            JavaScriptValue container, string name, JavaScriptNativeFunction callback)
        {
            var func = JavaScriptValue.CreateFunction(callback);
            container.SetProperty(name, func, true);
        }
    }
}