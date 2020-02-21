using System;

namespace VM
{
    /// <summary>
    /// Performs Arithmetic, Bitwise, and Logic <see cref="Instruction"/>s for the <see cref="Processor"/>.
    /// </summary>
    public class ArithmeticLogicUnit
    {
        /// <summary>
        /// Result of all <see cref="Instruction"/>s are stored here.
        /// </summary>
        public ushort Accumulator { get; private set; }

        /// <summary>
        /// <see cref="Flags"/> indicating results of <see cref="Instruction"/>s.
        /// </summary>
        public ushort Flag { get; private set; }

        /// <summary>
        /// Next Address to following a Conditional Jump <see cref="Instruction"/>.
        /// </summary>
        public ushort JumpAddress { get; private set; }

        /// <summary>
        /// Resets the <see cref="ArithmeticLogicUnit"/> back to intial values.
        /// </summary>
        public void Reset()
        {
            Accumulator = 0;
            Flag = 0;
        }

        private void SetFlag(Flags flag)
        {
            Flag = flag.Set(Flag);
        }

        private void ClearFlag(Flags flag)
        {
            Flag = flag.Clear(Flag);
        }

        /// <summary>
        /// Determines if <see cref="Flags"/> is set.
        /// </summary>
        /// <param name="flag"><see cref="Flags"/> to check.</param>
        /// <returns>True if <see cref="Flags"/> is set; otherwise false.</returns>
        public bool IsSet(Flags flag)
        {
            return flag.IsSet(Flag);
        }

        private void ClearFlags(Instruction instruction)
        {
            if (instruction.IsCarryFlag())
            {
                ClearFlag(Flags.CARRY);
            }

            if (instruction.IsZeroFlag())
            {
                ClearFlag(Flags.ZERO);
            }

            if (instruction == Instruction.CMP)
            {
                ClearFlag(Flags.LESSTHAN);
                ClearFlag(Flags.GREATERTHAN);
                ClearFlag(Flags.EQUAL);
            }
        }

        private void SetFlags(Instruction instruction, ushort valueA, ushort valueB, uint temp)
        {
            if (instruction.IsCarryFlag() && temp > ushort.MaxValue)
            {
                SetFlag(Flags.CARRY);
            }

            if (instruction.IsZeroFlag() && Accumulator == 0)
            {
                SetFlag(Flags.ZERO);
            }

            if (instruction == Instruction.CMP)
            {
                if (valueA < valueB)
                {
                    SetFlag(Flags.LESSTHAN);
                }
                else if (valueA == valueB)
                {
                    SetFlag(Flags.EQUAL);
                }
                else // (valueA > valueB)
                {
                    SetFlag(Flags.GREATERTHAN);
                }
            }
        }

        private static Flags SetFlagToCheckForLogicalJumps(Instruction instruction)
        {
            Flags flag = 0;

            switch (instruction)
            {
                case Instruction.JLT:
                case Instruction.JLTR:
                    flag = Flags.LESSTHAN;
                    break;

                case Instruction.JGT:
                case Instruction.JGTR:
                    flag = Flags.GREATERTHAN;
                    break;

                case Instruction.JE:
                case Instruction.JER:
                case Instruction.JNE:
                case Instruction.JNER:
                    flag = Flags.EQUAL;
                    break;

                case Instruction.JZ:
                case Instruction.JZR:
                case Instruction.JNZ:
                case Instruction.JNZR:
                    flag = Flags.ZERO;
                    break;
            }

            return flag;
        }

        /// <summary>
        /// Executes the specified <see cref="Instruction"/> with the specified values.
        /// NOTE: <see cref="Instruction"/>s with only one operand only use <paramref name="valueA"/>.
        /// </summary>
        /// <param name="instruction"><see cref="Instruction"/> to execute.</param>
        /// <param name="valueA">Value for <see cref="Instruction"/>.</param>
        /// <param name="valueB">Value for <see cref="Instruction"/>.</param>
        public void Execute(Instruction instruction, ushort valueA, ushort valueB)
        {
            uint temp = 0;

            ClearFlags(instruction);

            var flag = SetFlagToCheckForLogicalJumps(instruction);

            switch (instruction)
            {
                case Instruction.INC:
                case Instruction.ADD:
                    temp = (uint)(valueA + valueB);

                    Accumulator = (ushort)temp;
                    break;

                case Instruction.DEC:
                case Instruction.SUB:
                    temp = (uint)(valueA - valueB);

                    Accumulator = (ushort)temp;
                    break;

                case Instruction.MUL:
                    temp = (uint)(valueA * valueB);

                    Accumulator = (ushort)temp;
                    break;

                case Instruction.DIV:
                    if (valueB == 0)
                    {
                        throw new DivideByZeroException();
                    }

                    temp = (uint)(valueA / valueB);

                    Accumulator = (ushort)temp;
                    break;

                case Instruction.AND:
                    Accumulator = (ushort)(valueA & valueB);
                    break;

                case Instruction.OR:
                    Accumulator = (ushort)(valueA | valueB);
                    break;

                case Instruction.XOR:
                    Accumulator = (ushort)(valueA ^ valueB);
                    break;

                case Instruction.NOT:
                    Accumulator = (ushort)~valueA;
                    break;

                case Instruction.SRL:
                case Instruction.SRLR:
                    Accumulator = (ushort)(valueA << valueB);
                    break;

                case Instruction.SRR:
                case Instruction.SRRR:
                    Accumulator = (ushort)(valueA >> valueB);
                    break;

                case Instruction.CMPZ:
                    Accumulator = valueA;
                    break;

                case Instruction.JLT:
                case Instruction.JLTR:
                case Instruction.JGT:
                case Instruction.JGTR:
                case Instruction.JE:
                case Instruction.JER:
                case Instruction.JZ:
                case Instruction.JZR:
                    JumpAddress = IsSet(flag) ? valueA : valueB;
                    break;

                case Instruction.JNE:
                case Instruction.JNER:
                case Instruction.JNZ:
                case Instruction.JNZR:
                    JumpAddress = !IsSet(flag) ? valueA : valueB;
                    break;
            }

            SetFlags(instruction, valueA, valueB, temp);
        }
    }
}
