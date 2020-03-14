using NUnit.Framework;
using System;
using VM.Hardware;

namespace VM.HardwareTests.Tests
{
    [TestFixture]
    public class ArithmeticLogicUnitTests
    {
        private const ushort NOTUSED = 0;

        private ArithmeticLogicUnit alu;

        [SetUp]
        public void SetUp()
        {
            alu = new ArithmeticLogicUnit();
        }

        private void AssertZeroValueAndFlag()
        {
            Assert.That(alu.Accumulator, Is.Zero);
            Assert.That(alu.IsSet(Flags.ZERO), Is.True);
        }

        [Test]
        public void INC_IncrementsValue()
        {
            alu.Execute(Instruction.INC, 0x1234, 1);

            Assert.That(alu.Accumulator, Is.EqualTo(0x1235));
            Assert.That(alu.IsSet(Flags.CARRY), Is.False);
        }

        [Test]
        public void INC_Overflow_WrapsAndSetsZeroAndCarryFlags()
        {
            alu.Execute(Instruction.INC, 0xffff, 1);

            AssertZeroValueAndFlag();
            Assert.That(alu.IsSet(Flags.CARRY), Is.True);
        }

        [Test]
        public void DEC_DecrementsValue()
        {
            alu.Execute(Instruction.DEC, 0x1234, 1);

            Assert.That(alu.Accumulator, Is.EqualTo(0x1233));
            Assert.That(alu.IsSet(Flags.CARRY), Is.False);
        }

        [Test]
        public void DEC_Underflow_WrapsAndSetsCarryFlag()
        {
            alu.Execute(Instruction.DEC, 0x0000, 1);

            Assert.That(alu.Accumulator, Is.EqualTo(0xffff));
            Assert.That(alu.IsSet(Flags.CARRY), Is.True);
        }

        [Test]
        public void DEC_ZeroResult_SetsZeroFlag()
        {
            alu.Execute(Instruction.DEC, 0x0001, 1);

            AssertZeroValueAndFlag();
        }

        [Test]
        public void ADD_AdditionOfTwoValues()
        {
            alu.Execute(Instruction.ADD, 0x1200, 0x0034);

            Assert.That(alu.Accumulator, Is.EqualTo(0x1234));
            Assert.That(alu.IsSet(Flags.CARRY), Is.False);
        }

        [Test]
        public void ADD_Overflow_WrapsAndSetsCarryFlag()
        {
            alu.Execute(Instruction.ADD, 0xff00, 0x1000);

            Assert.That(alu.Accumulator, Is.EqualTo(0x0f00));
            Assert.That(alu.IsSet(Flags.CARRY), Is.True);
        }

        [Test]
        public void ADD_ZeroResult_SetsZeroAndCarryFlags()
        {
            alu.Execute(Instruction.ADD, 0xff00, 0x0100);

            AssertZeroValueAndFlag();
            Assert.That(alu.IsSet(Flags.CARRY), Is.True);
        }

        [Test]
        public void SUB_SubtractionOfTwoValues()
        {
            alu.Execute(Instruction.SUB, 0x1200, 0x0034);

            Assert.That(alu.Accumulator, Is.EqualTo(0x11cc));
            Assert.That(alu.IsSet(Flags.CARRY), Is.False);
        }

        [Test]
        public void SUB_Underflow_WrapsAndSetsCarryFlag()
        {
            alu.Execute(Instruction.SUB, 0x1000, 0xff00);

            Assert.That(alu.Accumulator, Is.EqualTo(0x1100));
            Assert.That(alu.IsSet(Flags.CARRY), Is.True);
        }

        [Test]
        public void SUB_ZeroResult_SetsZeroFlag()
        {
            alu.Execute(Instruction.SUB, 0x1234, 0x1234);

            AssertZeroValueAndFlag();
        }

        [Test]
        public void MUL_MultiplicationOfTwoValues()
        {
            alu.Execute(Instruction.MUL, 0x00f0, 0x00f0);

            Assert.That(alu.Accumulator, Is.EqualTo(0xe100));
            Assert.That(alu.IsSet(Flags.CARRY), Is.False);
        }

        [Test]
        public void MUL_Overflow_WrapsAndSetsCarryFlag()
        {
            alu.Execute(Instruction.MUL, 0x1234, 0xffff);

            Assert.That(alu.Accumulator, Is.EqualTo(0xedcc));
            Assert.That(alu.IsSet(Flags.CARRY), Is.True);
        }

        [Test]
        public void MUL_ZeroResult_SetsZeroFlag()
        {
            alu.Execute(Instruction.MUL, 0x1234, 0x0000);

            AssertZeroValueAndFlag();
        }

        [Test]
        public void DIV_DivisionOfTwoValues()
        {
            alu.Execute(Instruction.DIV, 0x0040, 0x0002);

            Assert.That(alu.Accumulator, Is.EqualTo(0x0020));
            Assert.That(alu.IsSet(Flags.CARRY), Is.False);
        }

        [Test]
        public void DIV_ByZero_ThrowsException()
        {
            Assert.That(() => alu.Execute(Instruction.DIV, 0x1234, 0x0000), Throws.InstanceOf<DivideByZeroException>()
                .With.Message.EqualTo("Attempted to divide by zero."));
        }

        [Test]
        public void DIV_ZeroResult_SetsZeroFlag()
        {
            alu.Execute(Instruction.DIV, 0x0001, 0xffff);

            AssertZeroValueAndFlag();
        }

        [Test]
        public void MOD_ModuloOfTwoValues()
        {
            alu.Execute(Instruction.MOD, 5, 3);

            Assert.That(alu.Accumulator, Is.EqualTo(2));
            Assert.That(alu.IsSet(Flags.CARRY), Is.False);
        }

        [Test]
        public void MOD_ByZero_ThrowsException()
        {
            Assert.That(() => alu.Execute(Instruction.MOD, 0x1234, 0x0000), Throws.InstanceOf<DivideByZeroException>()
                .With.Message.EqualTo("Attempted to divide by zero."));
        }

        [Test]
        public void MOD_ZeroResult_SetsZeroFlag()
        {
            alu.Execute(Instruction.MOD, 5, 5);

            AssertZeroValueAndFlag();
        }

        [Test]
        public void AND_BinaryAndOfTwoValues()
        {
            alu.Execute(Instruction.AND, 0x00ff, 0xaaaa);

            Assert.That(alu.Accumulator, Is.EqualTo(0x00aa));
        }

        [Test]
        public void AND_ZeroResult_SetsZeroFlag()
        {
            alu.Execute(Instruction.AND, 0x00ff, 0xff00);

            AssertZeroValueAndFlag();
        }

        [Test]
        public void OR_BinaryOrOfTwoValues()
        {
            alu.Execute(Instruction.OR, 0x0055, 0xaaaa);

            Assert.That(alu.Accumulator, Is.EqualTo(0xaaff));
        }

        [Test]
        public void OR_ZeroResult_SetsZeroFlag()
        {
            alu.Execute(Instruction.OR, 0x0000, 0x0000);

            AssertZeroValueAndFlag();
        }

        [Test]
        public void XOR_BinaryXorOfTwoValues()
        {
            alu.Execute(Instruction.XOR, 0x00ff, 0xaaaa);

            Assert.That(alu.Accumulator, Is.EqualTo(0xaa55));
        }

        [Test]
        public void XOR_ZeroResult_SetsZeroFlag()
        {
            alu.Execute(Instruction.XOR, 0x00ff, 0x00ff);

            AssertZeroValueAndFlag();
        }

        [Test]
        public void NOT_BinaryNotOfValue()
        {
            alu.Execute(Instruction.NOT, 0xaaaa, NOTUSED);

            Assert.That(alu.Accumulator, Is.EqualTo(0x5555));
        }

        [Test]
        public void NOT_ZeroResult_SetsZeroFlag()
        {
            alu.Execute(Instruction.NOT, 0xffff, NOTUSED);

            AssertZeroValueAndFlag();
        }

        [Test]
        public void SRL_ShiftsValueLeftByAmount()
        {
            for (byte i = 0; i < 16; i++)
            {
                alu.Execute(Instruction.SRL, 0x0001, i);

                Assert.That(alu.Accumulator, Is.EqualTo(0x0001 << i));
            }
        }

        [Test]
        public void SRL_ExcessiveShift_ZeroesOutValue()
        {
            alu.Execute(Instruction.SRL, 0x0001, 16);

            AssertZeroValueAndFlag();
        }

        [Test]
        public void SRLR_ShiftsValueLeftByAmount()
        {
            for (byte i = 0; i < 16; i++)
            {
                alu.Execute(Instruction.SRLR, 0x0001, i);

                Assert.That(alu.Accumulator, Is.EqualTo(0x0001 << i));
            }
        }

        [Test]
        public void SRLR_ExcessiveShift_ZeroesOutRegister()
        {
            alu.Execute(Instruction.SRLR, 0x0001, 16);

            AssertZeroValueAndFlag();
        }

        [Test]
        public void SRR_ShiftsValueRightByAmount()
        {
            for (byte i = 0; i < 16; i++)
            {
                alu.Execute(Instruction.SRR, 0x8000, i);

                Assert.That(alu.Accumulator, Is.EqualTo(0x8000 >> i));
            }
        }

        [Test]
        public void SRR_ExcessiveShift_ZeroesOutValue()
        {
            alu.Execute(Instruction.SRR, 0x8000, 16);

            AssertZeroValueAndFlag();
        }

        [Test]
        public void SRRR_ShiftsValueLeftByAmount()
        {
            for (byte i = 0; i < 16; i++)
            {
                alu.Execute(Instruction.SRRR, 0x8000, i);

                Assert.That(alu.Accumulator, Is.EqualTo(0x8000 >> i));
            }
        }

        [Test]
        public void SRRR_ExcessiveShift_ZeroesOutValue()
        {
            alu.Execute(Instruction.SRRR, 0x8000, 16);

            AssertZeroValueAndFlag();
        }

        [Test]
        public void CMP_LessThan_SetsLessThanFlag()
        {
            alu.Execute(Instruction.CMP, 0x1234, 0x4321);

            Assert.That(alu.IsSet(Flags.LESSTHAN), Is.True);
            Assert.That(alu.IsSet(Flags.EQUAL), Is.False);
            Assert.That(alu.IsSet(Flags.GREATERTHAN), Is.False);
        }

        [Test]
        public void CMP_EqualTo_SetsEqualFlag()
        {
            alu.Execute(Instruction.CMP, 0x1234, 0x1234);

            Assert.That(alu.IsSet(Flags.LESSTHAN), Is.False);
            Assert.That(alu.IsSet(Flags.EQUAL), Is.True);
            Assert.That(alu.IsSet(Flags.GREATERTHAN), Is.False);
        }

        [Test]
        public void CMP_GreaterThan_SetsGreaterThanFlag()
        {
            alu.Execute(Instruction.CMP, 0x4321, 0x1234);

            Assert.That(alu.IsSet(Flags.LESSTHAN), Is.False);
            Assert.That(alu.IsSet(Flags.EQUAL), Is.False);
            Assert.That(alu.IsSet(Flags.GREATERTHAN), Is.True);
        }

        [Test]
        public void CMPZ_Zero_SetsZeroFlag()
        {
            alu.Execute(Instruction.CMPZ, 0, NOTUSED);

            Assert.That(alu.IsSet(Flags.ZERO), Is.True);
        }

        [Test]
        public void CMPZ_NotZero_DoesNothing()
        {
            alu.Execute(Instruction.CMPZ, 0xffff, NOTUSED);

            Assert.That(alu.IsSet(Flags.ZERO), Is.False);
        }

        [Test]
        public void JLT_LessThanFlagIsSet_UsesValueA()
        {
            alu.Execute(Instruction.CMP, 0x1234, 0x4321);
            alu.Execute(Instruction.JLT, 0x0000, 0xffff);

            Assert.That(alu.JumpAddress, Is.EqualTo(0x0000));
        }

        [Test]
        public void JLT_LessThanFlagIsNotSet_UsesValueB()
        {
            alu.Execute(Instruction.CMP, 0x4321, 0x1234);
            alu.Execute(Instruction.JLT, 0x0000, 0xffff);

            Assert.That(alu.JumpAddress, Is.EqualTo(0xffff));
        }

        [Test]
        public void JLTR_LessThanFlagIsSet_UsesValueA()
        {
            alu.Execute(Instruction.CMP, 0x1234, 0x4321);
            alu.Execute(Instruction.JLTR, 0x0000, 0xffff);

            Assert.That(alu.JumpAddress, Is.EqualTo(0x0000));
        }

        [Test]
        public void JLTR_LessThanFlagIsNotSet_UsesValueB()
        {
            alu.Execute(Instruction.CMP, 0x4321, 0x1234);
            alu.Execute(Instruction.JLTR, 0x0000, 0xffff);

            Assert.That(alu.JumpAddress, Is.EqualTo(0xffff));
        }

        [Test]
        public void JGT_GreaterThanFlagIsSet_UsesValueA()
        {
            alu.Execute(Instruction.CMP, 0x4321, 0x1234);
            alu.Execute(Instruction.JGT, 0x0000, 0xffff);

            Assert.That(alu.JumpAddress, Is.EqualTo(0x0000));
        }

        [Test]
        public void JGT_GreaterThanFlagIsNotSet_UsesValueA()
        {
            alu.Execute(Instruction.CMP, 0x1234, 0x4321);
            alu.Execute(Instruction.JGT, 0x0000, 0xffff);

            Assert.That(alu.JumpAddress, Is.EqualTo(0xffff));
        }

        [Test]
        public void JGTR_GreaterThanFlagIsSet_UsesValueA()
        {
            alu.Execute(Instruction.CMP, 0x4321, 0x1234);
            alu.Execute(Instruction.JGTR, 0x0000, 0xffff);

            Assert.That(alu.JumpAddress, Is.EqualTo(0x0000));
        }

        [Test]
        public void JGTR_GreaterThanFlagIsNotSet_UsesValueB()
        {
            alu.Execute(Instruction.CMP, 0x1234, 0x4321);
            alu.Execute(Instruction.JGTR, 0x0000, 0xffff);

            Assert.That(alu.JumpAddress, Is.EqualTo(0xffff));
        }

        [Test]
        public void JE_EqualFlagIsSet_UsesValueA()
        {
            alu.Execute(Instruction.CMP, 0x1234, 0x1234);
            alu.Execute(Instruction.JE, 0x0000, 0xffff);

            Assert.That(alu.JumpAddress, Is.EqualTo(0x0000));
        }

        [Test]
        public void JE_EqualFlagIsNotSet_UsesValueB()
        {
            alu.Execute(Instruction.CMP, 0x1234, 0x4321);
            alu.Execute(Instruction.JE, 0x0000, 0xffff);

            Assert.That(alu.JumpAddress, Is.EqualTo(0xffff));
        }

        [Test]
        public void JER_EqualFlagIsSet_UsesValueA()
        {
            alu.Execute(Instruction.CMP, 0x1234, 0x1234);
            alu.Execute(Instruction.JER, 0x0000, 0xffff);

            Assert.That(alu.JumpAddress, Is.EqualTo(0x0000));
        }

        [Test]
        public void JER_EqualFlagIsNotSet_UsesValueB()
        {
            alu.Execute(Instruction.CMP, 0x1234, 0x4321);
            alu.Execute(Instruction.JER, 0x0000, 0xffff);

            Assert.That(alu.JumpAddress, Is.EqualTo(0xffff));
        }

        [Test]
        public void JNE_EqualFlagIsNotSet_UsesValueA()
        {
            alu.Execute(Instruction.CMP, 0x1234, 0x4321);
            alu.Execute(Instruction.JNE, 0x0000, 0xffff);

            Assert.That(alu.JumpAddress, Is.EqualTo(0x0000));
        }

        [Test]
        public void JNE_EqualFlagIsSet_UsesValueB()
        {
            alu.Execute(Instruction.CMP, 0x1234, 0x1234);
            alu.Execute(Instruction.JNE, 0x0000, 0xffff);

            Assert.That(alu.JumpAddress, Is.EqualTo(0xffff));
        }

        [Test]
        public void JNER_EqualFlagIsNotSet_UsesValueA()
        {
            alu.Execute(Instruction.CMP, 0x1234, 0x4321);
            alu.Execute(Instruction.JNER, 0x0000, 0xffff);

            Assert.That(alu.JumpAddress, Is.EqualTo(0x0000));
        }

        [Test]
        public void JNER_EqualFlagIsSet_UsesValueB()
        {
            alu.Execute(Instruction.CMP, 0x1234, 0x1234);
            alu.Execute(Instruction.JNER, 0x0000, 0xffff);

            Assert.That(alu.JumpAddress, Is.EqualTo(0xffff));
        }

        [Test]
        public void JZ_ZeroFlagIsSet_UsesValueA()
        {
            alu.Execute(Instruction.ADD, 0, 0);
            alu.Execute(Instruction.JZ, 0x0000, 0xffff);

            Assert.That(alu.JumpAddress, Is.EqualTo(0x0000));
        }

        [Test]
        public void JZ_ZeroFlagIsNotSet_UsesValueB()
        {
            alu.Execute(Instruction.ADD, 0, 1);
            alu.Execute(Instruction.JZ, 0x0000, 0xffff);

            Assert.That(alu.JumpAddress, Is.EqualTo(0xffff));
        }

        [Test]
        public void JZR_ZeroFlagIsSet_UsesValueA()
        {
            alu.Execute(Instruction.ADD, 0, 0);
            alu.Execute(Instruction.JZR, 0x0000, 0xffff);

            Assert.That(alu.JumpAddress, Is.EqualTo(0x0000));
        }

        [Test]
        public void JZR_ZeroFlagIsNotSet_UsesValueB()
        {
            alu.Execute(Instruction.ADD, 0, 1);
            alu.Execute(Instruction.JZR, 0x0000, 0xffff);

            Assert.That(alu.JumpAddress, Is.EqualTo(0xffff));
        }

        [Test]
        public void JNZ_ZeroFlagIsNotSet_UsesValueA()
        {
            alu.Execute(Instruction.ADD, 0, 1);
            alu.Execute(Instruction.JNZ, 0x0000, 0xffff);

            Assert.That(alu.JumpAddress, Is.EqualTo(0x0000));
        }

        [Test]
        public void JNZ_ZeroFlagIsSet_UsesValueB()
        {
            alu.Execute(Instruction.ADD, 0, 0);
            alu.Execute(Instruction.JNZ, 0x0000, 0xffff);

            Assert.That(alu.JumpAddress, Is.EqualTo(0xffff));
        }

        [Test]
        public void JNZR_ZeroFlagIsNotSet_UsesValueA()
        {
            alu.Execute(Instruction.ADD, 0, 1);
            alu.Execute(Instruction.JNZR, 0x0000, 0xffff);

            Assert.That(alu.JumpAddress, Is.EqualTo(0x0000));
        }

        [Test]
        public void JNZR_ZeroFlagIsSet_UsesValueB()
        {
            alu.Execute(Instruction.ADD, 0, 0);
            alu.Execute(Instruction.JNZR, 0x0000, 0xffff);

            Assert.That(alu.JumpAddress, Is.EqualTo(0xffff));
        }
    }
}
