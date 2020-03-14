#pragma warning disable CA1822 // Mark members as static
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using VM.Hardware;
using VM.Software.Assembling;

namespace VM.HardwareTests.Tests
{
    public class ProcessorTests
    {
        private const uint MEMORY_SIZE = 0x8000;
        private const ushort STACK_SIZE = 0x400;
        private const ushort STACK_START_ADDRESS = (ushort)(MEMORY_SIZE - Processor.DATASIZE);
        private const ushort STACK_END_ADDRESS = STACK_START_ADDRESS - ((STACK_SIZE - 1) * Processor.DATASIZE);

        private Memory memory;
        private Processor processor;
        private Flasher flasher;

        private bool resetOccured;
        private Exception resetException;
        private Instruction resetInstruction;


        private void OnReset(object sender, ResetEventArgs e)
        {
            resetOccured = true;
            resetInstruction = e.Instruction;
            resetException = e.Exception;
        }

        [SetUp]
        public void Setup()
        {
            memory = new Memory(MEMORY_SIZE);
            flasher = new Flasher(memory);
            processor = new Processor(memory, STACK_SIZE);

            resetOccured = false;
            resetException = null;
            processor.Reset += OnReset;
        }

        [TearDown]
        public void TearDown()
        {
            processor.Reset -= OnReset;
        }

        #region Helper methods
        private void Flash(params string[] source)
        {
            flasher.Flash(Assembler.Assemble(string.Join(Environment.NewLine, source)));
        }

        private void Flash(ushort address, params byte[] values)
        {
            flasher.Flash(address, values);
        }

        private void Flash(ushort address, params ushort[] values)
        {
            flasher.Flash(address, values);
        }

        private void ExecuteProgram(int steps, ushort? expectedAddress = null)
        {
            if (expectedAddress == null)
            {
                expectedAddress = flasher.Address;
            }

            for (var i = 0; i < steps; i++)
            {
                processor.Step();
            }

            Assert.That(processor.ProgramCounter, Is.EqualTo(expectedAddress.Value));

            Assert.That(resetException, Is.Null);
        }

        private void FlashAndExecute(string source, ushort? expectedAddress = null)
        {
            flasher.Address = 0;

            Flash(source);

            ExecuteProgram(1, expectedAddress);
        }

        private void FlashAndExecute(IEnumerable<string> source, ushort? expectedAddress = null)
        {
            flasher.Address = 0;

            Flash(string.Join(Environment.NewLine, source));

            ExecuteProgram(source.Count(), expectedAddress);
        }

        private void AssertZeroValueAndFlag(Register register = Register.ACC)
        {
            Assert.That(processor[register], Is.Zero);
            Assert.That(processor[Flags.ZERO], Is.True);
        }

        private void AssertProcessorIsInInitialState()
        {
            Assert.That(processor.ProgramCounter, Is.Zero);
            Assert.That(processor[Register.ACC], Is.Zero);
            Assert.That(processor[Register.FLAG], Is.Zero);
            Assert.That(processor[Register.SP], Is.EqualTo(STACK_START_ADDRESS));
            Assert.That(processor[Register.FP], Is.EqualTo(STACK_START_ADDRESS));
            Assert.That(processor[Register.R0], Is.Zero);
            Assert.That(processor[Register.R1], Is.Zero);
            Assert.That(processor[Register.R2], Is.Zero);
            Assert.That(processor[Register.R3], Is.Zero);
            Assert.That(processor[Register.R4], Is.Zero);
            Assert.That(processor[Register.R5], Is.Zero);
            Assert.That(processor[Register.R6], Is.Zero);
            Assert.That(processor[Register.R7], Is.Zero);
            Assert.That(processor[Register.T0], Is.Zero);
            Assert.That(processor[Register.T1], Is.Zero);
            Assert.That(processor[Register.T2], Is.Zero);
            Assert.That(processor[Register.T3], Is.Zero);
            Assert.That(processor[Register.T4], Is.Zero);
            Assert.That(processor[Register.T5], Is.Zero);
            Assert.That(processor[Register.T6], Is.Zero);
            Assert.That(processor[Register.T7], Is.Zero);
        }

        private void AssertExceptionOccursAndProcessorResets(Type exceptionType, Instruction expectedInstruction, string expectedMessage)
        {
            Assert.That(() => processor.Step(), Throws.Exception.TypeOf(exceptionType)
                .With.Message.EqualTo(expectedMessage));

            Assert.That(resetOccured, Is.True);
            Assert.That(resetInstruction, Is.EqualTo(expectedInstruction));
            Assert.That(resetException.Message, Is.EqualTo(expectedMessage));

            AssertProcessorIsInInitialState();
        }
        #endregion

        [Test]
        public void Constructor_NullMemory_ResetsAndThrowsException()
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
            var count = 10;

            var tickCount = 0;

            void OnTick(object sender, EventArgs e)
            {
                tickCount++;
            }

            processor.Tick += OnTick;

            FlashAndExecute(Enumerable.Repeat("NOP", count));

            Assert.That(tickCount, Is.EqualTo(count));

            processor.Tick -= OnTick;
        }

        [Test]
        public void ModifyingPrivateRegistersDirectly_ResetsAndThrowsException()
        {
            var stackRegisters = new Register[]
            {
                Register.PC, Register.ARG, Register.RET
            };

            foreach (var register in stackRegisters)
            {
                flasher.Address = 0;

                Flash($"LDVR 0x1234 ${register}");

                AssertExceptionOccursAndProcessorResets(
                    typeof(InvalidOperationException), Instruction.LDVR,
                    $"{register} register cannot be modified directly by code.");
            }
        }

        [Test]
        public void NonPrivateRegister_ValuesCanBeModified()
        {
            var publicRegisters = new Register[]
            {
                Register.ACC, Register.FLAG, Register.SP, Register.FP,

                Register.R0, Register.R1, Register.R2, Register.R3,
                Register.R4, Register.R5, Register.R6, Register.R7,
                Register.R8, Register.R9, Register.R10, Register.R11,
                Register.R12, Register.R13, Register.R14, Register.R15,

                Register.S0, Register.S1, Register.S2, Register.S3,
                Register.S4, Register.S5, Register.S6, Register.S7,
                Register.S8, Register.S9, Register.S10, Register.S11,
                Register.S12, Register.S13, Register.S14, Register.S15,

                Register.T0, Register.T1, Register.T2, Register.T3,
                Register.T4, Register.T5, Register.T6, Register.T7,
                Register.T8, Register.T9, Register.T10, Register.T11,
                Register.T12, Register.T13, Register.T14, Register.T15,
            };

            var source = new List<string>();

            foreach (var register in publicRegisters)
            {
                source.Add($"LDVR 0x1234 ${register}");
            }

            FlashAndExecute(source);

            foreach (var register in publicRegisters)
            {
                Assert.That(processor[register], Is.EqualTo(0x1234));
            }
        }

        [Test]
        public void InvalidInstruction_ResetsAndThrowsException()
        {
            flasher.Flash(0xff);

            AssertExceptionOccursAndProcessorResets(
                typeof(InvalidOperationException), (Instruction)0xff,
                "Unknown instruction 0xFF.");
        }

        [Test]
        public void InvalidRegisterInInstruction_ResetsAndThrowsException()
        {
            flasher.Flash(new byte[] { (byte)Instruction.INC, 0xff });

            AssertExceptionOccursAndProcessorResets(
                typeof(InvalidOperationException), Instruction.INC,
                "Unknown register 0xFF.");
        }

        [Test]
        public void GetRegister_InvalidRegister_ResetsAndThrowsException()
        {
            Assert.That(() => processor[(Register)0xff], Throws.InvalidOperationException
                .With.Message.EqualTo("Unknown register 0xFF."));
        }

        [Test]
        public void InvalidMemoryAccess_ResetsAndThrowsException()
        {
            Flash("STVA 0x1234 0xffff");

            AssertExceptionOccursAndProcessorResets(
                typeof(IndexOutOfRangeException), Instruction.STVA,
                "Invalid memory address 0xFFFF. Valid Range: [0x0000, 0x7FFF]");
        }

        [Test]
        public void ResetProcessor_ResetsProcessor()
        {
            FlashAndExecute("NOP");

            Assert.That(processor[Register.PC], Is.Not.Zero);

            processor.ResetProcessor();

            AssertProcessorIsInInitialState();

            Assert.That(resetOccured, Is.True);
            Assert.That(resetException, Is.Null);
        }

        #region Instructions
        [Test]
        public void NOP_DoesNothing()
        {
            FlashAndExecute(Enumerable.Repeat("NOP", 10));
        }

        #region Register
        [Test]
        public void MOVE_MovesValueFromRegisterAToRegisterB()
        {
            var source = new string[]
            {
                "LDVR 0x1234 $R0",
                "MOVE $R0 $R1"
            };

            FlashAndExecute(source);

            Assert.That(processor[Register.R1], Is.EqualTo(0x1234));
        }

        [Test]
        public void INC_IncrementsRegisterValue()
        {
            var source = new string[]
            {
                "LDVR 0x1234 $R0",
                "INC $R0"
            };

            FlashAndExecute(source);

            Assert.That(processor[Register.R0], Is.EqualTo(0x1235));
            Assert.That(processor[Flags.CARRY], Is.False);
        }

        [Test]
        public void INC_Overflow_WrapsAndSetsZeroAndCarryFlags()
        {
            var source = new string[]
            {
                "LDVR 0xffff $R0",
                "INC $R0"
            };

            FlashAndExecute(source);

            AssertZeroValueAndFlag(Register.R0);
            Assert.That(processor[Flags.CARRY], Is.True);
        }

        [Test]
        public void DEC_DecrementsRegisterValue()
        {
            var source = new string[]
            {
                "LDVR 0x1234 $R0",
                "DEC $R0"
            };

            FlashAndExecute(source);

            Assert.That(processor[Register.R0], Is.EqualTo(0x1233));
            Assert.That(processor[Flags.CARRY], Is.False);
        }

        [Test]
        public void DEC_Underflow_WrapsAndSetsCarryFlag()
        {
            var source = new string[]
            {
                "LDVR 0x0000 $R0",
                "DEC $R0"
            };

            FlashAndExecute(source);

            Assert.That(processor[Register.R0], Is.EqualTo(0xffff));
            Assert.That(processor[Flags.CARRY], Is.True);
        }

        [Test]
        public void DEC_ZeroResult_SetsZeroFlag()
        {
            var source = new string[]
            {
                "LDVR 0x0001 $R0",
                "DEC $R0"
            };

            FlashAndExecute(source);

            AssertZeroValueAndFlag(Register.R0);
        }
        #endregion

        #region Load
        [Test]
        public void LDVR_LoadsValueIntoRegister()
        {
            FlashAndExecute("LDVR 0x1234 $R0");

            Assert.That(processor[Register.R0], Is.EqualTo(0x1234));
        }

        [Test]
        public void LDAR_LoadsValueFromAddressIntoRegister()
        {
            Flash(0x10, 0x1234);

            FlashAndExecute("LDAR 0x10 $R0");

            Assert.That(processor[Register.R0], Is.EqualTo(0x1234));
        }

        [Test]
        public void LDRR_LoadsValueFromAddressInRegisterIntoRegister()
        {
            Flash(0x10, 0x1234);

            FlashAndExecute(new string[]
            {
                "LDVR 0x10 $R0",
                "LDRR $R0 $R1"
            });

            Assert.That(processor[Register.R1], Is.EqualTo(0x1234));
        }

        [Test]
        public void LBVR_LoadsByteValueIntoRegister()
        {
            FlashAndExecute("LBVR 0xff $R0");

            Assert.That(processor[Register.R0], Is.EqualTo(0x00ff));
        }

        [Test]
        public void LBAR_LoadsByteValueFromAddressIntoRegister()
        {
            Flash(0x10, 0xff);

            FlashAndExecute("LBAR 0x10 $R0");

            Assert.That(processor[Register.R0], Is.EqualTo(0x00ff));
        }

        [Test]
        public void LBRR_LoadsByteValueFromAddressInRegisterIntoRegister()
        {
            Flash(0x10, 0xff);

            FlashAndExecute(new string[]
            {
                "LDVR 0x10 $R0",
                "LBRR $R0 $R1"
            });

            Assert.That(processor[Register.R1], Is.EqualTo(0x00ff));
        }
        #endregion

        #region Store
        [Test]
        public void STVA_StoresValueAtAddress()
        {
            Assert.That(memory.GetU16(0x10), Is.Zero);

            FlashAndExecute("STVA 0x1234 0x10");

            Assert.That(memory.GetU16(0x10), Is.EqualTo(0x1234));
        }

        [Test]
        public void STVR_StoresValueAtAddressInRegister()
        {
            Assert.That(memory.GetU16(0x10), Is.Zero);

            FlashAndExecute(new string[]
            {
                "LDVR 0x10 $R0",
                "STVR 0x1234 $R0"
            });

            Assert.That(memory.GetU16(0x10), Is.EqualTo(0x1234));
        }

        [Test]
        public void STRA_StoresValueInRegisterAtAddress()
        {
            Assert.That(memory.GetU16(0x10), Is.Zero);

            FlashAndExecute(new string[]
            {
                "LDVR 0x1234 $R0",
                "STRA $R0 0x10"
            });

            Assert.That(memory.GetU16(0x10), Is.EqualTo(0x1234));
        }

        [Test]
        public void STRR_StoresValueInRegisterAtAddressInRegister()
        {
            Assert.That(memory.GetU16(0x10), Is.Zero);

            FlashAndExecute(new string[]
            {
                "LDVR 0x1234 $R0",
                "LDVR 0x10 $R1",
                "STRR $R0 $R1"
            });

            Assert.That(memory.GetU16(0x10), Is.EqualTo(0x1234));
        }

        [Test]
        public void SBVA_StoresByteValueAtAddress()
        {
            Assert.That(memory.GetU8(0x10), Is.Zero);

            FlashAndExecute("SBVA 0xff 0x10");

            Assert.That(memory.GetU8(0x10), Is.EqualTo(0xff));
        }

        [Test]
        public void SBVR_StoresByteValueAtAddressInRegister()
        {
            Assert.That(memory.GetU8(0x10), Is.Zero);

            FlashAndExecute(new string[]
            {
                "LDVR 0x10 $R0",
                "SBVR 0xff $R0"
            });

            Assert.That(memory.GetU8(0x10), Is.EqualTo(0xff));
        }

        [Test]
        public void SBRA_StoresByteValueInRegisterAtAddress()
        {
            Assert.That(memory.GetU8(0x10), Is.Zero);

            FlashAndExecute(new string[]
            {
                "LBVR 0xff $R0",
                "SBRA $R0 0x10"
            });

            Assert.That(memory.GetU8(0x10), Is.EqualTo(0xff));
        }

        [Test]
        public void SBRR_StoresByteValueInRegisterAtAddressInRegister()
        {
            Assert.That(memory.GetU8(0x10), Is.Zero);

            FlashAndExecute(new string[]
            {
                "LBVR 0xff $R0",
                "LDVR 0x10 $R1",
                "SBRR $R0 $R1"
            });

            Assert.That(memory.GetU8(0x10), Is.EqualTo(0xff));
        }
        #endregion

        #region Arithmetic
        [Test]
        public void ADD_AdditionOfTwoRegisterValues()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x1200 $R0",
                "LDVR 0x0034 $R1",
                "ADD $R0 $R1"
            });

            Assert.That(processor[Register.ACC], Is.EqualTo(0x1234));
            Assert.That(processor[Flags.CARRY], Is.False);
        }

        [Test]
        public void ADD_Overflow_WrapsAndSetsCarryFlag()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0xff00 $R0",
                "LDVR 0x1000 $R1",
                "ADD $R0 $R1"
            });

            Assert.That(processor[Register.ACC], Is.EqualTo(0x0f00));
            Assert.That(processor[Flags.CARRY], Is.True);
        }

        [Test]
        public void ADD_ZeroResult_SetsZeroAndCarryFlags()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0xff00 $R0",
                "LDVR 0x0100 $R1",
                "ADD $R0 $R1"
            });

            AssertZeroValueAndFlag();
            Assert.That(processor[Flags.CARRY], Is.True);
        }

        [Test]
        public void SUB_SubtractionOfTwoRegisterValues()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x1200 $R0",
                "LDVR 0x0034 $R1",
                "SUB $R0 $R1"
            });

            Assert.That(processor[Register.ACC], Is.EqualTo(0x11cc));
            Assert.That(processor[Flags.CARRY], Is.False);
        }

        [Test]
        public void SUB_Underflow_WrapsAndSetsCarryFlag()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x1000 $R0",
                "LDVR 0xff00 $R1",
                "SUB $R0 $R1"
            });

            Assert.That(processor[Register.ACC], Is.EqualTo(0x1100));
            Assert.That(processor[Flags.CARRY], Is.True);
        }

        [Test]
        public void SUB_ZeroResult_SetsZeroFlag()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x1234 $R0",
                "LDVR 0x1234 $R1",
                "SUB $R0 $R1"
            });

            AssertZeroValueAndFlag();
        }

        [Test]
        public void MUL_MultiplicationOfTwoRegisterValues()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x00f0 $R0",
                "LDVR 0x00f0 $R1",
                "MUL $R0 $R1"
            });

            Assert.That(processor[Register.ACC], Is.EqualTo(0xe100));
            Assert.That(processor[Flags.CARRY], Is.False);
        }

        [Test]
        public void MUL_Overflow_WrapsAndSetsCarryFlag()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x1234 $R0",
                "LDVR 0xffff $R1",
                "MUL $R0 $R1"
            });

            Assert.That(processor[Register.ACC], Is.EqualTo(0xedcc));
            Assert.That(processor[Flags.CARRY], Is.True);
        }

        [Test]
        public void MUL_ZeroResult_SetsZeroFlag()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x1234 $R0",
                "LDVR 0x0000 $R1",
                "MUL $R0 $R1"
            });

            AssertZeroValueAndFlag();
        }

        [Test]
        public void DIV_DivisionOfTwoRegisterValues()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x0040 $R0",
                "LDVR 0x0002 $R1",
                "DIV $R0 $R1"
            });

            Assert.That(processor[Register.ACC], Is.EqualTo(0x0020));
            Assert.That(processor[Flags.CARRY], Is.False);
        }

        [Test]
        public void DIV_ByZero_ResetsAndThrowsException()
        {
            Flash(
                "LDVR 0x1234 $R0",
                "LDVR 0x0000 $R1",
                "DIV $R0 $R1"
            );

            processor.Step();
            processor.Step();

            AssertExceptionOccursAndProcessorResets(
                typeof(DivideByZeroException), Instruction.DIV,
                "Attempted to divide by zero.");
        }

        [Test]
        public void DIV_ZeroResult_SetsZeroFlag()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x0001 $R0",
                "LDVR 0xffff $R1",
                "DIV $R0 $R1"
            });

            AssertZeroValueAndFlag();
        }

        [Test]
        public void MOD_ModuloOfTwoRegisterValues()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x0005 $R0",
                "LDVR 0x0003 $R1",
                "MOD $R0 $R1"
            });

            Assert.That(processor[Register.ACC], Is.EqualTo(0x0002));
            Assert.That(processor[Flags.CARRY], Is.False);
        }

        [Test]
        public void MOD_ByZero_ResetsAndThrowsException()
        {
            Flash(
                "LDVR 0x1234 $R0",
                "LDVR 0x0000 $R1",
                "MOD $R0 $R1"
            );

            processor.Step();
            processor.Step();

            AssertExceptionOccursAndProcessorResets(
                typeof(DivideByZeroException), Instruction.MOD,
                "Attempted to divide by zero.");
        }

        [Test]
        public void MOD_ZeroResult_SetsZeroFlag()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x000f $R0",
                "LDVR 0x0005 $R1",
                "MOD $R0 $R1"
            });

            AssertZeroValueAndFlag();
        }
        #endregion

        #region Bitwise
        [Test]
        public void AND_BinaryAndOfTwoRegisterValues()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x00ff $R0",
                "LDVR 0xaaaa $R1",
                "AND $R0 $R1"
            });

            Assert.That(processor[Register.ACC], Is.EqualTo(0x00aa));
        }

        [Test]
        public void AND_ZeroResult_SetsZeroFlag()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x00ff $R0",
                "LDVR 0xff00 $R1",
                "AND $R0 $R1"
            });

            AssertZeroValueAndFlag();
        }

        [Test]
        public void OR_BinaryOrOfTwoRegisterValues()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x0055 $R0",
                "LDVR 0xaaaa $R1",
                "OR $R0 $R1"
            });

            Assert.That(processor[Register.ACC], Is.EqualTo(0xaaff));
        }

        [Test]
        public void OR_ZeroResult_SetsZeroFlag()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x0000 $R0",
                "LDVR 0x0000 $R1",
                "OR $R0 $R1"
            });

            AssertZeroValueAndFlag();
        }

        [Test]
        public void XOR_BinaryXorOfTwoRegisterValues()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x00ff $R0",
                "LDVR 0xaaaa $R1",
                "XOR $R0 $R1"
            });

            Assert.That(processor[Register.ACC], Is.EqualTo(0xaa55));
        }

        [Test]
        public void XOR_ZeroResult_SetsZeroFlag()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x00ff $R0",
                "LDVR 0x00ff $R1",
                "XOR $R0 $R1"
            });

            AssertZeroValueAndFlag();
        }

        [Test]
        public void NOT_BinaryNotOfRegisterValue()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0xaaaa $R0",
                "NOT $R0"
            });

            Assert.That(processor[Register.ACC], Is.EqualTo(0x5555));
        }

        [Test]
        public void NOT_ZeroResult_SetsZeroFlag()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0xffff $R0",
                "NOT $R0"
            });

            AssertZeroValueAndFlag();
        }

        [Test]
        public void SRL_ShiftsRegisterLeftByAmount([Range(0, 15)] int shiftAmount)
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x0001 $R0",
               $"SRL $R0 {shiftAmount}"
            });

            Assert.That(processor[Register.ACC], Is.EqualTo(0x0001 << shiftAmount));
        }

        [Test]
        public void SRL_ExcessiveShift_ZeroesOutRegister()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x0001 $R0",
                "SRL $R0 16"
            });

            AssertZeroValueAndFlag();
        }

        [Test]
        public void SRLR_ShiftsRegisterLeftByAmountInRegister([Range(0, 15)] int shiftAmount)
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x0001 $R0",
               $"LBVR {shiftAmount} $R1",
                "SRLR $R0 $R1"
            });

            Assert.That(processor[Register.ACC], Is.EqualTo(0x0001 << shiftAmount));
        }

        [Test]
        public void SRLR_ExcessiveShift_ZeroesOutRegister()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x0001 $R0",
                "LBVR 16 $R1",
                "SRLR $R0 $R1"
            });

            AssertZeroValueAndFlag();
        }

        [Test]
        public void SRR_ShiftsRegisterRightByAmount([Range(0, 15)] int shiftAmount)
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x8000 $R0",
               $"SRR $R0 {shiftAmount}"
            });

            Assert.That(processor[Register.ACC], Is.EqualTo(0x8000 >> shiftAmount));
        }

        [Test]
        public void SRR_ExcessiveShift_ZeroesOutRegister()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x8000 $R0",
                "SRR $R0 16"
            });

            AssertZeroValueAndFlag();
        }

        [Test]
        public void SRRR_ShiftsRegisterLeftByAmountInRegister([Range(1, 15)] int shiftAmount)
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x8000 $R0",
               $"LBVR {shiftAmount} $R1",
                "SRRR $R0 $R1"
            });

            Assert.That(processor[Register.ACC], Is.EqualTo(0x8000 >> shiftAmount));
        }

        [Test]
        public void SRRR_ExcessiveShift_ZeroesOutRegister()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x8000 $R0",
                "LBVR 16 $R1",
                "SRRR $R0 $R1"
            });

            AssertZeroValueAndFlag();
        }
        #endregion

        #region Jump
        [Test]
        public void JUMP_DoesUnconditionalJumpUsingAddress()
        {
            FlashAndExecute("JUMP 0x1234", 0x1234);
        }

        [Test]
        public void JUMPR_DoesUnconditionalJumpUsingRegisterValue()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x1234 $R0",
                "JUMPR $R0"
            }
            , 0x1234);
        }
        #endregion

        #region Logic
        [Test]
        public void CMP_LessThan_SetsLessThanFlag()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x1234 $R0",
                "LDVR 0x4321 $R1",
                "CMP $R0 $R1"
            });

            Assert.That(processor[Flags.LESSTHAN], Is.True);
            Assert.That(processor[Flags.EQUAL], Is.False);
            Assert.That(processor[Flags.GREATERTHAN], Is.False);
        }

        [Test]
        public void CMP_EqualTo_SetsEqualFlag()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x1234 $R0",
                "LDVR 0x1234 $R1",
                "CMP $R0 $R1"
            });

            Assert.That(processor[Flags.LESSTHAN], Is.False);
            Assert.That(processor[Flags.EQUAL], Is.True);
            Assert.That(processor[Flags.GREATERTHAN], Is.False);
        }

        [Test]
        public void CMP_GreaterThan_SetsGreaterThanFlag()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x4321 $R0",
                "LDVR 0x1234 $R1",
                "CMP $R0 $R1"
            });

            Assert.That(processor[Flags.LESSTHAN], Is.False);
            Assert.That(processor[Flags.EQUAL], Is.False);
            Assert.That(processor[Flags.GREATERTHAN], Is.True);
        }

        [Test]
        public void CMPZ_Zero_SetsZeroFlag()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x0000 $R0",
                "CMPZ $R0"
            });

            Assert.That(processor[Flags.ZERO], Is.True);
        }

        [Test]
        public void CMPZ_NotZero_DoesNothing()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0xffff $R0",
                "CMPZ $R0"
            });

            Assert.That(processor[Flags.ZERO], Is.False);
        }

        [Test]
        public void JLT_LessThanFlagIsSet_ChangesPC()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x1234 $R0",
                "LDVR 0x4321 $R1",
                "CMP $R0 $R1",
                "JLT 0x008f"
            }, 0x008f);
        }

        [Test]
        public void JLT_LessThanFlagIsNotSet_DoesNothing()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x4321 $R0",
                "LDVR 0x1234 $R1",
                "CMP $R0 $R1",
                "JLT 0x008f"
            });
        }

        [Test]
        public void JLTR_LessThanFlagIsSet_ChangesPC()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x1234 $R0",
                "LDVR 0x4321 $R1",
                "CMP $R0 $R1",
                "LDVR 0x008f $R2",
                "JLTR $R2"
            }, 0x008f);
        }

        [Test]
        public void JLTR_LessThanFlagIsNotSet_DoesNothing()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x4321 $R0",
                "LDVR 0x1234 $R1",
                "CMP $R0 $R1",
                "LDVR 0x008f $R2",
                "JLTR $R2"
            });
        }

        [Test]
        public void JGT_GreaterThanFlagIsSet_ChangesPC()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x4321 $R0",
                "LDVR 0x1234 $R1",
                "CMP $R0 $R1",
                "JGT 0x008f"
            }, 0x008f);
        }

        [Test]
        public void JGT_GreaterThanFlagIsNotSet_ChangesPC()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x1234 $R0",
                "LDVR 0x4321 $R1",
                "CMP $R0 $R1",
                "JGT 0x008f"
            });
        }

        [Test]
        public void JGTR_GreaterThanFlagIsSet_ChangesPC()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x4321 $R0",
                "LDVR 0x1234 $R1",
                "CMP $R0 $R1",
                "LDVR 0x008f $R2",
                "JGTR $R2"
            }, 0x008f);
        }

        [Test]
        public void JGTR_GreaterThanFlagIsNotSet_DoesNothing()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x1234 $R0",
                "LDVR 0x4321 $R1",
                "CMP $R0 $R1",
                "LDVR 0x008f $R2",
                "JGTR $R2"
            });
        }

        [Test]
        public void JE_EqualFlagIsSet_ChangesPC()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x1234 $R0",
                "LDVR 0x1234 $R1",
                "CMP $R0 $R1",
                "JE 0x008f"
            }, 0x008f);
        }

        [Test]
        public void JE_EqualFlagIsNotSet_DoesNothing()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x1234 $R0",
                "LDVR 0x4321 $R1",
                "CMP $R0 $R1",
                "JE 0x008f"
            });
        }

        [Test]
        public void JER_EqualFlagIsSet_ChangesPC()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x1234 $R0",
                "LDVR 0x1234 $R1",
                "CMP $R0 $R1",
                "LDVR 0x008f $R2",
                "JER $R2"
            }, 0x008f);
        }

        [Test]
        public void JER_EqualFlagIsNotSet_DoesNothing()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x1234 $R0",
                "LDVR 0x4321 $R1",
                "CMP $R0 $R1",
                "LDVR 0x008f $R2",
                "JER $R2"
            });
        }

        [Test]
        public void JNE_EqualFlagIsNotSet_ChangesPC()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x1234 $R0",
                "LDVR 0x4321 $R1",
                "CMP $R0 $R1",
                "JNE 0x008f"
            }, 0x008f);
        }

        [Test]
        public void JNE_EqualFlagIsSet_DoesNothing()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x1234 $R0",
                "LDVR 0x1234 $R1",
                "CMP $R0 $R1",
                "JNE 0x008f"
            });
        }

        [Test]
        public void JNER_EqualFlagIsNotSet_ChangesPC()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x1234 $R0",
                "LDVR 0x4321 $R1",
                "CMP $R0 $R1",
                "LDVR 0x008f $R2",
                "JNER $R2"
            }, 0x008f);
        }

        [Test]
        public void JNER_EqualFlagIsSet_DoesNothing()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x1234 $R0",
                "LDVR 0x1234 $R1",
                "CMP $R0 $R1",
                "LDVR 0x008f $R2",
                "JNER $R2"
            });
        }

        [Test]
        public void JZ_ZeroFlagIsSet_ChangesPC()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x0000 $R0",
                "CMPZ $R0",
                "JZ 0x008f"
            }, 0x008f);
        }

        [Test]
        public void JZ_ZeroFlagIsNotSet_DoesNothing()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0xffff $R0",
                "CMPZ $R0",
                "JZ 0x008f"
            });
        }

        [Test]
        public void JZR_ZeroFlagIsSet_ChangesPC()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x0000 $R0",
                "CMPZ $R0",
                "LDVR 0x008f $R1",
                "JZR $R1"
            }, 0x008f);
        }

        [Test]
        public void JZR_ZeroFlagIsNotSet_DoesNothing()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0xffff $R0",
                "CMPZ $R0",
                "LDVR 0x008f $R1",
                "JZR $R1"
            });
        }

        [Test]
        public void JNZ_ZeroFlagIsNotSet_ChangesPC()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0xffff $R0",
                "CMPZ $R0",
                "JNZ 0x008f"
            }, 0x008f);
        }

        [Test]
        public void JNZ_ZeroFlagIsSet_DoesNothing()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x0000 $R0",
                "CMPZ $R0",
                "JNZ 0x008f"
            });
        }

        [Test]
        public void JNZR_ZeroFlagIsNotSet_ChangesPC()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0xffff $R0",
                "CMPZ $R0",
                "LDVR 0x008f $R1",
                "JNZR $R1"
            }, 0x008f);
        }

        [Test]
        public void JNZR_ZeroFlagIsSet_DoesNothing()
        {
            FlashAndExecute(new string[]
            {
                "LDVR 0x0000 $R0",
                "CMPZ $R0",
                "LDVR 0x008f $R1",
                "JNZR $R1"
            });
        }
        #endregion

        #region Subroutines
        [Test]
        public void CALL_PushesArgCountAndSetsPCToValue()
        {
            Assert.That(processor[Register.SP], Is.EqualTo(STACK_START_ADDRESS));
            Assert.That(processor[Register.FP], Is.EqualTo(STACK_START_ADDRESS));

            FlashAndExecute("CALL 0 0x1234", 0x1234);

            /**
             * Expected stack
             * 
             * Registers    | Stack Contents
             * FP, SP       | TOP
             *              | --------------
             *              | PREV FP
             *              | PREV PC
             * ARG, PREV FP | 0        (ARGV)
             *              | --------------
             *              | BOTTOM
             */

            Assert.That(processor[Register.SP], Is.EqualTo(STACK_START_ADDRESS - 3 * Processor.DATASIZE));

            var framePointer = processor[Register.FP];
            Assert.That(framePointer, Is.EqualTo(STACK_START_ADDRESS - 3 * Processor.DATASIZE));

            var previousFramepointer = (ushort)(framePointer + Processor.DATASIZE);
            Assert.That(memory.GetU16(previousFramepointer), Is.EqualTo(STACK_START_ADDRESS));

            var previousProgramCounter = (ushort)(framePointer + 2 * Processor.DATASIZE);
            Assert.That(memory.GetU16(previousProgramCounter), Is.EqualTo(4));

            var argPointer = processor[Register.ARG];
            Assert.That(argPointer, Is.EqualTo(STACK_START_ADDRESS));
            Assert.That(memory.GetU16(argPointer), Is.EqualTo(0));
        }

        [Test]
        public void CALL_WithArguments_PushesArgCountAndSetsPCToValueFromRegister()
        {
            Assert.That(processor[Register.SP], Is.EqualTo(STACK_START_ADDRESS));
            Assert.That(processor[Register.FP], Is.EqualTo(STACK_START_ADDRESS));

            FlashAndExecute(new string[]
            {
                "LDVR 0x1111 $R0",
                "LDVR 0x2222 $R1",
                "PUSH $R1",
                "PUSH $R0",
                "CALL 2 0x1234"
            }, 0x1234);

            /**
             * Expected stack
             * 
             * Registers    | Stack Contents
             * FP, SP       | TOP
             *              | --------------
             *              | PREV FP
             *              | PREV PC
             * ARG          | 2         (ARGV)
             *              | 0x1111    (R0 - ARG0)
             * PREV FP      | 0x2222    (R1 - ARG1)
             *              | --------------
             *              | BOTTOM
             */

            Assert.That(processor[Register.SP], Is.EqualTo(STACK_START_ADDRESS - 5 * Processor.DATASIZE));

            var framePointer = processor[Register.FP];
            Assert.That(framePointer, Is.EqualTo(STACK_START_ADDRESS - 5 * Processor.DATASIZE));

            var previousFramepointer = (ushort)(framePointer + Processor.DATASIZE);
            Assert.That(memory.GetU16(previousFramepointer), Is.EqualTo(STACK_START_ADDRESS));

            var previousProgramCounter = (ushort)(framePointer + 2 * Processor.DATASIZE);
            Assert.That(memory.GetU16(previousProgramCounter), Is.EqualTo(16));

            var argPointer = processor[Register.ARG];
            Assert.That(argPointer, Is.EqualTo(STACK_START_ADDRESS - 2 * Processor.DATASIZE));

            Assert.That(memory.GetU16(argPointer), Is.EqualTo(2));
            Assert.That(memory.GetU16((ushort)(argPointer + 1 * Processor.DATASIZE)), Is.EqualTo(0x1111));
            Assert.That(memory.GetU16((ushort)(argPointer + 2 * Processor.DATASIZE)), Is.EqualTo(0x2222));
        }

        [Test]
        public void CALLR_PushesArgCountAndSetsPCToValueFromRegister()
        {
            Assert.That(processor[Register.SP], Is.EqualTo(STACK_START_ADDRESS));
            Assert.That(processor[Register.FP], Is.EqualTo(STACK_START_ADDRESS));

            FlashAndExecute(new string[]
            {
                "LDVR 0x1234 $R0",
                "CALLR 0 $R0"
            }, 0x1234);

            /**
            * Expected stack
            * 
            * Registers    | Stack Contents
            * FP, SP       | TOP
            *              | --------------
            *              | PREV FP
            *              | PREV PC
            * ARG, PREV FP | 0          (ARGV)
            *              | --------------
            *              | BOTTOM
            */

            Assert.That(processor[Register.SP], Is.EqualTo(STACK_START_ADDRESS - 3 * Processor.DATASIZE));

            var framePointer = processor[Register.FP];
            Assert.That(framePointer, Is.EqualTo(STACK_START_ADDRESS - 3 * Processor.DATASIZE));

            var previousFramepointer = (ushort)(framePointer + Processor.DATASIZE);
            Assert.That(memory.GetU16(previousFramepointer), Is.EqualTo(STACK_START_ADDRESS));

            var previousProgramCounter = (ushort)(framePointer + 2 * Processor.DATASIZE);
            Assert.That(memory.GetU16(previousProgramCounter), Is.EqualTo(7));

            var argPointer = processor[Register.ARG];
            Assert.That(argPointer, Is.EqualTo(STACK_START_ADDRESS));
            Assert.That(memory.GetU16(argPointer), Is.EqualTo(0));
        }

        [Test]
        public void CALLR_WithArguments_PushesArgCountAndSetsPCToValueFromRegister()
        {
            Assert.That(processor[Register.SP], Is.EqualTo(STACK_START_ADDRESS));
            Assert.That(processor[Register.FP], Is.EqualTo(STACK_START_ADDRESS));

            FlashAndExecute(new string[]
            {
                "LDVR 0x1111 $R0",
                "LDVR 0x2222 $R1",
                "LDVR 0x1234 $R2",
                "PUSH $R1",
                "PUSH $R0",
                "CALLR 2 $R2"
            }, 0x1234);

            /**
             * Expected stack
             * 
             * Registers    | Stack Contents
             * FP, SP       | TOP
             *              | --------------
             *              | PREV FP
             *              | PREV PC
             * ARG          | 2         (ARGV)
             *              | 0x1111    (R0 - ARG0)
             * PREV FP      | 0x2222    (R1 - ARG1)
             *              | --------------
             *              | BOTTOM
             */

            Assert.That(processor[Register.SP], Is.EqualTo(STACK_START_ADDRESS - 5 * Processor.DATASIZE));

            var framePointer = processor[Register.FP];
            Assert.That(framePointer, Is.EqualTo(STACK_START_ADDRESS - 5 * Processor.DATASIZE));

            var previousFramepointer = (ushort)(framePointer + Processor.DATASIZE);
            Assert.That(memory.GetU16(previousFramepointer), Is.EqualTo(STACK_START_ADDRESS));

            var previousProgramCounter = (ushort)(framePointer + 2 * Processor.DATASIZE);
            Assert.That(memory.GetU16(previousProgramCounter), Is.EqualTo(19));

            var argPointer = processor[Register.ARG];
            Assert.That(argPointer, Is.EqualTo(STACK_START_ADDRESS - 2 * Processor.DATASIZE));

            Assert.That(memory.GetU16(argPointer), Is.EqualTo(2));
            Assert.That(memory.GetU16((ushort)(argPointer + 1 * Processor.DATASIZE)), Is.EqualTo(0x1111));
            Assert.That(memory.GetU16((ushort)(argPointer + 2 * Processor.DATASIZE)), Is.EqualTo(0x2222));
        }

        [Test]
        public void CALL_StackOverflow_ResetsAndThrowsException()
        {
            Flash(
               $"LDVR {STACK_END_ADDRESS} $SP",
                "CALL 0 0x1234"
            );

            processor.Step();

            AssertExceptionOccursAndProcessorResets(
                typeof(StackOverflowException), Instruction.CALL, "Stack is full.");
        }

        [Test]
        public void CALLR_StackOverflow_ResetsAndThrowsException()
        {
            Flash(
               $"LDVR {STACK_END_ADDRESS} $SP",
                "LDVR 0x1234 $R0",
                "CALLR 0 $R0"
            );

            processor.Step();
            processor.Step();

            AssertExceptionOccursAndProcessorResets(
                typeof(StackOverflowException), Instruction.CALLR, "Stack is full.");
        }

        [Test]
        public void RET_WithOutReturnValues_PushesReturnCountAndResetsPC()
        {
            Assert.That(processor[Register.SP], Is.EqualTo(STACK_START_ADDRESS));
            Assert.That(processor[Register.FP], Is.EqualTo(STACK_START_ADDRESS));

            Flash("CALL 0 0x1234");

            flasher.Address = 0x1234;
            Flash("RET 0");

            processor.Step();
            processor.Step();

            Assert.That(processor[Register.PC], Is.EqualTo(4));

            /**
            * Expected stack
            * 
            * Registers    | Stack Contents
            *              | TOP
            * RET          | 0          (RETV)
            *              | --------------
            *              | -          (Was PREV FP)
            * SP           | -          (Was PREV PC)
            * FP, ARG      | 0          (ARGV)
            *              | --------------
            *              | BOTTOM
            */

            Assert.That(processor[Register.SP], Is.EqualTo(STACK_START_ADDRESS - 1 * Processor.DATASIZE));

            var framePointer = processor[Register.FP];
            Assert.That(framePointer, Is.EqualTo(STACK_START_ADDRESS));

            var retPointer = processor[Register.RET];
            Assert.That(retPointer, Is.EqualTo(STACK_START_ADDRESS - 3 * Processor.DATASIZE));
            Assert.That(memory.GetU16(retPointer), Is.EqualTo(0));

            var argPointer = processor[Register.ARG];
            Assert.That(argPointer, Is.EqualTo(STACK_START_ADDRESS));
            Assert.That(memory.GetU16(argPointer), Is.EqualTo(0));
        }

        [Test]
        public void RET_WithReturnValues_PushesReturnCountAndResetsPC()
        {
            Assert.That(processor[Register.SP], Is.EqualTo(STACK_START_ADDRESS));
            Assert.That(processor[Register.FP], Is.EqualTo(STACK_START_ADDRESS));

            Flash("CALL 0 0x1234");

            flasher.Address = 0x1234;
            Flash(
                "LDVR 0x1111 $R0",
                "LDVR 0x2222 $R1",
                "PUSH $R1",
                "PUSH $R0",
                "RET 2"
            );

            processor.Step();
            processor.Step();
            processor.Step();
            processor.Step();
            processor.Step();
            processor.Step();

            Assert.That(processor[Register.PC], Is.EqualTo(4));

            /**
            * Expected stack
            * 
            * Registers    | Stack Contents
            *              | TOP
            * RET          | 2          (RETV)
            *              | 0x1111     (R0 - RET0)
            *              | 0x2222     (R1 - RET1)
            *              | --------------
            *              | -          (Was PREV FP)
            * SP           | -          (Was PREV PC)
            * FP, ARG      | 0          (ARGV)
            *              | --------------
            *              | BOTTOM
            */

            Assert.That(processor[Register.SP], Is.EqualTo(STACK_START_ADDRESS - 1 * Processor.DATASIZE));

            var framePointer = processor[Register.FP];
            Assert.That(framePointer, Is.EqualTo(STACK_START_ADDRESS));

            var retPointer = processor[Register.RET];
            Assert.That(retPointer, Is.EqualTo(STACK_START_ADDRESS - 5 * Processor.DATASIZE));

            Assert.That(memory.GetU16(retPointer), Is.EqualTo(2));
            Assert.That(memory.GetU16((ushort)(retPointer + 1 * Processor.DATASIZE)), Is.EqualTo(0x1111));
            Assert.That(memory.GetU16((ushort)(retPointer + 2 * Processor.DATASIZE)), Is.EqualTo(0x2222));

            var argPointer = processor[Register.ARG];
            Assert.That(argPointer, Is.EqualTo(STACK_START_ADDRESS));
            Assert.That(memory.GetU16(argPointer), Is.EqualTo(0));
        }

        [Test]
        public void RET_EmptyCallStack_ResetsAndThrowsException()
        {
            Flash("RET 0");

            AssertExceptionOccursAndProcessorResets(
                typeof(InvalidOperationException), Instruction.RET, "In base frame, nothing to return to.");
        }
        #endregion

        #region Stack
        [Test]
        public void PUSH_PutsValueFromRegisterOntoStack()
        {
            Assert.That(processor[Register.SP], Is.EqualTo(STACK_START_ADDRESS));

            FlashAndExecute(new string[]
            {
                "LDVR 0x1234 $R0",
                "PUSH $R0"
            });

            var sp = processor[Register.SP];

            Assert.That(sp, Is.Not.EqualTo(STACK_START_ADDRESS));
            Assert.That(memory.GetU16((ushort)(sp + sizeof(ushort))), Is.EqualTo(0x1234));
        }

        [Test]
        public void PUSH_CanFillStack()
        {
            var source = new List<string> { "LDVR 0x1234 $R0" };

            source.AddRange(Enumerable.Repeat("PUSH $R0", STACK_SIZE));

            Assert.That(processor[Register.SP], Is.EqualTo(STACK_START_ADDRESS));

            FlashAndExecute(source);

            Assert.That(processor[Register.SP], Is.EqualTo(STACK_END_ADDRESS - Processor.DATASIZE));
        }

        [Test]
        public void PUSH_FullStack_ResetsAndThrowsException()
        {
            var source = new List<string> { "LDVR 0x1234 $R0" };

            source.AddRange(Enumerable.Repeat("PUSH $R0", STACK_SIZE));

            Assert.That(processor[Register.SP], Is.EqualTo(STACK_START_ADDRESS));

            FlashAndExecute(source);

            Flash("PUSH $R0");

            AssertExceptionOccursAndProcessorResets(
                typeof(StackOverflowException), Instruction.PUSH, "Stack is full.");
        }

        [Test]
        public void POP_PutsValueFromStackIntoRegister()
        {
            Assert.That(processor[Register.SP], Is.EqualTo(STACK_START_ADDRESS));

            FlashAndExecute(new string[]
            {
                "LDVR 0x1234 $R0",
                "PUSH $R0",
                "POP $R1"
            });

            Assert.That(processor[Register.SP], Is.EqualTo(STACK_START_ADDRESS));

            Assert.That(processor[Register.R1], Is.EqualTo(0x1234));
        }

        [Test]
        public void POP_EmptyStack_ResetsAndThrowsException()
        {
            Flash("POP $R0");

            AssertExceptionOccursAndProcessorResets(
                typeof(InvalidOperationException), Instruction.POP, "Stack frame is empty.");
        }

        [Test]
        public void PEEK_PutsValueFromStackIntoRegisterWithoutRemovingItFromStack()
        {
            Assert.That(processor[Register.SP], Is.EqualTo(STACK_START_ADDRESS));

            FlashAndExecute(new string[]
            {
                "LDVR 0x1234 $R0",
                "PUSH $R0",
                "PEEK $R1"
            });

            Assert.That(processor[Register.SP], Is.EqualTo(STACK_START_ADDRESS - Processor.DATASIZE));

            Assert.That(processor[Register.R1], Is.EqualTo(0x1234));
        }

        [Test]
        public void PEEK_OnEmptyStack_ResetsAndThrowsException()
        {
            Flash("PEEK $R0");

            AssertExceptionOccursAndProcessorResets(
                typeof(InvalidOperationException), Instruction.PEEK, "Stack frame is empty.");
        }
        #endregion

        #region Processor
        [Test]
        public void HALT_HaltsExecution()
        {
            var haltOccured = false;

            void OnHalt(object sender, EventArgs e)
            {
                haltOccured = true;
            };

            processor.Halt += OnHalt;

            FlashAndExecute(Enumerable.Repeat("NOP", 50).Append("HALT"));

            Assert.That(haltOccured, Is.True);

            processor.Halt -= OnHalt;
        }

        [Test]
        public void RESET_ResetsProcessorToBeginningOfProgram()
        {
            FlashAndExecute(Enumerable.Repeat("NOP", 50).Append("RESET"), 0);

            Assert.That(resetOccured, Is.True);

            AssertProcessorIsInInitialState();
        }
        #endregion

        //// TODO: Segfaults:
        //// FUTURE: memory access (Load and Store) outside of DATA (static or dynamic) is invalid
        //// FUTURE: executing outside of CODE is invalid (Jump, Logic, Subroutines)
        #endregion
    }
}
#pragma warning restore CA1822 // Mark members as static