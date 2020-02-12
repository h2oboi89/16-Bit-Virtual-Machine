namespace VM
{
    /// <summary>
    /// Utility for writing programs to memory.
    /// </summary>
    public class Flasher
    {
        private readonly Memory memory;

        /// <summary>
        /// Creates a new <see cref="Flasher"/> for writing the specified <see cref="Memory"/>.
        /// </summary>
        /// <param name="memory"><see cref="Memory"/> to flash.</param>
        public Flasher(Memory memory)
        {
            this.memory = memory;
        }

        /// <summary>
        /// Next address in memory that will be written to.
        /// </summary>
        public int Address
        {
            get { return memory.Address; }
            set { memory.Address = value; }
        }

        /// <summary>
        /// Writes a U8 to memory.
        /// </summary>
        /// <param name="value">Value to write to memory.</param>
        public void WriteU8(byte value)
        {
            memory.WriteU8(value);
        }

        /// <summary>
        /// Writes a U16 to memory.
        /// </summary>
        /// <param name="value">Value to write to memory.</param>
        public void WriteU16(ushort value)
        {
            memory.WriteU16(value);
        }

        /// <summary>
        /// Writes a <see cref="Register"/> identifier to memory.
        /// </summary>
        /// <param name="register">Identifier to write to memory.</param>
        private void WriteRegister(Register register)
        {
            memory.WriteU8((byte)register);
        }

        /// <summary>
        /// Writes an <see cref="Instruction"/> identifier to memory.
        /// </summary>
        /// <param name="instruction">Identifier to write to memory.</param>
        public void WriteInstruction(Instruction instruction)
        {
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
            memory.WriteU16(value);
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
            memory.WriteU16(value);
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
            memory.WriteU16(value);
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
            memory.WriteU16(value1);
            memory.WriteU16(value2);
        }
    }
}
