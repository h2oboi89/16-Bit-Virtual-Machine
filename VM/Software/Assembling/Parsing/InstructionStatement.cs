using System.Collections.Generic;
using System.Linq;

namespace VM.Software.Assembling.Parsing
{
    /// <summary>
    /// Represents an <see cref="Instruction"/> and its associated arguments in the assembly source.
    /// </summary>
    public sealed class InstructionStatement : Statement
    {
        /// <summary>
        /// User specified <see cref="Instruction"/> from the assembly source.
        /// </summary>
        public Instruction Instruction { get; }

        /// <summary>
        /// <see cref="Instruction"/> arguments parsed from the assembly source.
        /// </summary>
        public IEnumerable<Argument> Arguments { get; }

        /// <summary>
        /// Creates a new <see cref="InstructionStatement"/> with the specified values parsed from the assembly source.
        /// </summary>
        /// <param name="instruction"><see cref="Instruction"/> from assembly source.</param>
        /// <param name="arguments"><see cref="Instruction"/> arguments from assembly source.</param>
        public InstructionStatement(Instruction instruction, params Argument[] arguments)
        {
            Instruction = instruction;
            Arguments = arguments;
        }

        /// <summary>
        /// Sets values for all <see cref="Argument.identifier"/>.
        /// </summary>
        /// <param name="labels">Values for <see cref="Argument.identifier"/>.</param>
        public void SetIdentifiers(IEnumerable<LabelStatement> labels)
        {
            foreach(var argument in Arguments)
            {
                argument.SetValue(labels);
            }
        }

        /// <summary>
        /// Size of this statment in it's <see cref="byte"/> collection form as returned by <see cref="ToBytes"/>.
        /// </summary>
        public ushort Size => (ushort)ToBytes().Count();

        /// <summary>
        /// Returns a collection of <see cref="byte"/>s that represent this <see cref="InstructionStatement"/> as it appears in the final compiled executable.
        /// NOTE: Not all <see cref="Argument"/> values are valid until <see cref="SetIdentifiers(IEnumerable{LabelStatement})"/> has been called.
        /// </summary>
        /// <returns>Collection of <see cref="byte"/>s that represent the current <see cref="InstructionStatement"/>.</returns>
        public IEnumerable<byte> ToBytes()
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

        /// <summary>
        /// Determines whether the specified object is equal to the current <see cref="InstructionStatement"/>.
        /// </summary>
        /// <param name="obj">The object to compare to the current <see cref="InstructionStatement"/>.</param>
        /// <returns>True if the specified object is equal to the current <see cref="InstructionStatement"/>; othersise, false.</returns>
        public override bool Equals(object obj) => Equals(obj as InstructionStatement);

        /// <summary>
        /// Determines whether the specified <see cref="InstructionStatement"/> is equal to the current instance.
        /// </summary>
        /// <param name="other">The <see cref="InstructionStatement"/> to compare to this instance.</param>
        /// <returns>True if specified <see cref="InstructionStatement"/> is equal to this instance; otherwise, false.</returns>
        public bool Equals(InstructionStatement other)
        {
            if (other == null) return false;

            if (!base.Equals(other)) return false;

            if (Instruction != other.Instruction) return false;

            return Arguments.SequenceEqual(other.Arguments);
        }

        /// <summary>
        /// Returns the hash code for this <see cref="InstructionStatement"/>.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode() => base.GetHashCode() + 43 * (byte)Instruction + Arguments.Sum(a => a.GetHashCode());

        /// <summary>
        /// Retuns a <see cref="string"/> that represents the current <see cref="InstructionStatement"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="InstructionStatement"/>.</returns>
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
