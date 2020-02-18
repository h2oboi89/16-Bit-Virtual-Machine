using NUnit.Framework;
using System;

namespace VM.Tests
{
    [TestFixture]
    public class StackTests
    {
        private Memory memory;
        private Stack stack;

        [SetUp]
        public void SetUp()
        {
            memory = new Memory(0x10);
            stack = new Stack(memory, 0x0e, 4);
        }
        
        [Test]
        public void Constructor_NullMemoryArgument_ThrowsException()
        {
            Assert.That(() => new Stack(null, 0, 0), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("memory"));
        }

        [Test]
        public void Constructor_MisalignedStartAddress_ThrowsException()
        {
            Assert.That(() => new Stack(memory, 1, 0), Throws.ArgumentException
                .With.Property("ParamName").EqualTo("startAddress").And
                .With.Message.StartsWith("Start Address must be byte aligned for 2."));
        }

        [Test]
        public void Constructor_StartAddressGreaterThanMemoryMax_ThrowsException()
        {
            Assert.That(() => new Stack(memory, 0x10, 0), Throws.InstanceOf<ArgumentOutOfRangeException>()
                .With.Property("ParamName").EqualTo("startAddress").And
                .With.Message.StartsWith("Start Address must be a valid memory address."));
        }

        [Test]
        public void Constructor_SizeLimitExceedsStartAddress_ThrowsException()
        {
            Assert.That(() => new Stack(memory, 0x0e, 9), Throws.InstanceOf<ArgumentOutOfRangeException>()
                .With.Property("ParamName").EqualTo("startAddress").And
                .With.Message.StartsWith("Start Address must be greater than size of stack."));
        }

        [Test]
        public void Push_AddsValueToStack()
        {
            Assert.That(stack.StackPointer, Is.EqualTo(0x0e));

            stack.Push(0x1234);

            Assert.That(memory.GetU16(0x0e), Is.EqualTo(0x1234));
            Assert.That(stack.StackPointer, Is.EqualTo(0x0c));
        }

        [Test]
        public void Push_StackOverflow_ThrowsException()
        {
            for(ushort i = 0; i < 4; i++)
            {
                stack.Push(i);
            }

            Assert.That(() => stack.Push(0x1234), Throws.InstanceOf<StackOverflowException>()
                .With.Message.EqualTo("Stack is full."));
        }

        [Test]
        public void Pop_RemovesValueFromStack()
        {
            Assert.That(stack.StackPointer, Is.EqualTo(0x0e));

            stack.Push(0x1234);
            var value = stack.Pop();

            Assert.That(value, Is.EqualTo(0x1234));
            Assert.That(stack.StackPointer, Is.EqualTo(0x0e));
        }

        [Test]
        public void Pop_EmptyStack_ThrowsException()
        {
            Assert.That(() => stack.Pop(), Throws.InvalidOperationException
                .With.Message.EqualTo("Stack is empty."));
        }

        [Test]
        public void Peek_ReturnsValueWithoutRemovingFromStack()
        {
            Assert.That(stack.StackPointer, Is.EqualTo(0x0e));

            stack.Push(0x1234);
            var value = stack.Peek();

            Assert.That(value, Is.EqualTo(0x1234));
            Assert.That(stack.StackPointer, Is.EqualTo(0x0c));
        }

        [Test]
        public void Peek_EmptyStack_ThrowsException()
        {
            Assert.That(() => stack.Peek(), Throws.InvalidOperationException
                .With.Message.EqualTo("Stack is empty."));
        }

        [Test]
        public void MultiplePeeks_ReturnSameValue()
        {
            Assert.That(stack.StackPointer, Is.EqualTo(0x0e));

            stack.Push(0x1234);

            for (var i = 0; i < 10; i++)
            {
                var value = stack.Peek();

                Assert.That(value, Is.EqualTo(0x1234));
            }

            Assert.That(stack.StackPointer, Is.EqualTo(0x0c));
        }

        [Test]
        public void ValuesArePoppedInReversePushOrder()
        {
            Assert.That(stack.StackPointer, Is.EqualTo(0x0e));

            var pushValues = new ushort[] { 0, 1, 2, 3 };

            foreach(var value in pushValues)
            {
                stack.Push(value);
            }

            var values = new ushort[pushValues.Length];

            for(var i = 0; i < pushValues.Length; i++)
            {
                values[i] = stack.Pop();
            }

            Assert.That(values, Is.EqualTo(new ushort[] { 3, 2, 1, 0 }));
            Assert.That(stack.StackPointer, Is.EqualTo(0x0e));
        }
    }
}
