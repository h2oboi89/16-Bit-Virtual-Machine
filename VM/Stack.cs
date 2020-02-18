using System;

namespace VM
{
    /// <summary>
    /// Represents a frame in the <see cref="Processor"/> stack.
    /// </summary>
    public class Stack
    {
        private readonly Memory memory;
        private readonly ushort startAddress;
        private readonly ushort endAddress;

        /// <summary>
        /// Address of hte next available location on the <see cref="Stack"/>
        /// </summary>
        public ushort StackPointer { get; private set; }

        /// <summary>
        /// Creates a new <see cref="Stack"/> at the specified region in <see cref="Memory"/>.
        /// NOTE: <see cref="Stack"/> grows down (<see cref="StackPointer"/> value gets smaller each time a value is added to the <see cref="Stack"/>).
        /// </summary>
        /// <param name="memory"><see cref="Memory"/> that will contain the <see cref="Stack"/>.</param>
        /// <param name="startAddress">Address of first value in <see cref="Stack"/>.</param>
        /// <param name="sizeLimit">How many items the <see cref="Stack"/> can hold.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="startAddress"/> is not byte aligned with <see cref="Processor.DATASIZE"/></exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="startAddress"/> and <paramref name="sizeLimit"/> do not fit into <see cref="Memory"/> based on their values.</exception>
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
                throw new ArgumentOutOfRangeException(nameof(startAddress), "Start Address must allow room for size of stack.");
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

        /// <summary>
        /// Inserts a value at the top of the <see cref="Stack"/>.
        /// Decrements <see cref="StackPointer"/>.
        /// </summary>
        /// <param name="value">Value to put on <see cref="Stack"/>.</param>
        /// <exception cref="StackOverflowException">Thrown when <see cref="Stack"/> is full.</exception>
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

        /// <summary>
        /// Removes and returns value at the top of the <see cref="Stack"/>.
        /// Increments <see cref="StackPointer"/>.
        /// </summary>
        /// <returns>Value from top of <see cref="Stack"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="Stack"/> is empty.</exception>
        public ushort Pop()
        {
            // FUTURE: zero out memory on POP for security?
            CheckForEmptyStack();
            StackPointer += Processor.DATASIZE;
            return memory.GetU16(StackPointer);
        }

        /// <summary>
        /// Returns the value at the top of the <see cref="Stack"/> without removing it.
        /// Does not increment <see cref="StackPointer"/>.
        /// </summary>
        /// <returns>Value from top of <see cref="Stack"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="Stack"/> is empty.</exception>
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
