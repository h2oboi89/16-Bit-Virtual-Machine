using System;
using System.Runtime.Serialization;

namespace VM.Software.Assembling
{
    /// <summary>
    /// <see cref="Exception"/> type thrown  by <see cref="Assembler"/>.
    /// </summary>
    [Serializable]
    public sealed class AssemblingException : Exception
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public AssemblingException() { }

        /// <summary>
        /// Creates a new instance with the specified <paramref name="message"/>.
        /// </summary>
        /// <param name="message">Error message.</param>
        public AssemblingException(string message) : base(message) { }

        /// <summary>
        /// Creates a new instance with the specified <paramref name="message"/> and <paramref name="innerException"/>.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="innerException">Underlying exception that triggered this one.</param>
        public AssemblingException(string message, Exception innerException) : base(message, innerException) { }

        private AssemblingException(SerializationInfo serializationInfo, StreamingContext streamingContext) :
            base(serializationInfo, streamingContext)
        { }
    }
}
