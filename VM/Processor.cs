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

        private readonly Stack stack;

        private readonly ArithmeticLogicUnit alu;

        private bool continueExecution = false;

        #region Utility variables for storing references during decode and execution
        private ushort value;
        private ushort valueA;
        private ushort valueB;

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
        /// Creates a new <see cref="Processor"/> with the specified <see cref="Memory"/>
        /// </summary>
        /// <param name="memory"><see cref="Memory"/> that this <see cref="Processor"/> can utilize.</param>
        /// <param name="stackSize">Number of items that can fit in the <see cref="Stack"/>.</param>
        public Processor(Memory memory, ushort stackSize)
        {
            // TODO: memory map? (program, static data, dynamic data, stack, IO) 
            this.memory = memory ?? throw new ArgumentNullException(nameof(memory));

            registers = new Memory((ushort)((Enum.GetValues(typeof(Register)).Length + 1) * DATASIZE));

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

        private void Initialize()
        {
            registers.Clear();

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

        /// <summary>
        /// Gets value of <see cref="Register"/>.
        /// </summary>
        /// <param name="register"><see cref="Register"/> to get value from.</param>
        /// <returns>value of the <see cref="Register"/></returns>
        public ushort GetRegister(Register register)
        {
            ValidateRegister(register);

            if (register == Register.SP)
            {
                return stack.StackPointer;
            }
            if (register == Register.FP)
            {
                return stack.FramePointer;
            }
            if (register == Register.ACC)
            {
                return alu.Accumulator;
            }
            if (register == Register.FLAG)
            {
                return (ushort)alu.Flag;
            }

            return registers.GetU16((ushort)((byte)register * DATASIZE));
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
                throw new InvalidOperationException($"{register.Name()} register cannot be modified directly by code.");
            }

            // stop develop from accidentally modifying Stack and ALU registers
            if (register.IsStack() || register.IsAlu())
            {
                throw new InvalidOperationException($"{register.Name()} is managed by another class.");
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

        private void PushState()
        {
            stack.Push(GetRegister(Register.R0));
            stack.Push(GetRegister(Register.R1));
            stack.Push(GetRegister(Register.R2));
            stack.Push(GetRegister(Register.R3));
            stack.Push(GetRegister(Register.R4));
            stack.Push(GetRegister(Register.R5));
            stack.Push(GetRegister(Register.R6));
            stack.Push(GetRegister(Register.R7));

            stack.Push(GetRegister(Register.PC));

            stack.PushFrame();
        }

        private void PopState()
        {
            stack.PopFrame();

            SetRegister(Register.PC, stack.Pop());

            SetRegister(Register.R7, stack.Pop());
            SetRegister(Register.R6, stack.Pop());
            SetRegister(Register.R5, stack.Pop());
            SetRegister(Register.R4, stack.Pop());
            SetRegister(Register.R3, stack.Pop());
            SetRegister(Register.R2, stack.Pop());
            SetRegister(Register.R1, stack.Pop());
            SetRegister(Register.R0, stack.Pop());
        }

        /// <summary>
        /// Determines if <see cref="Flags"/> is set.
        /// </summary>
        /// <param name="flag"><see cref="Flags"/> to check.</param>
        /// <returns>True if <see cref="Flags"/> is set; otherwise false.</returns>
        public bool IsSet(Flags flag)
        {
            return alu.IsSet(flag);
        }

        private Instruction Fetch()
        {
            return (Instruction)FetchU8();
        }

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
                    address = FetchU16();
                    break;

                case Instruction.CALLR:
                    register = FetchRegister();

                    address = GetRegister(register);
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
                    SetRegister(destination, value, true);
                    break;

                case Instruction.STVA:
                case Instruction.STVR:
                case Instruction.STRA:
                case Instruction.STRR:
                    memory.SetU16(address, value);
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
                    SetRegister(Register.PC, jumpAddress);
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
                    SetRegister(Register.PC, alu.JumpAddress);
                    break;

                case Instruction.CALL:
                case Instruction.CALLR:
                    PushState();

                    SetRegister(Register.PC, address);
                    break;

                case Instruction.RET:
                    PopState();
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