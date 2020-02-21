namespace VM
{
    /// <summary>
    /// Virtual Machine <see cref="Processor"/> register identifiers
    /// </summary>
    public enum Register
    {
        #region System Registers
        /// <summary>
        /// Program Counter. 
        /// Address of next value to fetch from memory.
        /// </summary>
        PC,
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
        R0,
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
        #endregion

        #region Temporary Registers
        /// <summary>
        /// General purpose register.
        /// Not preserved during subroutines.
        /// </summary>
        T0,
        /// <summary>
        /// General purpose register.
        /// Not preserved during subroutines.
        /// </summary>
        T1,
        /// <summary>
        /// General purpose register.
        /// Not preserved during subroutines.
        /// </summary>
        T2,
        /// <summary>
        /// General purpose register.
        /// Not preserved during subroutines.
        /// </summary>
        T3,
        /// <summary>
        /// General purpose register.
        /// Not preserved during subroutines.
        /// </summary>
        T4,
        /// <summary>
        /// General purpose register.
        /// Not preserved during subroutines.
        /// </summary>
        T5,
        /// <summary>
        /// General purpose register.
        /// Not preserved during subroutines.
        /// </summary>
        T6,
        /// <summary>
        /// General purpose register.
        /// Not preserved during subroutines.
        /// </summary>
        T7,
        #endregion

        #region Subroutine Registers
        /// <summary>
        /// Subroutine Argument Register
        /// </summary>
        A0,
        /// <summary>
        /// Subroutine Argument Register
        /// </summary>
        A1,
        /// <summary>
        /// Subroutine Argument Register
        /// </summary>
        A2,
        /// <summary>
        /// Subroutine Argument Register
        /// </summary>
        A3,
        /// <summary>
        /// Subroutine Return Value Register
        /// </summary>
        V0,
        /// <summary>
        /// Subroutine Return Value Register
        /// </summary>
        V1
        #endregion
    }
}
