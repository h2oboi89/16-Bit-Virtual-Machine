using System;

namespace VM
{
    public class Stack
    {
        private readonly Memory memory;
        private readonly ushort startAddress;
        private readonly ushort endAddress;

        public ushort StackPointer { get; private set; }

        public Stack(Memory memory, ushort startAddress, ushort sizeLimit)
        {
            this.memory = memory ?? throw new ArgumentNullException(nameof(memory));

            if (startAddress % Processor.DATASIZE != 0)
            {
                throw new ArgumentException($"Start Address must be byte aligned for {Processor.DATASIZE}.", nameof(startAddress));
            }

            if (startAddress + Processor.DATASIZE > memory.MaxAddress + 1)
            {
                throw new ArgumentOutOfRangeException(nameof(startAddress), "Start Address must be a valid memory address.");
            }

            if (startAddress < ((sizeLimit - 1) * Processor.DATASIZE))
            {
                throw new ArgumentOutOfRangeException(nameof(startAddress), "Start Address must be greater than size of stack.");
            }

            this.startAddress = startAddress;
            endAddress = (ushort)(startAddress - ((sizeLimit - 1) * Processor.DATASIZE));

            StackPointer = startAddress;
        }

        private void CheckForStackOverflow()
        {
            if (StackPointer < endAddress)
            {
                throw new StackOverflowException("Stack is full.");
            }
        }

        public void Push(ushort value)
        {
            CheckForStackOverflow();
            memory.SetU16(StackPointer, value);
            StackPointer -= Processor.DATASIZE;
        }

        private void CheckForEmptyStack()
        {
            if (StackPointer == startAddress)
            {
                throw new InvalidOperationException("Stack is empty.");
            }
        }

        // FUTURE: zero out memory on POP for security?
        public ushort Pop()
        {
            CheckForEmptyStack();
            StackPointer += Processor.DATASIZE;
            return memory.GetU16(StackPointer);
        }

        public ushort Peek()
        {
            CheckForEmptyStack();
            var address = (ushort)(StackPointer + Processor.DATASIZE);
            return memory.GetU16(address);
        }

        // TODO: stack: update all to work with frames, not just full stack
        // IE: treat each frame as a unique stack
    }
}
