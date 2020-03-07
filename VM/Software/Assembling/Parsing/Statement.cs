using System.Collections.Generic;
using System.Globalization;
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

        public override bool Equals(object obj)
        {
            return Equals(obj as Statement);
        }

        public bool Equals(Statement statement)
        {
            if (statement == null) return false;

            return ToBytes().Equals(statement.ToBytes());
        }

        public override int GetHashCode()
        {
            return 17 * Address.GetHashCode() * 31 * ToBytes().GetHashCode();
        }

        public override string ToString() => Utility.FormatU16(Address);
    }
}