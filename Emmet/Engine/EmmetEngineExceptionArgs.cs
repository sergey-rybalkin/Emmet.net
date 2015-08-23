using Emmet.Diagnostics;
using V8.Net;

namespace Emmet.Engine
{
    /// <summary>
    /// Contains additional information about JavaScript exceptions.
    /// </summary>
    public class EmmetEngineExceptionArgs : ExceptionArgs
    {
        private readonly string _error;

        private readonly string _message;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmmetEngineExceptionArgs"/> class.
        /// </summary>
        /// <param name="compilationError">The compilation error.</param>
        public EmmetEngineExceptionArgs(string message, Handle error)
        {
            _message = message;
            _error = error.AsString;
        }

        /// <summary>
        /// Gets the error message specific to the exception.
        /// </summary>
        public override string Message
        {
            get { return string.Format("{0}: {1}", _message, _error); }
        }
    }
}