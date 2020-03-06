using System;
using System.Collections.Generic;
using System.Linq;
using VM.Software.Assembling.Parsing;
using VM.Software.Assembling.Scanning;

namespace VM.Software.Assembling
{
    public static class Assembler
    {
        public static IEnumerable<byte> Assemble(string source)
        {
            IEnumerable<Statement> statements;

            try
            {
                statements = Parser.Parse(Scanner.Scan(source));
            }
            catch (Exception e)
            {
                if (e is ScanningException || e is ParsingException)
                {
                    throw new AssemblingException(e.Message, e);
                }
                else
                {
                    throw new AssemblingException("Unexpected error during assembly.", e);
                }
            }

            var size = statements.Sum(s => s.Size);

            var binary = new byte[size];

            ushort address = 0;
            var labels = new List<LabelStatement>();

            foreach (var statement in statements)
            {
                if (statement is LabelStatement label)
                {
                    labels.CheckForDuplicate(label);

                    labels.Add(label);
                }

                address = statement.SetAddress(address);
            }

            address = 0;
            foreach (var statement in statements)
            {
                statement.SetIdentifiers(labels);

                foreach(var b in statement.ToBytes())
                {
                    binary[address++] = b;
                }
            }

            return binary;
        }

        private static void CheckForDuplicate(this List<LabelStatement> labels, LabelStatement label)
        {
            foreach(var labelStatement in labels)
            {
                if (labelStatement.Identifier == label.Identifier)
                {
                    throw new AssemblingException($"Label '{label.Identifier}' is already defined.");
                }
            }
        }
    }
}
