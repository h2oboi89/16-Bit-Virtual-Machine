using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using VM.Hardware;
using VM.Hardware.IO;
using VM.Software.Assembling;

namespace VM.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const ushort CONSOLE_ADDRESS = 0xf000;
        const byte WIDTH = 80;
        const byte HEIGHT = 25;

        private readonly Processor processor;
        private readonly Memory memory;
        private readonly TextBoxConsole console;
        private readonly Flasher flasher;

        private IEnumerable<byte> executable;

        public MainWindow()
        {
            InitializeComponent();

            memory = new Memory(0x10000);
            console = new TextBoxConsole(Console, Dispatcher, memory, CONSOLE_ADDRESS, WIDTH, HEIGHT);
            processor = new Processor(memory, 0x800);
            flasher = new Flasher(memory);
        }

        private void NewProgram_Button_Click(object sender, RoutedEventArgs e)
        {
            // FUTURE: detect if unsaved content?
            Code.Clear();
        }

        private void LoadProgram_Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();

            if (dialog.ShowDialog() == true)
            {
                Code.Text = File.ReadAllText(dialog.FileName);
            }
        }

        private void SaveProgram_Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog();

            if (dialog.ShowDialog() == true)
            {
                File.WriteAllText(dialog.FileName, Code.Text);
            }

            // FUTURE: only show file dialog if not associated with a file

            // FUTURE: add save as
        }

        private void Assemble_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                executable = null;

                executable = Assembler.Assemble(Code.Text);

                flasher.Address = 0;
                flasher.Flash(executable);

            }
            catch (AssemblingException error)
            {
                MessageBox.Show(error.Message, "Error during assembly", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Reset()
        {
            processor.ResetProcessor();
            console.Clear();
        }

        private void ProgramRun_Button_Click(object sender, RoutedEventArgs e)
        {
            Reset();

            if (executable != null)
            {
                Task.Run(() => processor.Run());
            }
        }

        private void ProgramStep_Button_Click(object sender, RoutedEventArgs e)
        {
            if (executable != null)
            {
                processor.Step();
            }
        }

        private void Reset_Button_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }
    }
}
