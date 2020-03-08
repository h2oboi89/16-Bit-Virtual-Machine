using System.Collections.Generic;
using System.Linq;

namespace VM.Software.Assembling.Parsing
{
    public abstract class Statement
    {
        public ushort Address { get; internal set; }

        protected abstract IEnumerable<byte> GetBytes();

        public IEnumerable<byte> ToBytes() => GetBytes();

        public ushort Size => (ushort)ToBytes().Count();

        public virtual void SetIdentifiers(IEnumerable<LabelStatement> labels) { }

        public override bool Equals(object obj) => Equals(obj as Statement);

        public bool Equals(Statement statement) => statement == null
            ? false
            : ToBytes().Equals(statement.ToBytes());

        public override int GetHashCode() => 61 * Address.GetHashCode() * 31 * ToBytes().GetHashCode();

        public override string ToString() => Utility.FormatU16(Address);
    }
}