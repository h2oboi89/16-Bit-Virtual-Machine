using System;
using System.Collections.Generic;

namespace VM.Software.Assembling.Parsing
{
    public sealed class LabelStatement : Statement
    {
        public string Identifier { get; }

        public override int Size => sizeof(ushort);

        public LabelStatement(string lexeme)
        {
            if (lexeme == null)
            {
                throw new ArgumentNullException(nameof(lexeme));
            }

            Identifier = lexeme.TrimEnd(':');
        }

        public override IEnumerable<byte> GetBytes()
        {
            throw new System.NotImplementedException();
        }
    }
}