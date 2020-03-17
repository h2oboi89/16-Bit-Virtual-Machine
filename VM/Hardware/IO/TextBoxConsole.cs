using System;
using System.Text;
using System.Windows.Controls;
using System.Windows.Threading;

namespace VM.Hardware.IO
{
    public class TextBoxConsole : Console
    {
        private readonly TextBox textBox;
        private readonly char[,] text;
        private readonly Dispatcher dispatcher;

        public TextBoxConsole(TextBox textBox, Dispatcher dispatcher, Memory memory, ushort baseAddress, byte width, byte height) :
            base(memory, baseAddress, width, height)
        {
            this.textBox = textBox;
            this.dispatcher = dispatcher;

            text = new char[Height, Width + 2];

            for (var i = 0; i < Height; i++)
            {
                for (var j = 0; j < Width; j++)
                {
                    text[i, j] = ' ';
                }

                text[i, Width] = Environment.NewLine[0];
                text[i, Width + 1] = Environment.NewLine[1];
            }
        }

        private void Publish()
        {
            var output = new StringBuilder();

            foreach (var c in text)
            {
                output.Append(c);
            }

            dispatcher.Invoke(() => textBox.Text = output.ToString());
        }

        protected override void Write(char value, int row, int column)
        {
            text[row, column] = value;

            Publish();
        }

        public void Clear()
        {
            for (var i = 0; i < Height; i++)
            {
                for (var j = 0; j < Width; j++)
                {
                    text[i, j] = ' ';
                }
            }

            Publish();
        }
    }
}
