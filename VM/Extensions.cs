using System;

namespace VM
{
    internal static class Extensions
    {
        public static bool IsPrivate(this Register register)
        {
            return register < Register.SP;
        }

        public static bool IsStack(this Register register)
        {
            return register == Register.SP || register == Register.FP;
        }

        public static bool IsValid(this Register register)
        {
            return Enum.IsDefined(typeof(Register), register);
        }

        public static string Name(this Register register)
        {
            return Enum.GetName(typeof(Register), register);
        }

        public static bool IsValid(this Instruction instruction)
        {
            return Enum.IsDefined(typeof(Instruction), instruction);
        }

        public static ushort Set(this Flag flag, ushort bitfield)
        {
            var mask = (ushort)(0x0001 << (ushort)flag);

            return (ushort)(bitfield | mask);
        }

        public static ushort Clear(this Flag flag, ushort bitfield)
        {
            var mask = (ushort)~(0x0001 << (ushort)flag);

            return (ushort)(bitfield & mask);
        }

        public static bool IsSet(this Flag flag, ushort bitfield)
        {
            return ((bitfield >> (ushort)flag) & 0x0001) != 0;
        }
    }
}
