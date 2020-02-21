#pragma warning disable CA1822 // Mark members as static
using NUnit.Framework;
using System;

namespace VM.Tests
{
    public class ProcessorTests
    {
        private const uint MEMORY_SIZE = 0x8000;
        private const ushort STACK_SIZE = 0x400;
        private const ushort STACK_START_ADDRESS = (ushort)(MEMORY_SIZE - Processor.DATASIZE);
        private const ushort STACK_END_ADDRESS = STACK_START_ADDRESS - ((STACK_SIZE - 1) * Processor.DATASIZE);
        private const ushort STATE_SIZE = 18;

        private Memory memory;
        private Processor processor;
        private Flasher flasher;

        private bool resetOccured;
        private Exception exception;

        private void OnReset(object sender, ResetEventArgs e)
        {
            resetOccured = true;
            exception = e.Exception;
        }

        private bool haltOccured;

        private void OnHalt(object sender, EventArgs e)
        {
            haltOccured = true;
        }

        private int tickCount = 0;

        private void OnTick(object sender, EventArgs e)
        {
            tickCount++;
        }

        [SetUp]
        public void Setup()
        {
            memory = new Memory(MEMORY_SIZE);
            flasher = new Flasher(memory);
            processor = new Processor(memory, STACK_SIZE);

            resetOccured = false;
            exception = null;
            processor.Reset += OnReset;

            haltOccured = false;
            processor.Halt += OnHalt;
        }

        [TearDown]
        public void TearDown()
        {
            processor.Reset -= OnReset;
            processor.Halt -= OnHalt;
        }

        #region Helper methods
        private void FlashNNops(ushort n)
        {
            for (var i = 0; i < n; i++)
            {
                flasher.WriteInstruction(Instruction.NOP);
            }
        }

        /// <summary>
        /// Executes program and asserts that PC is at correct value.
        /// If <paramref name="expectedAddress"/> is not specified then <see cref="Flasher.Address"/> is used.
        /// </summary>
        /// <param name="expectedAddress">Optional expected PC value.</param>
        private void ExecuteProgram(ushort? expectedAddress = null)
        {
            if (expectedAddress == null)
            {
                expectedAddress = flasher.Address;
            }

            for (var i = 0; i < flasher.InstructionCount; i++)
            {
                processor.Step();
            }

            Assert.That(processor.GetRegister(Register.PC), Is.EqualTo(expectedAddress.Value));
        }

        /// <summary>
        /// Writes values to <see cref="Memory"/> and resets <see cref="Flasher.Address"/> to 0.
        /// Should be called before all program instructions are written to allow <see cref="AssertPCIsAtExpectedAddress"/> to function.
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

        private void LoadValueIntoRegister(ushort value, Register register)
        {
            flasher.WriteInstruction(Instruction.LDVR, value, register);
        }

        private void LoadValueIntoRegisterR0(ushort value)
        {
            LoadValueIntoRegister(value, Register.R0);
        }

        private void SetupBinaryInstruction(Instruction instruction, ushort valueA, ushort valueB)
        {
            LoadValueIntoRegister(valueA, Register.R0);
            LoadValueIntoRegister(valueB, Register.R1);
            flasher.WriteInstruction(instruction, Register.R0, Register.R1);
        }

        private void AssertZeroValueAndFlag(Register register = Register.ACC)
        {
            Assert.That(processor.GetRegister(register), Is.Zero);
            Assert.That(processor.IsSet(Flag.ZERO), Is.True);
        }

        private void AssertProcessorIsInInitialState()
        {
            Assert.That(processor.GetRegister(Register.PC), Is.Zero);
            Assert.That(processor.GetRegister(Register.ACC), Is.Zero);
            Assert.That(processor.GetRegister(Register.FLAG), Is.Zero);

            Assert.That(processor.GetRegister(Register.SP), Is.EqualTo(STACK_START_ADDRESS));
            Assert.That(processor.GetRegister(Register.FP), Is.EqualTo(STACK_START_ADDRESS));

            Assert.That(processor.GetRegister(Register.R0), Is.Zero);
            Assert.That(processor.GetRegister(Register.R1), Is.Zero);
            Assert.That(processor.GetRegister(Register.R2), Is.Zero);
            Assert.That(processor.GetRegister(Register.R3), Is.Zero);
            Assert.That(processor.GetRegister(Register.R4), Is.Zero);
            Assert.That(processor.GetRegister(Register.R5), Is.Zero);
            Assert.That(processor.GetRegister(Register.R6), Is.Zero);
            Assert.That(processor.GetRegister(Register.R7), Is.Zero);

            Assert.That(processor.GetRegister(Register.T0), Is.Zero);
            Assert.That(processor.GetRegister(Register.T1), Is.Zero);
            Assert.That(processor.GetRegister(Register.T2), Is.Zero);
            Assert.That(processor.GetRegister(Register.T3), Is.Zero);
            Assert.That(processor.GetRegister(Register.T4), Is.Zero);
            Assert.That(processor.GetRegister(Register.T5), Is.Zero);
            Assert.That(processor.GetRegister(Register.T6), Is.Zero);
            Assert.That(processor.GetRegister(Register.T7), Is.Zero);
        }

        private void AssertExceptionOccursAndProcessorResets(Type exceptionType, string expectedMessage)
        {
            Assert.That(() => processor.Step(), Throws.Exception.TypeOf(exceptionType)
                .With.Message.EqualTo(expectedMessage));

            Assert.That(resetOccured, Is.True);
            Assert.That(exception.Message, Is.EqualTo(expectedMessage));

            AssertProcessorIsInInitialState();
        }

        private void FillStack(ushort amount = STACK_SIZE)
        {
            LoadValueIntoRegisterR0(0x1234);
            for (var i = 0; i < amount; i++)
            {
                flasher.WriteInstruction(Instruction.PUSH, Register.R0);
            }

            Assert.That(processor.GetRegister(Register.SP), Is.EqualTo(STACK_START_ADDRESS));

            ExecuteProgram();
        }
        #endregion

        [Test]
        public void Constructor_NullMemory_ThrowsException()
        {
            Assert.That(() => new Processor(null, 0), Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_InitializesRegisters()
        {
            AssertProcessorIsInInitialState();
        }

        [Test]
        public void Tick_FiresAfterEachStep()
        {
            FlashNNops(10);

            processor.Tick += OnTick;

            ExecuteProgram();

            Assert.That(tickCount, Is.EqualTo(10));

            processor.Tick -= OnTick;
        }

        [Test]
        public void ModifyingPrivateRegistersDirectly_ResetsAndThrowsException()
        {
            var privateRegisters = new Register[]
            {
                Register.PC, Register.ACC, Register.FLAG
            };

            foreach (var register in privateRegisters)
            {
                flasher.Address = 0;

                LoadValueIntoRegister(0x1234, register);

                AssertExceptionOccursAndProcessorResets(
                    typeof(InvalidOperationException),
                    $"{Enum.GetName(typeof(Register), register)} register cannot be modified directly by code.");
            }
        }

        [Test]
        public void ModifyingStackRegisters_ResetsAndThrowsException()
        {
            var stackRegisters = new Register[]
            {
                Register.SP, Register.FP
            };

            foreach (var register in stackRegisters)
            {
                flasher.Address = 0;

                LoadValueIntoRegister(0x1234, register);

                AssertExceptionOccursAndProcessorResets(
                    typeof(InvalidOperationException),
                    $"{Enum.GetName(typeof(Register), register)} is managed by {typeof(Stack)}.");
            }
        }

        [Test]
        public void NonPrivateRegister_ValuesCanBeModified()
        {
            var publicRegisters = new Register[]
            {
                Register.R0, Register.R1, Register.R2, Register.R3,
                Register.R4, Register.R5, Register.R6, Register.R7,
                Register.T0, Register.T1, Register.T2, Register.T3,
                Register.T4, Register.T5, Register.T6, Register.T7
            };

            foreach (var register in publicRegisters)
            {
                Reset();

                LoadValueIntoRegister(0x1234, register);

                ExecuteProgram();

                Assert.That(processor.GetRegister(register), Is.EqualTo(0x1234));
            }
        }

        [Test]
        public void InvalidRegisterInInstruction_ResetsAndThrowsException()
        {
            flasher.WriteInstruction(Instruction.INC, (Register)0xff);

            AssertExceptionOccursAndProcessorResets(
                typeof(InvalidOperationException),
                "Unknown register 0xFF.");
        }

        [Test]
        public void GetRegister_InvalidRegister_ThrowsException()
        {
            processor.Step();

            Assert.That(processor.GetRegister(Register.PC), Is.Not.Zero);

            Assert.That(() => processor.GetRegister((Register)0xff), Throws.InvalidOperationException
                .With.Message.EqualTo("Unknown register 0xFF."));
        }

        [Test]
        public void InvalidMemoryAccess_ResetsAndThrowsException()
        {
            flasher.WriteInstruction(Instruction.STVA, 0x1234, 0xffff);

            AssertExceptionOccursAndProcessorResets(
                typeof(IndexOutOfRangeException),
                "Invalid memory address 0xFFFF. Valid Range: [0x0000, 0x7FFF]");
        }

        #region Instructions
        [Test]
        public void NOP_DoesNothing()
        {
            FlashNNops(50);

            ExecuteProgram();
        }

        #region Register
        [Test]
        public void MOVE_MovesValueFromRegisterAToRegisterB()
        {
            LoadValueIntoRegisterR0(0x1234);
            flasher.WriteInstruction(Instruction.MOVE, Register.R0, Register.R1);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.R1), Is.EqualTo(0x1234));
        }

        [Test]
        public void INC_IncrementsRegisterValue()
        {
            LoadValueIntoRegisterR0(0x1234);
            flasher.WriteInstruction(Instruction.INC, Register.R0);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.R0), Is.EqualTo(0x1235));
            Assert.That(processor.IsSet(Flag.CARRY), Is.False);
        }

        [Test]
        public void INC_Overflow_WrapsAndSetsZeroAndCarryFlags()
        {
            LoadValueIntoRegisterR0(0xffff);
            flasher.WriteInstruction(Instruction.INC, Register.R0);

            ExecuteProgram();

            AssertZeroValueAndFlag(Register.R0);
            Assert.That(processor.IsSet(Flag.CARRY), Is.True);
        }

        [Test]
        public void DEC_DecrementsRegisterValue()
        {
            LoadValueIntoRegisterR0(0x1234);
            flasher.WriteInstruction(Instruction.DEC, Register.R0);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.R0), Is.EqualTo(0x1233));
            Assert.That(processor.IsSet(Flag.CARRY), Is.False);
        }

        [Test]
        public void DEC_Underflow_WrapsAndSetsCarryFlag()
        {
            LoadValueIntoRegisterR0(0x0000);
            flasher.WriteInstruction(Instruction.DEC, Register.R0);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.R0), Is.EqualTo(0xffff));
            Assert.That(processor.IsSet(Flag.CARRY), Is.True);
        }

        [Test]
        public void DEC_ZeroResult_SetsZeroFlag()
        {
            LoadValueIntoRegisterR0(0x0001);
            flasher.WriteInstruction(Instruction.DEC, Register.R0);

            ExecuteProgram();

            AssertZeroValueAndFlag(Register.R0);
        }
        #endregion

        #region Load
        [Test]
        public void LDVR_LoadsValueIntoRegister()
        {
            LoadValueIntoRegisterR0(0x1234);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.R0), Is.EqualTo(0x1234));
        }

        [Test]
        public void LDAR_LoadsValueFromAddressIntoRegister()
        {
            ushort address = 0x10;

            WriteValueToMemory(address, 0x1234);

            flasher.WriteInstruction(Instruction.LDAR, address, Register.R0);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.R0), Is.EqualTo(0x1234));
        }

        [Test]
        public void LDRR_LoadsValueFromAddressInRegisterIntoRegister()
        {
            ushort address = 0x10;

            WriteValueToMemory(address, 0x1234);

            LoadValueIntoRegisterR0(address);
            flasher.WriteInstruction(Instruction.LDRR, Register.R0, Register.R1);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.R1), Is.EqualTo(0x1234));
        }
        #endregion

        #region Store
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

            LoadValueIntoRegisterR0(address);
            flasher.WriteInstruction(Instruction.STVR, 0x1234, Register.R0);

            Assert.That(memory.GetU16(address), Is.Zero);

            ExecuteProgram();

            Assert.That(memory.GetU16(address), Is.EqualTo(0x1234));
        }

        [Test]
        public void STRA_StoresValueInRegisterAtAddress()
        {
            ushort address = 0x10;

            LoadValueIntoRegisterR0(0x1234);
            flasher.WriteInstruction(Instruction.STRA, Register.R0, address);

            ExecuteProgram();

            Assert.That(memory.GetU16(address), Is.EqualTo(0x1234));
        }

        [Test]
        public void STRR_StoresValueInRegisterAtAddressInRegister()
        {
            ushort address = 0x10;

            LoadValueIntoRegister(0x1234, Register.R0);
            LoadValueIntoRegister(address, Register.R1);
            flasher.WriteInstruction(Instruction.STRR, Register.R0, Register.R1);

            ExecuteProgram();

            Assert.That(memory.GetU16(address), Is.EqualTo(0x1234));
        }
        #endregion

        #region Arithmetic
        [Test]
        public void ADD_AdditionOfTwoRegisterValues()
        {
            SetupBinaryInstruction(Instruction.ADD, 0x1200, 0x0034);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.ACC), Is.EqualTo(0x1234));
            Assert.That(processor.IsSet(Flag.CARRY), Is.False);
        }

        [Test]
        public void ADD_Overflow_WrapsAndSetsCarryFlag()
        {
            SetupBinaryInstruction(Instruction.ADD, 0xff00, 0x1000);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.ACC), Is.EqualTo(0x0f00));
            Assert.That(processor.IsSet(Flag.CARRY), Is.True);
        }

        [Test]
        public void ADD_ZeroResult_SetsZeroAndCarryFlags()
        {
            SetupBinaryInstruction(Instruction.ADD, 0xff00, 0x0100);

            ExecuteProgram();

            AssertZeroValueAndFlag();
            Assert.That(processor.IsSet(Flag.CARRY), Is.True);
        }

        [Test]
        public void SUB_SubtractionOfTwoRegisterValues()
        {
            SetupBinaryInstruction(Instruction.SUB, 0x1200, 0x0034);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.ACC), Is.EqualTo(0x11cc));
            Assert.That(processor.IsSet(Flag.CARRY), Is.False);
        }

        [Test]
        public void SUB_Underflow_WrapsAndSetsCarryFlag()
        {
            SetupBinaryInstruction(Instruction.SUB, 0x1000, 0xff00);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.ACC), Is.EqualTo(0x1100));
            Assert.That(processor.IsSet(Flag.CARRY), Is.True);
        }

        [Test]
        public void SUB_ZeroResult_SetsZeroFlag()
        {
            SetupBinaryInstruction(Instruction.SUB, 0x1234, 0x1234);

            ExecuteProgram();

            AssertZeroValueAndFlag();
        }

        [Test]
        public void MUL_MultiplicationOfTwoRegisterValues()
        {
            SetupBinaryInstruction(Instruction.MUL, 0x00f0, 0x00f0);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.ACC), Is.EqualTo(0xe100));
            Assert.That(processor.IsSet(Flag.CARRY), Is.False);
        }

        [Test]
        public void MUL_Overflow_WrapsAndSetsCarryFlag()
        {
            SetupBinaryInstruction(Instruction.MUL, 0x1234, 0xffff);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.ACC), Is.EqualTo(0xedcc));
            Assert.That(processor.IsSet(Flag.CARRY), Is.True);
        }

        [Test]
        public void MUL_ZeroResult_SetsZeroFlag()
        {
            SetupBinaryInstruction(Instruction.MUL, 0x1234, 0x0000);

            ExecuteProgram();

            AssertZeroValueAndFlag();
        }

        [Test]
        public void DIV_DivisionOfTwoRegisterValues()
        {
            SetupBinaryInstruction(Instruction.DIV, 0x0040, 0x0002);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.ACC), Is.EqualTo(0x0020));
            Assert.That(processor.IsSet(Flag.CARRY), Is.False);
        }

        [Test]
        public void DIV_ByZero_ResetsAndThrowsException()
        {
            SetupBinaryInstruction(Instruction.DIV, 0x1234, 0x0000);

            processor.Step();
            processor.Step();

            AssertExceptionOccursAndProcessorResets(
                typeof(DivideByZeroException),
                "Attempted to divide by zero.");
        }

        [Test]
        public void DIV_ZeroResult_SetsZeroFlag()
        {
            SetupBinaryInstruction(Instruction.DIV, 0x0001, 0xffff);

            ExecuteProgram();

            AssertZeroValueAndFlag();
        }

        [Test]
        public void AND_BinaryAndOfTwoRegisterValues()
        {
            SetupBinaryInstruction(Instruction.AND, 0x00ff, 0xaaaa);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.ACC), Is.EqualTo(0x00aa));
        }

        [Test]
        public void AND_ZeroResult_SetsZeroFlag()
        {
            SetupBinaryInstruction(Instruction.AND, 0x00ff, 0xff00);

            ExecuteProgram();

            AssertZeroValueAndFlag();
        }
        #endregion

        #region Bitwise
        [Test]
        public void OR_BinaryOrOfTwoRegisterValues()
        {
            SetupBinaryInstruction(Instruction.OR, 0x0055, 0xaaaa);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.ACC), Is.EqualTo(0xaaff));
        }

        [Test]
        public void OR_ZeroResult_SetsZeroFlag()
        {
            SetupBinaryInstruction(Instruction.OR, 0x0000, 0x0000);

            ExecuteProgram();

            AssertZeroValueAndFlag();
        }

        [Test]
        public void XOR_BinaryXorOfTwoRegisterValues()
        {
            SetupBinaryInstruction(Instruction.XOR, 0x00ff, 0xaaaa);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.ACC), Is.EqualTo(0xaa55));
        }

        [Test]
        public void XOR_ZeroResult_SetsZeroFlag()
        {
            SetupBinaryInstruction(Instruction.XOR, 0x00ff, 0x00ff);

            ExecuteProgram();

            AssertZeroValueAndFlag();
        }

        [Test]
        public void NOT_BinaryNotOfRegisterValue()
        {
            LoadValueIntoRegisterR0(0xaaaa);
            flasher.WriteInstruction(Instruction.NOT, Register.R0);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.ACC), Is.EqualTo(0x5555));
        }

        [Test]
        public void NOT_ZeroResult_SetsZeroFlag()
        {
            LoadValueIntoRegisterR0(0xffff);
            flasher.WriteInstruction(Instruction.NOT, Register.R0);

            ExecuteProgram();

            AssertZeroValueAndFlag();
        }

        [Test]
        public void SRL_ShiftsRegisterLeftByAmount()
        {
            for (byte i = 0; i < 16; i++)
            {
                Reset();

                LoadValueIntoRegisterR0(0x0001);
                flasher.WriteInstruction(Instruction.SRL, Register.R0, i);

                ExecuteProgram();

                Assert.That(processor.GetRegister(Register.ACC), Is.EqualTo(0x0001 << i));
            }
        }

        [Test]
        public void SRL_ExcessiveShift_ZeroesOutRegister()
        {
            LoadValueIntoRegisterR0(0x0001);
            flasher.WriteInstruction(Instruction.SRL, Register.R0, 16);

            ExecuteProgram();

            AssertZeroValueAndFlag();
        }

        [Test]
        public void SRLR_ShiftsRegisterLeftByAmountInRegister()
        {
            for (byte i = 0; i < 16; i++)
            {
                Reset();

                LoadValueIntoRegister(0x0001, Register.R0);
                LoadValueIntoRegister(i, Register.R1);
                flasher.WriteInstruction(Instruction.SRLR, Register.R0, Register.R1);

                ExecuteProgram();

                Assert.That(processor.GetRegister(Register.ACC), Is.EqualTo(0x0001 << i));
            }
        }

        [Test]
        public void SRLR_ExcessiveShift_ZeroesOutRegister()
        {
            LoadValueIntoRegister(0x0001, Register.R0);
            LoadValueIntoRegister(16, Register.R1);
            flasher.WriteInstruction(Instruction.SRLR, Register.R0, Register.R1);

            ExecuteProgram();

            AssertZeroValueAndFlag();
        }

        [Test]
        public void SRR_ShiftsRegisterRightByAmount()
        {
            for (byte i = 0; i < 16; i++)
            {
                Reset();

                LoadValueIntoRegisterR0(0x8000);
                flasher.WriteInstruction(Instruction.SRR, Register.R0, i);

                ExecuteProgram();

                Assert.That(processor.GetRegister(Register.ACC), Is.EqualTo(0x8000 >> i));
            }
        }

        [Test]
        public void SRR_ExcessiveShift_ZeroesOutRegister()
        {
            LoadValueIntoRegisterR0(0x8000);
            flasher.WriteInstruction(Instruction.SRR, Register.R0, 16);

            ExecuteProgram();

            AssertZeroValueAndFlag();
        }

        [Test]
        public void SRRR_ShiftsRegisterLeftByAmountInRegister()
        {
            for (byte i = 0; i < 16; i++)
            {
                Reset();

                LoadValueIntoRegister(0x8000, Register.R0);
                LoadValueIntoRegister(i, Register.R1);
                flasher.WriteInstruction(Instruction.SRRR, Register.R0, Register.R1);

                ExecuteProgram();

                Assert.That(processor.GetRegister(Register.ACC), Is.EqualTo(0x8000 >> i));
            }
        }

        [Test]
        public void SRRR_ExcessiveShift_ZeroesOutRegister()
        {
            LoadValueIntoRegister(0x8000, Register.R0);
            LoadValueIntoRegister(16, Register.R1);
            flasher.WriteInstruction(Instruction.SRRR, Register.R0, Register.R1);

            ExecuteProgram();

            AssertZeroValueAndFlag();
        }
        #endregion

        #region Jump
        [Test]
        public void JUMP_DoesUnconditionalJumpUsingAddress()
        {
            flasher.WriteInstruction(Instruction.JUMP, 0x1234);

            ExecuteProgram(0x1234);
        }

        [Test]
        public void JUMPR_DoesUnconditionalJumpUsingRegisterValue()
        {
            LoadValueIntoRegisterR0(0x1234);
            flasher.WriteInstruction(Instruction.JUMPR, Register.R0);

            ExecuteProgram(0x1234);
        }
        #endregion

        #region Logic
        [Test]
        public void CMP_LessThan_SetsLessThanFlag()
        {
            SetupBinaryInstruction(Instruction.CMP, 0x1234, 0x4321);

            ExecuteProgram();

            Assert.That(processor.IsSet(Flag.LESSTHAN), Is.True);
            Assert.That(processor.IsSet(Flag.EQUAL), Is.False);
            Assert.That(processor.IsSet(Flag.GREATERTHAN), Is.False);
        }

        [Test]
        public void CMP_EqualTo_SetsEqualFlag()
        {
            SetupBinaryInstruction(Instruction.CMP, 0x1234, 0x1234);

            ExecuteProgram();

            Assert.That(processor.IsSet(Flag.LESSTHAN), Is.False);
            Assert.That(processor.IsSet(Flag.EQUAL), Is.True);
            Assert.That(processor.IsSet(Flag.GREATERTHAN), Is.False);
        }

        [Test]
        public void CMP_GreaterThan_SetsGreaterThanFlag()
        {
            SetupBinaryInstruction(Instruction.CMP, 0x4321, 0x1234);

            ExecuteProgram();

            Assert.That(processor.IsSet(Flag.LESSTHAN), Is.False);
            Assert.That(processor.IsSet(Flag.EQUAL), Is.False);
            Assert.That(processor.IsSet(Flag.GREATERTHAN), Is.True);
        }

        [Test]
        public void CMPZ_Zero_SetsZeroFlag()
        {
            LoadValueIntoRegisterR0(0x0000);
            flasher.WriteInstruction(Instruction.CMPZ, Register.R0);

            ExecuteProgram();

            Assert.That(processor.IsSet(Flag.ZERO), Is.True);
        }

        [Test]
        public void CMPZ_NotZero_DoesNothing()
        {
            LoadValueIntoRegisterR0(0xffff);
            flasher.WriteInstruction(Instruction.CMPZ, Register.R0);

            ExecuteProgram();

            Assert.That(processor.IsSet(Flag.ZERO), Is.False);
        }

        [Test]
        public void JLT_LessThanFlagIsSet_ChangesPC()
        {
            SetupBinaryInstruction(Instruction.CMP, 0x1234, 0x4321);
            flasher.WriteInstruction(Instruction.JLT, 0x008f);

            ExecuteProgram(0x008f);
        }

        [Test]
        public void JLT_LessThanFlagIsNotSet_DoesNothing()
        {
            SetupBinaryInstruction(Instruction.CMP, 0x4321, 0x1234);
            flasher.WriteInstruction(Instruction.JLT, 0x008f);

            ExecuteProgram();
        }

        [Test]
        public void JLTR_LessThanFlagIsSet_ChangesPC()
        {
            SetupBinaryInstruction(Instruction.CMP, 0x1234, 0x4321);
            LoadValueIntoRegister(0x008f, Register.R3);
            flasher.WriteInstruction(Instruction.JLTR, Register.R3);

            ExecuteProgram(0x008f);
        }

        [Test]
        public void JLTR_LessThanFlagIsNotSet_DoesNothing()
        {
            SetupBinaryInstruction(Instruction.CMP, 0x4321, 0x1234);
            LoadValueIntoRegister(0x008f, Register.R3);
            flasher.WriteInstruction(Instruction.JLTR, Register.R3);

            ExecuteProgram();
        }

        [Test]
        public void JGT_GreaterThanFlagIsSet_ChangesPC()
        {
            SetupBinaryInstruction(Instruction.CMP, 0x4321, 0x1234);
            flasher.WriteInstruction(Instruction.JGT, 0x008f);

            ExecuteProgram(0x008f);
        }

        [Test]
        public void JGT_GreaterThanFlagIsNotSet_ChangesPC()
        {
            SetupBinaryInstruction(Instruction.CMP, 0x1234, 0x4321);
            flasher.WriteInstruction(Instruction.JGT, 0x008f);

            ExecuteProgram();
        }

        [Test]
        public void JGTR_GreaterThanFlagIsSet_ChangesPC()
        {
            SetupBinaryInstruction(Instruction.CMP, 0x4321, 0x1234);
            LoadValueIntoRegister(0x008f, Register.R3);
            flasher.WriteInstruction(Instruction.JGTR, Register.R3);

            ExecuteProgram(0x008f);
        }

        [Test]
        public void JGTR_GreaterThanFlagIsNotSet_DoesNothing()
        {
            SetupBinaryInstruction(Instruction.CMP, 0x1234, 0x4321);
            LoadValueIntoRegister(0x008f, Register.R3);
            flasher.WriteInstruction(Instruction.JGTR, Register.R3);

            ExecuteProgram();
        }

        [Test]
        public void JE_EqualFlagIsSet_ChangesPC()
        {
            SetupBinaryInstruction(Instruction.CMP, 0x1234, 0x1234);
            flasher.WriteInstruction(Instruction.JE, 0x008f);

            ExecuteProgram(0x008f);
        }

        [Test]
        public void JE_EqualFlagIsNotSet_DoesNothing()
        {
            SetupBinaryInstruction(Instruction.CMP, 0x1234, 0x4321);
            flasher.WriteInstruction(Instruction.JE, 0x008f);

            ExecuteProgram();
        }

        [Test]
        public void JER_EqualFlagIsSet_ChangesPC()
        {
            SetupBinaryInstruction(Instruction.CMP, 0x1234, 0x1234);
            LoadValueIntoRegister(0x008f, Register.R3);
            flasher.WriteInstruction(Instruction.JER, Register.R3);

            ExecuteProgram(0x008f);
        }

        [Test]
        public void JER_EqualFlagIsNotSet_DoesNothing()
        {
            SetupBinaryInstruction(Instruction.CMP, 0x1234, 0x4321);
            LoadValueIntoRegister(0x008f, Register.R3);
            flasher.WriteInstruction(Instruction.JER, Register.R3);

            ExecuteProgram();
        }

        [Test]
        public void JNE_EqualFlagIsNotSet_ChangesPC()
        {
            SetupBinaryInstruction(Instruction.CMP, 0x1234, 0x4321);
            flasher.WriteInstruction(Instruction.JNE, 0x008f);

            ExecuteProgram(0x008f);
        }

        [Test]
        public void JNE_EqualFlagIsSet_DoesNothing()
        {
            SetupBinaryInstruction(Instruction.CMP, 0x1234, 0x1234);
            flasher.WriteInstruction(Instruction.JNE, 0x008f);

            ExecuteProgram();
        }

        [Test]
        public void JNER_EqualFlagIsNotSet_ChangesPC()
        {
            SetupBinaryInstruction(Instruction.CMP, 0x1234, 0x4321);
            LoadValueIntoRegister(0x008f, Register.R3);
            flasher.WriteInstruction(Instruction.JNER, Register.R3);

            ExecuteProgram(0x008f);
        }

        [Test]
        public void JNER_EqualFlagIsSet_DoesNothing()
        {
            SetupBinaryInstruction(Instruction.CMP, 0x1234, 0x1234);
            LoadValueIntoRegister(0x008f, Register.R3);
            flasher.WriteInstruction(Instruction.JNER, Register.R3);

            ExecuteProgram();
        }

        [Test]
        public void JZ_ZeroFlagIsSet_ChangesPC()
        {
            LoadValueIntoRegisterR0(0x0000);
            flasher.WriteInstruction(Instruction.CMPZ, Register.R0);
            flasher.WriteInstruction(Instruction.JZ, 0x008f);

            ExecuteProgram(0x008f);
        }

        [Test]
        public void JZ_ZeroFlagIsNotSet_ChangesPC()
        {
            LoadValueIntoRegisterR0(0xffff);
            flasher.WriteInstruction(Instruction.CMPZ, Register.R0);
            flasher.WriteInstruction(Instruction.JZ, 0x008f);

            ExecuteProgram();
        }

        [Test]
        public void JZR_ZeroFlagIsSet_ChangesPC()
        {
            LoadValueIntoRegister(0x0000, Register.R0);
            flasher.WriteInstruction(Instruction.CMPZ, Register.R0);
            LoadValueIntoRegister(0x008f, Register.R3);
            flasher.WriteInstruction(Instruction.JZR, Register.R3);

            ExecuteProgram(0x008f);
        }

        [Test]
        public void JZR_ZeroFlagIsNotSet_ChangesPC()
        {
            LoadValueIntoRegister(0xffff, Register.R0);
            flasher.WriteInstruction(Instruction.CMPZ, Register.R0);
            LoadValueIntoRegister(0x008f, Register.R3);
            flasher.WriteInstruction(Instruction.JZR, Register.R3);

            ExecuteProgram();
        }

        [Test]
        public void JNZ_ZeroFlagIsNotSet_ChangesPC()
        {
            LoadValueIntoRegisterR0(0xffff);
            flasher.WriteInstruction(Instruction.CMPZ, Register.R0);
            flasher.WriteInstruction(Instruction.JNZ, 0x008f);

            ExecuteProgram(0x008f);
        }

        [Test]
        public void JNZ_ZeroFlagIsSet_DoesNothing()
        {
            LoadValueIntoRegisterR0(0x0000);
            flasher.WriteInstruction(Instruction.CMPZ, Register.R0);
            flasher.WriteInstruction(Instruction.JNZ, 0x008f);

            ExecuteProgram();
        }

        [Test]
        public void JNZR_ZeroFlagIsNotSet_ChangesPC()
        {
            LoadValueIntoRegister(0xffff, Register.R0);
            flasher.WriteInstruction(Instruction.CMPZ, Register.R0);
            LoadValueIntoRegister(0x008f, Register.R3);
            flasher.WriteInstruction(Instruction.JNZR, Register.R3);

            ExecuteProgram(0x008f);
        }

        [Test]
        public void JNZR_ZeroFlagIsSet_DoesNothing()
        {
            LoadValueIntoRegister(0x0000, Register.R0);
            flasher.WriteInstruction(Instruction.CMPZ, Register.R0);
            LoadValueIntoRegister(0x008f, Register.R3);
            flasher.WriteInstruction(Instruction.JNZR, Register.R3);

            ExecuteProgram();
        }
        #endregion

        #region Subroutines
        [Test]
        public void CALL_SavesStateAndSetsPCToValue()
        {
            flasher.WriteInstruction(Instruction.CALL, 0x1234);

            processor.Step();

            Assert.That(processor.GetRegister(Register.PC), Is.EqualTo(0x1234));

            Assert.That(processor.GetRegister(Register.SP), Is.EqualTo(STACK_START_ADDRESS - STATE_SIZE));
            Assert.That(processor.GetRegister(Register.FP), Is.EqualTo(STACK_START_ADDRESS - STATE_SIZE));
        }

        [Test]
        public void CALLR_SavesStateAndSetsPCToValueFromRegister()
        {
            LoadValueIntoRegisterR0(0x1234);
            flasher.WriteInstruction(Instruction.CALLR, Register.R0);

            processor.Step();
            processor.Step();

            Assert.That(processor.GetRegister(Register.PC), Is.EqualTo(0x1234));

            Assert.That(processor.GetRegister(Register.SP), Is.EqualTo(STACK_START_ADDRESS - STATE_SIZE));
            Assert.That(processor.GetRegister(Register.FP), Is.EqualTo(STACK_START_ADDRESS - STATE_SIZE));
        }

        [Test]
        public void RET_LoadsSavedStateAndSetsPCToValueBeforeCall()
        {
            ushort subroutineAddress = 0x4000;

            LoadValueIntoRegister(0x0123, Register.R0);
            LoadValueIntoRegister(0x4567, Register.R1);
            LoadValueIntoRegister(0x89ab, Register.R2);
            LoadValueIntoRegister(0xcdef, Register.R3);
            flasher.WriteInstruction(Instruction.CALL, subroutineAddress);

            var returnAddress = flasher.Address;

            flasher.Address = subroutineAddress;
            LoadValueIntoRegister(0xffff, Register.R0);
            LoadValueIntoRegister(0xffff, Register.R1);
            LoadValueIntoRegister(0xffff, Register.R2);
            LoadValueIntoRegister(0xffff, Register.R3);
            flasher.WriteInstruction(Instruction.RET);

            // Load values into registers
            processor.Step();
            processor.Step();
            processor.Step();
            processor.Step();

            // Call
            processor.Step();

            Assert.That(processor.GetRegister(Register.PC), Is.EqualTo(0x4000));
            Assert.That(processor.GetRegister(Register.SP), Is.EqualTo(STACK_START_ADDRESS - 18));
            Assert.That(processor.GetRegister(Register.FP), Is.EqualTo(STACK_START_ADDRESS - 18));

            // Overwrite registers
            processor.Step();
            processor.Step();
            processor.Step();
            processor.Step();

            Assert.That(processor.GetRegister(Register.R0), Is.EqualTo(0xffff));
            Assert.That(processor.GetRegister(Register.R1), Is.EqualTo(0xffff));
            Assert.That(processor.GetRegister(Register.R2), Is.EqualTo(0xffff));
            Assert.That(processor.GetRegister(Register.R3), Is.EqualTo(0xffff));

            // Return
            processor.Step();

            Assert.That(processor.GetRegister(Register.PC), Is.EqualTo(returnAddress));
            Assert.That(processor.GetRegister(Register.SP), Is.EqualTo(STACK_START_ADDRESS));
            Assert.That(processor.GetRegister(Register.FP), Is.EqualTo(STACK_START_ADDRESS));

            Assert.That(processor.GetRegister(Register.R0), Is.EqualTo(0x0123));
            Assert.That(processor.GetRegister(Register.R1), Is.EqualTo(0x4567));
            Assert.That(processor.GetRegister(Register.R2), Is.EqualTo(0x89ab));
            Assert.That(processor.GetRegister(Register.R3), Is.EqualTo(0xcdef));
        }

        [Test]
        public void Subroutine_AcceptsArgumentsAndReturnsValues()
        {
            // TODO: implement
        }

        // TODO: ret on empty call stack
        // TODO: stack overflow via CALL and CALLR

        // TODO: Subroutine instructions
        #endregion

        #region Stack
        [Test]
        public void PUSH_PutsValueFromRegisterOntoStack()
        {
            LoadValueIntoRegisterR0(0x1234);
            flasher.WriteInstruction(Instruction.PUSH, Register.R0);

            Assert.That(processor.GetRegister(Register.SP), Is.EqualTo(STACK_START_ADDRESS));

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.SP), Is.Not.EqualTo(STACK_START_ADDRESS));
        }

        [Test]
        public void PUSH_CanFillStack()
        {
            FillStack();

            Assert.That(processor.GetRegister(Register.SP), Is.EqualTo(STACK_END_ADDRESS - Processor.DATASIZE));
        }

        [Test]
        public void PUSH_FullStack_ThrowsException()
        {
            FillStack();

            flasher.WriteInstruction(Instruction.PUSH, Register.R0);

            AssertExceptionOccursAndProcessorResets(typeof(StackOverflowException), "Stack is full.");
        }

        [Test]
        public void POP_PutsValueFromStackIntoRegister()
        {
            LoadValueIntoRegisterR0(0x1234);
            flasher.WriteInstruction(Instruction.PUSH, Register.R0);
            flasher.WriteInstruction(Instruction.POP, Register.R1);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.SP), Is.EqualTo(STACK_START_ADDRESS));

            Assert.That(processor.GetRegister(Register.R1), Is.EqualTo(0x1234));
        }

        [Test]
        public void POP_OnEmptyStack_ThrowsException()
        {
            flasher.WriteInstruction(Instruction.POP, Register.R0);

            AssertExceptionOccursAndProcessorResets(typeof(InvalidOperationException), "Stack is empty.");
        }

        [Test]
        public void PEEK_PutsValueFromStackIntoRegisterWithoutRemovingItFromStack()
        {
            LoadValueIntoRegisterR0(0x1234);
            flasher.WriteInstruction(Instruction.PUSH, Register.R0);
            flasher.WriteInstruction(Instruction.PEEK, Register.R1);

            ExecuteProgram();

            Assert.That(processor.GetRegister(Register.SP), Is.Not.EqualTo(STACK_START_ADDRESS));

            Assert.That(processor.GetRegister(Register.R1), Is.EqualTo(0x1234));
        }

        [Test]
        public void PEEK_OnEmptyStack_ThrowsException()
        {
            flasher.WriteInstruction(Instruction.PEEK, Register.R0);

            AssertExceptionOccursAndProcessorResets(typeof(InvalidOperationException), "Stack is empty.");
        }
        #endregion

        #region Processor
        [Test]
        public void HALT_HaltsExecution()
        {
            FlashNNops(50);

            flasher.WriteInstruction(Instruction.HALT);

            processor.Run();

            Assert.That(processor.GetRegister(Register.PC), Is.EqualTo(flasher.Address));

            Assert.That(haltOccured, Is.True);
        }

        [Test]
        public void RESET_ResetsProcessorToBeginningOfProgram()
        {
            FlashNNops(50);

            flasher.WriteInstruction(Instruction.RESET);

            processor.Run();

            Assert.That(resetOccured, Is.True);
            Assert.That(exception, Is.Null);

            AssertProcessorIsInInitialState();
        }
        #endregion

        // TODO: Segfaults:
        // FUTURE: memory access (Load and Store) outside of DATA (static or dynamic) is invalid
        // FUTURE: executing outside of CODE is invalid (Jump, Logic, Subroutines)

        [Test]
        public void InvalidInstruction_ResetsAndThrowsException()
        {
            flasher.WriteInstruction((Instruction)0xff);

            AssertExceptionOccursAndProcessorResets(
                typeof(InvalidOperationException),
                "Unknown instruction 0xFF.");
        }
        #endregion
    }
}
#pragma warning restore CA1822 // Mark members as static