namespace VM
{
    public class Flasher
    {
        private readonly Memory memory;

        public Flasher(Memory memory)
        {
            this.memory = memory;
        }

        public int Address
        {
            get { return memory.Address; }
            set { memory.Address = value; }
        }

        public void WriteU8(byte value)
        {
            memory.WriteU8(value);
        }

        public void WriteU16(ushort value)
        {
            memory.WriteU16(value);
        }

        private void WriteRegister(Register register)
        {
            memory.WriteU8((byte)register);
        }

        public void WriteInstruction(Instruction instruction)
        {
            WriteU8((byte)instruction);
        }

        public void WriteMemory(Instruction instruction, ushort value)
        {
            WriteInstruction(instruction);
            memory.WriteU16(value);
        }

        public void WriteInstruction(Instruction instruction, ushort value, Register register)
        {
            WriteInstruction(instruction);
            memory.WriteU16(value);
            WriteRegister(register);
        }

        public void WriteInstruction(Instruction instruction, Register register1, Register register2)
        {
            WriteInstruction(instruction);
            WriteRegister(register1);
            WriteRegister(register2);
        }

        public void WriteInstruction(Instruction instruction, Register register, ushort value)
        {
            WriteInstruction(instruction);
            WriteRegister(register);
            memory.WriteU16(value);
        }

        public void WriteInstruction(Instruction instruction, ushort value1, ushort value2)
        {
            WriteInstruction(instruction);
            memory.WriteU16(value1);
            memory.WriteU16(value2);
        }
    }
}
