using System;

namespace Emmet.Diagnostics
{
    /// <summary>
    /// Generic exception class arguments container.
    /// </summary>
    [Serializable]
    public class ExceptionArgs
    {
        /// <summary>
        /// Gets the error message specific to the exception.
        /// </summary>
        public virtual string Message
        {
            get { return string.Empty; }
        }
    }
}