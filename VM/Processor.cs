using System;

namespace VM
{
    /// <summary>
    /// Central Processing Unit of the Virtual Machine
    /// </summary>
    public sealed class Processor
    {
        private readonly Memory memory;

        private readonly Memory registers;

        #region Utility variables for storing references during decode and execution
        private ushort value;
        private ushort valueA;
        private ushort valueB;

        private uint temp;

        private byte shiftAmount;

        private ushort result;

        private ushort address;

        private Register register;
        private Register source;
        private Register destination;
        private Register registerA;
        private Register registerB;

        private Flag flag;
        #endregion

        /// <summary>
        /// Size of <see cref="Register"/>s and Literals.
        /// </summary>
        public const int DATASIZE = sizeof(ushort);

        /// <summary>
        /// Creates a new <see cref="Processor"/> with the specified <see cref="Memory"/>
        /// </summary>
        /// <param name="memory"><see cref="Memory"/> that this <see cref="Processor"/> can utilize.</param>
        public Processor(Memory memory)
        {
            // TODO: memory map? (program, static data, dynamic data, stack, IO) 
            this.memory = memory ?? throw new ArgumentNullException(nameof(memory));

            registers = new Memory((ushort)((Enum.GetValues(typeof(Register)).Length + 1) * DATASIZE));

            Reset();
        }

        private void Reset()
        {
            registers.Clear();

            SetRegister(Register.SP, (ushort)(memory.MaxAddress - DATASIZE), false);
            SetRegister(Register.FP, (ushort)(memory.MaxAddress - DATASIZE), false);
        }

        private void ResetAndThrow(Exception exception)
        {
            Reset();
            throw exception;
        }

        private void ValidateRegister(Register register)
        {
            if (!register.IsValid())
            {
                ResetAndThrow(new InvalidOperationException($"Unknown register {Utility.FormatU8((byte)register)}."));
            }
        }

        private void ValidateInstruction(Instruction instruction)
        {
            if (!instruction.IsValid())
            {
                ResetAndThrow(new InvalidOperationException($"Unknown instruction {Utility.FormatU8((byte)instruction)}."));
            }
        }

        /// <summary>
        /// Gets value of <see cref="Register"/>.
        /// </summary>
        /// <param name="register"><see cref="Register"/> to get value from.</param>
        /// <returns>value of the <see cref="Register"/></returns>
        public ushort GetRegister(Register register)
        {
            ValidateRegister(register);

            return registers.GetU16((ushort)((byte)register * DATASIZE));
        }

        private void SetRegister(Register register, ushort value, bool direct = true)
        {
            if (register.IsPrivate() && direct)
            {
                ResetAndThrow(new InvalidOperationException($"{register.Name()} register cannot be modified directly by code."));
            }

            registers.SetU16((ushort)((byte)register * DATASIZE), value);
        }

        private byte FetchU8()
        {
            var addresss = GetRegister(Register.PC);
            var value = memory.GetU8(addresss);

            SetRegister(Register.PC, (ushort)(addresss + sizeof(byte)), false);

            return value;
        }

        private Register FetchRegister()
        {
            var register = (Register)FetchU8();

            ValidateRegister(register);

            return register;
        }

        private ushort FetchU16()
        {
            var address = GetRegister(Register.PC);
            var value = memory.GetU16(address);

            SetRegister(Register.PC, (ushort)(address + sizeof(ushort)), false);

            return value;
        }

        private void Store(ushort address, ushort value)
        {
            try
            {
                memory.SetU16(address, value);
            }
            catch (IndexOutOfRangeException exception)
            {
                ResetAndThrow(exception);
            }
        }

        #region Flags
        private void SetFlag(Flag flag)
        {
            var value = GetRegister(Register.FLAG);

            value = flag.Set(value);

            SetRegister(Register.FLAG, value, false);
        }

        private void ClearFlag(Flag flag)
        {
            var value = GetRegister(Register.FLAG);

            value = flag.Clear(value);

            SetRegister(Register.FLAG, value, false);
        }

        /// <summary>
        /// Determines if <see cref="Flag"/> is set.
        /// </summary>
        /// <param name="flag"><see cref="Flag"/> to check.</param>
        /// <returns>True if <see cref="Flag"/> is set; otherwise false.</returns>
        public bool IsSet(Flag flag)
        {
            var value = GetRegister(Register.FLAG);

            return flag.IsSet(value);
        }

        private void ClearFlags(Instruction instruction)
        {
            switch (instruction)
            {
                case Instruction.INC:
                case Instruction.DEC:
                case Instruction.ADD:
                case Instruction.SUB:
                case Instruction.MUL:
                case Instruction.DIV:
                    ClearFlag(Flag.ZERO);
                    ClearFlag(Flag.CARRY);
                    break;

                case Instruction.CMP:
                    ClearFlag(Flag.LESSTHAN);
                    ClearFlag(Flag.GREATERTHAN);
                    ClearFlag(Flag.EQUAL);
                    break;

                case Instruction.CMPZ:
                    ClearFlag(Flag.ZERO);
                    break;
            }
        }

        private static bool IsCarryInstruction(Instruction instruction)
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

        private static bool IsZeroInstruction(Instruction instruction)
        {
            switch (instruction)
            {
                case Instruction.INC:
                case Instruction.DEC:
                case Instruction.ADD:
                case Instruction.SUB:
                case Instruction.MUL:
                case Instruction.DIV:
                case Instruction.CMPZ:
                    return true;
                default:
                    return false;
            }
        }

        private void SetFlags(Instruction instruction)
        {
            if (temp > ushort.MaxValue && IsCarryInstruction(instruction))
            {
                SetFlag(Flag.CARRY);
            }

            if (result == 0 && IsZeroInstruction(instruction))
            {
                SetFlag(Flag.ZERO);
            }

            if (instruction == Instruction.CMP)
            {
                if (valueA < valueB)
                {
                    SetFlag(Flag.LESSTHAN);
                }
                else if (valueA == valueB)
                {
                    SetFlag(Flag.EQUAL);
                }
                else if (valueA > valueB)
                {
                    SetFlag(Flag.GREATERTHAN);
                }
            }
        }
        #endregion

        private Instruction Fetch()
        {
            var instruction = (Instruction)FetchU8();

            ValidateInstruction(instruction);

            return instruction;
        }

        private void Decode(Instruction instruction)
        {
            switch (instruction)
            {
                case Instruction.NOP:
                case Instruction.RET:
                case Instruction.RESET:
                case Instruction.HALT:
                    break;

                case Instruction.MOVE:
                    source = FetchRegister();
                    destination = FetchRegister();

                    value = GetRegister(source);
                    break;

                case Instruction.INC:
                case Instruction.DEC:
                    register = FetchRegister();

                    valueA = GetRegister(register);
                    valueB = 1;
                    break;

                case Instruction.LDVR:
                    value = FetchU16();
                    destination = FetchRegister();
                    break;

                case Instruction.LDAR:
                    address = FetchU16();
                    destination = FetchRegister();

                    value = memory.GetU16(address);
                    break;

                case Instruction.LDRR:
                    source = FetchRegister();
                    destination = FetchRegister();

                    address = GetRegister(source);
                    value = memory.GetU16(address);
                    break;

                case Instruction.STVA:
                    value = FetchU16();
                    address = FetchU16();
                    break;

                case Instruction.STVR:
                    value = FetchU16();
                    destination = FetchRegister();

                    address = GetRegister(destination);
                    break;

                case Instruction.STRA:
                    source = FetchRegister();
                    address = FetchU16();

                    value = GetRegister(source);
                    break;

                case Instruction.STRR:
                    source = FetchRegister();
                    destination = FetchRegister();

                    value = GetRegister(source);
                    address = GetRegister(destination);
                    break;

                case Instruction.ADD:
                case Instruction.SUB:
                case Instruction.MUL:
                case Instruction.DIV:
                case Instruction.AND:
                case Instruction.OR:
                case Instruction.XOR:
                case Instruction.CMP:
                    registerA = FetchRegister();
                    registerB = FetchRegister();

                    valueA = GetRegister(registerA);
                    valueB = GetRegister(registerB);

                    register = Register.ACC;
                    break;

                case Instruction.NOT:
                    register = FetchRegister();

                    value = GetRegister(register);
                    break;

                case Instruction.SRL:
                case Instruction.SRR:
                    register = FetchRegister();
                    shiftAmount = FetchU8();

                    value = GetRegister(register);
                    break;

                case Instruction.SRLR:
                case Instruction.SRRR:
                    registerA = FetchRegister();
                    registerB = FetchRegister();

                    value = GetRegister(registerA);
                    shiftAmount = (byte)GetRegister(registerB);
                    break;

                // TODO: Subroutine instructions

                // TODO: Jump instructions

                case Instruction.CMPZ:
                    register = FetchRegister();

                    result = GetRegister(register);
                    break;

                case Instruction.JLT:
                case Instruction.JGT:
                case Instruction.JE:
                case Instruction.JNE:
                case Instruction.JZ:
                case Instruction.JNZ:
                    address = FetchU16();
                    break;

                case Instruction.JLTR:
                case Instruction.JGTR:
                case Instruction.JER:
                case Instruction.JNER:
                case Instruction.JZR:
                case Instruction.JNZR:
                    register = FetchRegister();

                    address = GetRegister(register);
                    break;

                    // TODO: Stack instructions
            }

            SetFlagToCheckForLogicalJumps(instruction);
        }

        private void SetFlagToCheckForLogicalJumps(Instruction instruction)
        {
            switch (instruction)
            {
                case Instruction.JLT:
                case Instruction.JLTR:
                    flag = Flag.LESSTHAN;
                    break;

                case Instruction.JGT:
                case Instruction.JGTR:
                    flag = Flag.GREATERTHAN;
                    break;

                case Instruction.JE:
                case Instruction.JER:
                case Instruction.JNE:
                case Instruction.JNER:
                    flag = Flag.EQUAL;
                    break;

                case Instruction.JZ:
                case Instruction.JZR:
                case Instruction.JNZ:
                case Instruction.JNZR:
                    flag = Flag.ZERO;
                    break;
            }
        }

        private void Execute(Instruction instruction)
        {
            switch (instruction)
            {
                case Instruction.NOP:
                case Instruction.CMP:
                case Instruction.CMPZ:
                    break;

                case Instruction.MOVE:
                case Instruction.LDVR:
                case Instruction.LDAR:
                case Instruction.LDRR:
                    SetRegister(destination, value);
                    break;

                case Instruction.STVA:
                case Instruction.STVR:
                case Instruction.STRA:
                case Instruction.STRR:
                    Store(address, value);
                    break;

                case Instruction.INC:
                case Instruction.ADD:
                    temp = (uint)(valueA + valueB);

                    result = (ushort)(temp);

                    SetRegister(register, result, false);
                    break;

                case Instruction.DEC:
                case Instruction.SUB:
                    temp = (uint)(valueA - valueB);

                    result = (ushort)temp;

                    SetRegister(register, result, false);
                    break;

                case Instruction.MUL:
                    temp = (uint)(valueA * valueB);

                    result = (ushort)temp;

                    SetRegister(register, result, false);
                    break;

                case Instruction.DIV:
                    if (valueB == 0)
                    {
                        ResetAndThrow(new DivideByZeroException());
                    }

                    temp = (uint)(valueA / valueB);

                    result = (ushort)temp;

                    SetRegister(register, result, false);
                    break;

                case Instruction.AND:
                    result = (ushort)(valueA & valueB);

                    SetRegister(register, result, false);
                    break;

                case Instruction.OR:
                    result = (ushort)(valueA | valueB);

                    SetRegister(register, result, false);
                    break;

                case Instruction.XOR:
                    result = (ushort)(valueA ^ valueB);

                    SetRegister(register, result, false);
                    break;

                case Instruction.NOT:
                    result = (ushort)~value;

                    SetRegister(Register.ACC, result, false);
                    break;

                case Instruction.SRL:
                case Instruction.SRLR:
                    result = (ushort)(value << shiftAmount);

                    SetRegister(Register.ACC, result, false);
                    break;

                case Instruction.SRR:
                case Instruction.SRRR:
                    result = (ushort)(value >> shiftAmount);

                    SetRegister(Register.ACC, result, false);
                    break;

                // TODO: Subroutine instructions

                // TODO: Jump instructions

                case Instruction.JLT:
                case Instruction.JLTR:
                case Instruction.JGT:
                case Instruction.JGTR:
                case Instruction.JE:
                case Instruction.JER:
                case Instruction.JZ:
                case Instruction.JZR:
                    if (IsSet(flag))
                    {
                        SetRegister(Register.PC, address, false);
                    }
                    break;

                case Instruction.JNE:
                case Instruction.JNER:
                case Instruction.JNZ:
                case Instruction.JNZR:
                    if (!IsSet(flag))
                    {
                        SetRegister(Register.PC, address, false);
                    }
                    break;

                // TODO: Stack instructions

                // TODO: Halt Instruction

                case Instruction.RESET:
                    Reset();
                    break;
            }
        }

        /// <summary>
        /// Fetches and executes the next <see cref="Instruction"/>.
        /// </summary>
        public void Step()
        {
            var instruction = Fetch();
            Decode(instruction);
            ClearFlags(instruction);
            Execute(instruction);
            SetFlags(instruction);
        }
    }
}