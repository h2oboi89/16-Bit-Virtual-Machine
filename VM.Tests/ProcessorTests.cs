#pragma warning disable CA1822 // Mark members as static
using NUnit.Framework;

namespace VM.Tests
{
    public class ProcessorTests
    {
        private Memory memory;
        private Processor processor;
        private Flasher flasher;

        [SetUp]
        public void Setup()
        {
            memory = new Memory(0x100);
            flasher = new Flasher(memory);
            processor = new Processor(memory);
        }

        private void AssertPCIsAtEndOfProgram()
        {
            Assert.That(processor.GetRegister(Register.PC), Is.EqualTo(flasher.Address));
        }

        [Test]
        public void Constructor_NullMemory_ThrowsException()
        {
            Assert.That(() => new Processor(null), Throws.ArgumentNullException);
        }

        [Test]
        public void Processor_Initializes_WithSpecifiedValues()
        {
            Assert.That(processor.GetRegister(Register.PC), Is.Zero);
            Assert.That(processor.GetRegister(Register.RA), Is.Zero);
            Assert.That(processor.GetRegister(Register.ACC), Is.Zero);
            Assert.That(processor.GetRegister(Register.FLAG), Is.Zero);

            Assert.That(processor.GetRegister(Register.SP), Is.EqualTo(memory.MaxAddress - Processor.DATASIZE));
            Assert.That(processor.GetRegister(Register.FP), Is.EqualTo(memory.MaxAddress - Processor.DATASIZE));

            Assert.That(processor.GetRegister(Register.R1), Is.Zero);
            Assert.That(processor.GetRegister(Register.R2), Is.Zero);
            Assert.That(processor.GetRegister(Register.R3), Is.Zero);
            Assert.That(processor.GetRegister(Register.R4), Is.Zero);
            Assert.That(processor.GetRegister(Register.R5), Is.Zero);
            Assert.That(processor.GetRegister(Register.R6), Is.Zero);
            Assert.That(processor.GetRegister(Register.R7), Is.Zero);
            Assert.That(processor.GetRegister(Register.R8), Is.Zero);

            Assert.That(processor.GetRegister(Register.T1), Is.Zero);
            Assert.That(processor.GetRegister(Register.T2), Is.Zero);
            Assert.That(processor.GetRegister(Register.T3), Is.Zero);
            Assert.That(processor.GetRegister(Register.T4), Is.Zero);
            Assert.That(processor.GetRegister(Register.T5), Is.Zero);
            Assert.That(processor.GetRegister(Register.T6), Is.Zero);
            Assert.That(processor.GetRegister(Register.T7), Is.Zero);
            Assert.That(processor.GetRegister(Register.T8), Is.Zero);
        }

        [Test]
        public void ModifyingPrivateRegistersDirectly_ThrowsExceptionAndResetsProcessor()
        {
            var privateRegisters = new Register[]
            {
                Register.PC, Register.RA, Register.ACC, Register.FLAG, Register.SP, Register.FP
            };

            foreach (var register in privateRegisters)
            {
                flasher.Address = 0;

                flasher.WriteInstruction(Instruction.LDV, 0x1234, register);

                Assert.That(() => processor.Step(), Throws.InvalidOperationException
                        .With.Message.EqualTo($"{register} cannot be modified directly by code."));

                Assert.That(processor.GetRegister(Register.PC), Is.Zero);
            }
        }

        #region Instructions
        [Test]
        public void NOP_DoesNothing()
        {
            for (var i = 0; i < 10; i++)
            {
                processor.Step();

                Assert.That(processor.GetRegister(Register.PC), Is.EqualTo(i + 1));
            }
        }

        [Test]
        public void MOVE_MovesValueFromRegisterAToRegisterB()
        {
            flasher.WriteInstruction(Instruction.LDV, 0x1234, Register.R1);
            flasher.WriteInstruction(Instruction.MOVE, Register.R1, Register.R2);

            processor.Step();
            processor.Step();

            AssertPCIsAtEndOfProgram();

            Assert.That(processor.GetRegister(Register.R2), Is.EqualTo(0x1234));
        }

        [Test]
        public void INC_IncrementsRegisterValue()
        {
            flasher.WriteInstruction(Instruction.LDV, 0x1234, Register.R1);
            flasher.WriteInstruction(Instruction.INC, Register.R1);

            processor.Step();
            processor.Step();

            AssertPCIsAtEndOfProgram();

            Assert.That(processor.GetRegister(Register.R1), Is.EqualTo(0x1235));
            Assert.That(processor.GetFlag(Flag.OVERFLOW), Is.False);
        }

        [Test]
        public void INC_Overflow_WrapsAround()
        {
            flasher.WriteInstruction(Instruction.LDV, ushort.MaxValue, Register.R1);
            flasher.WriteInstruction(Instruction.INC, Register.R1);

            processor.Step();
            processor.Step();

            AssertPCIsAtEndOfProgram();

            Assert.That(processor.GetRegister(Register.R1), Is.EqualTo(ushort.MinValue));
            Assert.That(processor.GetFlag(Flag.OVERFLOW), Is.True);
        }

        [Test]
        public void DEC_decrementsRegisterValue()
        {
            flasher.WriteInstruction(Instruction.LDV, 0x1234, Register.R1);
            flasher.WriteInstruction(Instruction.DEC, Register.R1);

            processor.Step();
            processor.Step();

            AssertPCIsAtEndOfProgram();

            Assert.That(processor.GetRegister(Register.R1), Is.EqualTo(0x1233));
            Assert.That(processor.GetFlag(Flag.UNDERFLOW), Is.False);
        }

        [Test]
        public void DEC_Underflow_WrapsAround()
        {
            flasher.WriteInstruction(Instruction.LDV, ushort.MinValue, Register.R1);
            flasher.WriteInstruction(Instruction.DEC, Register.R1);

            processor.Step();
            processor.Step();

            AssertPCIsAtEndOfProgram();

            Assert.That(processor.GetRegister(Register.R1), Is.EqualTo(ushort.MaxValue));
            Assert.That(processor.GetFlag(Flag.UNDERFLOW), Is.True);
        }

        [Test]
        public void LDV_LoadsValueIntoRegister()
        {
            flasher.WriteInstruction(Instruction.LDV, 0x1234, Register.R1);

            processor.Step();

            AssertPCIsAtEndOfProgram();

            Assert.That(processor.GetRegister(Register.R1), Is.EqualTo(0x1234));
        }

        [Test]
        public void STV_StoresValueIntoMemory()
        {
            flasher.WriteInstruction(Instruction.STV, 0x1234, 0x10);

            Assert.That(memory.GetU16(0x10), Is.Zero);

            processor.Step();

            AssertPCIsAtEndOfProgram();

            Assert.That(memory.GetU16(0x10), Is.EqualTo(0x1234));
        }
        #endregion
    }
}
#pragma warning restore CA1822 // Mark members as static