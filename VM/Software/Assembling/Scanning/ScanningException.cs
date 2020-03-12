using System;
using System.Runtime.Serialization;

namespace VM.Software.Assembling.Scanning
{
    /// <summary>
    /// <see cref="Exception"/> type thrown  by <see cref="Scanner"/>.
    /// </summary>
    [Serializable]
    public sealed class ScanningException : Exception
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ScanningException() { }

        /// <summary>
        /// Creates a new instance with the specified <paramref name="message"/>.
        /// </summary>
        /// <param name="message">Error message.</param>
        public ScanningException(string message) : base(message) { }

        /// <summary>
        /// Creates a new instance with the specified <paramref name="message"/> and <paramref name="innerException"/>.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="innerException">Underlying exception that triggered this one.</param>
        public ScanningException(string message, Exception innerException) : base(message, innerException) { }

        private ScanningException(SerializationInfo serializationInfo, StreamingContext streamingContext) :
            base(serializationInfo, streamingContext)
        { }
    }
}
