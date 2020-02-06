using System;
using System.Collections.Generic;
using System.Text;

namespace VM
{
    public class Flasher
    {
        private readonly Memory memory;

        public Flasher(Memory memory)
        {
            this.memory = memory;
        }

        private void WriteInstruction(Instruction instruction)
        {
            memory.WriteU8((byte)instruction);
        }

        private void WriteRegister(Register register)
        {
            memory.WriteU8((byte)register);
        }

        public void WriteMemory(Instruction instruction)
        {
            WriteInstruction(instruction);
        }

        public void WriteMemory(Instruction instruction, ushort value)
        {
            WriteInstruction(instruction);
            memory.WriteU16(value);
        }

        public void WriteMemory(Instruction instruction, ushort value, Register register)
        {
            WriteInstruction(instruction);
            memory.WriteU16(value);
            WriteRegister(register);
        }

        public void WriteMemory(Instruction instruction, Register register1, Register register2)
        {
            WriteInstruction(instruction);
            WriteRegister(register1);
            WriteRegister(register2);
        }

        public void WriteMemory(Instruction instruction, Register register, ushort value)
        {
            WriteInstruction(instruction);
            WriteRegister(register);
            memory.WriteU16(value);
        }

        public void WriteMemory(Instruction instruction, ushort value1, ushort value2)
        {
            WriteInstruction(instruction);
            memory.WriteU16(value1);
            memory.WriteU16(value2);
        }
    }
}
