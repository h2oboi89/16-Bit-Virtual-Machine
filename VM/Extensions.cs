using System;
using VM.Hardware;

namespace VM
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

        public static ushort MemoryAddress(this Register register)
        {
            return (ushort)((register - Register.R0) * Processor.DATASIZE);
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

        /// <summary>
        ///     Similar to <see cref="string.Substring(int, int)"/>.
        /// </summary>
        /// <param name="str"><see cref="string"/> to extract substring from.</param>
        /// <param name="startIndex">The inclusive zero-based starting character position of a substring in <paramref name="str"/>.</param>
        /// <param name="endIndex">The exclusive zero-based ending character position of a substring in <paramref name="str"/>.</param>
        /// <returns>
        ///     A string that is equivalent to the substring in <paramref name="str"/> that begins at <paramref name="startIndex"/> and ends before <paramref name="endIndex"/>, 
        ///     or <see cref="string.Empty"/> if <paramref name="startIndex"/> is equal to the length of <paramref name="str"/> or <paramref name="endIndex"/>.
        /// </returns>
        public static string Extract(this string str, int startIndex, int endIndex)
        {
            return str.Substring(startIndex, endIndex - startIndex);
        }
    }
}
