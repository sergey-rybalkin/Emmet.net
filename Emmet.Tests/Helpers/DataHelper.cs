using System;
using System.IO;
using System.Reflection;

namespace Emmet.Tests.Helpers
{
    /// <summary>
    /// Contains helper methods for accessing sample data for unit tests.
    /// </summary>
    public static partial class DataHelper
    {
        /// <summary>
        /// Gets specified embedded resource as string.
        /// </summary>
        /// <param name="fileName">Name of the file to return.</param>
        /// <exception cref="InvalidOperationException">
        /// Indicates that target resource does not have cursor marker.
        /// </exception>
        internal static string GetEmbeddedResource(string fileName)
        {
            string resourceName = "Emmet.Tests.Resources." + fileName;
            Assembly assembly = Assembly.GetCallingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (null == stream)
                    throw new InvalidOperationException($"Embedded resource not found: {fileName}");

                StreamReader reader = new StreamReader(stream);

                return reader.ReadToEnd();
            }
        }
    }
}