namespace VM
{
    /// <summary>
    /// Instruction Opcodes for the Virtual Machine
    /// </summary>
    public enum Instruction
    {
        #region Register Instructions
        /// <summary>
        /// Move value between <see cref="Register"/>s. 
        /// Arguments: Source (<see cref="Register"/>), Destination (<see cref="Register"/>).
        /// </summary>
        MOVE,
        /// <summary>
        /// Increment value in <see cref="Register"/> by 1.
        /// Arguments: Register (<see cref="Register"/>).
        /// </summary>
        INC,
        /// <summary>
        /// Decrement value in <see cref="Register"/> by 1.
        /// Arguments: Register (<see cref="Register"/>).
        /// </summary>
        DEC,
        #endregion

        #region Load Instructions
        /// <summary>
        /// Load value into <see cref="Register"/>.
        /// Arguments: Value (<see cref="ushort"/>), Destination (<see cref="Register"/>).
        /// </summary>
        LDV,
        /// <summary>
        /// Load value from <see cref="Memory"/> into <see cref="Register"/>.
        /// Arguments: Address (<see cref="ushort"/>), Destination (<see cref="Register"/>).
        /// </summary>
        LDA,
        /// <summary>
        /// Load value from <see cref="Memory"/> pointed to by <see cref="Register"/> into another <see cref="Register"/>.
        /// Arguments: Address (<see cref="Register"/>), Destination (<see cref="Register"/>).
        /// </summary>
        LDR,
        #endregion

        #region Store Instructions
        /// <summary>
        /// Store value into <see cref="Memory"/>.
        /// Arguments: Value (<see cref="ushort"/>), Address (<see cref="ushort"/>).
        /// </summary>
        STV,
        /// <summary>
        /// Store value from <see cref="Register"/> into <see cref="Memory"/>.
        /// Arguments: Source (<see cref="Register"/>), Address (<see cref="ushort"/>).
        /// </summary>
        STA,
        /// <summary>
        /// Store value from <see cref="Register"/> into <see cref="Memory"/> pointed to by <see cref="Register"/>.
        /// Arguments: Source (<see cref="Register"/>), Address (<see cref="Register"/>).
        /// </summary>
        STR,
        #endregion

        #region Arithmetic Instructions
        /// <summary>
        /// Adds values in two registers and stores them in <see cref="Register.ACC"/> (A + B).
        /// Arguments: A (<see cref="Register"/>), B (<see cref="Register"/>).
        /// </summary>
        ADD,
        /// <summary>
        /// Subtracts values in two registers and stores them in <see cref="Register.ACC"/> (A - B).
        /// Arguments: A (<see cref="Register"/>), B (<see cref="Register"/>).
        /// </summary>
        SUB,
        /// <summary>
        /// Multiplies values in two registers and stores them in <see cref="Register.ACC"/> (A * B).
        /// Arguments: A (<see cref="Register"/>), B (<see cref="Register"/>).
        /// </summary>
        MUL,
        /// <summary>
        /// Divides values in two registers and stores them in <see cref="Register.ACC"/> (A / B).
        /// Arguments: A (<see cref="Register"/>), B (<see cref="Register"/>).
        /// </summary>
        DIV,
        #endregion

        #region Binary Instructions
        /// <summary>
        /// Performs bitwise and on two <see cref="Register"/>s and stores value in <see cref="Register.ACC"/> (A &amp; B).
        /// Arguments: A (<see cref="Register"/>), B (<see cref="Register"/>).
        /// </summary>
        AND,
        /// <summary>
        /// Performs bitwise or on two <see cref="Register"/>s and stores value in <see cref="Register.ACC"/> (A | B).
        /// Arguments: A (<see cref="Register"/>), B (<see cref="Register"/>).
        /// </summary>
        OR,
        /// <summary>
        /// Performs bitwise not on <see cref="Register"/>s and stores value in <see cref="Register.ACC"/> (~A).
        /// Arguments: A (<see cref="Register"/>).
        /// </summary>
        NOT,
        /// <summary>
        /// Performs bitwise and on two <see cref="Register"/>s and stores value in <see cref="Register.ACC"/> (A (+) B).
        /// Arguments: A (<see cref="Register"/>), B (<see cref="Register"/>).
        /// </summary>
        XOR,
        /// <summary>
        /// Shift Register Left by specified amount. Maximum is 15. Result is stored in <see cref="Register.ACC"/> (Register &lt;&lt; Value).
        /// Arguments: Register (<see cref="Register"/>), Value (<see cref="byte"/>).
        /// </summary>
        SRL,
        /// <summary>
        /// Shift Register Right by specified amount. Maximum is 15. Result is stored in <see cref="Register.ACC"/> (Register &gt;&gt; Value).
        /// Arguments: Register (<see cref="Register"/>), Value (<see cref="byte"/>).
        /// </summary>
        SRR,
        #endregion

        #region Subroutine Instructions
        /// <summary>
        /// Call subroutine.
        /// Arguments: Address (<see cref="Register"/>).
        /// </summary>
        CAL,
        /// <summary>
        /// Return from subroutine.
        /// Arguments: NONE
        /// </summary>
        RET,
        #endregion

        #region Jump Instructions
        /// <summary>
        /// Jump to address.
        /// Arguments: Address (<see cref="ushort"/>).
        /// </summary>
        JMP,
        /// <summary>
        /// Jump to address specified by <see cref="Register"/>.
        /// Arguments: Address (<see cref="Register"/>).
        /// </summary>
        JMPR,
        #endregion

        #region Logic Instructions
        /// <summary>
        /// Compare values in <see cref="Register"/>s with each other (A &lt;cmp&gt; B).
        /// Sets bits in <see cref="Register.FLG"/> that get used by the various Jump <see cref="Instruction"/>s.
        /// Arguments: A (<see cref="Register"/>), B (<see cref="Register"/>).
        /// </summary>
        CMP,
        /// <summary>
        /// Jump if Less Than. Jumps to address pointed to by <see cref="Register"/> if <see cref="Register.FLG"/> Less Than bit is set.
        /// Arguments: Address (<see cref="Register"/>).
        /// </summary>
        JLT,
        /// <summary>
        /// Jump if Greater Than. Jumps to address pointed to by <see cref="Register"/> if <see cref="Register.FLG"/> Greater Than bit is set.
        /// Arguments: Address (<see cref="Register"/>).
        /// </summary>
        JGT,
        /// <summary>
        /// Jump if Equal. Jumps to address pointed to by <see cref="Register"/> if <see cref="Register.FLG"/> Equal bit is set.
        /// Arguments: Address (<see cref="Register"/>).
        /// </summary>
        JE,
        /// <summary>
        /// Jump if Not Equal. Jumps to address pointed to by <see cref="Register"/> if <see cref="Register.FLG"/> Equal bit is not set.
        /// Arguments: Address (<see cref="Register"/>).
        /// </summary>
        JNE,
        /// <summary>
        /// Jump if Zero. Jumps to address pointed to by <see cref="Register"/> if <see cref="Register.FLG"/> Zero bit is set.
        /// Arguments: Address (<see cref="Register"/>).
        /// </summary>
        JZ,
        /// <summary>
        /// Jump if Not Zero. Jumps to address pointed to by <see cref="Register"/> if <see cref="Register.FLG"/> Zero bit not is set.
        /// Arguments: Address (<see cref="Register"/>).
        /// </summary>
        JNZ,
        #endregion

        #region Stack Instructions
        /// <summary>
        /// Push value from <see cref="Register"/> onto top of stack.
        /// Arguments: Source (<see cref="Register"/>).
        /// </summary>
        PUSH,
        /// <summary>
        /// Pop value from top of stack into <see cref="Register"/>.
        /// Arguments: Destination (<see cref="Register"/>).
        /// </summary>
        POP,
        /// <summary>
        /// Peek value from top of stack into <see cref="Register"/>.
        /// Same as <see cref="POP"/> but <see cref="Register.SP"/> remains unchanged.
        /// Arguments: Destination (<see cref="Register"/>).
        /// </summary>
        PEEK,
        #endregion

        #region System Instructions
        /// <summary>
        /// Perform system call.
        /// Arguments: ???
        /// </summary>
        SYS,
        /// <summary>
        /// Program Halt.
        /// Arguments: NONE.
        /// </summary>
        HALT
        #endregion
    }
}
