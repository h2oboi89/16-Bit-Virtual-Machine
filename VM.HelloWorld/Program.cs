using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VM.HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            ushort consoleAddress = 0xf000;

            var memory = new Memory(0x10000);
            var console = new Console(consoleAddress, 80, 25);

            memory.MemoryWrite += (o, e) =>
            {
                console.Write(e.Address, e.Value);
            };

            var processor = new Processor(memory, 0x800);

            var flasher = new Flasher(memory);

            var bytes = Encoding.ASCII.GetBytes("Hello, World!\0");

            var values = new ushort[bytes.Length / 2];

            for (var i = 0; i < bytes.Length - 1; i += 2)
            {
                values[i / 2] = (ushort)(bytes[i] << 8 | bytes[i + 1]);
            }

            var address = consoleAddress;

            foreach (var value in values)
            {
                flasher.WriteInstruction(Instruction.STVA, value, address);
                address += 2;
            }

            flasher.WriteInstruction(Instruction.HALT);

            var start = DateTime.Now;
            processor.Run();
            var end = DateTime.Now;

            var duration = end - start;

            var instructionsPerSecond = flasher.InstructionCount / (duration.TotalMilliseconds / 1000.0);

            System.Console.SetCursorPosition(0, 20);

            System.Console.WriteLine($"Ran {flasher.InstructionCount} instructions in {duration} ({instructionsPerSecond} instructions per second)");

            if (Debugger.IsAttached)
            {
                System.Console.ReadLine();
            }
        }
    }
}
