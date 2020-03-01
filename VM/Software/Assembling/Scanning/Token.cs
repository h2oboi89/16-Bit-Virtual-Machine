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
        public readonly TokenType Type;
        /// <summary>
        /// The section of source that this token represents.
        /// </summary>
        public readonly string Lexeme;
        /// <summary>
        /// Literal value of this token.
        /// </summary>
        public readonly ushort? Literal;
        /// <summary>
        /// Line number of the source this token was scanned from.
        /// </summary>
        public readonly int Line;

        /// <summary>
        /// Creates a new Token with the given parameters.
        /// </summary>
        /// <param name="line">Line number of the source this token was scanned from.</param>
        /// <param name="type">The grammatical type of the scanned lexeme.</param>
        /// <param name="lexeme">The section of source that this token represents.</param>
        /// <param name="literal">Literal value of this token. Default is null.</param>
        public Token(int line, TokenType type, string lexeme, ushort? literal = null)
        {
            Line = line;
            Type = type;
            Lexeme = lexeme;
            Literal = literal;
        }

        public override string ToString()
        {
            return $"{Line} : {Type} '{Lexeme}' [{Literal}]";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Token);
        }

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

            return (Type == token.Type &&
                Lexeme == token.Lexeme &&
                Literal == token.Literal &&
                Line == token.Line);
        }

        public static bool operator ==(Token lhs, Token rhs)
        {
            if (lhs is null)
            {
                return rhs is null ? true : false;
            }

            return lhs.Equals(rhs);
        }

        public static bool operator !=(Token lhs, Token rhs) => !(lhs == rhs);

        public override int GetHashCode()
        {
            return Type.GetHashCode() + Lexeme.GetHashCode() + Literal.GetHashCode() + Line.GetHashCode();
        }
    }
}
