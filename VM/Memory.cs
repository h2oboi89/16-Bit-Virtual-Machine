using System;

namespace VM
{
    /// <summary>
    /// Represents a block of memory in the Virtual Machine
    /// </summary>
    public sealed class Memory
    {
        private readonly byte[] memory;

        /// <summary>
        /// Creates a new block of memory with the specified size.
        /// </summary>
        /// <param name="sizeInBytes">Size of memory in bytes.</param>
        public Memory(uint sizeInBytes)
        {
            if (sizeInBytes > ushort.MaxValue + 1)
            {
                throw new ArgumentOutOfRangeException(nameof(sizeInBytes), "Must be in range [0x0000, 0x10000]");
            }

            memory = new byte[sizeInBytes];
        }

        public delegate void MemoryWriteEventHandler(object sender, MemoryWriteEventArgs e);

        public event MemoryWriteEventHandler MemoryWrite;

        /// <summary>
        /// Highest available writeable address
        /// </summary>
        public ushort MaxAddress { get { return (ushort)(memory.Length - 1); } }

        private bool ValidAddress(ushort address, int size) => address + size <= memory.Length;

        private IndexOutOfRangeException MemoryException(ushort address)
        {
            return new IndexOutOfRangeException(
                $"Invalid memory address {Utility.FormatU16(address)}. " +
                $"Valid Range: [{Utility.FormatU16(0)}, {Utility.FormatU16(MaxAddress)}]"
                );
        }

        /// <summary>
        /// Erases all memory contents back to default (zero).
        /// </summary>
        public void Reset()
        {
            Array.Clear(memory, 0, memory.Length);
        }

        /// <summary>
        /// Retrieves a <see cref="byte"/> from memory at the specified location.
        /// </summary>
        /// <param name="address">Address to retrieve from.</param>
        /// <returns>Data at memory address.</returns>
        public byte GetU8(ushort address)
        {
            if (!ValidAddress(address, sizeof(byte)))
            {
                throw MemoryException(address);
            }

            return memory[address];
        }

        /// <summary>
        /// Stores a <see cref="byte"/> at the specified location in memory.
        /// </summary>
        /// <param name="address">Address to store at.</param>
        /// <param name="value">Data to store.</param>
        public void SetU8(ushort address, byte value)
        {
            if (!ValidAddress(address, sizeof(byte)))
            {
                throw MemoryException(address);
            }

            memory[address] = value;

            MemoryWrite?.Invoke(this, new MemoryWriteEventArgs(address, value));
        }

        /// <summary>
        /// Retrieves a <see cref="ushort"/> from memory at the specified location.
        /// </summary>
        /// <param name="address">Address to retrieve from.</param>
        /// <returns>Data at memory address.</returns>
        public ushort GetU16(ushort address)
        {
            if (!ValidAddress(address, sizeof(ushort)))
            {
                throw MemoryException(address);
            }

            return (ushort)((memory[address] << 8) | memory[address + 1]);
        }

        /// <summary>
        /// Stores a <see cref="ushort"/> at the specified location in memory.
        /// </summary>
        /// <param name="address">Address to store at.</param>
        /// <param name="value">Data to store.</param>
        public void SetU16(ushort address, ushort value)
        {
            if (!ValidAddress(address, sizeof(ushort)))
            {
                throw MemoryException(address);
            }

            foreach (var b in Utility.GetBytes(value))
            {
                SetU8(address++, b);
            }
        }
    }
}
