using System;
using VM.Software.Assembling.Scanning;

namespace VM.Software.Assembling.Parsing
{
    [Serializable]
    public sealed class ParsingException : Exception
    {
        public ParsingException() { }

        public ParsingException(string message) : base(message) { }

        public ParsingException(string message, Exception innerException) : base(message, innerException) { }
    }
}
