using System;

namespace VM
{
    /// <summary>
    /// Identifiers for the bits in <see cref="Register.FLAG"/>.
    /// </summary>
    [Flags]
    public enum Flags
    {
        /// <summary>
        /// Set if result of last arithmetic operation was zero.
        /// </summary>
        ZERO = 1,
        /// <summary>
        /// Set if result of last <see cref="Instruction.CMP"/> was that A &lt; B.
        /// </summary>
        LESSTHAN = 2,
        /// <summary>
        /// Set if result of last <see cref="Instruction.CMP"/> was that A == B.
        /// </summary>
        EQUAL = 4,
        /// <summary>
        /// Set if result of last <see cref="Instruction.CMP"/> was that A &gt; B.
        /// </summary>
        GREATERTHAN = 8,
        /// <summary>
        /// Set if result of last arithmetic operation resulted in a carry.
        /// </summary>
        CARRY = 16
    }
}
