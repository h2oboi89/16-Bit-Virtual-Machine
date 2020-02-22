using System.Text;

namespace VM
{
    public sealed class Console
    {
        public byte Width { get; private set; }
        public byte Height { get; private set; }

        private readonly byte[][] screen;

        private ushort baseAddress;

        public Console(ushort baseAddress, byte width, byte height)
        {
            this.Width = width;
            this.Height = height;

            screen = new byte[height][];

            for(var i = 0; i < screen.Length; i++)
            {
                screen[i] = new byte[width];
            }

            this.baseAddress = baseAddress;

            System.Console.SetWindowSize(Width + 1, Height + 1);
            System.Console.SetBufferSize(Width + 1, Height + 1);
            System.Console.CursorVisible = false;
        }

        private bool AddressInRange(ushort address)
        {
            var endAddress = (ushort)(baseAddress + (Width * Height));

            return address >= baseAddress && address <= endAddress;
        }

        public void Write(ushort address, byte value)
        {
            if (!AddressInRange(address))
            {
                return;
            }
            
            address -= baseAddress;
            
            var r = address / Width;
            var c = address % Width;

            screen[r][c] = value;

            var output = new StringBuilder();

            foreach(var row in screen)
            {
                output.AppendLine(Encoding.ASCII.GetString(row));
            }

            System.Console.Clear();
            System.Console.Write(output.ToString().Trim());
        }

        // TODO: flush to console every X milliseconds (250?)
    }
}
