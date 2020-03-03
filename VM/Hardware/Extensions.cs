using System;

namespace VM.Hardware
{
    internal static class Extensions
    {
        public static bool IsPrivate(this Register register)
        {
            return register < Register.ACC;
        }

        private static bool IsValid<T>(T value)
        {
            return Enum.IsDefined(value.GetType(), value);
        }

        public static bool IsValid(this Register register)
        {
            return IsValid<Register>(register);
        }

        public static string Name(this Register register)
        {
            return Enum.GetName(typeof(Register), register);
        }

        public static bool IsValid(this Instruction instruction)
        {
            return IsValid<Instruction>(instruction);
        }

        public static bool IsCarryFlag(this Instruction instruction)
        {
            switch (instruction)
            {
                case Instruction.INC:
                case Instruction.DEC:
                case Instruction.ADD:
                case Instruction.MUL:
                case Instruction.SUB:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsZeroFlag(this Instruction instruction)
        {
            switch (instruction)
            {
                case Instruction.INC:
                case Instruction.DEC:
                case Instruction.ADD:
                case Instruction.SUB:
                case Instruction.MUL:
                case Instruction.DIV:
                case Instruction.AND:
                case Instruction.OR:
                case Instruction.XOR:
                case Instruction.NOT:
                case Instruction.SRL:
                case Instruction.SRLR:
                case Instruction.SRR:
                case Instruction.SRRR:
                case Instruction.CMPZ:
                    return true;
                default:
                    return false;
            }
        }
    }
}
