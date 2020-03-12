using NUnit.Framework;
using System;
using System.Threading;
using VM.Hardware;
using VM.Software.Assembling;

namespace VM.Tests
{
    [TestFixture]
    public class FibonacciSequenceTest
    {
        [Test, Timeout(2000)]
        public void AllU16FibonacciNumbers()
        {
            var memory = new Memory(0x100);
            var processor = new Processor(memory, 0x10);
            var flasher = new Flasher(memory);

            const ushort SEQUENCEADDRESS = 0x00a0;

            var source = new string[]
            {
                "START:",
                    // initialize register values
                    "LDVR 0 $R0",
                    "LDVR 1 $R1",
                    "LDVR 0x10 $R2",
                   $"LDVR {SEQUENCEADDRESS} $R3",
                    "LDVR 2 $R4",

                    // Save R0 and R1 value to memory
                    "STRR $R0 $R3",
                    "CALL 0 INCREMENT",
                    "POP $T1", // return value count (0)

                    "STRR $R1 $R3",
                    "CALL 0 INCREMENT",
                    "POP $T1", // return value count (0)

                "LOOP:",
                    // Check for overflow
                    "AND $T0 $R2",
                    "JNZ END",

                    // calculate next Fibonacci number (R0 + R1 -> ACC)
                    "ADD $R0 $R1",
                
                    // Save Flag register for check later
                    "MOVE $FLAG $T0",

                    // Write Fibonacci number to memory
                    "STRR $ACC $R3",

                    // Shift all values, R1 -> R0, ACC -> R1
                    "MOVE $R1 $R0",
                    "MOVE $ACC $R1",

                    // Increment address to save fibonnaci values at
                    "CALL 0 INCREMENT",
                    "POP $T1", // return value count (0)

                    "JUMP LOOP",

                "END:",
                    "HALT",

                "INCREMENT:",
                    "ADD $R3 $R4",
                    "MOVE $ACC $R3",
                    "RET 0"
            };

            var binary = Assembler.Assemble(string.Join(Environment.NewLine, source));

            flasher.Flash(binary);

            processor.Run();

            var expected = new ushort[]
            {
                0, 1, 1, 2, 3,
                5, 8, 13, 21, 34,
                55, 89, 144, 233, 377,
                610, 987, 1597, 2584, 4181,
                6765, 10946, 17711, 28657, 46368,
                // next value is 75025 which is > ushort max (65535)
                9489 // (75025 & 0xffff) => 9489
            };

            for (var i = 0; i < expected.Length; i++)
            {
                ushort address = (ushort)(SEQUENCEADDRESS + (2 * i));
                var value = expected[i];
                Assert.That(memory.GetU16(address), Is.EqualTo(value));
            }
        }
    }
}
