#pragma warning disable CA1822 // Mark members as static
using NUnit.Framework;
using System;

namespace VM.Tests
{
    [TestFixture]
    public class MemoryTests
    {
        private Memory memory;

        [SetUp]
        public void SetUp()
        {
            memory = new Memory(0x100);
        }

        [Test]
        public void Constructor_InvalidSize_ThrowsException()
        {
            Assert.That(() => new Memory(0x10001),
                Throws.InstanceOf<ArgumentOutOfRangeException>().With.Message.StartsWith("Must be in range [0x0000, 0x10000]"));
        }

        [Test]
        public void MaxAddress_ReturnsSizeMinusOne()
        {
            Assert.That(memory.MaxAddress, Is.EqualTo(byte.MaxValue));
        }

        [Test]
        public void GetU8_InvalidAddress_ThrowsException()
        {
            Assert.That(() => memory.GetU8(0xff), Throws.Nothing);

            Assert.That(() => memory.GetU8(0x100),
                Throws.InstanceOf<IndexOutOfRangeException>().With.Message.StartsWith("Invalid memory address 0x0100."));
        }

        [Test]
        public void SetU8_InvalidAddress_ThrowsException()
        {
            Assert.That(() => memory.SetU8(0xff, 0x10), Throws.Nothing);

            Assert.That(() => memory.SetU8(0x100, 0x10),
                Throws.InstanceOf<IndexOutOfRangeException>().With.Message.StartsWith("Invalid memory address 0x0100."));
        }

        [Test]
        public void GetU8_SetU8_ReadAndWriteMemoryLocations()
        {
            for (byte i = 0; i < 10; i++)
            {
                memory.SetU8(i, i);

                Assert.That(memory.GetU8(i), Is.EqualTo(i));
            }
        }

        [Test]
        public void GetU16_InvalidAddress_ThrowsException()
        {
            Assert.That(() => memory.GetU16(0xfe), Throws.Nothing);

            Assert.That(() => memory.GetU16(0xff),
                Throws.InstanceOf<IndexOutOfRangeException>().With.Message.StartsWith("Invalid memory address 0x00FF."));

            Assert.That(() => memory.GetU16(0x100),
                Throws.InstanceOf<IndexOutOfRangeException>().With.Message.StartsWith("Invalid memory address 0x0100."));
        }

        [Test]
        public void SetU16_InvalidAddress_ThrowsException()
        {
            Assert.That(() => memory.SetU16(0xfe, 0x0010), Throws.Nothing);

            Assert.That(() => memory.SetU16(0xff, 0x0010),
                Throws.InstanceOf<IndexOutOfRangeException>().With.Message.StartsWith("Invalid memory address 0x00FF."));

            Assert.That(() => memory.SetU16(0x100, 0x0010),
                Throws.InstanceOf<IndexOutOfRangeException>().With.Message.StartsWith("Invalid memory address 0x0100."));
        }

        [Test]
        public void GetU16_SetU16_ReadAndWriteMemoryLocations()
        {
            for (var i = 0; i < 20; i++)
            {
                var address = (ushort)(i * sizeof(ushort));
                var value = (ushort)(0x1000 + i);
                memory.SetU16(address, value);

                Assert.That(memory.GetU16(address), Is.EqualTo(value));
            }
        }

        [Test]
        public void Clear_ErasesAllValues()
        {
            for (ushort i = 0; i < memory.MaxAddress; i++)
            {
                memory.SetU8(i, 0xff);
            }

            for (ushort i = 0; i < memory.MaxAddress; i++)
            {
                Assert.That(memory.GetU8(i), Is.Not.Zero);
            }

            memory.Clear();

            for (ushort i = 0; i < memory.MaxAddress; i++)
            {
                Assert.That(memory.GetU8(i), Is.Zero);
            }
        }
    }
}
#pragma warning restore CA1822 // Mark members as static