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
            this.memory = memory ?? throw new ArgumentNullException(nameof(memory));

            registers = new Memory((ushort)((Enum.GetValues(typeof(Register)).Length + 1) * DATASIZE));

            Reset();
        }

        private void Reset()
        {
            SetRegister(Register.PC, 0, false);
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
            if (!Enum.IsDefined(typeof(Register), register))
            {
                ResetAndThrow(new InvalidOperationException($"Unknown register {Utility.FormatU8((byte)register)}."));
            }
        }

        private void ValidateInstruction(Instruction instruction)
        {
            if (!Enum.IsDefined(typeof(Instruction), instruction))
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

        private static string GetRegisterName(Register register)
        {
            return Enum.GetName(typeof(Register), register);
        }

        private void SetRegister(Register register, ushort value, bool code = true)
        {
            if (register.IsPrivate() && code)
            {
                ResetAndThrow(new InvalidOperationException($"{GetRegisterName(register)} register cannot be modified directly by code."));
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

        private void SetFlag(Flag flag)
        {
            var currentValue = GetRegister(Register.FLAG);
            var mask = (ushort)(0x0001 << (ushort)flag);

            var value = (ushort)(currentValue | mask);
            SetRegister(Register.FLAG, value, false);
        }

        private void ClearFlag(Flag flag)
        {
            var currentValue = GetRegister(Register.FLAG);
            var mask = (ushort)~(1 << (ushort)flag);

            var value = (ushort)(currentValue & mask);
            SetRegister(Register.FLAG, value, false);
        }

        /// <summary>
        /// Gets <see cref="Flag"/> value.
        /// </summary>
        /// <param name="flag"><see cref="Flag"/> to get.</param>
        /// <returns>True if <see cref="Flag"/> is set; otherwise false.</returns>
        public bool GetFlag(Flag flag)
        {
            var value = GetRegister(Register.FLAG);
            var flagValue = (value >> (ushort)flag) & 0x0001;

            return flagValue != 0;
        }

        private void ClearFlags()
        {
            SetRegister(Register.FLAG, 0, false);
        }

        private void Execute(Instruction instruction)
        {
            ValidateInstruction(instruction);

            ushort value;
            ushort address;
            ushort currentValue;
            Register register;
            Register source;
            Register destination;

            // FUTURE: break up into Decode and Execute blocks
            switch (instruction)
            {
                case Instruction.NOP:
                    return;

                case Instruction.MOVE:
                    source = FetchRegister();
                    destination = FetchRegister();

                    value = GetRegister(source);

                    SetRegister(destination, value);
                    return;

                case Instruction.INC:
                    register = FetchRegister();

                    ClearFlag(Flag.UNDERFLOW);
                    ClearFlag(Flag.OVERFLOW);

                    currentValue = GetRegister(register);
                    value = (ushort)(currentValue + 1);

                    if (currentValue > value)
                    {
                        SetFlag(Flag.OVERFLOW);
                    }

                    SetRegister(register, value);
                    return;

                case Instruction.DEC:
                    register = FetchRegister();

                    ClearFlag(Flag.UNDERFLOW);
                    ClearFlag(Flag.OVERFLOW);

                    currentValue = GetRegister(register);
                    value = (ushort)(currentValue - 1);

                    if (currentValue < value)
                    {
                        SetFlag(Flag.UNDERFLOW);
                    }

                    SetRegister(register, value);
                    return;

                case Instruction.LDVR:
                    value = FetchU16();
                    destination = FetchRegister();

                    SetRegister(destination, value);
                    return;

                case Instruction.LDAR:
                    address = FetchU16();
                    destination = FetchRegister();

                    value = memory.GetU16(address);

                    SetRegister(destination, value);
                    return;

                case Instruction.LDRR:
                    source = FetchRegister();
                    destination = FetchRegister();

                    address = GetRegister(source);
                    value = memory.GetU16(address);

                    SetRegister(destination, value);
                    return;

                case Instruction.STVA:
                    value = FetchU16();
                    address = FetchU16();

                    memory.SetU16(address, value);
                    return;

                case Instruction.STVR:
                    value = FetchU16();
                    destination = FetchRegister();

                    address = GetRegister(destination);

                    memory.SetU16(address, value);
                    return;

                case Instruction.STRA:
                    source = FetchRegister();
                    address = FetchU16();

                    value = GetRegister(source);

                    memory.SetU16(address, value);
                    return;

                case Instruction.STRR:
                    source = FetchRegister();
                    destination = FetchRegister();

                    value = GetRegister(source);
                    address = GetRegister(destination);

                    memory.SetU16(address, value);
                    return;
            }
        }

        /// <summary>
        /// Fetches and executes the next <see cref="Instruction"/>.
        /// </summary>
        public void Step()
        {
            Execute((Instruction)FetchU8());
        }
    }
}