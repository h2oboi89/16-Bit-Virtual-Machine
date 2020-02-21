using System;
using System.Collections.Generic;

namespace VM
{
    /// <summary>
    /// Represents the call stack in a <see cref="Processor"/>.
    /// </summary>
    public class Stack
    {
        private readonly Memory memory;

        private readonly Stack<Frame> frames = new Stack<Frame>();

        private Frame StackFrame => frames.Peek();

        /// <summary>
        /// Address of first value in <see cref="Stack"/>.
        /// </summary>
        public ushort StartAddress { get; private set; }

        /// <summary>
        /// Address of last value in <see cref="Stack"/>.
        /// </summary>
        public ushort EndAddress { get; private set; }

        /// <summary>
        /// Address of the next available location on the <see cref="Stack"/>.
        /// </summary>
        public ushort StackPointer => StackFrame.Pointer;

        /// <summary>
        /// Value of <see cref="StackPointer"/> before last subroutine call.
        /// </summary>
        public ushort FramePointer { get; private set; }

        /// <summary>
        /// Creates a new <see cref="Stack"/> at the specified region in <see cref="Memory"/>.
        /// NOTE: <see cref="Stack"/> grows down (<see cref="StackPointer"/> value gets smaller each time a value is added to the <see cref="Stack"/>).
        /// </summary>
        /// <param name="memory"><see cref="Memory"/> that will contain the <see cref="Stack"/>.</param>
        /// <param name="startAddress">Address of first value in <see cref="Stack"/>.</param>
        /// <param name="endAddress">Address of the last value in <see cref="Stack"/>.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="startAddress"/> or <paramref name="endAddress"/> is not byte aligned with <see cref="Processor.DATASIZE"/></exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="startAddress"/> or <paramref name="endAddress"/> do not fit into <see cref="Memory"/> based on their values.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="startAddress"/> is less than <paramref name="endAddress"/>.</exception>
        public Stack(Memory memory, ushort startAddress, ushort endAddress)
        {
            this.memory = memory ?? throw new ArgumentNullException(nameof(memory));

            CheckAddressValidity(startAddress, nameof(startAddress), "Start");
            CheckAddressValidity(endAddress, nameof(endAddress), "End");

            if (startAddress <= endAddress)
            {
                throw new ArgumentOutOfRangeException(nameof(startAddress), "Start Address must be less than End Address.");
            }

            StartAddress = startAddress;
            EndAddress = endAddress;

            frames.Push(new Frame(memory, startAddress, endAddress));
        }

        private void CheckAddressValidity(ushort address, string name, string displayName)
        {
            if (address % Processor.DATASIZE != 0)
            {
                throw new ArgumentException($"{displayName} Address must be byte aligned for {Processor.DATASIZE}.", name);
            }

            if (address + Processor.DATASIZE > memory.MaxAddress + 1)
            {
                throw new ArgumentOutOfRangeException(name, $"{displayName} Address must be a valid memory address.");
            }
        }

        /// <summary>
        /// Resets the <see cref="Stack"/> back to intial values.
        /// </summary>
        public void Reset()
        {
            while (frames.Count > 1)
            {
                frames.Pop();
            }

            StackFrame.Reset();

            FramePointer = StackPointer;
        }

        /// <summary>
        /// Inserts a value at the top of the <see cref="Stack"/>.
        /// Decrements <see cref="StackPointer"/>.
        /// </summary>
        /// <param name="value">Value to put on <see cref="Stack"/>.</param>
        /// <exception cref="StackOverflowException">Thrown when <see cref="Stack"/> is full.</exception>
        public void Push(ushort value)
        {
            StackFrame.Push(value);
        }

        /// <summary>
        /// Removes and returns value at the top of the <see cref="Stack"/>.
        /// Increments <see cref="StackPointer"/>.
        /// </summary>
        /// <returns>Value from top of <see cref="Stack"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="Stack"/> is empty.</exception>
        public ushort Pop()
        {
            return StackFrame.Pop();
        }

        /// <summary>
        /// Returns the value at the top of the <see cref="Stack"/> without removing it.
        /// Does not increment <see cref="StackPointer"/>.
        /// </summary>
        /// <returns>Value from top of <see cref="Stack"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="Stack"/> is empty.</exception>
        public ushort Peek()
        {
            return StackFrame.Peek();
        }

        /// <summary>
        /// Closes current stack frame and starts a new one.
        /// </summary>
        public void PushFrame()
        {
            var startAddress = StackFrame.Pointer;

            frames.Push(new Frame(memory, startAddress, EndAddress));

            FramePointer = StackPointer;
        }

        /// <summary>
        /// Closes current stack frame and opens previous one.
        /// </summary>
        public void PopFrame()
        {
            if (frames.Count > 1)
            {
                frames.Pop();
            }
            else
            {
                throw new InvalidOperationException("Stack is empty.");
            }

            FramePointer = StackFrame.StartAddress;
        }

        /// <summary>
        /// Represents a frame in the <see cref="Processor"/> stack.
        /// </summary>
        internal class Frame
        {
            private readonly Memory memory;

            /// <summary>
            /// Address of first value in <see cref="Frame"/>.
            /// </summary>
            public ushort StartAddress { get; private set; }

            /// <summary>
            /// Address of last value in <see cref="Frame"/>.
            /// </summary>
            public ushort EndAddress { get; private set; }

            /// <summary>
            /// Address of the next available location on the <see cref="Frame"/>.
            /// </summary>
            public ushort Pointer { get; private set; }

            /// <summary>
            /// Creates a new <see cref="Frame"/> at the specified region in <see cref="Memory"/>.
            /// NOTE: <see cref="Frame"/> grows down (<see cref="Pointer"/> value gets smaller each time a value is added to the <see cref="Frame"/>).
            /// </summary>
            /// <param name="memory"><see cref="Memory"/> that will contain the <see cref="Frame"/>.</param>
            /// <param name="startAddress">Address of first value in <see cref="Frame"/>.</param>
            /// <param name="endAddress">Address of the last value in <see cref="Frame"/>.</param>
            internal Frame(Memory memory, ushort startAddress, ushort endAddress)
            {
                this.memory = memory;

                StartAddress = startAddress;
                EndAddress = endAddress;

                Pointer = startAddress;
            }

            /// <summary>
            /// Resets <see cref="Frame"/> back to initial <see cref="Pointer"/> value.
            /// </summary>
            internal void Reset()
            {
                Pointer = StartAddress;
            }

            private void CheckForStackOverflow()
            {
                if (Pointer < EndAddress)
                {
                    throw new StackOverflowException("Stack is full.");
                }
            }

            /// <summary>
            /// Inserts a value at the top of the <see cref="Frame"/>.
            /// Decrements <see cref="Pointer"/>.
            /// </summary>
            /// <param name="value">Value to put on <see cref="Frame"/>.</param>
            /// <exception cref="StackOverflowException">Thrown when <see cref="Frame"/> is full.</exception>
            public void Push(ushort value)
            {
                CheckForStackOverflow();
                memory.SetU16(Pointer, value);
                Pointer -= Processor.DATASIZE;
            }

            private void CheckForEmptyStack()
            {
                if (Pointer == StartAddress)
                {
                    throw new InvalidOperationException("Stack frame is empty.");
                }
            }

            /// <summary>
            /// Removes and returns value at the top of the <see cref="Frame"/>.
            /// Increments <see cref="Pointer"/>.
            /// </summary>
            /// <returns>Value from top of <see cref="Frame"/>.</returns>
            /// <exception cref="InvalidOperationException">Thrown if <see cref="Frame"/> is empty.</exception>
            public ushort Pop()
            {
                // FUTURE: zero out memory on POP for security?
                CheckForEmptyStack();
                Pointer += Processor.DATASIZE;
                return memory.GetU16(Pointer);
            }

            /// <summary>
            /// Returns the value at the top of the <see cref="Frame"/> without removing it.
            /// Does not increment <see cref="Pointer"/>.
            /// </summary>
            /// <returns>Value from top of <see cref="Frame"/>.</returns>
            /// <exception cref="InvalidOperationException">Thrown if <see cref="Frame"/> is empty.</exception>
            public ushort Peek()
            {
                CheckForEmptyStack();
                var address = (ushort)(Pointer + Processor.DATASIZE);
                return memory.GetU16(address);
            }
        }
    }
}
