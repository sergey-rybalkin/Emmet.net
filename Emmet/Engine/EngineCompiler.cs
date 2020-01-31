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

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineCompiler"/> class.
        /// </summary>
        public EngineCompiler()
        {
        }

        /// <summary>
        /// Finds and compiles Emmet source file.
        /// </summary>
        /// <param name="sourceContext">Source context to use during compilation.</param>
        /// <exception cref="FileNotFoundException">Indicates that Emmet script was not found.</exception>
        /// <exception cref="Exception{EmmetEngineExceptionArgs}">
        /// Indicates that JavaScript error occurred during compilation.
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
    }
}