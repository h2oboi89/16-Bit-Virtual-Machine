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

            var flasher = new Flasher(memory);

            // Main Program
            flasher.WriteInstruction(Instruction.PSH_LIT, 0x3333);
            flasher.WriteInstruction(Instruction.PSH_LIT, 0x2222);
            flasher.WriteInstruction(Instruction.PSH_LIT, 0x1111);

            flasher.WriteInstruction(Instruction.MOV_LIT_REG, 0x1234, Register.R1);
            flasher.WriteInstruction(Instruction.MOV_LIT_REG, 0x5678, Register.R2);

            flasher.WriteInstruction(Instruction.PSH_LIT, (ushort)0x0000);

            flasher.WriteInstruction(Instruction.CAL_LIT, subroutineAddress);
            flasher.WriteInstruction(Instruction.PSH_LIT, 0x4444);

            // Subroutine
            memory.Address = subroutineAddress;

            flasher.WriteInstruction(Instruction.PSH_LIT, 0x0102);
            flasher.WriteInstruction(Instruction.PSH_LIT, 0x0304);
            flasher.WriteInstruction(Instruction.PSH_LIT, 0x0506);

            flasher.WriteInstruction(Instruction.MOV_LIT_REG, 0x0708, Register.R1);
            flasher.WriteInstruction(Instruction.MOV_LIT_REG, 0x090a, Register.R8);

            flasher.WriteInstruction(Instruction.RET);
        }
    }
}
