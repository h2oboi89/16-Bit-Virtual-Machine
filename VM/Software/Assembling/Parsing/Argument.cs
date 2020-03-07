using System;
using System.Collections.Generic;

namespace VM.Software.Assembling.Parsing
{
    public class Argument
    {
        public ushort Value { get; private set; }

        public int size;

        private readonly string identifier;

        public Argument(ushort value, int size, string identifier = null)
        {
            Value = value;
            this.size = size;
            this.identifier = identifier;
        }

        public void SetValue(IEnumerable<LabelStatement> labels)
        {
            if (identifier == null) return;

            foreach(var label in labels)
            {
                if (identifier == label.Identifier)
                {
                    Value = label.Address;
                    break;
                }
            }
        }

        public IEnumerable<byte> GetBytes()
        {
            var bytes = new List<byte>();
            
            for(var i = 0; i < size; i++)
            {
                var shiftAmount = (size - 1 - i) * 8;
                bytes.Add((byte)((Value >> shiftAmount) & 0xff));
            }

            return bytes;
        }

        public override string ToString()
        {
            var value = size == 1 ? Utility.FormatU8((byte)Value) : Utility.FormatU16(Value);

            return identifier == null ? value : $"{identifier} ({value})";
        }
    }
}