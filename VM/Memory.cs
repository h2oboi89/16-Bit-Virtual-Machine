using System.Collections;
using System.Collections.Generic;

namespace VM
{
    /// <summary>
    /// Represents a block of memory in the Virtual Machine
    /// </summary>
    public class Memory : IEnumerable<byte>
    {
        private readonly byte[] memory;

        /// <summary>
        /// Creates a new block of memory with the specified size.
        /// </summary>
        /// <param name="sizeInBytes">Size of memory in bytes.</param>
        public Memory(int sizeInBytes)
        {
            memory = new byte[sizeInBytes];
        }

        /// <summary>
        /// Highest available writeable address
        /// </summary>
        public int MaxAddress { get { return memory.Length - 1; } }

        #region data type methods (U8, U16)
        /// <summary>
        /// Retrieves a <see cref="byte"/> from memory at the specified location.
        /// </summary>
        /// <param name="address">Address to retrieve from.</param>
        /// <returns>Data at memory address.</returns>
        public byte GetU8(int address)
        {
            return memory[address];
        }

        /// <summary>
        /// Stores a <see cref="byte"/> at the specified location in memory.
        /// </summary>
        /// <param name="address">Address to store at.</param>
        /// <param name="value">Data to store.</param>
        public void SetU8(int address, byte value)
        {
            memory[address] = value;
        }

        /// <summary>
        /// Retrieves a <see cref="ushort"/> from memory at the specified location.
        /// </summary>
        /// <param name="address">Address to retrieve from.</param>
        /// <returns>Data at memory address.</returns>
        public ushort GetU16(int address)
        {
            return (ushort)((memory[address] << 8) | memory[address + 1]);
        }

        /// <summary>
        /// Stores a <see cref="ushort"/> at the specified location in memory.
        /// </summary>
        /// <param name="address">Address to store at.</param>
        /// <param name="value">Data to store.</param>
        public void SetU16(int address, ushort value)
        {
            foreach (var b in Utility.GetBytes(value))
            {
                SetU8(address++, b);
            }
        }
        #endregion

        #region Used for flashing initial contents
        /// <summary>
        /// Address that is currently being written to.
        /// Used by <see cref="WriteU8(byte)"/> and <see cref="WriteU16(ushort)"/>.
        /// Automatically increments each time one of those methods is called.
        /// </summary>
        public int Address { get; set; }

        /// <summary>
        /// Writes the specifed <see cref="byte"/> to current memory location as specifed by <see cref="Address"/>.
        /// </summary>
        /// <param name="value">Value to write to memory.</param>
        public void WriteU8(byte value)
        {
            memory[Address++] = value;
        }

        /// <summary>
        /// Writes the specifed <see cref="ushort"/> to current memory location as specifed by <see cref="Address"/>.
        /// </summary>
        /// <param name="value">Value to write to memory.</param>
        public void WriteU16(ushort value)
        {
            foreach (var b in Utility.GetBytes(value))
            {
                WriteU8(b);
            }
        }
        #endregion

        #region IEnumerable implementation
        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="Memory"/>.
        /// </summary>
        /// <returns>An enumerator for the <see cref="Memory"/></returns>
        public IEnumerator<byte> GetEnumerator()
        {
            foreach (var value in memory)
            {
                yield return value;
            }
        }

        /// <summary>
        /// Returns an <see cref="IEnumerator"/> for the <see cref="Memory"/>
        /// </summary>
        /// <returns>An <see cref="IEnumerator"/> for the <see cref="Memory"/>.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return memory.GetEnumerator();
        }
        #endregion
    }
}
