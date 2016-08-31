using System.IO;
using Emmet.Diagnostics;
using V8.Net;

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
        public InternalHandle Read(
            V8Engine engine,
            bool isConstructCall,
            InternalHandle self,
            params InternalHandle[] args)
        {
            if (args.Length != 3)
            {
                this.TraceError("IEmmetFile read called with invalid number of arguments.");
                return engine.CreateValue(false);
            }

            string targetFilePath = args[0].AsString;
            int chunkSize = args[1].AsInt32;
            ObjectHandle callback = args[2];

            if (!File.Exists(targetFilePath))
            {
                this.TraceError($"Emmet requested file {targetFilePath} that does not exist.");
                callback.StaticCall(engine.CreateValue(true), engine.CreateNullValue());

                return engine.CreateValue(false);
            }

            char[] buf = new char[chunkSize];
            FileStream stream = File.OpenRead(targetFilePath);
            using (StreamReader reader = new StreamReader(stream))
            {
                chunkSize = reader.ReadBlock(buf, 0, chunkSize);
            }

            string retVal = new string(buf, 0, chunkSize);
            callback.StaticCall(engine.CreateValue(false), engine.CreateValue(retVal));

            return engine.CreateValue(true);
        }

        /// <summary>
        /// JavaScript callback. Returns absolute path to the file that is referenced from the file in the
        /// editor. Implementation copied from the Emmet project source code.
        /// </summary>
        public InternalHandle LocateFile(
            V8Engine engine,
            bool isConstructCall,
            InternalHandle self,
            params InternalHandle[] args)
        {
            if (args.Length != 2)
            {
                this.TraceError("IEmmetFile locateFile called with invalid number of arguments.");
                return engine.CreateValue(false);
            }

            string editorFile = args[0].AsString;
            string targetFile = args[1].AsString;

            if (targetFile.StartsWith("HTTP", System.StringComparison.InvariantCultureIgnoreCase))
                return engine.CreateValue(targetFile);

            string folder = Path.GetDirectoryName(editorFile);
            do
            {
                string retVal = Path.Combine(folder, targetFile);
                if (File.Exists(retVal))
                    return engine.CreateValue(retVal);
            }
            while (folder.Length > 3);

            return engine.CreateValue(string.Empty);
        }

        /// <summary>
        /// JavaScript callback. Creates absolute path by concatenating two arguments.
        /// </summary>
        public InternalHandle CreatePath(
            V8Engine engine,
            bool isConstructorCall,
            InternalHandle self,
            params InternalHandle[] args)
        {
            if (args.Length != 2)
            {
                this.TraceError("IEmmetFile createPath called with invalid number of arguments.");
                return engine.CreateValue(false);
            }

            string parent = args[0].AsString;
            string fileName = args[1].AsString;

            if (Path.HasExtension(parent))
                parent = Path.GetDirectoryName(parent);

            return engine.CreateValue(Path.Combine(parent, fileName));
        }

        /// <summary>
        /// JavaScript callback. Saves the specified content to the file with the specified name.
        /// </summary>
        public InternalHandle Save(
            V8Engine engine,
            bool isConstructorCall,
            InternalHandle self,
            params InternalHandle[] args)
        {
            if (args.Length != 2)
            {
                this.TraceError("IEmmetFile save called with invalid number of arguments.");
                return engine.CreateValue(false);
            }

            string filePath = args[0].AsString;
            string content = args[1].AsString;

            File.WriteAllText(filePath, content);

            return engine.CreateValue(true);
        }

        /// <summary>
        /// JavaScript callback. Returns file extension in lower case.
        /// </summary>
        public InternalHandle GetExtension(
            V8Engine engine,
            bool isConstructorCall,
            InternalHandle self,
            params InternalHandle[] args)
        {
            if (args.Length != 1)
            {
                this.TraceError("IEmmetFile getExt called with invalid number of arguments.");
                return engine.CreateValue(false);
            }

            string filePath = args[0].AsString;

            return engine.CreateValue(Path.GetExtension(filePath).ToLowerInvariant());
        }

    }
}