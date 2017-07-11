using System;
using System.IO;
using Emmet.Engine.ChakraInterop;
using static Emmet.Diagnostics.Tracer;

namespace Emmet.Engine
{
    /// <summary>
    /// Implementation of IEmmetFile interface for JavaScript engine. See
    /// https://github.com/emmetio/emmet/blob/master/lib/interfaces/IEmmetFile.js for details.
    /// </summary>
    public class EmmetFileCallbacks
    {
        /// <summary>
        /// JavaScript callback. Reads the specified file content and returns it as string.
        /// </summary>
        public JavaScriptValue Read(
            JavaScriptValue callee,
            bool isConstructCall,
            JavaScriptValue[] arguments,
            ushort argumentCount,
            IntPtr callbackData)
        {
            if (4 != argumentCount)
            {
                Trace("IEmmetFile read called with invalid number of arguments.");
                return JavaScriptValue.False;
            }

            string targetFilePath = arguments[1].ToString();
            int chunkSize = arguments[2].ToInt32();
            JavaScriptValue callback = arguments[3];

            if (!File.Exists(targetFilePath))
            {
                Trace($"Emmet requested file {targetFilePath} that does not exist.");
                callback.CallFunction(JavaScriptValue.FromBoolean(true), JavaScriptValue.Null);

                return JavaScriptValue.False;
            }

            char[] buf = new char[chunkSize];
            FileStream stream = File.OpenRead(targetFilePath);
            using (StreamReader reader = new StreamReader(stream))
            {
                chunkSize = reader.ReadBlock(buf, 0, chunkSize);
            }

            string retVal = new string(buf, 0, chunkSize);
            callback.CallFunction(JavaScriptValue.False, JavaScriptValue.FromString(retVal));

            return JavaScriptValue.True;
        }

        /// <summary>
        /// JavaScript callback. Returns absolute path to the file that is referenced from the file in the
        /// editor. Implementation copied from the Emmet project source code.
        /// </summary>
        public JavaScriptValue LocateFile(
            JavaScriptValue callee,
            bool isConstructCall,
            JavaScriptValue[] arguments,
            ushort argumentCount,
            IntPtr callbackData)
        {
            if (3 != argumentCount)
            {
                Trace("IEmmetFile locateFile called with invalid number of arguments.");
                return JavaScriptValue.False;
            }

            string editorFile = arguments[1].ToString();
            string targetFile = arguments[2].ToString();

            if (targetFile.StartsWith("HTTP", StringComparison.InvariantCultureIgnoreCase))
                return JavaScriptValue.FromString(targetFile);

            string folder = Path.GetDirectoryName(editorFile);
            do
            {
                string retVal = Path.Combine(folder, targetFile);
                if (File.Exists(retVal))
                    return JavaScriptValue.FromString(retVal);
            }
            while (folder.Length > 3);

            return JavaScriptValue.FromString(string.Empty);
        }

        /// <summary>
        /// JavaScript callback. Creates absolute path by concatenating two arguments.
        /// </summary>
        public JavaScriptValue CreatePath(
            JavaScriptValue callee,
            bool isConstructCall,
            JavaScriptValue[] arguments,
            ushort argumentCount,
            IntPtr callbackData)
        {
            if (3 != argumentCount)
            {
                Trace("IEmmetFile createPath called with invalid number of arguments.");
                return JavaScriptValue.True;
            }

            string parent = arguments[1].ToString();
            string fileName = arguments[2].ToString();

            if (Path.HasExtension(parent))
                parent = Path.GetDirectoryName(parent);

            return JavaScriptValue.FromString(Path.Combine(parent, fileName));
        }

        /// <summary>
        /// JavaScript callback. Saves the specified content to the file with the specified name.
        /// </summary>
        public JavaScriptValue Save(
            JavaScriptValue callee,
            bool isConstructCall,
            JavaScriptValue[] arguments,
            ushort argumentCount,
            IntPtr callbackData)
        {
            if (3 != argumentCount)
            {
                Trace("IEmmetFile save called with invalid number of arguments.");
                return JavaScriptValue.False;
            }

            string filePath = arguments[1].ToString();
            string content = arguments[2].ToString();

            File.WriteAllText(filePath, content);

            return JavaScriptValue.True;
        }

        /// <summary>
        /// JavaScript callback. Returns file extension in lower case.
        /// </summary>
        public JavaScriptValue GetExtension(
            JavaScriptValue callee,
            bool isConstructCall,
            JavaScriptValue[] arguments,
            ushort argumentCount,
            IntPtr callbackData)
        {
            if (2 != argumentCount)
            {
                Trace("IEmmetFile getExt called with invalid number of arguments.");
                return JavaScriptValue.False;
            }

            string filePath = arguments[1].ToString();

            return JavaScriptValue.FromString(Path.GetExtension(filePath).ToLowerInvariant());
        }
    }
}