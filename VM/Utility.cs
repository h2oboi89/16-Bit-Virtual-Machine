using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using VM.Hardware;

namespace VM
{
    class Utility
    {
        public static string FormatU8(byte value)
        {
            return FormatDateType(value.ToString("X", CultureInfo.InvariantCulture), sizeof(byte));
        }

        public static string FormatU16(ushort value)
        {
            return FormatDateType(value.ToString("X", CultureInfo.InvariantCulture), sizeof(ushort));
        }

        private static string FormatDateType(string hexValue, int size)
        {
            return $"0x{hexValue.PadLeft(size * 2, '0')}";
        }

        public static IEnumerable<byte> GetBytes(ushort value)
        {
            return new byte[] { (byte)((value >> 8) & 0xff), (byte)(value & 0xff) };
        }

        public static ushort GeneralPurposeRegisterMemorySize()
        {
            return (ushort)((Enum.GetValues(typeof(Register)).Length - (int)Register.R0) * Processor.DATASIZE);
        }

        private static T Parse<T>(string value)
        {
            foreach (var i in (T[])Enum.GetValues(typeof(T)))
            {
                if (Enum.GetName(typeof(T), i) == value)
                {
                    return i;
                }
            }

            throw new ArgumentException($"Unknown {typeof(T)} '{value}'");
        }

        public static Instruction ParseInstruction(string instruction)
        {
            return Parse<Instruction>(instruction);
        }

        public static Register ParseRegister(string register)
        {
            return Parse<Register>(register);
        }
    }
}
