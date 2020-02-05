using System;

namespace VM
{
    class Program
    {
        static Memory memory;
        static Processor processor;

        static void Main(string[] args)
        {
            Console.CancelKeyPress += (o, e) => Environment.Exit(1);

            memory = new Memory(256 * 256);
            processor = new Processor(memory);

            // Program
            WriteInstruction(Instruction.MOV_MEM_REG, 0x0100, Register.R1);
            WriteInstruction(Instruction.MOV_LIT_REG, 0x0001, Register.R2);
            WriteInstruction(Instruction.ADD_REG_REG, Register.R1, Register.R2);
            WriteInstruction(Instruction.MOV_REG_MEM, Register.ACC, 0x0100);
            WriteInstruction(Instruction.JMP_NOT_EQ, 0x0003, (ushort)0x0000);

            // print initial state
            processor.Debug();
            processor.ViewMemoryAt(processor.GetRegister(Register.IP));
            processor.ViewMemoryAt(0x0100);

            while (true)
            {
                Console.ReadLine();

                // execute and print state
                processor.Step();

                processor.Debug();
                processor.ViewMemoryAt(processor.GetRegister(Register.IP));
                processor.ViewMemoryAt(0x0100);
            }
        }

        static void WriteInstruction(Instruction instruction, ushort value, Register register)
        {
            memory.WriteU8((byte)instruction);
            memory.WriteU16(value);
            memory.WriteU8((byte)register);
        }

        static void WriteInstruction(Instruction instruction, Register register1, Register register2)
        {
            memory.WriteU8((byte)instruction);
            memory.WriteU8((byte)register1);
            memory.WriteU8((byte)register2);
        }

        static void WriteInstruction(Instruction instruction, Register register, ushort value)
        {
            memory.WriteU8((byte)instruction);
            memory.WriteU8((byte)register);
            memory.WriteU16(value);
        }

        static void WriteInstruction(Instruction instruction, ushort value1, ushort value2)
        {
            memory.WriteU8((byte)instruction);
            memory.WriteU16(value1);
            memory.WriteU16(value2);
        }
    }
}
