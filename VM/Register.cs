namespace VM
{
    /// <summary>
    /// Virtual Machine <see cref="Processor"/> register identifiers
    /// </summary>
    public enum Register : byte
    {
        /// <summary>
        /// Instruction Pointer
        /// </summary>
        IP,
        /// <summary>
        /// Accumulator
        /// </summary>
        ACC,
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
