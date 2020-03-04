using System;

namespace VM.Software.Assembling.Scanning
{
    /// <summary>
    /// Token serves as the smallest atomic element during scanning.
    /// </summary>
    public sealed class Token
    {
        /// <summary>
        /// The grammatical type of the scanned lexeme.
        /// </summary>
        public TokenType Type { get; }
        /// <summary>
        /// The section of source that this token represents.
        /// </summary>
        public string Lexeme { get; }
        /// <summary>
        /// Literal value of this token.
        /// </summary>
        public object Literal { get; }
        /// <summary>
        /// Line number of the source this token was scanned from.
        /// </summary>
        public int Line { get; }

        /// <summary>
        /// Creates a new Token with the given parameters.
        /// </summary>
        /// <param name="line">Line number of the source this token was scanned from.</param>
        /// <param name="type">The grammatical type of the scanned lexeme.</param>
        /// <param name="lexeme">The section of source that this token represents.</param>
        /// <param name="literal">Literal value of this token. Default is null.</param>
        public Token(int line, TokenType type, string lexeme, object literal = null)
        {
            Line = line;
            Type = type;
            Lexeme = lexeme;
            Literal = literal;
        }

        /// <summary>
        /// Converts the value of the current <see cref="Token"/> object to its equivalent string representation.
        /// </summary>
        /// <returns>The string representation of the value of this object, which consists of a formatted collection of its fields.</returns>
        public override string ToString()
        {
            var literalValue = string.Empty;

            if (Lexeme.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase)) {
                literalValue = Utility.FormatU16((ushort)Literal);
            }
            else if (Literal != null)
            {
                literalValue = Literal.ToString();
            }
            
            
            return $"{Line} : {Type} '{Lexeme}' [{literalValue}]";
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">An object to compare with this instance, or null.</param>
        /// <returns>True if <paramref name="obj"/> is an instance of <see cref="Token"/> and equals the value of this instance; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as Token);
        }

        /// <summary>
        /// Returns a value indicating whether this instance and a specified <see cref="Token"/> object represent the same value.
        /// </summary>
        /// <param name="token">A <see cref="Token"/> to compare to this instance.</param>
        /// <returns>True if <paramref name="token"/> is equal to this instance; otherwise, false.</returns>
        public bool Equals(Token token)
        {
            if (token is null)
            {
                return false;
            }

            if (ReferenceEquals(token, this))
            {
                return true;
            }

            if (!(Type == token.Type &&
                Lexeme == token.Lexeme &&
                Line == token.Line))
            {
                return false;
            }

            if (Literal == null)
            {
                return token.Literal == null;
            }

            return Literal.Equals(token.Literal);
        }

        /// <summary>
        /// Compares two <see cref="Token"/>s for equality.
        /// </summary>
        /// <param name="lhs">A <see cref="Token"/> to compare against.</param>
        /// <param name="rhs">The other <see cref="Token"/> to compare against the first.</param>
        /// <returns>True if <paramref name="lhs"/> is equal to <paramref name="rhs"/>; otherwise, false.</returns>
        public static bool operator ==(Token lhs, Token rhs)
        {
            if (lhs is null)
            {
                return rhs is null ? true : false;
            }

            return lhs.Equals(rhs);
        }

        /// <summary>
        /// Compares two <see cref="Token"/>s for inequality.
        /// </summary>
        /// <param name="lhs">A <see cref="Token"/> to compare against.</param>
        /// <param name="rhs">The other <see cref="Token"/> to compare against the first.</param>
        /// <returns>True if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>; otherwise, false.</returns>
        public static bool operator !=(Token lhs, Token rhs) => !(lhs == rhs);

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return Type.GetHashCode() + Lexeme.GetHashCode() + Literal.GetHashCode() + Line.GetHashCode();
        }
    }
}
