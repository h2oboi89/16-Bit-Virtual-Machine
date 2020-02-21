using System;

namespace VM
{
    public class ArithmeticLogicUnit
    {
        public ushort Accumulator { get; private set; }

        public ushort Flag { get; private set; }

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
                else if (valueA > valueB)
                {
                    SetFlag(Flags.GREATERTHAN);
                }
            }
        }

        public void Execute(Instruction instruction, ushort valueA, ushort valueB)
        {
            uint temp = 0;

            ClearFlags(instruction);

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
            }

            SetFlags(instruction, valueA, valueB, temp);
        }
    }
}
