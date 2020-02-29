using System;

namespace VM.Hardware.IO
{
    /// <summary>
    /// Implementation of <see cref="Console"/> that wraps the <see cref="System.Console"/>.
    /// </summary>
    public sealed class SystemConsole : Console
    {
        /// <summary>
        /// Creates a new <see cref="SystemConsole"/>.
        /// </summary>
        /// <param name="memory">Memory that <see cref="SystemConsole"/>is mapped to.</param>
        /// <param name="baseAddress">Start of region in <paramref name="memory"/> that <see cref="SystemConsole"/> is mapped to.</param>
        /// <param name="width">Width of <see cref="SystemConsole"/>.</param>
        /// <param name="height">Height of <see cref="SystemConsole"/>.</param>
        public SystemConsole(Memory memory, ushort baseAddress, byte width, byte height) :
            base(memory, baseAddress, width, height)
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
        /// Writes a character to the specified location on the <see cref="SystemConsole"/>.
        /// </summary>
        /// <param name="value">Character to write to screen.</param>
        /// <param name="column">Column to write value in (numbered left to right starting at 0).</param>
        /// <param name="row">Row to write value in (numbered top to bottom starting at 0).</param>
        protected override void Write(char value, int column, int row)
        {
            System.Console.SetCursorPosition(column, row);
            System.Console.Write(value);
        }

        /// <summary>
        /// Moves cursor past section used by program and resets cursor visibility.
        /// </summary>
        public override void Close()
        {
            base.Close();
            System.Console.SetCursorPosition(0, Height);
            System.Console.WriteLine();
            System.Console.CursorVisible = true;
        }
    }
}
