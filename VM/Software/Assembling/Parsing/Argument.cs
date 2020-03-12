using System;
using System.Collections.Generic;

namespace VM.Software.Assembling.Parsing
{
    /// <summary>
    /// Argument for <see cref="InstructionStatement"/>.
    /// </summary>
    public class Argument
    {
        private ushort value;
        private readonly int size;
        private readonly string identifier;

        /// <summary>
        /// Creates a new <see cref="Argument"/> with the specified parameters.
        /// </summary>
        /// <param name="value">Value for the <see cref="Argument"/>. May not be final value. See <see cref="SetValue(IEnumerable{LabelStatement})"/>.</param>
        /// <param name="size">Size in bytes of this parameter once assembled.</param>
        /// <param name="identifier">Identifier for this value if it is a variable type. Default is null.</param>
        public Argument(ushort value, int size, string identifier = null)
        {
            this.value = value;
            this.size = size;
            this.identifier = identifier;
        }

        /// <summary>
        /// Looks through the collection of <see cref="LabelStatement"/> for a matching label name to set this <see cref="Argument"/>s value.
        /// </summary>
        /// <param name="labels">Collection of parsed <see cref="LabelStatement"/>s.</param>
        public void SetValue(IEnumerable<LabelStatement> labels)
        {
            if (identifier == null) return;

            if (labels == null)
            {
                throw new ArgumentNullException(nameof(labels));
            }

            var found = false;

            foreach(var label in labels)
            {
                if (identifier == label.Identifier)
                {
                    value = label.Address;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                throw new AssemblingException($"Undefined identifier '{identifier}'");
            }
        }

        /// <summary>
        /// Returns a collection of <see cref="byte"/>s that represent this <see cref="Argument"/> as it appears in the final compiled executable.
        /// NOTE: Value may not be valid until <see cref="SetValue(IEnumerable{LabelStatement})"/> is called.
        /// </summary>
        /// <returns>Collection of <see cref="byte"/>s that represent the current <see cref="InstructionStatement"/>.</returns>
        public IEnumerable<byte> GetBytes()
        {
            var bytes = new List<byte>();
            
            for(var i = 0; i < size; i++)
            {
                var shiftAmount = (size - 1 - i) * 8;
                bytes.Add((byte)((value >> shiftAmount) & 0xff));
            }

            return bytes;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current <see cref="Argument"/>.
        /// </summary>
        /// <param name="obj">The object to compare to the current <see cref="Argument"/>.</param>
        /// <returns>True if the specified object is equal to the current <see cref="Argument"/>; othersise, false.</returns>
        public override bool Equals(object obj) => Equals(obj as Argument);

        /// <summary>
        /// Determines whether the specified <see cref="Argument"/> is equal to the current instance.
        /// </summary>
        /// <param name="other">The <see cref="Argument"/> to compare to this instance.</param>
        /// <returns>True if specified <see cref="Argument"/> is equal to this instance; otherwise, false.</returns>
        public bool Equals(Argument other)
        {
            if (other == null) return false;
            
            return value == other.value &&
                size == other.size &&
                identifier == other.identifier;
        }

        /// <summary>
        /// Returns the hash code for this <see cref="Argument"/>.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode() => 31 * value + 17 * size + 89 * identifier.GetHashCode();

        /// <summary>
        /// Retuns a <see cref="string"/> that represents the current <see cref="Argument"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="Argument"/>.</returns>
        public override string ToString()
        {
            var value = size == 1 ? Utility.FormatU8((byte)this.value) : Utility.FormatU16(this.value);

            return identifier == null ? value : $"{identifier} ({value})";
        }
    }
}