using System;
using System.Collections.Generic;
using VM.Software.Assembling.Parsing;
using VM.Software.Assembling.Scanning;

namespace VM.Software.Assembling
{
    /// <summary>
    /// Converts assembly into executable program for the <see cref="Hardware.Processor"/> to run.
    /// </summary>
    public static class Assembler
    {
        /// <summary>
        /// Converts assembly source code into executable binary.
        /// </summary>
        /// <param name="source">Assembly source code.</param>
        /// <returns>Collection of bytes representing executable.</returns>
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

            // FUTURE: optimizations;
            // - replace U16 values with U8 values where possible to save a byte?

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

                if (statement is InstructionStatement instruction)
                {
                    address += instruction.Size;
                }
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
                if (statement is InstructionStatement instructionStatement) {
                    instructionStatement.SetIdentifiers(labels);
                    binary.AddRange(instructionStatement.ToBytes());
                }

            }

            return binary;
        }
    }
}
