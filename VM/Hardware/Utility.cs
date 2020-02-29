using System.Collections.Generic;
using System.Globalization;

namespace VM.Hardware
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
    }
}
