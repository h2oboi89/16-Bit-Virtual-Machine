using NUnit.Framework;
using System;
using System.IO;
using VM.Hardware;
using VM.Hardware.IO;

namespace VM.Tests.HardwareTests.IOTests
{
    [TestFixture]
    public class SystemConsoleTests
    {
        private StringWriter consoleOut;

        private SystemConsole console;

        private Memory memory;

        [SetUp]
        public void SetUp()
        {
            consoleOut = new StringWriter();

            System.Console.SetOut(consoleOut);

            memory = new Memory(120);
            console = new SystemConsole(memory, 10, 10, 10);
        }

        [TearDown]
        public void TearDown()
        {
            console.Close();

            System.Console.SetOut(new StreamWriter(System.Console.OpenStandardOutput())
            {
                AutoFlush = true
            });
        }

        [Test]
        public void Constructor_NullMemory_ThrowsException()
        {
            Assert.That(() => new SystemConsole(null, 0, 10, 10), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("memory"));
        }

        [Test]
        public void Constructor_Console_TooSmall_ResizesConsole()
        {
            console.Close();

            var width = System.Console.WindowWidth;
            var height = System.Console.WindowHeight;

            System.Console.WindowWidth = 50;
            System.Console.WindowHeight = 50;

            console = new SystemConsole(memory, 0, 50, 50);

            Assert.That(System.Console.WindowWidth, Is.EqualTo(52));
            Assert.That(System.Console.WindowHeight, Is.EqualTo(52));

            System.Console.WindowWidth = width;
            System.Console.WindowHeight = height;
        }

        [Test]
        public void ConstructorInitializesConsole()
        {
            var output = consoleOut.ToString();

            Assert.That(output.Length, Is.EqualTo(console.Height * Environment.NewLine.Length));
        }

        [Test]
        public void WritingToValidMemory_UpdatesConsole()
        {
            var endOfInput = console.Height * Environment.NewLine.Length;

            memory.SetU8(10, (byte)'#');
            memory.SetU8(109, (byte)'#');

            var output = consoleOut.ToString();

            Assert.That(output[endOfInput], Is.EqualTo('#'));
            Assert.That(output[endOfInput + 1], Is.EqualTo('#'));
        }

        [Test]
        public void WritingOutsideConsoleMemory_DoesNothing()
        {
            var endOfInput = console.Height * Environment.NewLine.Length;

            memory.SetU8(0, (byte)'#');
            memory.SetU8(9, (byte)'#');
            memory.SetU8(110, (byte)'#');
            memory.SetU8(119, (byte)'#');

            var output = consoleOut.ToString();

            Assert.That(output.Length, Is.EqualTo(endOfInput));
        }
    }
}
