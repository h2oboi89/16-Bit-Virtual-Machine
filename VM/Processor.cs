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

        private Instruction instruction;

        #region Utility variables for storing references during decode and execution
        private ushort value;
        private ushort valueA;
        private ushort valueB;

        private byte shiftAmount;

        private ushort result;

        private ushort address;

        private Register register;
        private Register source;
        private Register destination;
        private Register registerA;
        private Register registerB;
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

        private void ClearFlags()
        {
            SetRegister(Register.FLAG, 0, false);
        }

        private void Fetch()
        {
            instruction = (Instruction)FetchU8();

            ValidateInstruction(instruction);
        }

        private void Decode()
        {
            switch (instruction)
            {
                case Instruction.NOP:
                case Instruction.RET:
                case Instruction.RESET:
                case Instruction.HALT:
                    return;

                case Instruction.MOVE:
                    source = FetchRegister();
                    destination = FetchRegister();

                    value = GetRegister(source);
                    return;

                case Instruction.INC:
                case Instruction.DEC:
                    register = FetchRegister();

                    ClearFlag(Flag.UNDERFLOW);
                    ClearFlag(Flag.OVERFLOW);
                    // TODO: Other flags?

                    value = GetRegister(register);
                    return;

                case Instruction.LDVR:
                    value = FetchU16();
                    destination = FetchRegister();
                    return;

                case Instruction.LDAR:
                    address = FetchU16();
                    destination = FetchRegister();

                    value = memory.GetU16(address);
                    return;

                case Instruction.LDRR:
                    source = FetchRegister();
                    destination = FetchRegister();

                    address = GetRegister(source);
                    value = memory.GetU16(address);
                    return;

                case Instruction.STVA:
                    value = FetchU16();
                    address = FetchU16();
                    return;

                case Instruction.STVR:
                    value = FetchU16();
                    destination = FetchRegister();

                    address = GetRegister(destination);
                    return;

                case Instruction.STRA:
                    source = FetchRegister();
                    address = FetchU16();

                    value = GetRegister(source);
                    return;

                case Instruction.STRR:
                    source = FetchRegister();
                    destination = FetchRegister();

                    value = GetRegister(source);
                    address = GetRegister(destination);
                    return;

                // TODO: Arithmetic Instructions

                case Instruction.AND:
                case Instruction.OR:
                case Instruction.XOR:
                    registerA = FetchRegister();
                    registerB = FetchRegister();

                    valueA = GetRegister(registerA);
                    valueB = GetRegister(registerB);
                    return;

                case Instruction.NOT:
                    register = FetchRegister();

                    value = GetRegister(register);
                    return;

                case Instruction.SRL:
                case Instruction.SRR:
                    register = FetchRegister();
                    shiftAmount = FetchU8();

                    value = GetRegister(register);
                    return;

                case Instruction.SRLR:
                case Instruction.SRRR:
                    registerA = FetchRegister();
                    registerB = FetchRegister();

                    value = GetRegister(registerA);
                    shiftAmount = (byte)GetRegister(registerB);
                    return;

                    // TODO: Subroutine instructions

                    // TODO: Jump instructions

                    // TODO: Logic instructions

                    // TODO: Stack instructions
            }
        }

        private void Execute()
        {
            switch (instruction)
            {
                case Instruction.NOP:
                    return;

                case Instruction.MOVE:
                case Instruction.LDVR:
                case Instruction.LDAR:
                case Instruction.LDRR:
                    SetRegister(destination, value);
                    return;

                case Instruction.INC:
                    result = (ushort)(value + 1);

                    if (value > result)
                    {
                        SetFlag(Flag.OVERFLOW);
                    }
                    // TODO: Other flags?

                    SetRegister(register, result);
                    return;

                case Instruction.DEC:
                    result = (ushort)(value - 1);

                    if (value < result)
                    {
                        SetFlag(Flag.UNDERFLOW);
                    }
                    // TODO: Other flags?

                    SetRegister(register, result);
                    return;

                case Instruction.STVA:
                case Instruction.STVR:
                case Instruction.STRA:
                case Instruction.STRR:
                    memory.SetU16(address, value);
                    return;

                // TODO: Arithmetic Instructions

                case Instruction.AND:
                    result = (ushort)(valueA & valueB);

                    SetRegister(Register.ACC, result, false);
                    return;

                case Instruction.OR:
                    result = (ushort)(valueA | valueB);

                    SetRegister(Register.ACC, result, false);
                    return;

                case Instruction.NOT:
                    result = (ushort)~value;

                    SetRegister(Register.ACC, result, false);
                    return;

                case Instruction.XOR:
                    result = (ushort)(valueA ^ valueB);

                    SetRegister(Register.ACC, result, false);
                    return;

                case Instruction.SRL:
                case Instruction.SRLR:
                    result = (ushort)(value << shiftAmount);

                    SetRegister(Register.ACC, result, false);
                    return;

                case Instruction.SRR:
                case Instruction.SRRR:
                    result = (ushort)(value >> shiftAmount);

                    SetRegister(Register.ACC, result, false);
                    return;

                // TODO: Subroutine instructions

                // TODO: Jump instructions

                // TODO: Logic instructions

                // TODO: Stack instructions

                // TODO: Halt Instruction

                case Instruction.RESET:
                    Reset();
                    return;
            }
        }

        /// <summary>
        /// Fetches and executes the next <see cref="Instruction"/>.
        /// </summary>
        public void Step()
        {
            Fetch();
            Decode();
            Execute();
        }
    }
}