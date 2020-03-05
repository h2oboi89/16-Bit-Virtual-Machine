namespace VM.Software.Assembling.Parsing
{
    public class Argument
    {
        public ushort Value { get; }

        public int Size { get; }

        public bool IsLabel { get; }

        public Argument(ushort value, int size, bool isLabel = false)
        {
            Value = value;
            Size = size;
            IsLabel = isLabel;
        }
    }
}