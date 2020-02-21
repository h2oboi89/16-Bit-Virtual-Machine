#pragma warning disable CA1822 // Mark members as static
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
            memory = new Memory(0x100);
            stack = new Stack(memory, 0xfe, 0xf0);
        }

        private void FillStack()
        {
            for (var i = 0; i < 8; i++)
            {
                stack.Push((ushort)i);
                stack.PushFrame();
            }
        }

        [Test]
        public void Constructor_NullMemoryArgument_ThrowsException()
        {
            Assert.That(() => new Stack(null, 0, 0), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("memory"));
        }

        [Test]
        public void Constructor_MisalignedAddresses_ThrowsException()
        {
            Assert.That(() => new Stack(memory, 1, 0), Throws.ArgumentException
                .With.Property("ParamName").EqualTo("startAddress").And
                .With.Message.StartsWith("Start Address must be byte aligned for 2."));

            Assert.That(() => new Stack(memory, 0x0e, 1), Throws.ArgumentException
                .With.Property("ParamName").EqualTo("endAddress").And
                .With.Message.StartsWith("End Address must be byte aligned for 2."));
        }

        [Test]
        public void Constructor_StartAddressGreaterThanMemoryMax_ThrowsException()
        {
            Assert.That(() => new Stack(memory, 0x100, 0), Throws.InstanceOf<ArgumentOutOfRangeException>()
                .With.Property("ParamName").EqualTo("startAddress").And
                .With.Message.StartsWith("Start Address must be a valid memory address."));
        }

        [Test]
        public void Constructor_EndAddressGreaterThanMemoryMax_ThrowsException()
        {
            Assert.That(() => new Stack(memory, 0x0e, 0x100), Throws.InstanceOf<ArgumentOutOfRangeException>()
                .With.Property("ParamName").EqualTo("endAddress").And
                .With.Message.StartsWith("End Address must be a valid memory address."));
        }

        [Test]
        public void Constructor_StartAddressLessThanEndAddress_ThrowsException()
        {
            Assert.That(() => new Stack(memory, 0x0a, 0x0e), Throws.InstanceOf<ArgumentOutOfRangeException>()
                .With.Property("ParamName").EqualTo("startAddress").And
                .With.Message.StartsWith("Start Address must be less than End Address."));
        }

        [Test]
        public void Push_AddsValueToStack()
        {
            Assert.That(stack.StackPointer, Is.EqualTo(0xfe));

            stack.Push(0x1234);

            Assert.That(memory.GetU16(0xfe), Is.EqualTo(0x1234));
            Assert.That(stack.StackPointer, Is.EqualTo(0xfc));
        }

        [Test]
        public void Push_StackOverflow_ThrowsException()
        {
            for (ushort i = 0; i < 8; i++)
            {
                stack.Push(i);
            }

            Assert.That(() => stack.Push(0x1234), Throws.InstanceOf<StackOverflowException>()
                .With.Message.EqualTo("Stack is full."));
        }

        [Test]
        public void Pop_RemovesValueFromStack()
        {
            Assert.That(stack.StackPointer, Is.EqualTo(0xfe));

            stack.Push(0x1234);
            var value = stack.Pop();

            Assert.That(value, Is.EqualTo(0x1234));
            Assert.That(stack.StackPointer, Is.EqualTo(0xfe));
        }

        [Test]
        public void Pop_EmptyStack_ThrowsException()
        {
            Assert.That(() => stack.Pop(), Throws.InvalidOperationException
                .With.Message.EqualTo("Stack frame is empty."));
        }

        [Test]
        public void Peek_ReturnsValueWithoutRemovingFromStack()
        {
            Assert.That(stack.StackPointer, Is.EqualTo(0xfe));

            stack.Push(0x1234);
            var value = stack.Peek();

            Assert.That(value, Is.EqualTo(0x1234));
            Assert.That(stack.StackPointer, Is.EqualTo(0xfc));
        }

        [Test]
        public void Peek_EmptyStack_ThrowsException()
        {
            Assert.That(() => stack.Peek(), Throws.InvalidOperationException
                .With.Message.EqualTo("Stack frame is empty."));
        }

        [Test]
        public void MultiplePeeks_ReturnSameValue()
        {
            Assert.That(stack.StackPointer, Is.EqualTo(0xfe));

            stack.Push(0x1234);

            for (var i = 0; i < 10; i++)
            {
                var value = stack.Peek();

                Assert.That(value, Is.EqualTo(0x1234));
            }

            Assert.That(stack.StackPointer, Is.EqualTo(0xfc));
        }

        [Test]
        public void ValuesArePoppedInReversePushOrder()
        {
            Assert.That(stack.StackPointer, Is.EqualTo(0xfe));

            var pushValues = new ushort[] { 0, 1, 2, 3 };

            foreach (var value in pushValues)
            {
                stack.Push(value);
            }

            var values = new ushort[pushValues.Length];

            for (var i = 0; i < pushValues.Length; i++)
            {
                values[i] = stack.Pop();
            }

            Assert.That(values, Is.EqualTo(new ushort[] { 3, 2, 1, 0 }));
            Assert.That(stack.StackPointer, Is.EqualTo(0xfe));
        }

        [Test]
        public void PushFrame_StartsANewFrame()
        {
            stack.Push(1);

            stack.PushFrame();

            Assert.That(() => stack.Pop(), Throws.InvalidOperationException
                .With.Message.EqualTo("Stack frame is empty."));
        }

        [Test]
        public void PushFrame_MultipleFrames()
        {
            FillStack();

            for (var i = 0; i < 8; i++)
            {
                stack.PopFrame();
            }

            Assert.That(stack.Pop(), Is.EqualTo(0));

            Assert.That(() => stack.Pop(), Throws.InvalidOperationException
                .With.Message.EqualTo("Stack frame is empty."));
        }

        [Test]
        public void PushFrame_StackOverflow()
        {
            FillStack();

            Assert.That(() => stack.Push(1), Throws.InstanceOf<StackOverflowException>()
                .With.Message.EqualTo("Stack is full."));
        }

        [Test]
        public void PopFrame_GoesBackToPreviousFrame()
        {
            stack.Push(1);

            stack.PushFrame();

            stack.PopFrame();

            Assert.That(stack.Pop(), Is.EqualTo(1));
        }

        [Test]
        public void PopFrame_EmptyStack_ThrowsException()
        {
            Assert.That(() => stack.PopFrame(), Throws.InvalidOperationException
                .With.Message.EqualTo("Stack is empty."));
        }

        [Test]
        public void Reset_ResetsStack()
        {
            Assert.That(stack.StackPointer, Is.EqualTo(0xfe));

            stack.PushFrame();

            var pushValues = new ushort[] { 0, 1, 2, 3 };

            foreach (var value in pushValues)
            {
                stack.Push(value);
            }

            Assert.That(stack.StackPointer, Is.EqualTo(0xf6));

            stack.Reset();

            Assert.That(stack.StackPointer, Is.EqualTo(0xfe));
        }
    }
}
#pragma warning restore CA1822 // Mark members as static