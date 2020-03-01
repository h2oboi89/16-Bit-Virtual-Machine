using System;
using System.Collections.Generic;
using System.Globalization;

namespace VM
{
    static class Utility
    {
        public static string FormatU8(byte value)
        {
            return $"0x{value.ToString("X2", CultureInfo.InvariantCulture)}";
        }

        public static string FormatU16(ushort value)
        {
            return $"0x{value.ToString("X4", CultureInfo.InvariantCulture)}";
        }

        public static IEnumerable<byte> GetBytes(ushort value)
        {
            return new byte[] { (byte)((value >> 8) & 0xff), (byte)(value & 0xff) };
        }

        private static T Parse<T>(string value)
        {
            foreach(var i in (T[])Enum.GetValues(typeof(T)))
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
