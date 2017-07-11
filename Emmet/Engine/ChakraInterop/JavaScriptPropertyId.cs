using System;

namespace Emmet.Engine.ChakraInterop
{
    public struct JavaScriptPropertyId : IEquatable<JavaScriptPropertyId>
    {
        private readonly IntPtr _id;

        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptPropertyId"/> structure.
        /// </summary>
        /// <param name="id">Property identifier.</param>
        internal JavaScriptPropertyId(IntPtr id)
        {
            _id = id;
        }

        /// <summary>
        /// Gets an invalid ID.
        /// </summary>
        public static JavaScriptPropertyId Invalid
        {
            get { return new JavaScriptPropertyId(IntPtr.Zero); }
        }

        /// <summary>
        /// Gets the name associated with the property ID.
        /// </summary>
        public string Name
        {
            get
            {
                NativeMethods.JsGetPropertyNameFromId(this, out string name).ThrowIfError();
                return name;
            }
        }

        /// <summary>
        /// The equality operator for property IDs.
        /// </summary>
        /// <param name="left">The first property ID to compare.</param>
        /// <param name="right">The second property ID to compare.</param>
        /// <returns>Whether the two property IDs are the same.</returns>
        public static bool operator ==(JavaScriptPropertyId left, JavaScriptPropertyId right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     The inequality operator for property IDs.
        /// </summary>
        /// <param name="left">The first property ID to compare.</param>
        /// <param name="right">The second property ID to compare.</param>
        /// <returns>Whether the two property IDs are not the same.</returns>
        public static bool operator !=(JavaScriptPropertyId left, JavaScriptPropertyId right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Gets the property ID associated with the name.
        /// </summary>
        /// <param name="name">Name of the property to get ID of.</param>
        public static JavaScriptPropertyId FromString(string name)
        {
            NativeMethods.JsGetPropertyIdFromName(name, out JavaScriptPropertyId id).ThrowIfError();
            return id;
        }

        /// <summary>
        ///     Checks for equality between property IDs.
        /// </summary>
        /// <param name="other">The other property ID to compare.</param>
        /// <returns>Whether the two property IDs are the same.</returns>
        public bool Equals(JavaScriptPropertyId other)
        {
            return _id == other._id;
        }

        /// <summary>
        /// Checks for equality between property IDs.
        /// </summary>
        /// <param name="obj">The other property ID to compare.</param>
        /// <returns>Whether the two property IDs are the same.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            return obj is JavaScriptPropertyId && Equals((JavaScriptPropertyId)obj);
        }

        /// <summary>
        /// The hash code.
        /// </summary>
        /// <returns>The hash code of the property ID.</returns>
        public override int GetHashCode()
        {
            return _id.ToInt32();
        }

        /// <summary>
        /// Converts the property ID to a string.
        /// </summary>
        /// <returns>The name of the property ID.</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}