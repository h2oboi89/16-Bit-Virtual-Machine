namespace VM
{
    /// <summary>
    /// Instruction Opcodes for the Virtual Machine
    /// </summary>
    public enum Instruction : byte
    {
        /// <summary>
        /// Move Literal to <see cref="Register"/>
        /// </summary>
        MOV_LIT_REG,
        /// <summary>
        /// Move <see cref="Register"/> to <see cref="Register"/>
        /// </summary>
        MOV_REG_REG,
        /// <summary>
        /// Move <see cref="Register"/> to <see cref="Memory"/>
        /// </summary>
        MOV_REG_MEM,
        /// <summary>
        /// Move <see cref="Memory"/> to <see cref="Register"/>
        /// </summary>
        MOV_MEM_REG,
        /// <summary>
        /// Add <see cref="Register"/> to <see cref="Register"/>
        /// </summary>
        ADD_REG_REG,
        /// <summary>
        /// Jump if Not Equal
        /// </summary>
        JMP_NOT_EQ,
        /// <summary>
        /// Push Literal to Stack
        /// </summary>
        PSH_LIT,
        /// <summary>
        /// Push <see cref="Register"/> to Stack
        /// </summary>
        PSH_REG,
        /// <summary>
        /// Pop value from Stack
        /// </summary>
        POP,
        /// <summary>
        /// Call Literal
        /// </summary>
        CAL_LIT,
        /// <summary>
        /// Call <see cref="Register"/>
        /// </summary>
        CAL_REG,
        /// <summary>
        /// Return from Call
        /// </summary>
        RET
    }
}
