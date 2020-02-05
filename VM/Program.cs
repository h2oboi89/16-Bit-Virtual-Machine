using System;
using System.Diagnostics;

namespace VM
{
    class Program
    {
        static void Main(string[] args)
        {
            var memory = new Memory(256);

            var processor = new Processor(memory);

            memory.SetU8(0, (byte)Instruction.MOV_LIT_R1);
            memory.SetU16(1, 0x1234);

            memory.SetU8(3, (byte)Instruction.MOV_LIT_R2);
            memory.SetU16(4, 0xabcd);

            memory.SetU8(6, (byte)Instruction.ADD_REG_REG);
            memory.SetU8(7, (byte)Register.R1);
            memory.SetU8(8, (byte)Register.R2);

            // print initial state
            processor.Debug();

            for (var i = 0; i < 3; i++)
            {
                // execute and print state
                processor.Step();

                processor.Debug();
            }

            if (Debugger.IsAttached)
            {
                Console.ReadLine();
            }
        }
    }
}
