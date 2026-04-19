using System.IO;
using System.Reflection;
using Microsoft.ClearScript.V8;
using static Emmet.Diagnostics.Log;

namespace Emmet.Engine
{
    /// <summary>
    /// Compiles Emmet source files using the specified V8 engine instance.
    /// </summary>
    public class EngineCompiler
    {
#if DEBUG
        private const string s_emmetScript = @"Resources\emmet.js";
#else
        private const string s_emmetScript = @"Resources\emmet.min.js";
#endif

        private const string s_preferencesFileName = "preferences.json";

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
        public void CompileCore(V8ScriptEngine sourceContext)
        {
            string extensionFolder =
                Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string emmetScriptPath = Path.Combine(extensionFolder, s_emmetScript);

            if (!File.Exists(emmetScriptPath))
                throw new FileNotFoundException("Emmet script not found.", emmetScriptPath);

            string script = File.ReadAllText(emmetScriptPath);
            sourceContext.Execute(script);

            Trace("Emmet core compiled successfully.");
        }

        /// <summary>
        /// Loads JavaScript extensions and preferences from the specified directory.
        /// </summary>
        /// <param name="preferencesFile">JSON file that contains preferences in emmet format.</param>
        /// <param name="sourceContext">Source context to use during compilation.</param>
        public void LoadExtensions(string preferencesFile, V8ScriptEngine sourceContext)
        {
            if (File.Exists(preferencesFile))
            {
                // There is no native JSON API available so we need to create object string from file.
                string content = string.Join(" ", File.ReadAllLines(preferencesFile));
                sourceContext.Execute($"loadPreferences({content});");

                Trace($"Successfully loaded Emmet preferences from {preferencesFile}");
            }
        }
    }
}
