using System;
using System.Diagnostics;
using System.IO;
using VM.Hardware;
using VM.Hardware.IO;
using VM.Software.Assembling;

namespace VM.Terminal
{
    class Program
    {
        const ushort CONSOLE_ADDRESS = 0xf000;
        const byte WIDTH = 80;
        const byte HEIGHT = 25;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                System.Console.WriteLine("Require program to run");
                Environment.Exit(1);
            }

            var file = args[0];

            var programFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file);

            string source = File.ReadAllText(programFile);

            var binary = Assembler.Assemble(source);

            var memory = new Memory(0x10000);
            var console = new SystemConsole(memory, CONSOLE_ADDRESS, WIDTH, HEIGHT);
            var processor = new Processor(memory, 0x800);
            var flasher = new Flasher(memory);

            flasher.Flash(binary);

            var instructions = 0;

            processor.Tick += (o, e) =>
            {
                instructions++;
            };
            processor.Halt += (o, e) => console.Close();

            var start = DateTime.Now;
            processor.Run();
            var end = DateTime.Now;

            var duration = end - start;

            var instructionsPerSecond = instructions / (duration.TotalMilliseconds / 1000.0);

            System.Console.WriteLine($"Ran {instructions} instructions in {duration}");
            System.Console.WriteLine($"{instructionsPerSecond} instructions per second");

            if (Debugger.IsAttached)
            {
                System.Console.ReadLine();
            }
        }
    }
}
