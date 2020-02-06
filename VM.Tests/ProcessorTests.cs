using NUnit.Framework;
using System.Linq;

namespace VM.Tests
{
    public class Tests
    {
        private Memory memory;
        private Processor processor;
        private Flasher flasher;

        [SetUp]
        public void Setup()
        {
            memory = new Memory(256);
            flasher = new Flasher(memory);
            processor = new Processor(memory);
        }

        [Test]
        public void Processor_Initializes_WithSpecifiedValues()
        {
            Assert.That(processor.GetRegister(Register.IP), Is.Zero);
            Assert.That(processor.GetRegister(Register.ACC), Is.Zero);

            Assert.That(processor.GetRegister(Register.R1), Is.Zero);
            Assert.That(processor.GetRegister(Register.R2), Is.Zero);
            Assert.That(processor.GetRegister(Register.R3), Is.Zero);
            Assert.That(processor.GetRegister(Register.R4), Is.Zero);
            Assert.That(processor.GetRegister(Register.R5), Is.Zero);
            Assert.That(processor.GetRegister(Register.R6), Is.Zero);
            Assert.That(processor.GetRegister(Register.R7), Is.Zero);
            Assert.That(processor.GetRegister(Register.R8), Is.Zero);

            Assert.That(processor.GetRegister(Register.SP), Is.EqualTo(memory.Count() - 2));
            Assert.That(processor.GetRegister(Register.FP), Is.EqualTo(memory.Count() - 2));
        }

        [Test]
        public void MoveLiteralToRegister_LoadsLiteralToRegister()
        {
            flasher.WriteMemory(Instruction.MOV_LIT_REG, 0x1234, Register.R1);

            processor.Step();

            Assert.That(processor.GetRegister(Register.IP), Is.EqualTo(4));
            Assert.That(processor.GetRegister(Register.R1), Is.EqualTo(0x1234));
        }

        [Test]
        public void MoveRegisterToRegister_MovesValueBetweenRegisters()
        {
            flasher.WriteMemory(Instruction.MOV_LIT_REG, 0x1234, Register.R1);
            flasher.WriteMemory(Instruction.MOV_REG_REG, Register.R1, Register.R2);

            processor.Step();
            processor.Step();

            Assert.That(processor.GetRegister(Register.IP), Is.EqualTo(7));
            Assert.That(processor.GetRegister(Register.R2), Is.EqualTo(0x1234));
        }
    }
}