namespace VM
{
    class Memory
    {
        private readonly byte[] memory;

        public Memory(int sizeInBytes)
        {
            memory = new byte[sizeInBytes];
        }

        public byte GetU8(int i)
        {
            return memory[i];
        }

        public void SetU8(int i, byte value)
        {
            memory[i] = value;
        }

        public ushort GetU16(int i)
        {
            return (ushort)((memory[i] << 8) | memory[i + 1]);
        }

        public void SetU16(int i, ushort value)
        {
            SetU8(i, (byte)((value >> 8) & 0xff));
            SetU8(i + 1, (byte)(value & 0xff));
        }
    }
}
