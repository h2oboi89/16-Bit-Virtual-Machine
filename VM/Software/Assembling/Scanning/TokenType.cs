namespace VM.Software.Assembling.Scanning
{
    /// <summary>
    /// Types for <see cref="Token"/>.
    /// </summary>
    public enum TokenType
    {
        /// <summary>
        /// Variable names
        /// </summary>
        IDENTIFIER,
        /// <summary>
        /// <see cref="Instruction"/> value
        /// </summary>
        INSTRUCTION,
        /// <summary>
        /// <see cref="Register"/> value
        /// </summary>
        REGISTER,
        /// <summary>
        /// <see cref="ushort"/> value
        /// </summary>
        NUMBER,
        /// <summary>
        /// <see cref="char"/> value
        /// </summary>
        CHARACTER,
        /// <summary>
        /// Label declarations
        /// </summary>
        LABEL,
        /// <summary>
        /// Section declarations
        /// </summary>
        SECTION,
        /// <summary>
        /// End of file marker
        /// </summary>
        EOF
    }
}
