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
            memory = new Memory(256);
            flasher = new Flasher(memory);
            processor = new Processor(memory);
        }

        private void AssertThatIpRegisterIsEqualTo(ushort value)
        {
            Assert.That(processor.GetRegister(Register.IP), Is.EqualTo(value));
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

            Assert.That(processor.GetRegister(Register.SP), Is.EqualTo(memory.MaxAddress - Processor.DATA_SIZE));
            Assert.That(processor.GetRegister(Register.FP), Is.EqualTo(memory.MaxAddress - Processor.DATA_SIZE));
        }
    }
}
#pragma warning restore CA1822 // Mark members as static