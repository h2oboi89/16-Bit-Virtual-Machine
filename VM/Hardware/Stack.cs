using System;

namespace VM.Hardware
{
    /// <summary>
    /// Represents the call stack in a <see cref="Processor"/>.
    /// </summary>
    public sealed class Stack
    {
        private readonly Memory memory;

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
        public ushort StackPointer { get; set; }

        /// <summary>
        /// Value of <see cref="StackPointer"/> before last subroutine call.
        /// </summary>
        public ushort FramePointer { get; set; }

        /// <summary>
        /// Address of number of arguments being passed to subroutine.
        /// Set during <see cref="Instruction.CALL"/> and <see cref="Instruction.CALLR"/>.
        /// </summary>
        public ushort ArgumentsPointer { get; private set; }

        /// <summary>
        /// Address of number of return values from subroutine.
        /// Set during <see cref="Instruction.RET"/>.
        /// </summary>
        public ushort ReturnsPointer { get; private set; }

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

            Reset();
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
            FramePointer = StartAddress;
            StackPointer = StartAddress;
            ArgumentsPointer = StartAddress;
            ReturnsPointer = StartAddress;
        }

        private void CheckForStackOverflow()
        {
            if (StackPointer < EndAddress)
            {
                throw new StackOverflowException("Stack is full.");
            }
        }

        private void CheckForEmptyStack()
        {
            if (StackPointer == FramePointer)
            {
                throw new InvalidOperationException("Stack frame is empty.");
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

        /// <summary>
        /// Removes and returns value at the top of the <see cref="Stack"/>.
        /// Increments <see cref="StackPointer"/>.
        /// </summary>
        /// <returns>Value from top of <see cref="Stack"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="Stack"/> is empty.</exception>
        public ushort Pop(bool checkForEmptyStack = true)
        {
            if (checkForEmptyStack) CheckForEmptyStack();
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

        /// <summary>
        /// Closes current stack frame of subroutine caller and starts a new one for called subroutine.
        /// </summary>
        /// <param name="argumentCount">Number of subroutine arguments that were pushed to stack.</param>
        /// <param name="returnAddress"><see cref="Register.PC"/> value to return to after subroutine.</param>
        public void Call(ushort argumentCount, ushort returnAddress)
        {
            ArgumentsPointer = StackPointer;
            Push(argumentCount);

            Push(returnAddress);
            Push(FramePointer);
            FramePointer = StackPointer;
        }

        /// <summary>
        /// Closes current stack frame of returning subroutine and opens previous one for subroutine caller.
        /// </summary>
        /// <returns><see cref="Register.PC"/> value to return to.</returns>
        public ushort Return(ushort returnValueCount)
        {
            if (FramePointer == StartAddress)
            {
                throw new InvalidOperationException("In base frame, nothing to return to.");
            }

            ReturnsPointer = StackPointer;
            Push(returnValueCount);

            StackPointer = FramePointer;
            FramePointer = Pop(false);
            var returnAddress = Pop();

            return returnAddress;
        }
    }
}
