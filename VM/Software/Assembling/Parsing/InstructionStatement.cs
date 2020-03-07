using System;
using System.Collections.Generic;
using System.Linq;

namespace VM.Software.Assembling.Parsing
{
    public sealed class InstructionStatement : Statement
    {
        public Instruction Instruction { get; }
        public IEnumerable<Argument> Arguments { get; }

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

        public override string ToString()
        {
            var parts = new List<string> { $"{Instruction}" };

            foreach(var argument in Arguments)
            {
                parts.Add(argument.ToString());
            }

            return $"{base.ToString()} : " + string.Join(" ", parts);
        }
    }
}
