#pragma warning disable CA1822 // Mark members as static
using NUnit.Framework;
using System;
using VM.Hardware;

namespace VM.HardwareTests.Tests
{
    [TestFixture]
    public class StackTests
    {
        private Memory memory;
        private Stack stack;

        [SetUp]
        public void SetUp()
        {
            memory = new Memory(0x20);
            stack = new Stack(memory, 0x1e, 0x0a);
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
            Assert.That(stack.StackPointer, Is.EqualTo(0x1e));

            stack.Push(0x1234);

            Assert.That(memory.GetU16(0x1e), Is.EqualTo(0x1234));
            Assert.That(stack.StackPointer, Is.EqualTo(0x1c));
        }

        [Test]
        public void Push_StackOverflow_ThrowsException()
        {
            for (ushort i = 0; i < 11; i++)
            {
                stack.Push(i);
            }

            Assert.That(() => stack.Push(0x1234), Throws.InstanceOf<StackOverflowException>()
                .With.Message.EqualTo("Stack is full."));
        }

        [Test]
        public void Pop_RemovesValueFromStack()
        {
            Assert.That(stack.StackPointer, Is.EqualTo(0x1e));

            stack.Push(0x1234);
            var value = stack.Pop();

            Assert.That(value, Is.EqualTo(0x1234));
            Assert.That(stack.StackPointer, Is.EqualTo(0x1e));
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
            Assert.That(stack.StackPointer, Is.EqualTo(0x1e));

            stack.Push(0x1234);
            var value = stack.Peek();

            Assert.That(value, Is.EqualTo(0x1234));
            Assert.That(stack.StackPointer, Is.EqualTo(0x1c));
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
            Assert.That(stack.StackPointer, Is.EqualTo(0x1e));

            stack.Push(0x1234);

            for (var i = 0; i < 10; i++)
            {
                var value = stack.Peek();

                Assert.That(value, Is.EqualTo(0x1234));
            }

            Assert.That(stack.StackPointer, Is.EqualTo(0x1c));
        }

        [Test]
        public void ValuesArePoppedInReversePushOrder()
        {
            Assert.That(stack.StackPointer, Is.EqualTo(0x1e));

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
            Assert.That(stack.StackPointer, Is.EqualTo(0x1e));
        }

        [Test]
        public void Call_StartsANewFrame()
        {
            stack.Push(0x1111);

            stack.Call(0xffff, 0x1234);

            Assert.That(stack.FramePointer, Is.EqualTo(0x16));
            Assert.That(stack.StackPointer, Is.EqualTo(0x16));
            Assert.That(stack.ArgumentsPointer, Is.EqualTo(0x1c));
            Assert.That(memory.GetU16(stack.ArgumentsPointer), Is.EqualTo(0xffff));

            Assert.That(() => stack.Pop(), Throws.InvalidOperationException
                .With.Message.EqualTo("Stack frame is empty."));
        }

        [Test]
        public void Call_MultipleFrames()
        {
            stack.Call(0x1111, 0x0123);

            Assert.That(stack.FramePointer, Is.EqualTo(0x18));
            Assert.That(stack.StackPointer, Is.EqualTo(0x18));
            Assert.That(stack.ArgumentsPointer, Is.EqualTo(0x1e));
            Assert.That(memory.GetU16(stack.ArgumentsPointer), Is.EqualTo(0x1111));

            stack.Call(0x2222, 0x4567);

            Assert.That(stack.FramePointer, Is.EqualTo(0x12));
            Assert.That(stack.StackPointer, Is.EqualTo(0x12));
            Assert.That(stack.ArgumentsPointer, Is.EqualTo(0x18));
            Assert.That(memory.GetU16(stack.ArgumentsPointer), Is.EqualTo(0x2222));

            stack.Call(0x3333, 0x89ab);

            Assert.That(stack.FramePointer, Is.EqualTo(0x0c));
            Assert.That(stack.StackPointer, Is.EqualTo(0x0c));
            Assert.That(stack.ArgumentsPointer, Is.EqualTo(0x12));
            Assert.That(memory.GetU16(stack.ArgumentsPointer), Is.EqualTo(0x3333));
        }

        [Test]
        public void Call_StackOverflow()
        {
            stack.Call(0x1111, 0x0123);
            stack.Call(0x2222, 0x4567);
            stack.Call(0x3333, 0x89ab);

            Assert.That(() => stack.Call(0x4444, 0xcdef), Throws.InstanceOf<StackOverflowException>()
                .With.Message.EqualTo("Stack is full."));
        }

        [Test]
        public void Return_GoesBackToPreviousFrame()
        {
            stack.Call(0x1111, 0x0123); // 1
            stack.Call(0x2222, 0x4567); // 2
            stack.Call(0x3333, 0x89ab); // 3

            // subroutine 3
            Assert.That(stack.FramePointer, Is.EqualTo(0x0c));
            Assert.That(stack.StackPointer, Is.EqualTo(0x0c));

            Assert.That(stack.ArgumentsPointer, Is.EqualTo(0x12));
            Assert.That(memory.GetU16(stack.ArgumentsPointer), Is.EqualTo(0x3333));

            stack.Push(0xfedc);
            Assert.That(stack.Return(1), Is.EqualTo(0x89ab));

            // subroutine 2
            Assert.That(stack.FramePointer, Is.EqualTo(0x12));
            Assert.That(stack.StackPointer, Is.EqualTo(0x10));

            Assert.That(stack.ReturnsPointer, Is.EqualTo(0x0a));
            Assert.That(memory.GetU16(stack.ReturnsPointer), Is.EqualTo(1));
            Assert.That(memory.GetU16((ushort)(stack.ReturnsPointer + Processor.DATASIZE)), Is.EqualTo(0xfedc));

            stack.Push(0xba98);
            Assert.That(stack.Return(1), Is.EqualTo(0x4567));

            // subroutine 1
            Assert.That(stack.FramePointer, Is.EqualTo(0x18));
            Assert.That(stack.StackPointer, Is.EqualTo(0x16));

            Assert.That(stack.ReturnsPointer, Is.EqualTo(0x0e));
            Assert.That(memory.GetU16(stack.ReturnsPointer), Is.EqualTo(1));
            Assert.That(memory.GetU16((ushort)(stack.ReturnsPointer + Processor.DATASIZE)), Is.EqualTo(0xba98));

            stack.Push(0x7654);
            Assert.That(stack.Return(1), Is.EqualTo(0x0123));

            // main
            Assert.That(stack.FramePointer, Is.EqualTo(0x1e));
            Assert.That(stack.StackPointer, Is.EqualTo(0x1c));

            Assert.That(stack.ReturnsPointer, Is.EqualTo(0x14));
            Assert.That(memory.GetU16(stack.ReturnsPointer), Is.EqualTo(1));
            Assert.That(memory.GetU16((ushort)(stack.ReturnsPointer + Processor.DATASIZE)), Is.EqualTo(0x7654));
        }

        [Test]
        public void Return_EmptyStack_ThrowsException()
        {
            Assert.That(() => stack.Return(0), Throws.InvalidOperationException
                .With.Message.EqualTo("In base frame, nothing to return to."));
        }

        [Test]
        public void CallAndReturn_MultipleValues()
        {
            Assert.That(stack.FramePointer, Is.EqualTo(0x1e));
            Assert.That(stack.StackPointer, Is.EqualTo(0x1e));
            Assert.That(stack.ArgumentsPointer, Is.EqualTo(0x1e));
            Assert.That(stack.ReturnsPointer, Is.EqualTo(0x1e));

            stack.Push(0x2222);
            stack.Push(0x1111);

            stack.Call(2, 0x1234);

            Assert.That(stack.FramePointer, Is.EqualTo(0x14));
            Assert.That(stack.StackPointer, Is.EqualTo(0x14));
            Assert.That(stack.ArgumentsPointer, Is.EqualTo(0x1a));
            Assert.That(stack.ReturnsPointer, Is.EqualTo(0x1e));

            Assert.That(memory.GetU16(stack.ArgumentsPointer), Is.EqualTo(2));
            Assert.That(memory.GetU16((ushort)(stack.ArgumentsPointer + Processor.DATASIZE)), Is.EqualTo(0x1111));
            Assert.That(memory.GetU16((ushort)(stack.ArgumentsPointer + 2 * Processor.DATASIZE)), Is.EqualTo(0x2222));

            stack.Push(0x4444);
            stack.Push(0x3333);

            stack.Return(2);

            Assert.That(stack.FramePointer, Is.EqualTo(0x1e));
            Assert.That(stack.StackPointer, Is.EqualTo(0x18));
            Assert.That(stack.ArgumentsPointer, Is.EqualTo(0x1a));
            Assert.That(stack.ReturnsPointer, Is.EqualTo(0x10));

            Assert.That(memory.GetU16(stack.ReturnsPointer), Is.EqualTo(2));
            Assert.That(memory.GetU16((ushort)(stack.ReturnsPointer + Processor.DATASIZE)), Is.EqualTo(0x3333));
            Assert.That(memory.GetU16((ushort)(stack.ReturnsPointer + 2 * Processor.DATASIZE)), Is.EqualTo(0x4444));
        }

        [Test]
        public void Reset_ResetsStack()
        {
            Assert.That(stack.FramePointer, Is.EqualTo(0x1e));
            Assert.That(stack.StackPointer, Is.EqualTo(0x1e));
            Assert.That(stack.ArgumentsPointer, Is.EqualTo(0x1e));
            Assert.That(stack.ReturnsPointer, Is.EqualTo(0x1e));

            stack.Call(0x1111, 0x0123); // 1
            stack.Call(0x2222, 0x4567); // 2

            stack.Push(0xba98);
            Assert.That(stack.Return(1), Is.EqualTo(0x4567));

            // subroutine 1
            Assert.That(stack.FramePointer, Is.EqualTo(0x18));
            Assert.That(stack.StackPointer, Is.EqualTo(0x16));

            Assert.That(stack.ReturnsPointer, Is.EqualTo(0x10));
            Assert.That(memory.GetU16(stack.ReturnsPointer), Is.EqualTo(1));
            Assert.That(memory.GetU16((ushort)(stack.ReturnsPointer + Processor.DATASIZE)), Is.EqualTo(0xba98));

            stack.Reset();

            Assert.That(stack.FramePointer, Is.EqualTo(0x1e));
            Assert.That(stack.StackPointer, Is.EqualTo(0x1e));
            Assert.That(stack.ArgumentsPointer, Is.EqualTo(0x1e));
            Assert.That(stack.ReturnsPointer, Is.EqualTo(0x1e));
        }
    }
}
#pragma warning restore CA1822 // Mark members as static