using System;

namespace VM.Software.Assembling.Parsing
{
    /// <summary>
    /// Represents a label in the assembly source.
    /// </summary>
    public sealed class LabelStatement : Statement
    {
        /// <summary>
        /// User assigned name for this label.
        /// </summary>
        public string Identifier { get; }

        /// <summary>
        /// Creates a new <see cref="LabelStatement"/> from the specied lexeme from the assembly source.
        /// </summary>
        /// <param name="lexeme">Label that was parsed from the assembly source.</param>
        public LabelStatement(string lexeme)
        {
            if (lexeme == null)
            {
                throw new ArgumentNullException(nameof(lexeme));
            }

            Identifier = lexeme.TrimEnd(':');
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current <see cref="LabelStatement"/>.
        /// </summary>
        /// <param name="obj">The object to compare to the current <see cref="LabelStatement"/>.</param>
        /// <returns>True if the specified object is equal to the current <see cref="LabelStatement"/>; othersise, false.</returns>
        public override bool Equals(object obj) => Equals(obj as LabelStatement);

        /// <summary>
        /// Determines whether the specified <see cref="LabelStatement"/> is equal to the current instance.
        /// </summary>
        /// <param name="other">The <see cref="LabelStatement"/> to compare to this instance.</param>
        /// <returns>True if specified <see cref="LabelStatement"/> is equal to this instance; otherwise, false.</returns>
        public bool Equals(LabelStatement other)
        {
            if (other == null) return false;

            if (!base.Equals(other)) return false;

            return Identifier == other.Identifier;
        }

        /// <summary>
        /// Returns the hash code for this <see cref="LabelStatement"/>.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode() => base.GetHashCode() + Identifier.GetHashCode();

        /// <summary>
        /// Retuns a <see cref="string"/> that represents the current <see cref="LabelStatement"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="LabelStatement"/>.</returns>
        public override string ToString() => $"{base.ToString()} : {Identifier}";
    }
}