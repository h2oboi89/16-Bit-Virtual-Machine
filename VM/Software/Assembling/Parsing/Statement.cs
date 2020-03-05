using System.Collections.Generic;

namespace VM.Software.Assembling.Parsing
{
    public abstract class Statement
    {
        public ushort Address { get; }

        public abstract int Size { get; }

        public abstract IEnumerable<byte> GetBytes();

        public IEnumerable<byte> ToBytes() => GetBytes();
    }
}