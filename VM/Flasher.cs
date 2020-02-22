namespace VM
{
    /// <summary>
    /// Utility for writing programs to memory.
    /// </summary>
    public sealed class Flasher
    {
        private readonly Memory memory;

        /// <summary>
        /// Creates a new <see cref="Flasher"/> for writing the specified <see cref="Memory"/>.
        /// </summary>
        /// <param name="memory"><see cref="Memory"/> to flash.</param>
        public Flasher(Memory memory)
        {
            this.memory = memory;
            Address = 0;
        }

        private ushort address = 0;

        /// <summary>
        /// Next address in memory that will be written to.
        /// </summary>
        public ushort Address
        {
            get { return address; }
            set
            {
                address = value;
                InstructionCount = 0;
            }
        }

        /// <summary>
        /// Number of <see cref="Instruction"/>s that were written to memory since <see cref="Address"/> was set.
        /// </summary>
        public int InstructionCount { get; private set; }

        /// <summary>
        /// Writes a U8 to memory.
        /// </summary>
        /// <param name="value">Value to write to memory.</param>
        public void WriteU8(byte value)
        {
            memory.SetU8(address++, value);
        }

        /// <summary>
        /// Writes a U16 to memory.
        /// </summary>
        /// <param name="value">Value to write to memory.</param>
        public void WriteU16(ushort value)
        {
            foreach (var b in Utility.GetBytes(value))
            {
                WriteU8(b);
            }
        }

        /// <summary>
        /// Writes a <see cref="Register"/> identifier to memory.
        /// </summary>
        /// <param name="register">Identifier to write to memory.</param>
        private void WriteRegister(Register register)
        {
            WriteU8((byte)register);
        }

        /// <summary>
        /// Writes an <see cref="Instruction"/> identifier to memory.
        /// </summary>
        /// <param name="instruction">Identifier to write to memory.</param>
        public void WriteInstruction(Instruction instruction)
        {
            InstructionCount++;
            WriteU8((byte)instruction);
        }

        /// <summary>
        /// Writes an <see cref="Instruction"/> and <see cref="Register"/> to memory.
        /// </summary>
        /// <param name="instruction"><see cref="Instruction"/> to write to memory.</param>
        /// <param name="register"><see cref="Register"/> to write to memory.</param>
        public void WriteInstruction(Instruction instruction, Register register)
        {
            WriteInstruction(instruction);
            WriteRegister(register);
        }

        /// <summary>
        /// Writes an <see cref="Instruction"/> and <see cref="ushort"/> to memory.
        /// </summary>
        /// <param name="instruction"><see cref="Instruction"/> to write to memory.</param>
        /// <param name="value">Value to write to memory.</param>
        public void WriteInstruction(Instruction instruction, ushort value)
        {
            WriteInstruction(instruction);
            WriteU16(value);
        }

        /// <summary>
        /// Writes an <see cref="Instruction"/>, <see cref="ushort"/>, and <see cref="Register"/> to memory.
        /// </summary>
        /// <param name="instruction"><see cref="Instruction"/> to write to memory.</param>
        /// <param name="value">Value to write to memory.</param>
        /// <param name="register"><see cref="Register"/> to write to memory.</param>
        public void WriteInstruction(Instruction instruction, ushort value, Register register)
        {
            WriteInstruction(instruction);
            WriteU16(value);
            WriteRegister(register);
        }

        /// <summary>
        /// Writes an <see cref="Instruction"/> and two <see cref="Register"/>s to memory.
        /// </summary>
        /// <param name="instruction"><see cref="Instruction"/> to write to memory.</param>
        /// <param name="register1"><see cref="Register"/> to write to memory.</param>
        /// <param name="register2"><see cref="Register"/> to write to memory.</param>
        public void WriteInstruction(Instruction instruction, Register register1, Register register2)
        {
            WriteInstruction(instruction);
            WriteRegister(register1);
            WriteRegister(register2);
        }

        /// <summary>
        /// Writes an <see cref="Instruction"/>, <see cref="Register"/>, and <see cref="ushort"/> to memory.
        /// </summary>
        /// <param name="instruction"><see cref="Instruction"/> to write to memory.</param>
        /// <param name="register"><see cref="Register"/> to write to memory.</param>
        /// <param name="value">Value to write to memory.</param>
        public void WriteInstruction(Instruction instruction, Register register, ushort value)
        {
            WriteInstruction(instruction);
            WriteRegister(register);
            WriteU16(value);
        }

        /// <summary>
        /// Writes an <see cref="Instruction"/>, <see cref="Register"/>, and <see cref="byte"/> to memory.
        /// </summary>
        /// <param name="instruction"><see cref="Instruction"/> to write to memory.</param>
        /// <param name="register"><see cref="Register"/> to write to memory.</param>
        /// <param name="value">Value to write to memory.</param>
        public void WriteInstruction(Instruction instruction, Register register, byte value)
        {
            WriteInstruction(instruction);
            WriteRegister(register);
            WriteU8(value);
        }

        /// <summary>
        /// Writes an <see cref="Instruction"/> and two <see cref="ushort"/>s to memory.
        /// </summary>
        /// <param name="instruction"><see cref="Instruction"/> to write to memory.</param>
        /// <param name="value1">Value to write to memory.</param>
        /// <param name="value2">Value to write to memory.</param>
        public void WriteInstruction(Instruction instruction, ushort value1, ushort value2)
        {
            WriteInstruction(instruction);
            WriteU16(value1);
            WriteU16(value2);
        }
    }
}
