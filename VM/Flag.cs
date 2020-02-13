namespace VM
{
    /// <summary>
    /// Identifiers for the bits in <see cref="Register.FLAG"/>.
    /// </summary>
    public enum Flag
    {
        /// <summary>
        /// Set if result of last arithmetic operation was zero.
        /// </summary>
        ZERO,
        /// <summary>
        /// Set if result of last <see cref="Instruction.CMP"/> was that A &lt; B.
        /// </summary>
        LESSTHAN,
        /// <summary>
        /// Set if result of last <see cref="Instruction.CMP"/> was that A == B.
        /// </summary>
        EQUAL,
        /// <summary>
        /// Set if result of last <see cref="Instruction.CMP"/> was that A &gt; B.
        /// </summary>
        GREATERTHAN,
        /// <summary>
        /// Set if result of last arithmetic operation resulted in a carry.
        /// </summary>
        CARRY
    }
}
