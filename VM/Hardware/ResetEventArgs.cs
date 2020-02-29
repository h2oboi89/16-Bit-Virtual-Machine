using System;

namespace VM.Hardware
{
    /// <summary>
    /// Event arguments for <see cref="Processor.Reset"/>.
    /// </summary>
    public sealed class ResetEventArgs
    {
        /// <summary>
        /// <see cref="Instruction"/> that caused the reset.
        /// </summary>
        public Instruction Instruction { get; private set; }

        /// <summary>
        /// <see cref="Exception"/> that caused the reset.
        /// Null if reset was caused by <see cref="Instruction.RESET"/>.
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// Creates a new instance with the optionally specified <see cref="Exception"/>.
        /// </summary>
        /// <param name="instruction"><see cref="Instruction"/> that caused the reset.</param>
        /// <param name="exception"><see cref="Exception"/> that caused reset. Null if reset was caused by <see cref="Instruction.RESET"/>.</param>
        public ResetEventArgs(Instruction instruction, Exception exception = null)
        {
            Instruction = instruction;
            Exception = exception;
        }
    }
}