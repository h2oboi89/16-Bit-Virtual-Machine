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
            return registers.GetU16((ushort)((byte)register * DATA_SIZE));
        }

        private void SetRegister(Register register, ushort value)
        {
            registers.SetU16((ushort)((byte)register * DATA_SIZE), value);
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
            switch (instruction)
            {
                // TODO: execute each instruction
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