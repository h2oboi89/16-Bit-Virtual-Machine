using System;
using System.Collections.Generic;
using System.Linq;

namespace VM.Software.Assembling.Parsing
{
    public sealed class InstructionStatement : Statement
    {
        public Instruction Instruction { get; }
        public IEnumerable<Argument> Arguments { get; }

        public override byte Size => (byte)(sizeof(Instruction) + Arguments.Sum(a => a.Size));

        public InstructionStatement(Instruction instruction, params Argument[] arguments)
        {
            Instruction = instruction;
            Arguments = arguments;
        }

        public override void SetIdentifiers(IEnumerable<LabelStatement> labels)
        {
            foreach(var argument in Arguments)
            {
                argument.SetValue(labels);
            }
        }

        protected override IEnumerable<byte> GetBytes()
        {
            var bytes = new List<byte>
            {
                (byte)Instruction
            };

            foreach(var argument in Arguments)
            {
                bytes.AddRange(argument.GetBytes());
            }

            return bytes;
        }
    }
}
