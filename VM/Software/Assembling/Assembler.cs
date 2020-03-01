using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VM.Software.Assembling
{
    public static class Assembler
    {
        public static IEnumerable<byte> Assemble(string code)
        {
            if (code == null)
            {
                throw new ArgumentNullException(nameof(code));
            }

            var binary = new List<byte>();

            var lines = code.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            foreach(var line in lines)
            {
                var parts = line.Split(null);

                var instruction = Utility.ParseInstruction(parts[0]);

                binary.Add((byte)instruction);

                switch(instruction)
                {
                    case Instruction.MOVE:
                        if (parts.Length <= 3)
                        {

                        }
                        binary.Add((byte)Utility.ParseRegister(parts[1]));
                        binary.Add((byte)Utility.ParseRegister(parts[2]));
                        break;
                }
            }

            return binary;
        }
    }
}
