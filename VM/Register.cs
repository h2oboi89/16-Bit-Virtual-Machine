namespace VM
{
    /// <summary>
    /// Virtual Machine <see cref="Hardware.Processor"/> register identifiers
    /// </summary>
    public enum Register
    {
        #region System Registers
        /// <summary>
        /// Program Counter. 
        /// Address of next value to fetch from memory.
        /// </summary>
        PC = default(byte),
        /// <summary>
        /// Pointer to Subroutine argument count.
        /// Arguments will be below this value (higher address) on the <see cref="Hardware.Stack"/>.
        /// </summary>
        ARG,
        /// <summary>
        /// Pointer to Subroutine return values count.
        /// Return values will be below this value (higher address) on the <see cref="Hardware.Stack"/>.
        /// </summary>
        RET,
        /// <summary>
        /// Accumulator.
        /// Where result of all Arithmetic and Bitwise <see cref="Instruction"/>s is stored.
        /// </summary>
        ACC,
        /// <summary>
        /// Flag register.
        /// </summary>
        FLAG,
        /// <summary>
        /// Stack Pointer
        /// </summary>
        SP,
        /// <summary>
        /// Frame Pointer
        /// </summary>
        FP,
        #endregion

        #region General Purpose Registers
        /// <summary>
        /// General purpose register.
        /// </summary>
        R0 = 0x10,
        /// <summary>
        /// General purpose register.
        /// </summary>
        R1,
        /// <summary>
        /// General purpose register.
        /// </summary>
        R2,
        /// <summary>
        /// General purpose register.
        /// </summary>
        R3,
        /// <summary>
        /// General purpose register.
        /// </summary>
        R4,
        /// <summary>
        /// General purpose register.
        /// </summary>
        R5,
        /// <summary>
        /// General purpose register.
        /// </summary>
        R6,
        /// <summary>
        /// General purpose register.
        /// </summary>
        R7,
        /// <summary>
        /// General purpose register.
        /// </summary>
        R8,
        /// <summary>
        /// General purpose register.
        /// </summary>
        R9,
        /// <summary>
        /// General purpose register.
        /// </summary>
        R10,
        /// <summary>
        /// General purpose register.
        /// </summary>
        R11,
        /// <summary>
        /// General purpose register.
        /// </summary>
        R12,
        /// <summary>
        /// General purpose register.
        /// </summary>
        R13,
        /// <summary>
        /// General purpose register.
        /// </summary>
        R14,
        /// <summary>
        /// General purpose register.
        /// </summary>
        R15,

        /// <summary>
        /// General purpose register.
        /// </summary>
        S0 = 0x20,
        /// <summary>
        /// General purpose register.
        /// </summary>
        S1,
        /// <summary>
        /// General purpose register.
        /// </summary>
        S2,
        /// <summary>
        /// General purpose register.
        /// </summary>
        S3,
        /// <summary>
        /// General purpose register.
        /// </summary>
        S4,
        /// <summary>
        /// General purpose register.
        /// </summary>
        S5,
        /// <summary>
        /// General purpose register.
        /// </summary>
        S6,
        /// <summary>
        /// General purpose register.
        /// </summary>
        S7,
        /// <summary>
        /// General purpose register.
        /// </summary>
        S8,
        /// <summary>
        /// General purpose register.
        /// </summary>
        S9,
        /// <summary>
        /// General purpose register.
        /// </summary>
        S10,
        /// <summary>
        /// General purpose register.
        /// </summary>
        S11,
        /// <summary>
        /// General purpose register.
        /// </summary>
        S12,
        /// <summary>
        /// General purpose register.
        /// </summary>
        S13,
        /// <summary>
        /// General purpose register.
        /// </summary>
        S14,
        /// <summary>
        /// General purpose register.
        /// </summary>
        S15,

        /// <summary>
        /// General purpose register.
        /// </summary>
        T0 = 0x30,
        /// <summary>
        /// General purpose register.
        /// </summary>
        T1,
        /// <summary>
        /// General purpose register.
        /// </summary>
        T2,
        /// <summary>
        /// General purpose register.
        /// </summary>
        T3,
        /// <summary>
        /// General purpose register.
        /// </summary>
        T4,
        /// <summary>
        /// General purpose register.
        /// </summary>
        T5,
        /// <summary>
        /// General purpose register.
        /// </summary>
        T6,
        /// <summary>
        /// General purpose register.
        /// </summary>
        T7,
        /// <summary>
        /// General purpose register.
        /// </summary>
        T8,
        /// <summary>
        /// General purpose register.
        /// </summary>
        T9,
        /// <summary>
        /// General purpose register.
        /// </summary>
        T10,
        /// <summary>
        /// General purpose register.
        /// </summary>
        T11,
        /// <summary>
        /// General purpose register.
        /// </summary>
        T12,
        /// <summary>
        /// General purpose register.
        /// </summary>
        T13,
        /// <summary>
        /// General purpose register.
        /// </summary>
        T14,
        /// <summary>
        /// General purpose register.
        /// </summary>
        T15,
        #endregion
    }
}
