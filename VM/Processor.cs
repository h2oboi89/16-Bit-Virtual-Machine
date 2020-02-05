using System;

namespace VM
{
    class Processor
    {
        private readonly Memory memory;

        private readonly Memory registers;

        public Processor(Memory memory)
        {
            this.memory = memory;

            registers = new Memory(Enum.GetValues(typeof(Register)).Length * 2);
        }

        public void Debug()
        {
            foreach (var register in (Register[])Enum.GetValues(typeof(Register)))
            {
                Console.WriteLine($"{register.ToString().PadLeft(3, ' ')}: 0x{GetRegister(register).ToString("X").PadLeft(4, '0')}");
            }
            Console.WriteLine();
        }

        private ushort GetRegister(Register register)
        {
            return registers.GetU16((int)register * 2);
        }

        private void SetRegister(Register register, ushort value)
        {
            registers.SetU16((int)register * 2, value);
        }

        private byte Fetch()
        {
            var nextInstructionAddress = GetRegister(Register.IP);
            var value = memory.GetU8(nextInstructionAddress);

            SetRegister(Register.IP, (ushort)(nextInstructionAddress + sizeof(byte)));

            return value;
        }

        private ushort FetchU16()
        {
            var nextInstructionAddress = GetRegister(Register.IP);
            var value = memory.GetU16(nextInstructionAddress);

            SetRegister(Register.IP, (ushort)(nextInstructionAddress + sizeof(ushort)));

            return value;
        }

        private void Execute(Instruction instruction)
        {
            ushort literal;

            switch (instruction)
            {
                case Instruction.MOV_LIT_R1:
                    literal = FetchU16();
                    SetRegister(Register.R1, literal);
                    return;
                case Instruction.MOV_LIT_R2:
                    literal = FetchU16();
                    SetRegister(Register.R2, literal);
                    return;
                case Instruction.ADD_REG_REG:
                    var r1 = (Register)Fetch();
                    var r2 = (Register)Fetch();
                    var r1Val = GetRegister(r1);
                    var r2Val = GetRegister(r2);
                    SetRegister(Register.ACC, (ushort)(r1Val + r2Val));
                    return;
            }
        }

        public void Step()
        {
            Execute((Instruction)Fetch());
        }
    }
}
