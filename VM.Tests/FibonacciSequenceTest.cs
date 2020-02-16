using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace VM.Tests
{
    [TestFixture]
    public class FibonacciSequenceTest
    {
        [Test, Timeout(2000)]
        public void TestMethod()
        {
            var memory = new Memory(0x100);
            var processor = new Processor(memory);
            var flasher = new Flasher(memory);

            var sequence = new List<ushort> { 0, 1 };

            var resetEvent = new AutoResetEvent(false);

            processor.Reset += (o, e) =>
            {
                Assert.That(e.Instruction, Is.EqualTo(Instruction.RESET));
                Assert.That(e.Exception, Is.Null);

                resetEvent.Set();
            };

            var loopAddress = (ushort)12;
            var endAddress = (ushort)31;

            flasher.WriteInstruction(Instruction.LDVR, (ushort)0, Register.R1);
            flasher.WriteInstruction(Instruction.LDVR, 1, Register.R2);
            flasher.WriteInstruction(Instruction.LDVR, 1 << (ushort)Flag.CARRY, Register.R3);

            // LOOP Address
            flasher.WriteInstruction(Instruction.AND, Register.FLAG, Register.R3);
            flasher.WriteInstruction(Instruction.JNZ, endAddress);

            flasher.WriteInstruction(Instruction.ADD, Register.R1, Register.R2);
            flasher.WriteInstruction(Instruction.HALT);
            flasher.WriteInstruction(Instruction.MOVE, Register.R2, Register.R1);
            flasher.WriteInstruction(Instruction.MOVE, Register.ACC, Register.R2);
            flasher.WriteInstruction(Instruction.JUMP, loopAddress);

            // END Address
            flasher.WriteInstruction(Instruction.RESET);

            // FUTURE: replace HALT with a write to IO
            processor.Halt += (o, e) =>
            {
                sequence.Add(processor.GetRegister(Register.ACC));

                Task.Run(() => processor.Run());
            };

            processor.Run();

            // wait for reset at end
            resetEvent.WaitOne(1000);

            Assert.That(sequence.Count(), Is.EqualTo(26));

            Assert.That(sequence, Is.EquivalentTo(new ushort[]
            {
                0, 1, 1, 2, 3,
                5, 8, 13, 21, 34,
                55, 89, 144, 233, 377,
                610, 987, 1597, 2584, 4181,
                6765, 10946, 17711, 28657, 46368,
                // next value is 75025 which is > ushort max (65535)
                // 75025 & 0xffff results in value below
                9489
            }));
        }
    }
}
