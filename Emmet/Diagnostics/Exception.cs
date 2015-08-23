using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Emmet.Diagnostics
{
    /// <summary>
    /// Generic exception class that resolves most common problems with custom exceptions.
    /// </summary>
    /// <typeparam name="TExceptionArgs">The type of the exception arguments.</typeparam>
    [Serializable]
    public sealed class Exception<TExceptionArgs> : Exception
        where TExceptionArgs : ExceptionArgs
    {
        private const string ArgsName = "Args";

        private readonly TExceptionArgs _args;

        /// <summary>
        /// Initializes a new instance of the <see cref="Exception{TExceptionArgs}"/> class.
        /// </summary>
        /// <param name="args">Exception arguments that contain error information.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception, or a null reference (Nothing in Visual
        /// Basic) if no inner exception is specified.
        /// </param>
        public Exception(TExceptionArgs args, string message = null, Exception innerException = null)
            : base(message, innerException)
        {
            _args = args;
        }

        // This constructor is for deserialization; since the class is sealed, the constructor is
        // private. If this class were not sealed, this constructor should be protected
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        private Exception(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _args = (TExceptionArgs)info.GetValue(ArgsName, typeof(TExceptionArgs));
        }

        /// <summary>
        /// Gets the exception arguments.
        /// </summary>
        public TExceptionArgs Args
        {
            get { return _args; }
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        public override string Message
        {
            get
            {
                string baseMsg = base.Message;
                return (_args == null) ? baseMsg : _args.Message;
            }
        }

        /// <summary>
        /// When overridden in a derived class, sets the
        /// <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the
        /// exception.
        /// </summary>
        /// <param name="info">
        /// The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized
        /// object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual
        /// information about the source or destination.
        /// </param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Verify.ArgumentNotNull(info, "info");

            info.AddValue(ArgsName, _args);
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object" /> is equal to the current
        /// <see cref="T:System.Object" />.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        public override bool Equals(object obj)
        {
            Exception<TExceptionArgs> other = obj as Exception<TExceptionArgs>;

            if (other == null)
                return false;

            return Equals(_args, other._args) && ReferenceEquals(this, obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}