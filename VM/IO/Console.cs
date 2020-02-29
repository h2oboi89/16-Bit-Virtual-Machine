using System;

namespace VM.IO
{
    /// <summary>
    /// Represents a console that the <see cref="Processor"/> can write to via mapped <see cref="Memory"/>.
    /// </summary>
    public abstract class Console
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
        protected Console(Memory memory, ushort baseAddress, byte width, byte height)
        {
            this.memory = memory ?? throw new ArgumentNullException(nameof(memory));

            Width = width;
            Height = height;

            this.baseAddress = baseAddress;

            this.memory.MemoryWrite += OnMemoryWrite;
        }

        private bool AddressInRange(ushort address)
        {
            var endAddress = (ushort)(baseAddress + (Width * Height));

            return baseAddress <= address && address < endAddress;
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

            Write((char)e.Value, c, r);
        }

        /// <summary>
        /// Writes a character to the specified location on the <see cref="Console"/>.
        /// </summary>
        /// <param name="value">Character to write to screen.</param>
        /// <param name="column">Column to write value in (numbered left to right starting at 0).</param>
        /// <param name="row">Row to write value in (numbered top to bottom starting at 0).</param>
        protected abstract void Write(char value, int column, int row);

        /// <summary>
        /// Shuts down the <see cref="Console"/>.
        /// </summary>
        public virtual void Close()
        {
            memory.MemoryWrite -= OnMemoryWrite;
        }
    }
}
