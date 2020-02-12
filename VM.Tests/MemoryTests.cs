using NUnit.Framework;
using System;
using System.Linq;

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
            Assert.That(() => new Memory(-1),
                Throws.InstanceOf<ArgumentOutOfRangeException>().With.Message.StartsWith("Must be in range [0x0000, 0x10000]"));

            Assert.That(() => new Memory(ushort.MaxValue + 2),
                Throws.InstanceOf<ArgumentOutOfRangeException>().With.Message.StartsWith("Must be in range [0x0000, 0x10000]"));
        }

        [Test]
        public void MaxAddress_ReturnsSizeMinusOne()
        {
            Assert.That(memory.MaxAddress, Is.EqualTo(byte.MaxValue));
        }

        [Test]
        public void InvalidMemoryAccess_ThrowsException()
        {
            Assert.That(() => memory.GetU8(0xffff),
                Throws.InstanceOf<IndexOutOfRangeException>().With.Message.EqualTo("Invalid memory address 0xFFFF. Valid Range: [0x0000, 0x00FF]"));
        }

        [Test]
        public void GetU8_InvalidAddress_ThrowsException()
        {
            Assert.That(() => memory.GetU8((ushort)(memory.MaxAddress + 1)),
                Throws.InstanceOf<IndexOutOfRangeException>().With.Message.StartsWith("Invalid memory address 0x0100."));
        }

        [Test]
        public void SetU8_InvalidAddress_ThrowsException()
        {
            Assert.That(() => memory.SetU8((ushort)(memory.MaxAddress + 1), 0x10),
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
            Assert.That(() => memory.GetU16(memory.MaxAddress),
                Throws.InstanceOf<IndexOutOfRangeException>().With.Message.StartsWith("Invalid memory address 0x00FF."));

            Assert.That(() => memory.GetU16((ushort)(memory.MaxAddress + 1)),
                Throws.InstanceOf<IndexOutOfRangeException>().With.Message.StartsWith("Invalid memory address 0x0100."));
        }

        [Test]
        public void SetU16_InvalidAddress_ThrowsException()
        {
            Assert.That(() => memory.SetU16(memory.MaxAddress, 0x10),
                Throws.InstanceOf<IndexOutOfRangeException>().With.Message.StartsWith("Invalid memory address 0x00FF."));

            Assert.That(() => memory.SetU16((ushort)(memory.MaxAddress + 1), 0x10),
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
    }
}
