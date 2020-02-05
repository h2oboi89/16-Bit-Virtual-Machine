namespace VM
{
    enum Instruction : byte
    {
        MOV_LIT_REG,
        MOV_REG_REG,
        MOV_REG_MEM,
        MOV_MEM_REG,
        ADD_REG_REG,
        JMP_NOT_EQ,
        PSH_LIT,
        PSH_REG,
        POP,
        CAL_LIT,
        CAL_REG,
        RET
    }
}
