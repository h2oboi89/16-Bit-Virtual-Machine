namespace VM.Software.Assembling.Parsing
{
    /// <summary>
    /// Represents a statement in the assembled program
    /// </summary>
    public abstract class Statement
    {
        /// <summary>
        /// <see cref="VM.Hardware.Memory"/> address of this statement
        /// </summary>
        public ushort Address { get; internal set; }

        /// <summary>
        /// Determines whether the specified object is equal to the current <see cref="Statement"/>.
        /// </summary>
        /// <param name="obj">The object to compare to the current <see cref="Statement"/>.</param>
        /// <returns>True if the specified object is equal to the current <see cref="Statement"/>; othersise, false.</returns>
        public override bool Equals(object obj) => Equals(obj as Statement);

        /// <summary>
        /// Determines whether the specified <see cref="Statement"/> is equal to the current instance.
        /// </summary>
        /// <param name="other">The <see cref="Statement"/> to compare to this instance.</param>
        /// <returns>True if specified <see cref="Statement"/> is equal to this instance; otherwise, false.</returns>
        public bool Equals(Statement other)
        {
            if (other == null)
            {
                return false;
            }

            return Address == other.Address;
        }

        /// <summary>
        /// Returns the hash code for this <see cref="Statement"/>.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode() => 61 * Address.GetHashCode();

        /// <summary>
        /// Retuns a <see cref="string"/> that represents the current <see cref="Statement"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="Statement"/>.</returns>
        public override string ToString() => Utility.FormatU16(Address);
    }
}