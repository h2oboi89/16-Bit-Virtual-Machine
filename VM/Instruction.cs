namespace VM
{
    /// <summary>
    /// Instruction Opcodes for the Virtual Machine
    /// </summary>
    public enum Instruction
    {
        /// <summary>
        /// No Operation.
        /// Does nothing aside from increment the <see cref="Register.PC"/>.
        /// Arguments: NONE.
        /// </summary>
        NOP = default(ushort),

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
        /// Load Value into Register.
        /// Load value into <see cref="Register"/>.
        /// Arguments: Value (<see cref="ushort"/>), Destination (<see cref="Register"/>).
        /// </summary>
        LDVR,
        /// <summary>
        /// Load value at Address into Register
        /// Load value from <see cref="Memory"/> into <see cref="Register"/>.
        /// Arguments: Address (<see cref="ushort"/>), Destination (<see cref="Register"/>).
        /// </summary>
        LDAR,
        /// <summary>
        /// Load value at address in Register into Register.
        /// Load value from <see cref="Memory"/> pointed to by <see cref="Register"/> into another <see cref="Register"/>.
        /// Arguments: Address (<see cref="Register"/>), Destination (<see cref="Register"/>).
        /// </summary>
        LDRR,
        #endregion

        #region Store Instructions
        /// <summary>
        /// Store Value at Address.
        /// Store value into <see cref="Memory"/> at address.
        /// Arguments: Value (<see cref="ushort"/>), Address (<see cref="ushort"/>).
        /// </summary>
        STVA,
        /// <summary>
        /// Store Value at address in Register.
        /// Store value into <see cref="Memory"/> pointed to by <see cref="Register"/>.
        /// Arguments: Value (<see cref="ushort"/>), Address (<see cref="Register"/>).
        /// </summary>
        STVR,
        /// <summary>
        /// Store value in Register at Address.
        /// Store value from <see cref="Register"/> into <see cref="Memory"/>.
        /// Arguments: Source (<see cref="Register"/>), Address (<see cref="ushort"/>).
        /// </summary>
        STRA,
        /// <summary>
        /// Store value in Register at address in Register.
        /// Store value from <see cref="Register"/> into <see cref="Memory"/> pointed to by <see cref="Register"/>.
        /// Arguments: Source (<see cref="Register"/>), Address (<see cref="Register"/>).
        /// </summary>
        STRR,
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

        #region Bitwise Instructions
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
        /// Performs bitwise and on two <see cref="Register"/>s and stores value in <see cref="Register.ACC"/> (A (+) B).
        /// Arguments: A (<see cref="Register"/>), B (<see cref="Register"/>).
        /// </summary>
        XOR,
        /// <summary>
        /// Performs bitwise not on <see cref="Register"/>s and stores value in <see cref="Register.ACC"/> (~A).
        /// Arguments: A (<see cref="Register"/>).
        /// </summary>
        NOT,
        /// <summary>
        /// Shift Register Left.
        /// Shift value in register left by specified amount (Maximum is 15).
        /// Result is stored in <see cref="Register.ACC"/> (Register &lt;&lt; Value).
        /// Arguments: Register (<see cref="Register"/>), Value (<see cref="byte"/>).
        /// </summary>
        SRL,
        /// <summary>
        /// Shift Register Left Register
        /// Shift value in register left by specified amount in register (Maximum is 15).
        /// Result is stored in <see cref="Register.ACC"/> (Register &lt;&lt; Value).
        /// Arguments: Register (<see cref="Register"/>), Value (<see cref="Register"/>).
        /// </summary>
        SRLR,
        /// <summary>
        /// Shift Register Right.
        /// Shift value in register right by specified amount (Maximum is 15).
        /// Result is stored in <see cref="Register.ACC"/> (Register &lt;&lt; Value).
        /// Arguments: Register (<see cref="Register"/>), Value (<see cref="byte"/>).
        /// </summary>
        SRR,
        /// <summary>
        /// Shift Register Right Register
        /// Shift value in register right by specified amount in register (Maximum is 15).
        /// Result is stored in <see cref="Register.ACC"/> (Register &lt;&lt; Value).
        /// Arguments: Register (<see cref="Register"/>), Value (<see cref="Register"/>).
        /// </summary>
        SRRR,
        #endregion

        #region Subroutine Instructions
        /// <summary>
        /// Call subroutine at address.
        /// Argumemnts: Address (<see cref="ushort"/>.
        /// </summary>
        CALL,
        /// <summary>
        /// Call subroutine at address in Register.
        /// Arguments: Address (<see cref="Register"/>).
        /// </summary>
        CALLR,
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
        JUMP,
        /// <summary>
        /// Jump to address specified by <see cref="Register"/>.
        /// Arguments: Address (<see cref="Register"/>).
        /// </summary>
        JUMPR,
        #endregion

        #region Logic Instructions
        /// <summary>
        /// Compare values in <see cref="Register"/>s with each other (A &lt;cmp&gt; B).
        /// Sets bits in <see cref="Register.FLAG"/> that get used by the various Jump <see cref="Instruction"/>s.
        /// Arguments: A (<see cref="Register"/>), B (<see cref="Register"/>).
        /// </summary>
        CMP,
        /// <summary>
        /// Jump if Less Than.
        /// Jumps to address if <see cref="Flag.LESSTHAN"/> is set.
        /// Arguments: Address (<see cref="ushort"/>).
        /// </summary>
        JLT,
        /// <summary>
        /// Jump if Less Than.
        /// Jumps to address pointed to by <see cref="Register"/> if <see cref="Flag.LESSTHAN"/> is set.
        /// Arguments: Address (<see cref="Register"/>).
        /// </summary>
        JLTR,
        /// <summary>
        /// Jump if Greater Than.
        /// Jumps to address if <see cref="Flag.GREATERTHAN"/> is set.
        /// Arguments: Address (<see cref="ushort"/>).
        /// </summary>
        JGT,
        /// <summary>
        /// Jump if Greater Than.
        /// Jumps to address pointed to by <see cref="Register"/> if <see cref="Flag.GREATERTHAN"/> is set.
        /// Arguments: Address (<see cref="Register"/>).
        /// </summary>
        JGTR,
        /// <summary>
        /// Jump if Equal.
        /// Jumps to address if <see cref="Flag.EQUAL"/> is set.
        /// Arguments: Address (<see cref="ushort"/>).
        /// </summary>
        JE,
        /// <summary>
        /// Jump if Equal.
        /// Jumps to address pointed to by <see cref="Register"/> if <see cref="Flag.EQUAL"/> is set.
        /// Arguments: Address (<see cref="Register"/>).
        /// </summary>
        JER,
        /// <summary>
        /// Jump if Not Equal.
        /// Jumps to address if <see cref="Flag.EQUAL"/> is not set.
        /// Arguments: Address (<see cref="ushort"/>).
        /// </summary>
        JNE,
        /// <summary>
        /// Jump if Not Equal.
        /// Jumps to address pointed to by <see cref="Register"/> if <see cref="Flag.EQUAL"/> is not set.
        /// Arguments: Address (<see cref="Register"/>).
        /// </summary>
        JNER,
        /// <summary>
        /// Jump if Zero.
        /// Jumps to address if <see cref="Flag.ZERO"/> is set.
        /// Arguments: Address (<see cref="ushort"/>).
        /// </summary>
        JZ,
        /// <summary>
        /// Jump if Zero.
        /// Jumps to address pointed to by <see cref="Register"/> if <see cref="Flag.ZERO"/> is set.
        /// Arguments: Address (<see cref="Register"/>).
        /// </summary>
        JZR,
        /// <summary>
        /// Jump if Not Zero.
        /// Jumps to address if <see cref="Flag.ZERO"/> not is set.
        /// Arguments: Address (<see cref="ushort"/>).
        /// </summary>
        JNZ,
        /// <summary>
        /// Jump if Not Zero.
        /// Jumps to address pointed to by <see cref="Register"/> if <see cref="Flag.ZERO"/> not is set.
        /// Arguments: Address (<see cref="Register"/>).
        /// </summary>
        JNZR,
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

        #region Processor Instructions
        /// <summary>
        /// Program Halt.
        /// Arguments: NONE.
        /// </summary>
        HALT,
        /// <summary>
        /// Resets <see cref="Processor"/> back to initial state.
        /// Arguments: NONE.
        /// </summary>
        RESET
        #endregion
    }
}
