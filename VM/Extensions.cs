﻿using System;

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

        public static ushort Set(this Flags flag, ushort bitfield)
        {
            return (ushort)(bitfield | (ushort)flag);
        }

        public static ushort Clear(this Flags flag, ushort bitfield)
        {
            var mask = ~(ushort)flag;

            return (ushort)(bitfield & mask);
        }

        public static bool IsSet(this Flags flag, ushort bitfield)
        {
            return (bitfield & (ushort)flag) != 0;
        }
    }
}
