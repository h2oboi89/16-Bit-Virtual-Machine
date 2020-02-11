namespace VM
{
    /// <summary>
    /// Virtual Machine <see cref="Processor"/> register identifiers
    /// </summary>
    public enum Register
    {
        #region System Registers
        /// <summary>
        /// Instruction Pointer. 
        /// Address of next <see cref="Instruction"/> to be executed.
        /// </summary>
        IP,
        /// <summary>
        /// Return Address.
        /// Address that execution will resume at following <see cref="Instruction.RET"/>.
        /// </summary>
        RA,
        /// <summary>
        /// Accumulator.
        /// Where result of all Arithmetic and Binary <see cref="Instruction"/>s is stored.
        /// </summary>
        ACC,
        /// <summary>
        /// Flag register.
        /// <list type="bullet">
        ///     <item>
        ///         <term>0</term>
        ///         <description>zero</description>
        ///     </item>
        ///     <item>
        ///         <term>1</term>
        ///         <description>less than</description>
        ///     </item>
        ///     <item>
        ///         <term>2</term>
        ///         <description>greater than</description>
        ///     </item>
        ///     <item>
        ///         <term>3</term>
        ///         <description>equal</description>
        ///     </item>
        ///     <item>
        ///         <term>4</term>
        ///         <description>negative</description>
        ///     </item>
        ///     <item>
        ///         <term>5</term>
        ///         <description>overflow</description>
        ///     </item>
        ///     <item>
        ///         <term>6</term>
        ///         <description>carry</description>
        ///     </item>
        /// </list>
        /// </summary>
        FLG,
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
        #endregion

        #region Temporary Registers
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
        /// <summary>
        /// General purpose register.
        /// Not preserved during subroutines.
        /// </summary>
        T8
        #endregion
    }
}
