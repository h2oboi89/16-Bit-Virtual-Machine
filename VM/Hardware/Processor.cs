﻿using System;

namespace VM.Hardware
{
    /// <summary>
    /// Central Processing Unit of the Virtual Machine
    /// </summary>
    public sealed class Processor
    {
        private readonly Memory memory;

        private readonly Memory registers;

        private readonly Stack stack;

        private readonly ArithmeticLogicUnit alu;

        private bool continueExecution = false;

        #region Utility variables for storing references during decode and execution
        private ushort value;
        private ushort valueA;
        private ushort valueB;

        private ushort count;
        private ushort address;
        private ushort jumpAddress;

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
        /// Address of the next value to fetch from <see cref="Memory"/>.
        /// </summary>
        public ushort ProgramCounter { get; private set; }

        /// <summary>
        /// Creates a new <see cref="Processor"/> with the specified <see cref="Memory"/>
        /// </summary>
        /// <param name="memory"><see cref="Memory"/> that this <see cref="Processor"/> can utilize.</param>
        /// <param name="stackSize">Number of items that can fit in the <see cref="Stack"/>.</param>
        public Processor(Memory memory, ushort stackSize)
        {
            // TODO: memory map? (program, static data, dynamic data, stack, IO) 
            this.memory = memory ?? throw new ArgumentNullException(nameof(memory));

            registers = new Memory(Utility.GeneralPurposeRegisterMemorySize());

            var startAddress = (ushort)(memory.MaxAddress - DATASIZE / 2);
            var endAddress = (ushort)(startAddress - ((stackSize - 1) * DATASIZE));

            stack = new Stack(memory, startAddress, endAddress);

            alu = new ArithmeticLogicUnit();

            Initialize();
        }

        /// <summary>
        /// Event handler for <see cref="Reset"/> event.
        /// </summary>
        /// <param name="sender"><see cref="Processor"/> that fired the event.</param>
        /// <param name="e"><see cref="ResetEventArgs"/> for the event.</param>
        public delegate void ResetEventHandler(object sender, ResetEventArgs e);

        /// <summary>
        /// Event fired when processor resets. Execution is terminated when this fires.
        /// This can be from either the <see cref="Instruction.RESET"/> or an <see cref="Exception"/> during execution.
        /// </summary>
        public event ResetEventHandler Reset;

        /// <summary>
        /// Event fired when processor executes a <see cref="Instruction.HALT"/>.
        /// </summary>
        public event EventHandler Halt;

        /// <summary>
        /// Event fired after each <see cref="Instruction"/> is executed.
        /// </summary>
        public event EventHandler Tick;

        /// <summary>
        /// Resets processor back to initial state.
        /// </summary>
        public void ResetProcessor()
        {
            Initialize();
            Reset(this, new ResetEventArgs(Instruction.NOP));
        }

        /// <summary>
        /// Resets <see cref="Processor"/> to intitial state.
        /// </summary>
        public void Initialize()
        {
            ProgramCounter = 0;
            registers.Reset();
            stack.Reset();
            alu.Reset();
        }

        private static void ValidateRegister(Register register)
        {
            if (!register.IsValid())
            {
                throw new InvalidOperationException($"Unknown register {Utility.FormatU8((byte)register)}.");
            }
        }

        private static void ValidateInstruction(Instruction instruction)
        {
            if (!instruction.IsValid())
            {
                throw new InvalidOperationException($"Unknown instruction {Utility.FormatU8((byte)instruction)}.");
            }
        }

        private ushort GetRegister(Register register)
        {
            ValidateRegister(register);

            switch (register)
            {
                case Register.PC: return ProgramCounter;
                case Register.ARG: return stack.ArgumentsPointer;
                case Register.RET: return stack.ReturnsPointer;
                case Register.ACC: return alu.Accumulator;
                case Register.FLAG: return (ushort)alu.Flag;
                case Register.SP: return stack.StackPointer;
                case Register.FP: return stack.FramePointer;
                default: return registers.GetU16(register.MemoryAddress());
            }
        } 

        /// <summary>
        /// Sets the value of <see cref="Register"/> to <paramref name="value"/>.
        /// NOTE: <see cref="Register.SP"/> and <see cref="Register.FP"/> are managed by <see cref="Stack"/> and should not be modified by this method.
        /// </summary>
        /// <param name="register"><see cref="Register"/> whose value is to be set.</param>
        /// <param name="value">Value to store in register.</param>
        /// <param name="direct">True if a direct result of an <see cref="Instruction"/>; otherwise false.</param>
        private void SetRegister(Register register, ushort value, bool direct = false)
        {
            if (direct && register.IsPrivate())
            {
                throw new InvalidOperationException($"{register} register cannot be modified directly by code.");
            }

            switch (register)
            {
                case Register.ACC: alu.Accumulator = value; break;
                case Register.FLAG: alu.Flag = (Flags)value; break;
                case Register.SP: stack.StackPointer = value; break;
                case Register.FP: stack.FramePointer = value; break;
                default: registers.SetU16(register.MemoryAddress(), value); break;
            }
        }

        /// <summary>
        /// Gets value of <see cref="Register"/>.
        /// </summary>
        /// <param name="register"><see cref="Register"/> to get value from.</param>
        /// <returns>value of the <paramref name="register"/>.</returns>
        public ushort this[Register register] => GetRegister(register);

        private byte FetchU8()
        {
            var value = memory.GetU8(ProgramCounter);

            ProgramCounter += sizeof(byte);

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
            var value = memory.GetU16(ProgramCounter);

            ProgramCounter += sizeof(ushort);

            return value;
        }

        /// <summary>
        /// Determines if <paramref name="flag"/> is set.
        /// </summary>
        /// <param name="flag"><see cref="Flags"/> to check.</param>
        /// <returns>True if <paramref name="flag"/> is set; otherwise, false.</returns>
        public bool this[Flags flag] => alu.IsSet(flag);

        private Instruction Fetch() => (Instruction)FetchU8();

        private void Decode(Instruction instruction)
        {
            ValidateInstruction(instruction);

            switch (instruction)
            {
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

                case Instruction.LBVR:
                    value = FetchU8();
                    destination = FetchRegister();
                    break;

                case Instruction.LBAR:
                    address = FetchU16();
                    destination = FetchRegister();

                    value = memory.GetU8(address);
                    break;

                case Instruction.LBRR:
                    source = FetchRegister();
                    destination = FetchRegister();

                    address = GetRegister(source);
                    value = memory.GetU8(address);
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
                case Instruction.SBRA:
                    source = FetchRegister();
                    address = FetchU16();

                    value = GetRegister(source);
                    break;

                case Instruction.STRR:
                case Instruction.SBRR:
                    source = FetchRegister();
                    destination = FetchRegister();

                    value = GetRegister(source);
                    address = GetRegister(destination);
                    break;

                case Instruction.SBVA:
                    value = FetchU8();
                    address = FetchU16();
                    break;

                case Instruction.SBVR:
                    value = FetchU8();
                    destination = FetchRegister();

                    address = GetRegister(destination);
                    break;

                case Instruction.ADD:
                case Instruction.SUB:
                case Instruction.MUL:
                case Instruction.DIV:
                case Instruction.MOD:
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

                    valueA = GetRegister(register);
                    break;

                case Instruction.SRL:
                case Instruction.SRR:
                    register = FetchRegister();
                    valueB = FetchU8();

                    valueA = GetRegister(register);
                    break;

                case Instruction.SRLR:
                case Instruction.SRRR:
                    registerA = FetchRegister();
                    registerB = FetchRegister();

                    valueA = GetRegister(registerA);
                    valueB = GetRegister(registerB);
                    break;

                case Instruction.JUMP:
                    jumpAddress = FetchU16();
                    break;

                case Instruction.JUMPR:
                    register = FetchRegister();

                    jumpAddress = GetRegister(register);
                    break;

                case Instruction.CMPZ:
                    register = FetchRegister();

                    valueA = GetRegister(register);
                    break;

                case Instruction.JLT:
                case Instruction.JGT:
                case Instruction.JE:
                case Instruction.JNE:
                case Instruction.JZ:
                case Instruction.JNZ:
                    jumpAddress = FetchU16();
                    address = GetRegister(Register.PC);
                    break;

                case Instruction.JLTR:
                case Instruction.JGTR:
                case Instruction.JER:
                case Instruction.JNER:
                case Instruction.JZR:
                case Instruction.JNZR:
                    register = FetchRegister();

                    jumpAddress = GetRegister(register);
                    address = GetRegister(Register.PC);
                    break;

                case Instruction.CALL:
                    count = FetchU8();
                    address = FetchU16();
                    break;

                case Instruction.CALLR:
                    count = FetchU8();
                    register = FetchRegister();

                    address = GetRegister(register);
                    break;

                case Instruction.RET:
                    count = FetchU8();
                    break;

                case Instruction.PUSH:
                case Instruction.POP:
                case Instruction.PEEK:
                    register = FetchRegister();
                    break;
            }
        }

        private void Execute(Instruction instruction)
        {
            switch (instruction)
            {
                case Instruction.MOVE:
                case Instruction.LDVR:
                case Instruction.LDAR:
                case Instruction.LDRR:
                case Instruction.LBVR:
                case Instruction.LBAR:
                case Instruction.LBRR:
                    SetRegister(destination, value, true);
                    break;

                case Instruction.STVA:
                case Instruction.STVR:
                case Instruction.STRA:
                case Instruction.STRR:
                    memory.SetU16(address, value);
                    break;

                case Instruction.SBVA:
                case Instruction.SBVR:
                case Instruction.SBRA:
                case Instruction.SBRR:
                    memory.SetU8(address, (byte)value);
                    break;

                case Instruction.INC:
                case Instruction.DEC:
                    alu.Execute(instruction, valueA, valueB);

                    SetRegister(register, alu.Accumulator);
                    break;

                case Instruction.ADD:
                case Instruction.SUB:
                case Instruction.MUL:
                case Instruction.DIV:
                case Instruction.MOD:
                case Instruction.AND:
                case Instruction.OR:
                case Instruction.XOR:
                case Instruction.NOT:
                case Instruction.SRL:
                case Instruction.SRLR:
                case Instruction.SRR:
                case Instruction.SRRR:
                case Instruction.CMP:
                case Instruction.CMPZ:
                    alu.Execute(instruction, valueA, valueB);
                    break;

                case Instruction.JUMP:
                case Instruction.JUMPR:
                    ProgramCounter = jumpAddress;
                    break;

                case Instruction.JLT:
                case Instruction.JLTR:
                case Instruction.JGT:
                case Instruction.JGTR:
                case Instruction.JE:
                case Instruction.JER:
                case Instruction.JNE:
                case Instruction.JNER:
                case Instruction.JZ:
                case Instruction.JZR:
                case Instruction.JNZ:
                case Instruction.JNZR:
                    alu.Execute(instruction, jumpAddress, address);

                    ProgramCounter = alu.JumpAddress;
                    break;

                case Instruction.CALL:
                case Instruction.CALLR:
                    stack.Call(count, GetRegister(Register.PC));

                    ProgramCounter = address;
                    break;

                case Instruction.RET:
                    ProgramCounter = stack.Return(count);
                    break;

                case Instruction.PUSH:
                    value = GetRegister(register);

                    stack.Push(value);
                    break;

                case Instruction.POP:
                    value = stack.Pop();

                    SetRegister(register, value);
                    break;

                case Instruction.PEEK:
                    value = stack.Peek();

                    SetRegister(register, value);
                    break;

                case Instruction.HALT:
                    continueExecution = false;
                    break;

                case Instruction.RESET:
                    continueExecution = false;
                    Initialize();
                    break;
            }
        }

        /// <summary>
        /// Fetches and executes the next <see cref="Instruction"/>.
        /// </summary>
        public void Step()
        {
            var instruction = Fetch();

            try
            {
                Decode(instruction);
                Execute(instruction);

                Tick?.Invoke(this, new EventArgs());

                if (instruction == Instruction.HALT)
                {
                    Halt?.Invoke(this, new EventArgs());
                }

                if (instruction == Instruction.RESET)
                {
                    Reset?.Invoke(this, new ResetEventArgs(instruction, null));
                }
            }
            catch (Exception e)
            {
                continueExecution = false;
                Initialize();

                Reset?.Invoke(this, new ResetEventArgs(instruction, e));

                // FUTURE: don't throw exception?
                throw;
            }
        }

        /// <summary>
        /// Executes program until a <see cref="Instruction.HALT"/> occurs or an <see cref="Exception"/> is thrown.
        /// </summary>
        public void Run()
        {
            continueExecution = true;

            while (continueExecution)
            {
                Step();
            }
        }
    }
}