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

        [Test]
        public void MoveLiteralToRegister_LoadsLiteralToRegister()
        {
            flasher.WriteInstruction(Instruction.MOV_LIT_REG, 0x1234, Register.R1);

            processor.Step();

            Assert.That(processor.GetRegister(Register.IP), Is.EqualTo(4));
            Assert.That(processor.GetRegister(Register.R1), Is.EqualTo(0x1234));
        }

        [Test]
        public void MoveRegisterToRegister_MovesValueBetweenRegisters()
        {
            flasher.WriteInstruction(Instruction.MOV_LIT_REG, 0x1234, Register.R1);
            flasher.WriteInstruction(Instruction.MOV_REG_REG, Register.R1, Register.R2);

            processor.Step();
            processor.Step();

            Assert.That(processor.GetRegister(Register.IP), Is.EqualTo(7));
            Assert.That(processor.GetRegister(Register.R2), Is.EqualTo(0x1234));
        }

        [Test]
        public void MoveRegisterToMemory_MovesValueToMemory()
        {
            flasher.WriteInstruction(Instruction.MOV_LIT_REG, 0x1234, Register.R1);
            flasher.WriteInstruction(Instruction.MOV_REG_MEM, Register.R1, 0x0010);

            processor.Step();
            processor.Step();

            Assert.That(processor.GetRegister(Register.IP), Is.EqualTo(8));
            Assert.That(memory.GetU16(0x0010), Is.EqualTo(0x1234));
        }

        [Test]
        public void MoveMemoryToRegister_LoadsValueFromMemory()
        {
            flasher.WriteInstruction(Instruction.MOV_MEM_REG, 0x0010, Register.R1);
            flasher.Address = 0x10;
            flasher.WriteU16(0x1234);

            processor.Step();

            Assert.That(processor.GetRegister(Register.IP), Is.EqualTo(4));
            Assert.That(processor.GetRegister(Register.R1), Is.EqualTo(0x1234));
        }

        [Test]
        public void AddRegisterToRegister_PerformsAdditionAndStoresInAccRegister()
        {
            flasher.WriteInstruction(Instruction.MOV_LIT_REG, 0x1200, Register.R1);
            flasher.WriteInstruction(Instruction.MOV_LIT_REG, 0x0034, Register.R2);
            flasher.WriteInstruction(Instruction.ADD_REG_REG, Register.R1, Register.R2);

            processor.Step();
            processor.Step();
            processor.Step();

            Assert.That(processor.GetRegister(Register.IP), Is.EqualTo(11));
            Assert.That(processor.GetRegister(Register.ACC), Is.EqualTo(0x1234));
        }

        [Test]
        public void JumpNotEqual_DoesNothingIfEqual()
        {
            var answer = 42;
            Assert.That(answer, Is.EqualTo(42), "Some useful error message");
        }

        [Test]
        public void JumpNotEqual_ChangesIpRegisterIfNotEqual()
        {
            var answer = 42;
            Assert.That(answer, Is.EqualTo(42), "Some useful error message");
        }

        [Test]
        public void PushLiteral_PutsLiteralOnStack()
        {
            var answer = 42;
            Assert.That(answer, Is.EqualTo(42), "Some useful error message");
        }

        [Test]
        public void PushRegister_PutsRegisterContentsOnStack()
        {
            var answer = 42;
            Assert.That(answer, Is.EqualTo(42), "Some useful error message");
        }

        [Test]
        public void Pop_RemovesTopValueFromStack()
        {
            // Push multiple values and then pop them all
            var answer = 42;
            Assert.That(answer, Is.EqualTo(42), "Some useful error message");
        }

        // pop with empty stack?
        // push with full stack?

        [Test]
        public void CallLiteral_CallsSubroutineAtSpecifiedAddress()
        {
            var answer = 42;
            Assert.That(answer, Is.EqualTo(42), "Some useful error message");
        }

        [Test]
        public void CallRegister_CallsSubroutineSpecifiedByRegister()
        {
            var answer = 42;
            Assert.That(answer, Is.EqualTo(42), "Some useful error message");
        }

        // call with and without arguments

        [Test]
        public void Return_ReturnsFromSubroutine()
        {
            var answer = 42;
            Assert.That(answer, Is.EqualTo(42), "Some useful error message");
        }

        // Return outside of subroutine?

        // invalid memory accesses
    }
}