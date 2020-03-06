using System;
using System.Collections.Generic;

namespace VM.Software.Assembling.Parsing
{
    public class Argument
    {
        public ushort Value { get; private set; }

        public int Size { get; }

        private readonly string identifier;

        public Argument(ushort value, int size, string identifier = null)
        {
            Value = value;
            Size = size;
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
            
            for(var i = 0; i < Size; i++)
            {
                var shiftAmount = (Size - 1 - i) * 8;
                bytes.Add((byte)((Value >> shiftAmount) & 0xff));
            }

            return bytes;
        }
    }
}