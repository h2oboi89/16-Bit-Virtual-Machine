using System;

namespace VM
{
    /// <summary>
    /// Represents a console that the <see cref="Processor"/> can write to via mapped <see cref="Memory"/>.
    /// </summary>
    public sealed class Console
    {
        /// <summary>
        /// Width of the <see cref="Console"/> (one character (byte) per column).
        /// </summary>
        public byte Width { get; private set; }

        /// <summary>
        /// Height of the <see cref="Console"/> (one character (byte) per row).
        /// </summary>
        public byte Height { get; private set; }

        private readonly ushort baseAddress;
        private readonly Memory memory;

        /// <summary>
        /// Creates a new <see cref="Console"/>.
        /// </summary>
        /// <param name="memory">Memory that <see cref="Console"/>is mapped to.</param>
        /// <param name="baseAddress">Start of region in <paramref name="memory"/> that <see cref="Console"/> is mapped to.</param>
        /// <param name="width">Width of <see cref="Console"/>.</param>
        /// <param name="height">Height of <see cref="Console"/>.</param>
        public Console(Memory memory, ushort baseAddress, byte width, byte height)
        {
            this.memory = memory ?? throw new ArgumentNullException(nameof(memory));

            Width = width;
            Height = height;

            this.baseAddress = baseAddress;

            this.memory.MemoryWrite += OnMemoryWrite;

            InitializeConsole();
        }

        private void OnMemoryWrite(object sender, MemoryWriteEventArgs e)
        {
            if (!AddressInRange(e.Address))
            {
                return;
            }

            var address = e.Address - baseAddress;

            var r = address / Width;
            var c = address % Width;

            System.Console.SetCursorPosition(c, r);
            System.Console.Write((char)e.Value);
        }

        private void InitializeConsole()
        {
            if (System.Console.WindowWidth < Width + 2)
            {
                System.Console.WindowWidth = Width + 2;
            }
            if (System.Console.WindowHeight < Height + 2)
            {
                System.Console.WindowHeight = Height + 2;
            }

            System.Console.Clear();
            System.Console.CursorVisible = false;

            for (var i = 0; i < Height; i++)
            {
                System.Console.SetCursorPosition(Width, i);
                System.Console.Write(Environment.NewLine);
            }
        }

        /// <summary>
        /// Resets the <see cref="System.Console"/> by moving cursor past region used by <see cref="Console"/>.
        /// </summary>
        public void Stop()
        {
            memory.MemoryWrite -= OnMemoryWrite;
            System.Console.SetCursorPosition(0, Height);
            System.Console.WriteLine();
            System.Console.CursorVisible = true;
        }

        private bool AddressInRange(ushort address)
        {
            var endAddress = (ushort)(baseAddress + (Width * Height));

            return baseAddress <= address && address <= endAddress;
        }
    }
}
