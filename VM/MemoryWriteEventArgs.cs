using System;

namespace VM
{
    public sealed class MemoryWriteEventArgs : EventArgs
    {
        public ushort Address { get; private set; }
        public byte Value { get; private set; }

        public MemoryWriteEventArgs(ushort address, byte value)
        {
            Address = address;
            Value = value;
        }
    }
}
