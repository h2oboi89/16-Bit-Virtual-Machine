using System;
using System.Collections.Generic;
using System.Linq;

namespace VM.Hardware
{
    /// <summary>
    /// Utility for writing programs to memory.
    /// </summary>
    public sealed class Flasher
    {
        private readonly Memory memory;

        /// <summary>
        /// Creates a new <see cref="Flasher"/> for writing the specified <see cref="Memory"/>.
        /// </summary>
        /// <param name="memory"><see cref="Memory"/> to flash.</param>
        public Flasher(Memory memory)
        {
            this.memory = memory;
            Address = 0;
        }

        /// <summary>
        /// Next address in memory that will be written to.
        /// </summary>
        public ushort Address { get; set; }

        #region U8
        /// <summary>
        /// Flashes byte(s) to the next available spot in <see cref="Memory"/> as specified by <see cref="Address"/>.
        /// </summary>
        /// <param name="values">Byte(s) to write.</param>
        public void Flash(params byte[] values)
        {
            foreach (var value in values)
            {
                memory.SetU8(Address++, value);
            }
        }

        /// <summary>
        /// Flashes byte(s) to the specified location in <see cref="Memory"/>.
        /// This updates <see cref="Address"/>.
        /// </summary>
        /// <param name="address">Location in <see cref="Memory"/> to write to.</param>
        /// <param name="values">Byte(s) to write.</param>
        public void Flash(ushort address, params byte[] values)
        {
            Address = address;

            Flash(values);
        }

        /// <summary>
        /// Flashes a collection of bytes to the next available spot in <see cref="Memory"/> as specified by <see cref="Address"/>.
        /// </summary>
        /// <param name="binary">Bytes to write.</param>
        public void Flash(IEnumerable<byte> binary)
        {
            if (binary == null)
            {
                throw new ArgumentNullException(nameof(binary));
            }

            foreach (var value in binary)
            {
                Flash(value);
            }
        }
        #endregion

        #region U16
        /// <summary>
        /// Flashes ushort(s) to the next available spot in <see cref="Memory"/> as specified by <see cref="Address"/>.
        /// </summary>
        /// <param name="values">Ushort(s) to write.</param>
        public void Flash(params ushort[] values)
        {
            foreach (var value in values)
            {
                Flash(Utility.GetBytes(value).ToArray());
            }
        }

        /// <summary>
        /// Flashes ushort(s) to the specified location in <see cref="Memory"/>.
        /// This updates <see cref="Address"/>.
        /// </summary>
        /// <param name="address">Location in <see cref="Memory"/> to write to.</param>
        /// <param name="values">Ushort(s) to write.</param>
        public void Flash(ushort address, params ushort[] values)
        {
            Address = address;

            Flash(values);
        }
        #endregion
    }
}
