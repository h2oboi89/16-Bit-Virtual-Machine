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

            LoadProgram();

            // Execution

            var watchAddress = (ushort)(0xffff - 1 - 42);
            var watchLength = 44;

            // print initial state
            processor.Debug();
            processor.ViewMemoryAt(processor.GetRegister(Register.IP));
            processor.ViewMemoryAt(watchAddress, watchLength);

            while (true)
            {
                Console.ReadLine();

                // execute and print state
                processor.Step();

                processor.Debug();
                processor.ViewMemoryAt(processor.GetRegister(Register.IP));
                processor.ViewMemoryAt(watchAddress, watchLength);
            }
        }

        static void LoadProgram()
        {
            ushort subroutineAddress = 0x3000;

            // Main Program
            WriteMemory(Instruction.PSH_LIT, 0x3333);
            WriteMemory(Instruction.PSH_LIT, 0x2222);
            WriteMemory(Instruction.PSH_LIT, 0x1111);

            WriteMemory(Instruction.MOV_LIT_REG, 0x1234, Register.R1);
            WriteMemory(Instruction.MOV_LIT_REG, 0x5678, Register.R2);

            WriteMemory(Instruction.PSH_LIT, 0x0000);

            WriteMemory(Instruction.CAL_LIT, subroutineAddress);
            WriteMemory(Instruction.PSH_LIT, 0x4444);

            // Subroutine
            memory.Address = subroutineAddress;

            WriteMemory(Instruction.PSH_LIT, 0x0102);
            WriteMemory(Instruction.PSH_LIT, 0x0304);
            WriteMemory(Instruction.PSH_LIT, 0x0506);

            WriteMemory(Instruction.MOV_LIT_REG, 0x0708, Register.R1);
            WriteMemory(Instruction.MOV_LIT_REG, 0x090a, Register.R8);

            WriteMemory(Instruction.RET);
        }

        static void WriteInstruction(Instruction instruction)
        {
            memory.WriteU8((byte)instruction);
        }

        static void WriteRegister(Register register)
        {
            memory.WriteU8((byte)register);
        }

        static void WriteMemory(Instruction instruction)
        {
            WriteInstruction(instruction);
        }

        static void WriteMemory(Instruction instruction, ushort value)
        {
            WriteInstruction(instruction);
            memory.WriteU16(value);
        }

        static void WriteMemory(Instruction instruction, ushort value, Register register)
        {
            WriteInstruction(instruction);
            memory.WriteU16(value);
            WriteRegister(register);
        }

        static void WriteMemory(Instruction instruction, Register register1, Register register2)
        {
            WriteInstruction(instruction);
            WriteRegister(register1);
            WriteRegister(register2);
        }

        static void WriteMemory(Instruction instruction, Register register, ushort value)
        {
            WriteInstruction(instruction);
            WriteRegister(register);
            memory.WriteU16(value);
        }

        static void WriteMemory(Instruction instruction, ushort value1, ushort value2)
        {
            WriteInstruction(instruction);
            memory.WriteU16(value1);
            memory.WriteU16(value2);
        }
    }
}
