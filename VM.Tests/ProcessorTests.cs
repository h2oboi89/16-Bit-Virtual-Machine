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

        #region Instructions
        [Test]
        public void Processor_Initializes_WithSpecifiedValues()
        {
            Assert.That(processor.GetRegister(Register.IP), Is.Zero);
            Assert.That(processor.GetRegister(Register.RA), Is.Zero);
            Assert.That(processor.GetRegister(Register.ACC), Is.Zero);
            Assert.That(processor.GetRegister(Register.FLG), Is.Zero);

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
        public void LDV_LoadsValueIntoRegister()
        {
            flasher.WriteInstruction(Instruction.LDV, 0x1234, Register.R1);

            processor.Step();

            Assert.That(processor.GetRegister(Register.IP), Is.EqualTo(4));

            Assert.That(processor.GetRegister(Register.R1), Is.EqualTo(0x1234));
        }

        [Test]
        public void STV_StoresValueIntoMemory()
        {
            flasher.WriteInstruction(Instruction.STV, 0x1234, 0x10);

            Assert.That(memory.GetU16(0x10), Is.Zero);

            processor.Step();

            Assert.That(processor.GetRegister(Register.IP), Is.EqualTo(5));

            Assert.That(memory.GetU16(0x10), Is.EqualTo(0x1234));
        }
        #endregion
    }
}
#pragma warning restore CA1822 // Mark members as static