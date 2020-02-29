using System;

namespace VM.Hardware
{
    /// <summary>
    /// Event arguments for <see cref="Memory.MemoryWrite"/>.
    /// </summary>
    public sealed class MemoryWriteEventArgs : EventArgs
    {
        /// <summary>
        /// Address being written to.
        /// </summary>
        public ushort Address { get; private set; }

        /// <summary>
        /// Value being written.
        /// </summary>
        public byte Value { get; private set; }

        /// <summary>
        /// Creates a new instance with the specified parameters.
        /// </summary>
        /// <param name="address">Address being written to.</param>
        /// <param name="value">Value being written.</param>
        public MemoryWriteEventArgs(ushort address, byte value)
        {
            Address = address;
            Value = value;
        }
    }
}
