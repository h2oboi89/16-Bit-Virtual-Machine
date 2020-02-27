using BenchmarkDotNet.Attributes;

namespace VM.Benchmarks
{
    public class InstructionBenchmarks
    {
        [Params(10, 100, 1000)]
        public int N;

        const int COUNT = 1000;
        //const ushort consoleAddress = 0xf000;

        private Memory memory;
        //Console console;
        private Processor processor;
        private Flasher flasher;

        [GlobalSetup]
        public void GlobalSetup()
        {
            memory = new Memory(0x10000);
            //console = new Console(consoleAddress, 80, 25);
            processor = new Processor(memory, 0x800);
            flasher = new Flasher(memory);

            for (var i = 0; i < COUNT; i++)
            {
                flasher.WriteInstruction(Instruction.NOP);
            }
            flasher.WriteInstruction(Instruction.HALT);
        }

        [IterationCleanup]
        public void IterationCleanup()
        {
            processor.Initialize();
        }

        [Benchmark]
        public ushort Nop()
        {
            processor.Run();

            return processor.GetRegister(Register.PC);
        }
    }
}
