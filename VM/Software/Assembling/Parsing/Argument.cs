using System;
using System.Collections.Generic;

namespace VM.Software.Assembling.Parsing
{
    public class Argument
    {
        private ushort value;
        private readonly int size;
        private readonly string identifier;

        public Argument(ushort value, int size, string identifier = null)
        {
            this.value = value;
            this.size = size;
            this.identifier = identifier;
        }

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

        public override bool Equals(object obj) => Equals(obj as Argument);

        public bool Equals(Argument other)
        {
            if (other == null) return false;
            
            return value == other.value &&
                size == other.size &&
                identifier == other.identifier;
        }

        public override int GetHashCode() => 31 * value + 17 * size + 89 * identifier.GetHashCode();

        public override string ToString()
        {
            var value = size == 1 ? Utility.FormatU8((byte)this.value) : Utility.FormatU16(this.value);

            return identifier == null ? value : $"{identifier} ({value})";
        }
    }
}