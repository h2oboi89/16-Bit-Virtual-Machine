using NUnit.Framework;
using System.Threading;

namespace VM.Tests
{
    [TestFixture]
    public class FibonacciSequenceTest
    {
        [Test, Timeout(2000)]
        public void TestMethod()
        {
            var memory = new Memory(0x100);
            var processor = new Processor(memory, 0x10);
            var flasher = new Flasher(memory);

            const ushort SEQUENCEADDRESS = 0x00a0;

            var loopAddress = (ushort)38;
            var endAddress = (ushort)68;

            flasher.WriteInstruction(Instruction.LDVR, (ushort)0, Register.R1); // 0
            flasher.WriteInstruction(Instruction.LDVR, (ushort)1, Register.R2); // 4
            flasher.WriteInstruction(Instruction.LDVR, (ushort)Flags.CARRY, Register.R3); // 8
            flasher.WriteInstruction(Instruction.LDVR, SEQUENCEADDRESS, Register.R4); // 12
            flasher.WriteInstruction(Instruction.LDVR, (ushort)2, Register.R5); // 16

            // Save R1 and R2 values to memory
            flasher.WriteInstruction(Instruction.STRR, Register.R1, Register.R4); // 20
            flasher.WriteInstruction(Instruction.ADD, Register.R4, Register.R5); // 23
            flasher.WriteInstruction(Instruction.MOVE, Register.ACC, Register.R4); // 26

            flasher.WriteInstruction(Instruction.STRR, Register.R2, Register.R4); // 29
            flasher.WriteInstruction(Instruction.ADD, Register.R4, Register.R5); // 32
            flasher.WriteInstruction(Instruction.MOVE, Register.ACC, Register.R4); // 35

            // LOOP Address
            // Check if overflow happened
            flasher.WriteInstruction(Instruction.AND, Register.T0, Register.R3); // 38
            flasher.WriteInstruction(Instruction.JNZ, endAddress); // 41

            // calculate next Fibonacci number (R1 + R2 -> ACC)
            flasher.WriteInstruction(Instruction.ADD, Register.R1, Register.R2); // 44

            // Save Flag register for check later
            flasher.WriteInstruction(Instruction.MOVE, Register.FLAG, Register.T0); // 47

            // Write Fibonacci number to memory
            flasher.WriteInstruction(Instruction.STRR, Register.ACC, Register.R4); // 50

            // Shift all values, R2 -> R1, ACC -> R2
            flasher.WriteInstruction(Instruction.MOVE, Register.R2, Register.R1); // 53
            flasher.WriteInstruction(Instruction.MOVE, Register.ACC, Register.R2); // 56

            // Increment address to save fibonnaci values at
            flasher.WriteInstruction(Instruction.ADD, Register.R4, Register.R5); // 59
            flasher.WriteInstruction(Instruction.MOVE, Register.ACC, Register.R4); // 62

            // Go to start of loop
            flasher.WriteInstruction(Instruction.JUMP, loopAddress); // 65

            // END Address
            flasher.WriteInstruction(Instruction.RESET); // 68

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
