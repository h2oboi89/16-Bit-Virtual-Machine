using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VM.Software.Assembling.Parsing
{
    public sealed class InstructionStatement : Statement
    {
        public Instruction Instruction { get; }
        public IEnumerable<Argument> Arguments { get; }

        public override int Size => sizeof(Instruction) + Arguments.Sum(a => a.Size);

        public InstructionStatement(Instruction instruction, params Argument[] arguments)
        {
            Instruction = instruction;
            Arguments = arguments;
        }

        public override IEnumerable<byte> GetBytes()
        {
            throw new NotImplementedException();
        }
    }
}
