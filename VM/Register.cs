namespace VM
{
    /// <summary>
    /// Virtual Machine <see cref="Processor"/> register identifiers
    /// </summary>
    public enum Register : byte
    {
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
        /// Register #1
        /// </summary>
        R1,
        /// <summary>
        /// Register #2
        /// </summary>
        R2,
        /// <summary>
        /// Register #3
        /// </summary>
        R3,
        /// <summary>
        /// Register #4
        /// </summary>
        R4,
        /// <summary>
        /// Register #5
        /// </summary>
        R5,
        /// <summary>
        /// Register #6
        /// </summary>
        R6,
        /// <summary>
        /// Register #7
        /// </summary>
        R7,
        /// <summary>
        /// Register #8
        /// </summary>
        R8,
        /// <summary>
        /// Stack Pointer
        /// </summary>
        SP,
        /// <summary>
        /// Frame Pointer
        /// </summary>
        FP
    }
}
