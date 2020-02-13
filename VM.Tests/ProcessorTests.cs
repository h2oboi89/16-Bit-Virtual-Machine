#pragma warning disable CA1822 // Mark members as static
using NUnit.Framework;
using System;

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

        private void ExecuteProgram()
        {
            for (var i = 0; i < flasher.InstructionCount; i++)
            {
                processor.Step();
            }

            AssertPCIsAtEndOfProgram();
        }

        private void AssertPCIsAtEndOfProgram()
        {
            Assert.That(processor.GetRegister(Register.PC), Is.EqualTo(flasher.Address));
        }

        /// <summary>
        /// Writes values to <see cref="Memory"/> and resets <see cref="Flasher.Address"/> to 0.
        /// Should be called before all program instructions are written to allow <see cref="AssertPCIsAtEndOfProgram"/> to function.
        /// </summary>
        /// <param name="address">Location in memory to store value</param>
        /// <param name="value">Value to store in memory.</param>
        private void WriteValueToMemory(ushort address, ushort value)
        {
            flasher.Address = address;

            flasher.WriteU16(value);

            flasher.Address = 0;
        }

        private void Reset()
        {
            flasher.WriteInstruction(Instruction.RESET);
            processor.Step();
            flasher.Address = 0;
        }

        [Test]
        public void Constructor_NullMemory_ThrowsException()
        {
            Assert.That(() => new Processor(null), Throws.ArgumentNullException);
        }

        private void AssertProcessorIsInInitialState()
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
        public void Constructor_InitializesRegisters()
        {
            AssertProcessorIsInInitialState();
        }

        [Test]
        public void ModifyingPrivateRegistersDirectly_ResetsAndThrowsException()
        {
            var privateRegisters = new Register[]
            {
                Register.PC, Register.RA, Register.ACC, Register.FLAG, Register.SP, Register.FP
            };

            foreach (var register in privateRegisters)
            {
                flasher.Address = 0;

                flasher.WriteInstruction(Instruction.LDVR, 0x1234, register);

                Assert.That(() => processor.Step(), Throws.InvalidOperationException
                        .With.Message.EqualTo($"{Enum.GetName(typeof(Register), register)} register cannot be modified directly by code."));

                AssertProcessorIsInInitialState();
            }
        }

        [Test]
        public void InvalidRegisterInInstruction_ResetsAndThrowsException()
        {
            flasher.WriteInstruction(Instruction.INC, (Register)0xff);

            Assert.That(() => processor.Step(), Throws.InvalidOperationException
                       .With.Message.EqualTo("Unknown register 0xFF."));

            AssertProcessorIsInInitialState();
        }

        [Test]
        public void GetRegister_InvalidRegister_ResetsAndThrowsException()
        {
            processor.Step();

            Assert.That(processor.GetRegister(Register.PC), Is.Not.Zero);

            Assert.That(() => processor.GetRegister((Register)0xff), Throws.InvalidOperationException
                       .With.Message.EqualTo("Unknown register 0xFF."));

            AssertProcessorIsInInitialState();
        }

        [Test]
        public void InvalidMemoryAccess_ResetsAndThrowsException()
        {
            flasher.WriteInstruction(Instruction.STVA, 0x1234, 0xffff);

            Assert.That(() => processor.Step(), Throws.InstanceOf<IndexOutOfRangeException>()
                .With.Message.StartsWith("Invalid memory address 0xFFFF."));

            AssertProcessorIsInInitialState();
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
            flasher.WriteInstruction(Instruction.LDVR, 0x1234, Register.R1);
            flasher.WriteInstruction(Instruction.MOVE, Register.R1, Register.R2);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.R2), Is.EqualTo(0x1234));
        }

        [Test]
        public void INC_IncrementsRegisterValue()
        {
            flasher.WriteInstruction(Instruction.LDVR, 0x1234, Register.R1);
            flasher.WriteInstruction(Instruction.INC, Register.R1);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.R1), Is.EqualTo(0x1235));
            Assert.That(processor.IsSet(Flag.CARRY), Is.False);
        }

        [Test]
        public void INC_Overflow_WrapsAndSetsCarryAndZeroFlags()
        {
            flasher.WriteInstruction(Instruction.LDVR, ushort.MaxValue, Register.R1);
            flasher.WriteInstruction(Instruction.INC, Register.R1);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.R1), Is.EqualTo(ushort.MinValue));
            Assert.That(processor.IsSet(Flag.CARRY), Is.True);
            Assert.That(processor.IsSet(Flag.ZERO), Is.True);
        }

        [Test]
        public void DEC_DecrementsRegisterValue()
        {
            flasher.WriteInstruction(Instruction.LDVR, 0x1234, Register.R1);
            flasher.WriteInstruction(Instruction.DEC, Register.R1);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.R1), Is.EqualTo(0x1233));
            Assert.That(processor.IsSet(Flag.CARRY), Is.False);
        }

        [Test]
        public void DEC_Underflow_WrapsAndSetsCarryFlag()
        {
            flasher.WriteInstruction(Instruction.LDVR, ushort.MinValue, Register.R1);
            flasher.WriteInstruction(Instruction.DEC, Register.R1);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.R1), Is.EqualTo(ushort.MaxValue));
            Assert.That(processor.IsSet(Flag.CARRY), Is.True);
        }

        [Test]
        public void DEC_ZeroResult_SetsZeroFlag()
        {
            flasher.WriteInstruction(Instruction.LDVR, 0x0001, Register.R1);
            flasher.WriteInstruction(Instruction.DEC, Register.R1);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.R1), Is.Zero);
            Assert.That(processor.IsSet(Flag.ZERO), Is.True);
        }

        [Test]
        public void LDVR_LoadsValueIntoRegister()
        {
            flasher.WriteInstruction(Instruction.LDVR, 0x1234, Register.R1);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.R1), Is.EqualTo(0x1234));
        }

        [Test]
        public void LDAR_LoadsValueFromAddressIntoRegister()
        {
            ushort address = 0x10;

            WriteValueToMemory(address, 0x1234);

            flasher.WriteInstruction(Instruction.LDAR, address, Register.R1);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.R1), Is.EqualTo(0x1234));
        }

        [Test]
        public void LDRR_LoadsValueFromAddressInRegisterIntoRegister()
        {
            ushort address = 0x10;

            WriteValueToMemory(address, 0x1234);

            flasher.WriteInstruction(Instruction.LDVR, address, Register.R1);
            flasher.WriteInstruction(Instruction.LDRR, Register.R1, Register.R2);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.R2), Is.EqualTo(0x1234));
        }

        [Test]
        public void STVA_StoresValueAtAddress()
        {
            ushort address = 0x10;

            flasher.WriteInstruction(Instruction.STVA, 0x1234, address);

            Assert.That(memory.GetU16(address), Is.Zero);

            ExecuteProgram();

            Assert.That(memory.GetU16(address), Is.EqualTo(0x1234));
        }

        [Test]
        public void STVR_StoresValueAtAddressInRegister()
        {
            ushort address = 0x10;

            flasher.WriteInstruction(Instruction.LDVR, address, Register.R1);
            flasher.WriteInstruction(Instruction.STVR, 0x1234, Register.R1);

            Assert.That(memory.GetU16(address), Is.Zero);

            ExecuteProgram();

            Assert.That(memory.GetU16(address), Is.EqualTo(0x1234));
        }

        [Test]
        public void STRA_StoresValueInRegisterAtAddress()
        {
            ushort address = 0x10;

            flasher.WriteInstruction(Instruction.LDVR, 0x1234, Register.R1);
            flasher.WriteInstruction(Instruction.STRA, Register.R1, address);

            ExecuteProgram();

            Assert.That(memory.GetU16(address), Is.EqualTo(0x1234));
        }

        [Test]
        public void STRR_StoresValueInRegisterAtAddressInRegister()
        {
            ushort address = 0x10;

            flasher.WriteInstruction(Instruction.LDVR, 0x1234, Register.R1);
            flasher.WriteInstruction(Instruction.LDVR, address, Register.R2);
            flasher.WriteInstruction(Instruction.STRR, Register.R1, Register.R2);

            ExecuteProgram();

            Assert.That(memory.GetU16(address), Is.EqualTo(0x1234));
        }

        [Test]
        public void ADD_AdditionOfTwoRegisterValues()
        {
            flasher.WriteInstruction(Instruction.LDVR, 0x1200, Register.R1);
            flasher.WriteInstruction(Instruction.LDVR, 0x0034, Register.R2);
            flasher.WriteInstruction(Instruction.ADD, Register.R1, Register.R2);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.ACC), Is.EqualTo(0x1234));
            Assert.That(processor.IsSet(Flag.CARRY), Is.False);
        }

        [Test]
        public void ADD_Overflow_WrapsAndSetsCarryFlag()
        {
            flasher.WriteInstruction(Instruction.LDVR, 0xff00, Register.R1);
            flasher.WriteInstruction(Instruction.LDVR, 0x1000, Register.R2);
            flasher.WriteInstruction(Instruction.ADD, Register.R1, Register.R2);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.ACC), Is.EqualTo(0x0f00));
            Assert.That(processor.IsSet(Flag.CARRY), Is.True);
        }

        [Test]
        public void ADD_ZeroResult_SetsCarryAndZeroFlag()
        {
            flasher.WriteInstruction(Instruction.LDVR, 0xff00, Register.R1);
            flasher.WriteInstruction(Instruction.LDVR, 0x0100, Register.R2);
            flasher.WriteInstruction(Instruction.ADD, Register.R1, Register.R2);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.ACC), Is.Zero);
            Assert.That(processor.IsSet(Flag.CARRY), Is.True);
            Assert.That(processor.IsSet(Flag.ZERO), Is.True);
        }

        [Test]
        public void SUB_SubtractionOfTwoRegisterValues()
        {
            flasher.WriteInstruction(Instruction.LDVR, 0x1200, Register.R1);
            flasher.WriteInstruction(Instruction.LDVR, 0x0034, Register.R2);
            flasher.WriteInstruction(Instruction.SUB, Register.R1, Register.R2);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.ACC), Is.EqualTo(0x11cc));
            Assert.That(processor.IsSet(Flag.CARRY), Is.False);
        }

        [Test]
        public void SUB_Underflow_WrapsAndSetsCarryFlag()
        {
            flasher.WriteInstruction(Instruction.LDVR, 0x1000, Register.R1);
            flasher.WriteInstruction(Instruction.LDVR, 0xff00, Register.R2);
            flasher.WriteInstruction(Instruction.SUB, Register.R1, Register.R2);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.ACC), Is.EqualTo(0x1100));
            Assert.That(processor.IsSet(Flag.CARRY), Is.True);
        }

        [Test]
        public void SUB_ZeroResult_SetsZeroFlag()
        {
            flasher.WriteInstruction(Instruction.LDVR, 0x1234, Register.R1);
            flasher.WriteInstruction(Instruction.LDVR, 0x1234, Register.R2);
            flasher.WriteInstruction(Instruction.SUB, Register.R1, Register.R2);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.ACC), Is.Zero);
            Assert.That(processor.IsSet(Flag.ZERO), Is.True);
        }

        [Test]
        public void MUL_MultiplicationOfTwoRegisterValues()
        {
            flasher.WriteInstruction(Instruction.LDVR, 0x00f0, Register.R1);
            flasher.WriteInstruction(Instruction.LDVR, 0x00f0, Register.R2);
            flasher.WriteInstruction(Instruction.MUL, Register.R1, Register.R2);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.ACC), Is.EqualTo(0xe100));
            Assert.That(processor.IsSet(Flag.CARRY), Is.False);
        }

        [Test]
        public void MUL_Overflow_WrapsAndSetsCarryFlag()
        {
            flasher.WriteInstruction(Instruction.LDVR, 0x1234, Register.R1);
            flasher.WriteInstruction(Instruction.LDVR, 0xffff, Register.R2);
            flasher.WriteInstruction(Instruction.MUL, Register.R1, Register.R2);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.ACC), Is.EqualTo(0xedcc));
            Assert.That(processor.IsSet(Flag.CARRY), Is.True);
        }

        [Test]
        public void MUL_ZeroResult_SetsZeroFlag()
        {
            flasher.WriteInstruction(Instruction.LDVR, 0x1234, Register.R1);
            flasher.WriteInstruction(Instruction.LDVR, (ushort)0x0000, Register.R2);
            flasher.WriteInstruction(Instruction.MUL, Register.R1, Register.R2);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.ACC), Is.Zero);
            Assert.That(processor.IsSet(Flag.ZERO), Is.True);
        }

        [Test]
        public void DIV_DivisionOfTwoRegisterValues()
        {
            flasher.WriteInstruction(Instruction.LDVR, 0x0040, Register.R1);
            flasher.WriteInstruction(Instruction.LDVR, 0x0002, Register.R2);
            flasher.WriteInstruction(Instruction.DIV, Register.R1, Register.R2);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.ACC), Is.EqualTo(0x0020));
            Assert.That(processor.IsSet(Flag.CARRY), Is.False);
        }

        [Test]
        public void DIV_ByZero_ResetsAndThrowsException()
        {
            flasher.WriteInstruction(Instruction.LDVR, 0x1234, Register.R1);
            flasher.WriteInstruction(Instruction.LDVR, (ushort)0x0000, Register.R2);
            flasher.WriteInstruction(Instruction.DIV, Register.R1, Register.R2);

            processor.Step();
            processor.Step();

            Assert.That(() => processor.Step(), Throws.InstanceOf<DivideByZeroException>());

            AssertProcessorIsInInitialState();
        }

        [Test]
        public void DIV_ZeroResult_SetsZeroFlag()
        {
            flasher.WriteInstruction(Instruction.LDVR, 0x0001, Register.R1);
            flasher.WriteInstruction(Instruction.LDVR, 0xffff, Register.R2);
            flasher.WriteInstruction(Instruction.DIV, Register.R1, Register.R2);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.ACC), Is.Zero);
            Assert.That(processor.IsSet(Flag.ZERO), Is.True);
        }

        [Test]
        public void AND_BinaryAndOfTwoRegisterValues()
        {
            flasher.WriteInstruction(Instruction.LDVR, 0x00ff, Register.R1);
            flasher.WriteInstruction(Instruction.LDVR, 0xaaaa, Register.R2);
            flasher.WriteInstruction(Instruction.AND, Register.R1, Register.R2);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.ACC), Is.EqualTo(0x00aa));
        }

        [Test]
        public void OR_BinaryOrOfTwoRegisterValues()
        {
            flasher.WriteInstruction(Instruction.LDVR, 0x0055, Register.R1);
            flasher.WriteInstruction(Instruction.LDVR, 0xaaaa, Register.R2);
            flasher.WriteInstruction(Instruction.OR, Register.R1, Register.R2);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.ACC), Is.EqualTo(0xaaff));
        }

        [Test]
        public void XOR_BinaryXorOfTwoRegisterValues()
        {
            flasher.WriteInstruction(Instruction.LDVR, 0x00ff, Register.R1);
            flasher.WriteInstruction(Instruction.LDVR, 0xaaaa, Register.R2);
            flasher.WriteInstruction(Instruction.XOR, Register.R1, Register.R2);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.ACC), Is.EqualTo(0xaa55));
        }

        [Test]
        public void NOT_BinaryNotOfRegisterValue()
        {
            flasher.WriteInstruction(Instruction.LDVR, 0xaaaa, Register.R1);
            flasher.WriteInstruction(Instruction.NOT, Register.R1);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.ACC), Is.EqualTo(0x5555));
        }

        [Test]
        public void SRL_ShiftsRegisterLeftByAmount()
        {
            for (byte i = 0; i < 16; i++)
            {
                Reset();

                flasher.WriteInstruction(Instruction.LDVR, 0x0001, Register.R1);
                flasher.WriteInstruction(Instruction.SRL, Register.R1, i);

                ExecuteProgram();

                Assert.That(processor.GetRegister(Register.ACC), Is.EqualTo(0x0001 << i));
            }
        }

        [Test]
        public void SRL_ExcessiveShift_ZeroesOutRegister()
        {
            flasher.WriteInstruction(Instruction.LDVR, 0x0001, Register.R1);
            flasher.WriteInstruction(Instruction.SRL, Register.R1, 16);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.ACC), Is.Zero);
        }

        [Test]
        public void SRLR_ShiftsRegisterLeftByAmountInRegister()
        {
            for (byte i = 0; i < 16; i++)
            {
                Reset();

                flasher.WriteInstruction(Instruction.LDVR, 0x0001, Register.R1);
                flasher.WriteInstruction(Instruction.LDVR, i, Register.R2);
                flasher.WriteInstruction(Instruction.SRLR, Register.R1, Register.R2);

                ExecuteProgram();

                Assert.That(processor.GetRegister(Register.ACC), Is.EqualTo(0x0001 << i));
            }
        }

        [Test]
        public void SRLR_ExcessiveShift_ZeroesOutRegister()
        {
            flasher.WriteInstruction(Instruction.LDVR, 0x0001, Register.R1);
            flasher.WriteInstruction(Instruction.LDVR, 16, Register.R2);
            flasher.WriteInstruction(Instruction.SRLR, Register.R1, Register.R2);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.ACC), Is.Zero);
        }

        [Test]
        public void SRR_ShiftsRegisterRightByAmount()
        {
            for (byte i = 0; i < 16; i++)
            {
                Reset();

                flasher.WriteInstruction(Instruction.LDVR, 0x8000, Register.R1);
                flasher.WriteInstruction(Instruction.SRR, Register.R1, i);

                ExecuteProgram();

                Assert.That(processor.GetRegister(Register.ACC), Is.EqualTo(0x8000 >> i));
            }
        }

        [Test]
        public void SRR_ExcessiveShift_ZeroesOutRegister()
        {
            flasher.WriteInstruction(Instruction.LDVR, 0x8000, Register.R1);
            flasher.WriteInstruction(Instruction.SRR, Register.R1, 16);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.ACC), Is.Zero);
        }

        [Test]
        public void SRRR_ShiftsRegisterLeftByAmountInRegister()
        {
            for (byte i = 0; i < 16; i++)
            {
                Reset();

                flasher.WriteInstruction(Instruction.LDVR, 0x8000, Register.R1);
                flasher.WriteInstruction(Instruction.LDVR, i, Register.R2);
                flasher.WriteInstruction(Instruction.SRRR, Register.R1, Register.R2);

                ExecuteProgram();

                Assert.That(processor.GetRegister(Register.ACC), Is.EqualTo(0x8000 >> i));
            }
        }

        [Test]
        public void SRRR_ExcessiveShift_ZeroesOutRegister()
        {
            flasher.WriteInstruction(Instruction.LDVR, 0x8000, Register.R1);
            flasher.WriteInstruction(Instruction.LDVR, 16, Register.R2);
            flasher.WriteInstruction(Instruction.SRRR, Register.R1, Register.R2);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.ACC), Is.Zero);
        }

        // TODO: Subroutine instructions

        // TODO: Jump instructions

        // TODO: Logic instructions

        // TODO: Stack instructions
        // TODO: memory access into stack is invalid
        // TODO: stack overflow exception

        [Test]
        public void RESET_ResetsProcessorToBeginningOfProgram()
        {
            flasher.WriteInstruction(Instruction.RESET);

            processor.Step();

            AssertProcessorIsInInitialState();
        }

        [Test]
        public void InvalidInstruction_ResetsAndThrowsException()
        {
            flasher.WriteInstruction((Instruction)0xff);

            Assert.That(() => processor.Step(), Throws.InvalidOperationException
                        .With.Message.EqualTo($"Unknown instruction 0xFF."));

            AssertProcessorIsInInitialState();
        }
        #endregion
    }
}
#pragma warning restore CA1822 // Mark members as static