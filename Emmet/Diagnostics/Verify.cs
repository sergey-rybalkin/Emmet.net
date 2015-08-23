using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Emmet.Diagnostics
{
    /// <summary>
    /// Helper diagnostics methods used throughout the codebase.
    /// </summary>
    public static class Verify
    {
        /// <summary>
        /// Validates that the specified value of the argument is not <see langword="null"/>.
        /// </summary>
        /// <typeparam name="T">Type of the value of the argument to validate.</typeparam>
        /// <param name="value">The value to validate.</param>
        /// <param name="name">The name of the argument to validate.</param>
        /// <exception cref="ArgumentNullException">
        /// Indicates that <param name="value"/> is null.
        /// </exception>
        public static void ArgumentNotNull<T>(T value, string name) where T : class
        {
            Debug.Assert(null != value, string.Format("{0} is not null", name ?? string.Empty));
            if (null == value)
                throw new ArgumentNullException(name ?? string.Empty);
        }

        /// <summary>
        /// Validates that the specified value of the argument is not an empty string.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="name">The name of the argument to validate.</param>
        /// <exception cref="ArgumentNullException">
        /// Indicates that <param name="value"/> is null.
        /// </exception>
        public static void ArgumentNotEmpty(string value, string name)
        {
            Debug.Assert(!string.IsNullOrEmpty(value),
                         string.Format("{0} is not null or empty", name ?? string.Empty));
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(name ?? string.Empty);
        }

        /// <summary>
        /// Validates that the specified value of the argument contains elements.
        /// </summary>
        /// <typeparam name="T">Type of the elements in the collection.</typeparam>
        /// <param name="value">The value to validate.</param>
        /// <param name="name">The name of the argument to validate.</param>
        /// <exception cref="ArgumentNullException">
        /// Indicates that <param name="value" /> is either null or does not contain any elements.
        /// </exception>
        public static void ArgumentNotEmpty<T>(IReadOnlyCollection<T> value, string name)
        {
            Debug.Assert(value.Any(),
                         string.Format("{0} contains elements", name ?? string.Empty));
            if (!value.Any())
                throw new ArgumentNullException(name ?? string.Empty);
        }

        /// <summary>
        /// Validates that the specified value of the argument is a virtual path string.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="name">The name of the argument to validate.</param>
        /// <exception cref="ArgumentException">
        /// Indicates that <param name="value"/> is not a valid virtual path.
        /// </exception>
        public static void ArgumentIsVirtualPath(string value, string name)
        {
            if (string.IsNullOrEmpty(value))
                return;

            Debug.Assert(value.StartsWith("~/"),
                         string.Format("{0} is a valid virtual path", name ?? string.Empty));
            if (!value.StartsWith("~/"))
                throw new ArgumentException(@"Specified value is not a valid virtual path", name);
        }

        /// <summary>
        /// Validates that the specified directory path is valid and exists.
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        /// <param name="name">The name of the argument to validate.</param>
        /// <exception cref="DirectoryNotFoundException">
        /// Indicates that the specified directory does not exist.
        /// </exception>
        public static void ArgumentDirectoryExists(string directoryPath, string name)
        {
            ArgumentNotEmpty(directoryPath, @"directoryPath");

            Debug.Assert(Directory.Exists(directoryPath),
                         string.Format("{0} does not exist: {1}", name, directoryPath));

            if (!Directory.Exists(directoryPath))
                throw new DirectoryNotFoundException(
                    string.Format("{0} does not exist: {1}", name, directoryPath));
        }
    }
}