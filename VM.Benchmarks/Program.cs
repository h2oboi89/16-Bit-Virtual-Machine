using BenchmarkDotNet.Running;
using System.Diagnostics;

namespace VM.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<InstructionBenchmarks>();

            //System.Console.WriteLine(summary);

            if (Debugger.IsAttached)
            {
                System.Console.ReadLine();
            }
        }
    }
}
