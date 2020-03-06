using System.Collections.Generic;

namespace VM.Software.Assembling.Parsing
{
    public abstract class Statement
    {
        public ushort Address { get; private set; }

        public virtual byte Size => 0;

        protected abstract IEnumerable<byte> GetBytes();

        public IEnumerable<byte> ToBytes() => GetBytes();

        public ushort SetAddress(ushort address)
        {
            Address = address;

            return (ushort)(Address + Size);
        }

        public virtual void SetIdentifiers(IEnumerable<LabelStatement> labels) { }

        public override bool Equals(object obj)
        {
            return Equals(obj as Statement);
        }

        public bool Equals(Statement statement)
        {
            if (statement == null) return false;

            return ToBytes().Equals(statement.ToBytes());
        }
    }
}