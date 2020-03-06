using System;
using System.Collections.Generic;

namespace VM.Software.Assembling.Parsing
{
    public sealed class LabelStatement : Statement
    {
        public string Identifier { get; }

        public LabelStatement(string lexeme)
        {
            if (lexeme == null)
            {
                throw new ArgumentNullException(nameof(lexeme));
            }

            Identifier = lexeme.TrimEnd(':');
        }

        protected override IEnumerable<byte> GetBytes() => Array.Empty<byte>();
    }
}