namespace VM
{
    /// <summary>
    /// Instruction Opcodes for the Virtual Machine
    /// </summary>
    public enum Instruction : byte
    {
        /// <summary>
        /// No Operation.<br/>
        /// Does nothing aside from increment the <see cref="Register.PC"/>.<br/>
        /// Arguments: NONE.
        /// </summary>
        NOP = default,

        #region Register Instructions
        /// <summary>
        /// Move value between <see cref="Register"/>s.<br/>
        /// Arguments: Source (<see cref="Register"/>), Destination (<see cref="Register"/>).
        /// </summary>
        MOVE,
        /// <summary>
        /// Increment value in <see cref="Register"/> by 1.<br/>
        /// Arguments: Register (<see cref="Register"/>).
        /// </summary>
        INC,
        /// <summary>
        /// Decrement value in <see cref="Register"/> by 1.<br/>
        /// Arguments: Register (<see cref="Register"/>).
        /// </summary>
        DEC,
        #endregion

        #region Load Instructions
        /// <summary>
        /// Load Value into Register.<br/>
        /// Load value into <see cref="Register"/>.<br/>
        /// Arguments: Value (<see cref="ushort"/>), Destination (<see cref="Register"/>).
        /// </summary>
        LDVR,
        /// <summary>
        /// Load value at Address into Register.<br/>
        /// Load value from <see cref="Hardware.Memory"/> into <see cref="Register"/>.<br/>
        /// Arguments: Address (<see cref="ushort"/>), Destination (<see cref="Register"/>).
        /// </summary>
        LDAR,
        /// <summary>
        /// Load value at address in Register into Register.<br/>
        /// Load value from <see cref="Hardware.Memory"/> pointed to by <see cref="Register"/> into another <see cref="Register"/>.<br/>
        /// Arguments: Address (<see cref="Register"/>), Destination (<see cref="Register"/>).
        /// </summary>
        LDRR,
        /// <summary>
        /// Load Byte Value Into Register.<br/>
        /// Load value into <see cref="Register"/>.<br/>
        /// Arguments: Value (<see cref="byte"/>), Destination (<see cref="Register"/>).
        /// </summary>
        LBVR,
        /// <summary>
        /// Load Byte value at Address into Register.<br/>
        /// Load value from <see cref="Hardware.Memory"/> into <see cref="Register"/>.<br/>
        /// Arguments: Address (<see cref="ushort"/>), Destination (<see cref="Register"/>).
        /// </summary>
        LBAR,
        /// <summary>
        /// Load Byte value at address in Register into Register.<br/>
        /// Load value from <see cref="Hardware.Memory"/> pointed to by <see cref="Register"/> into another <see cref="Register"/>.<br/>
        /// Arguments: Address (<see cref="Register"/>), Destination (<see cref="Register"/>).
        /// </summary>
        LBRR,
        #endregion

        #region Store Instructions
        /// <summary>
        /// Store Value at Address.<br/>
        /// Store value into <see cref="Hardware.Memory"/> at address.<br/>
        /// Arguments: Value (<see cref="ushort"/>), Address (<see cref="ushort"/>).
        /// </summary>
        STVA,
        /// <summary>
        /// Store Value at address in Register.<br/>
        /// Store value into <see cref="Hardware.Memory"/> pointed to by <see cref="Register"/>.<br/>
        /// Arguments: Value (<see cref="ushort"/>), Address (<see cref="Register"/>).
        /// </summary>
        STVR,
        /// <summary>
        /// Store value in Register at Address.<br/>
        /// Store value from <see cref="Register"/> into <see cref="Hardware.Memory"/>.<br/>
        /// Arguments: Source (<see cref="Register"/>), Address (<see cref="ushort"/>).
        /// </summary>
        STRA,
        /// <summary>
        /// Store value in Register at address in Register.<br/>
        /// Store value from <see cref="Register"/> into <see cref="Hardware.Memory"/> pointed to by <see cref="Register"/>.<br/>
        /// Arguments: Source (<see cref="Register"/>), Address (<see cref="Register"/>).
        /// </summary>
        STRR,
        /// <summary>
        /// Store Byte value at Address.<br/>
        /// Store value into <see cref="Hardware.Memory"/> at address.<br/>
        /// Arguments: Value (<see cref="byte"/>), Address (<see cref="ushort"/>).
        /// </summary>
        SBVA,
        /// <summary>
        /// Store Byte value at address in Register.<br/>
        /// Store value into <see cref="Hardware.Memory"/> pointed to by <see cref="Register"/>.<br/>
        /// Arguments: Value (<see cref="byte"/>), Address (<see cref="Register"/>).
        /// </summary>
        SBVR,
        /// <summary>
        /// Store Byte value in Register at Address.<br/>
        /// Store value from <see cref="Register"/> into <see cref="Hardware.Memory"/>.<br/>
        /// Arguments: Source (<see cref="Register"/>), Address (<see cref="ushort"/>).
        /// </summary>
        SBRA,
        /// <summary>
        /// Store Byte value in Register at address in Register.<br/>
        /// Store value from <see cref="Register"/> into <see cref="Hardware.Memory"/> pointed to by <see cref="Register"/>.<br/>
        /// Arguments: Source (<see cref="Register"/>), Address (<see cref="Register"/>).
        /// </summary>
        SBRR,
        #endregion

        #region Arithmetic Instructions
        /// <summary>
        /// Adds values in two registers and stores them in <see cref="Register.ACC"/> (A + B).<br/>
        /// Arguments: A (<see cref="Register"/>), B (<see cref="Register"/>).
        /// </summary>
        ADD,
        /// <summary>
        /// Subtracts values in two registers and stores them in <see cref="Register.ACC"/> (A - B).<br/>
        /// Arguments: A (<see cref="Register"/>), B (<see cref="Register"/>).
        /// </summary>
        SUB,
        /// <summary>
        /// Multiplies values in two registers and stores them in <see cref="Register.ACC"/> (A * B).<br/>
        /// Arguments: A (<see cref="Register"/>), B (<see cref="Register"/>).
        /// </summary>
        MUL,
        /// <summary>
        /// Divides values in two registers and stores them in <see cref="Register.ACC"/> (A / B).<br/>
        /// Arguments: A (<see cref="Register"/>), B (<see cref="Register"/>).
        /// </summary>
        DIV,
        // TODO: Modulo instruction?
        #endregion

        #region Bitwise Instructions
        /// <summary>
        /// Performs bitwise and on two <see cref="Register"/>s and stores value in <see cref="Register.ACC"/> (A &amp; B).<br/>
        /// Arguments: A (<see cref="Register"/>), B (<see cref="Register"/>).
        /// </summary>
        AND,
        /// <summary>
        /// Performs bitwise or on two <see cref="Register"/>s and stores value in <see cref="Register.ACC"/> (A | B).<br/>
        /// Arguments: A (<see cref="Register"/>), B (<see cref="Register"/>).
        /// </summary>
        OR,
        /// <summary>
        /// Performs bitwise and on two <see cref="Register"/>s and stores value in <see cref="Register.ACC"/> (A (+) B).<br/>
        /// Arguments: A (<see cref="Register"/>), B (<see cref="Register"/>).
        /// </summary>
        XOR,
        /// <summary>
        /// Performs bitwise not on <see cref="Register"/>s and stores value in <see cref="Register.ACC"/> (~A).<br/>
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
        /// Shift Register Left Register.<br/>
        /// Shift value in register left by specified amount in register (Maximum is 15).<br/>
        /// Result is stored in <see cref="Register.ACC"/> (Register &lt;&lt; Value).<br/>
        /// Arguments: Register (<see cref="Register"/>), Value (<see cref="Register"/>).
        /// </summary>
        SRLR,
        /// <summary>
        /// Shift Register Right.<br/>
        /// Shift value in register right by specified amount (Maximum is 15).<br/>
        /// Result is stored in <see cref="Register.ACC"/> (Register &lt;&lt; Value).<br/>
        /// Arguments: Register (<see cref="Register"/>), Value (<see cref="byte"/>).
        /// </summary>
        SRR,
        /// <summary>
        /// Shift Register Right Register.<br/>
        /// Shift value in register right by specified amount in register (Maximum is 15).<br/>
        /// Result is stored in <see cref="Register.ACC"/> (Register &lt;&lt; Value).<br/>
        /// Arguments: Register (<see cref="Register"/>), Value (<see cref="Register"/>).
        /// </summary>
        SRRR,
        #endregion

        #region Jump Instructions
        /// <summary>
        /// Jump to address.<br/>
        /// Arguments: Address (<see cref="ushort"/>).
        /// </summary>
        JUMP,
        /// <summary>
        /// Jump to address specified by <see cref="Register"/>.<br/>
        /// Arguments: Address (<see cref="Register"/>).
        /// </summary>
        JUMPR,
        #endregion

        #region Logic Instructions
        /// <summary>
        /// Compare values in <see cref="Register"/>s with each other (A &lt;cmp&gt; B).<br/>
        /// Sets bits in <see cref="Register.FLAG"/> that get used by the various Logical Jump <see cref="Instruction"/>s.<br/>
        /// Arguments: A (<see cref="Register"/>), B (<see cref="Register"/>).
        /// </summary>
        CMP,
        /// <summary>
        /// Compare value in <see cref="Register"/> against zero.<br/>
        /// Sets bits in <see cref="Register.FLAG"/> that get used by the various Logical Jump <see cref="Instruction"/>s.<br/>
        /// Arguments: A (<see cref="Register"/>).
        /// </summary>
        CMPZ,
        // TODO: CMPNZ?
        /// <summary>
        /// Jump if Less Than.<br/>
        /// Jumps to address if <see cref="Flags.LESSTHAN"/> is set.<br/>
        /// Arguments: Address (<see cref="ushort"/>).
        /// </summary>
        JLT,
        /// <summary>
        /// Jump if Less Than.<br/>
        /// Jumps to address pointed to by <see cref="Register"/> if <see cref="Flags.LESSTHAN"/> is set.<br/>
        /// Arguments: Address (<see cref="Register"/>).
        /// </summary>
        JLTR,
        /// <summary>
        /// Jump if Greater Than.<br/>
        /// Jumps to address if <see cref="Flags.GREATERTHAN"/> is set.<br/>
        /// Arguments: Address (<see cref="ushort"/>).
        /// </summary>
        JGT,
        /// <summary>
        /// Jump if Greater Than.<br/>
        /// Jumps to address pointed to by <see cref="Register"/> if <see cref="Flags.GREATERTHAN"/> is set.<br/>
        /// Arguments: Address (<see cref="Register"/>).
        /// </summary>
        JGTR,
        /// <summary>
        /// Jump if Equal.<br/>
        /// Jumps to address if <see cref="Flags.EQUAL"/> is set.<br/>
        /// Arguments: Address (<see cref="ushort"/>).
        /// </summary>
        JE,
        /// <summary>
        /// Jump if Equal.<br/>
        /// Jumps to address pointed to by <see cref="Register"/> if <see cref="Flags.EQUAL"/> is set.<br/>
        /// Arguments: Address (<see cref="Register"/>).
        /// </summary>
        JER,
        /// <summary>
        /// Jump if Not Equal.<br/>
        /// Jumps to address if <see cref="Flags.EQUAL"/> is not set.<br/>
        /// Arguments: Address (<see cref="ushort"/>).
        /// </summary>
        JNE,
        /// <summary>
        /// Jump if Not Equal.<br/>
        /// Jumps to address pointed to by <see cref="Register"/> if <see cref="Flags.EQUAL"/> is not set.<br/>
        /// Arguments: Address (<see cref="Register"/>).
        /// </summary>
        JNER,
        /// <summary>
        /// Jump if Zero.<br/>
        /// Jumps to address if <see cref="Flags.ZERO"/> is set.<br/>
        /// Arguments: Address (<see cref="ushort"/>).
        /// </summary>
        JZ,
        /// <summary>
        /// Jump if Zero.<br/>
        /// Jumps to address pointed to by <see cref="Register"/> if <see cref="Flags.ZERO"/> is set.<br/>
        /// Arguments: Address (<see cref="Register"/>).
        /// </summary>
        JZR,
        /// <summary>
        /// Jump if Not Zero.<br/>
        /// Jumps to address if <see cref="Flags.ZERO"/> not is set.<br/>
        /// Arguments: Address (<see cref="ushort"/>).
        /// </summary>
        JNZ,
        /// <summary>
        /// Jump if Not Zero.<br/>
        /// Jumps to address pointed to by <see cref="Register"/> if <see cref="Flags.ZERO"/> not is set.<br/>
        /// Arguments: Address (<see cref="Register"/>).
        /// </summary>
        JNZR,
        #endregion

        #region Subroutine Instructions
        /// <summary>
        /// Call subroutine at address.<br/>
        /// Arguments: Arguments Count (<see cref="byte"/>), Address (<see cref="ushort"/>).
        /// </summary>
        CALL,
        /// <summary>
        /// Call subroutine at address in Register.<br/>
        /// Arguments: Arguments Count (<see cref="byte"/>), Address (<see cref="Register"/>).
        /// </summary>
        CALLR,
        /// <summary>
        /// Return from subroutine.<br/>
        /// Arguments: Return Values Count (<see cref="byte"/>).
        /// </summary>
        RET,
        #endregion

        #region Stack Instructions
        /// <summary>
        /// Push value from <see cref="Register"/> onto top of stack.<br/>
        /// Arguments: Source (<see cref="Register"/>).
        /// </summary>
        PUSH,
        /// <summary>
        /// Pop value from top of stack into <see cref="Register"/>.<br/>
        /// Arguments: Destination (<see cref="Register"/>).
        /// </summary>
        POP,
        /// <summary>
        /// Peek value from top of stack into <see cref="Register"/>.<br/>
        /// Same as <see cref="POP"/> but <see cref="Register.SP"/> remains unchanged.<br/>
        /// Arguments: Destination (<see cref="Register"/>).
        /// </summary>
        PEEK,
        #endregion

        #region Processor Instructions
        /// <summary>
        /// Program Halt.<br/>
        /// Arguments: NONE.
        /// </summary>
        HALT,
        /// <summary>
        /// Resets <see cref="Hardware.Processor"/> back to initial state.<br/>
        /// Arguments: NONE.
        /// </summary>
        RESET
        #endregion
    }
}
