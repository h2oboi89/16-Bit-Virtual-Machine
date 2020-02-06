using System;
using System.Linq;

namespace VM
{
    /// <summary>
    /// Central Processing Unit of the Virtual Machine
    /// </summary>
    public class Processor
    {
        private readonly Memory memory;

        private readonly Memory registers;

        private ushort stackFrameSize;

        /// <summary>
        /// Size of <see cref="Register"/>s and Literals.
        /// </summary>
        public const int DATA_SIZE = sizeof(ushort);

        /// <summary>
        /// Creates a new <see cref="Processor"/> with the specified <see cref="Memory"/>
        /// </summary>
        /// <param name="memory"><see cref="Memory"/> that this <see cref="Processor"/> can utilize.</param>
        public Processor(Memory memory)
        {
            this.memory = memory;

            registers = new Memory(Enum.GetValues(typeof(Register)).Length * DATA_SIZE);

            SetRegister(Register.SP, (ushort)(memory.MaxAddress - DATA_SIZE));
            SetRegister(Register.FP, (ushort)(memory.MaxAddress - DATA_SIZE));

            stackFrameSize = 0;
        }

        /// <summary>
        /// Prints <see cref="Register"/> values to the <see cref="Console"/>
        /// </summary>
        public void Debug()
        {
            foreach (var register in (Register[])Enum.GetValues(typeof(Register)))
            {
                Console.WriteLine($"{register.ToString().PadLeft(3, ' ')}: {Utility.FormatU16(GetRegister(register))}");
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Prints <see cref="Memory"/> values to the <see cref="Console"/>.
        /// </summary>
        /// <param name="address">First <see cref="Memory"/> address to print.</param>
        /// <param name="count">Number of bytes to print.</param>
        public void ViewMemoryAt(ushort address, int count = 8)
        {
            Console.WriteLine($"{Utility.FormatU16(address)}: {string.Join(" ", memory.Skip(address).Take(count).Select(x => Utility.FormatU8(x)))}");
        }

        /// <summary>
        /// Gets value of <see cref="Register"/>.
        /// </summary>
        /// <param name="register"><see cref="Register"/> to get value from.</param>
        /// <returns>value of the <see cref="Register"/></returns>
        public ushort GetRegister(Register register)
        {
            return registers.GetU16((int)register * DATA_SIZE);
        }

        private void SetRegister(Register register, ushort value)
        {
            registers.SetU16((int)register * DATA_SIZE, value);
        }

        private byte Fetch()
        {
            var nextInstructionAddress = GetRegister(Register.IP);
            var value = memory.GetU8(nextInstructionAddress);

            SetRegister(Register.IP, (ushort)(nextInstructionAddress + sizeof(byte)));

            return value;
        }

        private Register FetchRegister()
        {
            return (Register)Fetch();
        }

        private ushort FetchU16()
        {
            var nextInstructionAddress = GetRegister(Register.IP);
            var value = memory.GetU16(nextInstructionAddress);

            SetRegister(Register.IP, (ushort)(nextInstructionAddress + sizeof(ushort)));

            return value;
        }

        private void Push(ushort value)
        {
            var spAddress = GetRegister(Register.SP);
            memory.SetU16(spAddress, value);
            SetRegister(Register.SP, (ushort)(spAddress - DATA_SIZE));
            stackFrameSize += DATA_SIZE;
        }

        private ushort Pop()
        {
            var nextSpAddress = (ushort)(GetRegister(Register.SP) + DATA_SIZE);
            SetRegister(Register.SP, nextSpAddress);
            stackFrameSize -= DATA_SIZE;
            return memory.GetU16(nextSpAddress);
        }

        private void PushState()
        {
            Push(GetRegister(Register.R1));
            Push(GetRegister(Register.R2));
            Push(GetRegister(Register.R3));
            Push(GetRegister(Register.R4));
            Push(GetRegister(Register.R5));
            Push(GetRegister(Register.R6));
            Push(GetRegister(Register.R7));
            Push(GetRegister(Register.R8));
            Push(GetRegister(Register.IP));
            Push(stackFrameSize);

            SetRegister(Register.FP, GetRegister(Register.SP));
            stackFrameSize = 0;
        }

        private void PopState()
        {
            var framePointerAddress = GetRegister(Register.FP);
            SetRegister(Register.SP, framePointerAddress);

            this.stackFrameSize = Pop();
            var stackFrameSize = this.stackFrameSize;

            SetRegister(Register.IP, Pop());
            SetRegister(Register.R8, Pop());
            SetRegister(Register.R7, Pop());
            SetRegister(Register.R6, Pop());
            SetRegister(Register.R5, Pop());
            SetRegister(Register.R4, Pop());
            SetRegister(Register.R3, Pop());
            SetRegister(Register.R2, Pop());
            SetRegister(Register.R1, Pop());

            var argCount = Pop();
            for (var i = 0; i < argCount; i++)
            {
                Pop();
            }

            SetRegister(Register.FP, (ushort)(framePointerAddress + stackFrameSize));
        }

        private void Execute(Instruction instruction)
        {
            ushort value;
            ushort address;
            Register register;
            Register registerTo;
            Register registerFrom;

            switch (instruction)
            {
                case Instruction.MOV_LIT_REG:
                    value = FetchU16();
                    registerTo = FetchRegister();

                    SetRegister(registerTo, value);
                    return;

                case Instruction.MOV_REG_REG:
                    registerFrom = FetchRegister();
                    registerTo = FetchRegister();

                    SetRegister(registerTo, GetRegister(registerFrom));
                    return;

                case Instruction.MOV_REG_MEM:
                    registerFrom = FetchRegister();
                    address = FetchU16();

                    memory.SetU16(address, GetRegister(registerFrom));
                    return;

                case Instruction.MOV_MEM_REG:
                    address = FetchU16();
                    registerTo = FetchRegister();

                    SetRegister(registerTo, memory.GetU16(address));
                    return;

                case Instruction.ADD_REG_REG:
                    var r1 = FetchRegister();
                    var r2 = FetchRegister();

                    SetRegister(Register.ACC, (ushort)(GetRegister(r1) + GetRegister(r2)));
                    return;

                case Instruction.JMP_NOT_EQ:
                    value = FetchU16();
                    address = FetchU16();

                    if (value != GetRegister(Register.ACC))
                    {
                        SetRegister(Register.IP, address);
                    }
                    return;

                case Instruction.PSH_LIT:
                    value = FetchU16();

                    Push(value);
                    return;

                case Instruction.PSH_REG:
                    registerFrom = FetchRegister();

                    Push(GetRegister(registerFrom));
                    return;

                case Instruction.POP:
                    registerTo = FetchRegister();

                    value = Pop();
                    SetRegister(registerTo, value);
                    return;

                case Instruction.CAL_LIT:
                    address = FetchU16();

                    PushState();
                    SetRegister(Register.IP, address);
                    return;

                case Instruction.CAL_REG:
                    register = FetchRegister();

                    address = GetRegister(register);
                    PushState();
                    SetRegister(Register.IP, address);
                    return;

                case Instruction.RET:
                    PopState();
                    return;
            }
        }

        /// <summary>
        /// Fetches and executes the next <see cref="Instruction"/>.
        /// </summary>
        public void Step()
        {
            Execute((Instruction)Fetch());
        }
    }
}