using System;
using System.Runtime.Serialization;

namespace VM.Software.Assembling.Parsing
{
    /// <summary>
    /// <see cref="Exception"/> type thrown  by <see cref="Parser"/>.
    /// </summary>
    [Serializable]
    public sealed class ParsingException : Exception
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ParsingException() { }

        /// <summary>
        /// Creates a new instance with the specified <paramref name="message"/>.
        /// </summary>
        /// <param name="message">Error message.</param>
        public ParsingException(string message) : base(message) { }

        /// <summary>
        /// Creates a new instance with the specified <paramref name="message"/> and <paramref name="innerException"/>.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="innerException">Underlying exception that triggered this one.</param>
        public ParsingException(string message, Exception innerException) : base(message, innerException) { }

        private ParsingException(SerializationInfo serializationInfo, StreamingContext streamingContext) : 
            base(serializationInfo, streamingContext) { }
    }
}
