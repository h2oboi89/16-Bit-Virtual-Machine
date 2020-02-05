using System;
using System.Linq;

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
                Console.WriteLine($"{register.ToString().PadLeft(3, ' ')}: {Utility.FormatU16(GetRegister(register))}");
            }
            Console.WriteLine();
        }

        public void ViewMemoryAt(ushort address)
        {
            Console.WriteLine($"{Utility.FormatU16(address)}: {string.Join(' ', memory.Skip(address).Take(8).Select(x => Utility.FormatU8(x)))}");
        }

        public ushort GetRegister(Register register)
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
            ushort value;
            ushort address;
            Register registerTo;
            Register registerFrom;

            switch (instruction)
            {
                case Instruction.MOV_LIT_REG:
                    value = FetchU16();
                    registerTo = (Register)Fetch();

                    SetRegister(registerTo, value);
                    return;

                case Instruction.MOV_REG_REG:
                    registerFrom = (Register)Fetch();
                    registerTo = (Register)Fetch();

                    SetRegister(registerTo, GetRegister(registerFrom));
                    return;

                case Instruction.MOV_REG_MEM:
                    registerFrom = (Register)Fetch();
                    address = FetchU16();

                    memory.SetU16(address, GetRegister(registerFrom));
                    return;

                case Instruction.MOV_MEM_REG:
                    address = FetchU16();
                    registerTo = (Register)Fetch();

                    SetRegister(registerTo, memory.GetU16(address));
                    return;

                case Instruction.ADD_REG_REG:
                    var r1 = (Register)Fetch();
                    var r2 = (Register)Fetch();

                    SetRegister(Register.ACC, (ushort)(GetRegister(r1) + GetRegister(r2)));
                    return;

                case Instruction.JMP_NOT_EQ:
                    value = FetchU16();
                    address = FetchU16();

                    if (value != GetRegister(Register.ACC))
                    {
                        SetRegister(Register.IP, address);
                    }
                    return;
            }
        }

        public void Step()
        {
            Execute((Instruction)Fetch());
        }
    }
}
