using System;
using System.Collections.Generic;

namespace VM
{
    /// <summary>
    /// Central Processing Unit of the Virtual Machine
    /// </summary>
    public sealed class Processor
    {
        private readonly Memory memory;

        private readonly Memory registers;

        private readonly Stack<Stack> stacks = new Stack<Stack>();
        private Stack Stack => stacks.Peek();

        private bool continueExecution = false;

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
        /// <param name="stackSize">Number of items that can fit in the <see cref="VM.Stack"/>.</param>
        public Processor(Memory memory, ushort stackSize)
        {
            // TODO: memory map? (program, static data, dynamic data, stack, IO) 
            this.memory = memory ?? throw new ArgumentNullException(nameof(memory));

            registers = new Memory((ushort)((Enum.GetValues(typeof(Register)).Length + 1) * DATASIZE));

            var startAddress = (ushort)(memory.MaxAddress - DATASIZE / 2);
            var endAddress = (ushort)(startAddress - ((stackSize - 1) * DATASIZE));

            stacks.Push(new Stack(memory, startAddress, endAddress));

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

        private void Initialize()
        {
            registers.Clear();

            while(stacks.Count > 1)
            {
                stacks.Pop();
            }

            Stack.Reset();
            SetRegister(Register.SP, Stack.StackPointer);

            // TODO: update once we implement frames
            SetRegister(Register.FP, Stack.StackPointer);
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

        private void SetRegister(Register register, ushort value, bool direct = false)
        {
            if (register.IsPrivate() && direct)
            {
                throw new InvalidOperationException($"{register.Name()} register cannot be modified directly by code.");
            }

            registers.SetU16((ushort)((byte)register * DATASIZE), value);
        }

        private byte FetchU8()
        {
            var addresss = GetRegister(Register.PC);
            var value = memory.GetU8(addresss);

            SetRegister(Register.PC, (ushort)(addresss + sizeof(byte)));

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

            SetRegister(Register.PC, (ushort)(address + sizeof(ushort)));

            return value;
        }

        private void Push(ushort value)
        {
            Stack.Push(value);
            SetRegister(Register.SP, Stack.StackPointer);
        }

        private ushort Pop()
        {
            var value = Stack.Pop();
            SetRegister(Register.SP, Stack.StackPointer);

            return value;
        }

        private void PushState()
        {
            Push(GetRegister(Register.R0));
            Push(GetRegister(Register.R1));
            Push(GetRegister(Register.R2));
            Push(GetRegister(Register.R3));
            Push(GetRegister(Register.R4));
            Push(GetRegister(Register.R5));
            Push(GetRegister(Register.R6));
            Push(GetRegister(Register.R7));

            Push(GetRegister(Register.PC));

            Push((ushort)(Stack.Size + DATASIZE));

            var startAddress = Stack.StackPointer;
            var endAddress = Stack.EndAddress;

            stacks.Push(new Stack(memory, startAddress, endAddress));

            SetRegister(Register.FP, GetRegister(Register.SP));
        }

        #region Flags
        private void SetFlag(Flag flag)
        {
            var value = GetRegister(Register.FLAG);

            value = flag.Set(value);

            SetRegister(Register.FLAG, value);
        }

        private void ClearFlag(Flag flag)
        {
            var value = GetRegister(Register.FLAG);

            value = flag.Clear(value);

            SetRegister(Register.FLAG, value);
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

        private static bool IsCarryFlagInstruction(Instruction instruction)
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

        private static bool IsZeroFlagInstruction(Instruction instruction)
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

        private void ClearFlags(Instruction instruction)
        {
            if (IsCarryFlagInstruction(instruction))
            {
                ClearFlag(Flag.CARRY);
            }

            if (IsZeroFlagInstruction(instruction))
            {
                ClearFlag(Flag.ZERO);
            }

            if (instruction == Instruction.CMP)
            {
                ClearFlag(Flag.LESSTHAN);
                ClearFlag(Flag.GREATERTHAN);
                ClearFlag(Flag.EQUAL);
            }
        }

        private void SetFlags(Instruction instruction)
        {
            if (IsCarryFlagInstruction(instruction) && temp > ushort.MaxValue)
            {
                SetFlag(Flag.CARRY);
            }

            if (IsZeroFlagInstruction(instruction) && result == 0)
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
            return (Instruction)FetchU8();
        }

        private void Decode(Instruction instruction)
        {
            ValidateInstruction(instruction);

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

                case Instruction.JUMP:
                    address = FetchU16();
                    break;

                case Instruction.JUMPR:
                    register = FetchRegister();

                    address = GetRegister(register);
                    break;

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

                case Instruction.CALL:
                    address = FetchU16();
                    break;
                // TODO: Subroutine instructions

                case Instruction.PUSH:
                case Instruction.POP:
                case Instruction.PEEK:
                    register = FetchRegister();
                    break;
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
                    SetRegister(destination, value, true);
                    break;

                case Instruction.STVA:
                case Instruction.STVR:
                case Instruction.STRA:
                case Instruction.STRR:
                    memory.SetU16(address, value);
                    break;

                case Instruction.INC:
                case Instruction.ADD:
                    temp = (uint)(valueA + valueB);

                    result = (ushort)(temp);

                    SetRegister(register, result);
                    break;

                case Instruction.DEC:
                case Instruction.SUB:
                    temp = (uint)(valueA - valueB);

                    result = (ushort)temp;

                    SetRegister(register, result);
                    break;

                case Instruction.MUL:
                    temp = (uint)(valueA * valueB);

                    result = (ushort)temp;

                    SetRegister(register, result);
                    break;

                case Instruction.DIV:
                    if (valueB == 0)
                    {
                        throw new DivideByZeroException();
                    }

                    temp = (uint)(valueA / valueB);

                    result = (ushort)temp;

                    SetRegister(register, result);
                    break;

                case Instruction.AND:
                    result = (ushort)(valueA & valueB);

                    SetRegister(register, result);
                    break;

                case Instruction.OR:
                    result = (ushort)(valueA | valueB);

                    SetRegister(register, result);
                    break;

                case Instruction.XOR:
                    result = (ushort)(valueA ^ valueB);

                    SetRegister(register, result);
                    break;

                case Instruction.NOT:
                    result = (ushort)~value;

                    SetRegister(Register.ACC, result);
                    break;

                case Instruction.SRL:
                case Instruction.SRLR:
                    result = (ushort)(value << shiftAmount);

                    SetRegister(Register.ACC, result);
                    break;

                case Instruction.SRR:
                case Instruction.SRRR:
                    result = (ushort)(value >> shiftAmount);

                    SetRegister(Register.ACC, result);
                    break;

                case Instruction.JUMP:
                case Instruction.JUMPR:
                    SetRegister(Register.PC, address);
                    break;

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
                        SetRegister(Register.PC, address);
                    }
                    break;

                case Instruction.JNE:
                case Instruction.JNER:
                case Instruction.JNZ:
                case Instruction.JNZR:
                    if (!IsSet(flag))
                    {
                        SetRegister(Register.PC, address);
                    }
                    break;

                case Instruction.CALL:
                    PushState();

                    SetRegister(Register.PC, address);
                    break;
                // TODO: Subroutine instructions

                case Instruction.PUSH:
                    value = GetRegister(register);

                    Push(value);
                    break;

                case Instruction.POP:
                    value = Pop();

                    SetRegister(register, value);
                    break;

                case Instruction.PEEK:
                    value = Stack.Peek();

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
                ClearFlags(instruction);
                Execute(instruction);
                SetFlags(instruction);

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