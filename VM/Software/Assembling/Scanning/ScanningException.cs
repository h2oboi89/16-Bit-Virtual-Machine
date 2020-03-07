using System;

namespace VM.Software.Assembling.Scanning
{
    public sealed class ScanningException : Exception
    {
        public ScanningException() { }

        public ScanningException(string message) : base(message) { }

        public ScanningException(string message, Exception innerException) : base(message, innerException) { }
    }
}
