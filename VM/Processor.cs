using System;
using System.Linq;

namespace VM
{
    /// <summary>
    /// Central Processing Unit of the Virtual Machine
    /// </summary>
    public sealed class Processor
    {
        private readonly Memory memory;

        private readonly Memory registers;

        private ushort stackFrameSize;

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

            SetRegister(Register.SP, (ushort)(memory.MaxAddress - DATASIZE));
            SetRegister(Register.FP, (ushort)(memory.MaxAddress - DATASIZE));

            stackFrameSize = 0;
        }

        /// <summary>
        /// Gets value of <see cref="Register"/>.
        /// </summary>
        /// <param name="register"><see cref="Register"/> to get value from.</param>
        /// <returns>value of the <see cref="Register"/></returns>
        public ushort GetRegister(Register register)
        {
            return registers.GetU16((ushort)((byte)register * DATASIZE));
        }

        private void SetRegister(Register register, ushort value)
        {
            registers.SetU16((ushort)((byte)register * DATASIZE), value);
        }

        private byte FetchU8()
        {
            var nextInstructionAddress = GetRegister(Register.IP);
            var value = memory.GetU8(nextInstructionAddress);

            SetRegister(Register.IP, (ushort)(nextInstructionAddress + sizeof(byte)));

            return value;
        }

        private Register FetchRegister()
        {
            return (Register)FetchU8();
        }

        private ushort FetchU16()
        {
            var nextInstructionAddress = GetRegister(Register.IP);
            var value = memory.GetU16(nextInstructionAddress);

            SetRegister(Register.IP, (ushort)(nextInstructionAddress + sizeof(ushort)));

            return value;
        }

        private void Execute(Instruction instruction)
        {
            ushort value;

            switch (instruction)
            {
                case Instruction.LDV:
                    value = FetchU16();
                    var register = FetchRegister();

                    SetRegister(register, value);
                    return;

                case Instruction.STV:
                    value = FetchU16();
                    var address = FetchU16();

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