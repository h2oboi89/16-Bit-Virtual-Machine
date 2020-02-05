using System.Collections;
using System.Collections.Generic;

namespace VM
{
    class Memory : IEnumerable<byte>
    {
        private readonly byte[] memory;

        public Memory(int sizeInBytes)
        {
            memory = new byte[sizeInBytes];
        }

        #region data type methods (U8, U16)
        public byte GetU8(int address)
        {
            return memory[address];
        }

        public void SetU8(int address, byte value)
        {
            memory[address] = value;
        }

        public ushort GetU16(int address)
        {
            return (ushort)((memory[address] << 8) | memory[address + 1]);
        }

        public void SetU16(int address, ushort value)
        {
            foreach (var b in Utility.GetBytes(value))
            {
                SetU8(address++, b);
            }
        }
        #endregion

        #region Used for flashing initial contents
        public int Address { get; set; }

        public void WriteU8(byte value)
        {
            memory[Address++] = value;
        }

        public void WriteU16(ushort value)
        {
            foreach (var b in Utility.GetBytes(value))
            {
                WriteU8(b);
            }
        }
        #endregion

        #region IEnumerable implementation
        public IEnumerator<byte> GetEnumerator()
        {
            foreach (var value in memory)
            {
                yield return value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return memory.GetEnumerator();
        }
        #endregion
    }
}
