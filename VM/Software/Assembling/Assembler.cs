using System;
using System.Collections.Generic;
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

                throw;
            }

            var labels = DetermineAddresses(statements);

            return GenerateBinary(statements, labels);
        }

        private static IEnumerable<LabelStatement> DetermineAddresses(IEnumerable<Statement> statements)
        {
            ushort address = 0;
            var labels = new List<LabelStatement>();

            foreach (var statement in statements)
            {
                if (statement is LabelStatement label)
                {
                    labels.CheckForDuplicate(label);

                    labels.Add(label);
                }

                statement.Address = address;

                address += statement.Size;
            }

            return labels;
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

        private static IEnumerable<byte> GenerateBinary(IEnumerable<Statement> statements, IEnumerable<LabelStatement> labels)
        {
            var binary = new List<byte>();

            foreach (var statement in statements)
            {
                statement.SetIdentifiers(labels);

                binary.AddRange(statement.ToBytes());
            }

            return binary;
        }
    }
}
