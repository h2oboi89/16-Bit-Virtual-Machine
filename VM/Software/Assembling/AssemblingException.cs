using System;

namespace VM.Software.Assembling
{
    public class AssemblingException : Exception
    {
        public AssemblingException() { }

        public AssemblingException(string message) : base(message) { }

        public AssemblingException(string message, Exception innerException) : base(message, innerException) { }
    }
}
