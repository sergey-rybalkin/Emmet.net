namespace Emmet.Engine
{
    /// <summary>
    /// Represents a range of text and mimics Emmet engine range objects.
    /// </summary>
    public struct Range
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Range"/> struct.
        /// </summary>
        /// <param name="start">The start position of the range.</param>
        /// <param name="end">The end position of the range.</param>
        public Range(int start, int end)
        {
            Start = start;
            End = end;
        }

        /// <summary>
        /// Gets or sets the start position of the range.
        /// </summary>
        public int Start { get; private set; }

        /// <summary>
        /// Gets or sets the end position of the range.
        /// </summary>
        public int End { get; private set; }
    }
}