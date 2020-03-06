using System;
using System.Collections.Generic;
using System.Globalization;

namespace VM.Software.Assembling.Scanning
{
    /// <summary>
    /// Scans source code and generates a collection of <see cref="Token"/>s for the following compilation steps.
    /// </summary>
    public static class Scanner
    {
        private static string _source;
        private static List<Token> _tokens;
        private static int _start;
        private static int _current;
        private static int _line;

        /// <summary>
        /// Scans the specified source and returns a collection of <see cref="Token"/>s.
        /// </summary>
        /// <param name="source">source code to scan.</param>
        /// <returns>Collection of <see cref="Token"/>s representing the source.</returns>
        public static IEnumerable<Token> Scan(string source)
        {
            _source = source;
            _tokens = new List<Token>();
            _start = 0;
            _current = 0;
            _line = 1;

            while (!IsAtEnd())
            {
                _start = _current;
                ScanToken();
            }

            _tokens.Add(new Token(_line, TokenType.EOF, "", null));

            return _tokens;
        }

        private static void ScanToken()
        {
            var c = Advance();

            switch (c)
            {
                case ' ':
                case '\r':
                case '\t':
                    // ignore whitespace
                    break;
                case '\n': _line++; break;
                case ';':
                    // ignore comments
                    SkipToEndOfLine();
                    break;
                case '.': Section(); break;
                case '\'': Character(); break;
                case '$': Register(); break;
                default:
                    if (IsDigit(c))
                    {
                        Number();
                    }
                    else if (IsAlpha(c))
                    {
                        Text();
                    }
                    else
                    {
                        ThrowException($"Unexpected character '{c}'");
                    }
                    break;
            }
        }

        private static void SkipToEndOfLine()
        {
            while (Peek() != '\n' && !IsAtEnd())
            {
                Advance();
            }
        }

        private static void IdentifierToken(TokenType type)
        {
            Identifier();

            AddToken(type);
        }

        private static void Section() => IdentifierToken(TokenType.SECTION);

        private static void Character()
        {
            if (!IsCharacter(Advance()))
            {
                ThrowException($"Unexpected non-printable ASCII character '{_source[_current - 1]}'");
            }

            if (Advance() == '\'')
            {
                AddToken(TokenType.CHARACTER, _source[_current - 2]);
            }
            else
            {
                ThrowException($"Expected ''' but got '{_source[_current - 1]}'");
            }
        }

        private static void Register()
        {
            Identifier();

            try
            {
                var lexeme = _source.Extract(_start + 1, _current);
                var register = Utility.ParseRegister(lexeme);

                AddToken(TokenType.REGISTER, register);
            } catch (ArgumentException e)
            {
                ThrowException(e.Message);
            }
        }

        private static void Text()
        {
            Identifier();

            try
            {
                var lexeme = _source.Extract(_start, _current);
                var instruction = Utility.ParseInstruction(lexeme);

                AddToken(TokenType.INSTRUCTION, instruction);
                return;
            }
            catch (ArgumentException) { }

            if (Peek() == ':')
            {
                Advance();
                AddToken(TokenType.LABEL);
            }
            else
            {
                var lexeme = _source.Extract(_start, _current);

                AddToken(TokenType.IDENTIFIER, lexeme);
            }
        }

        private static void Number()
        {
            if (Peek() == 'x' || Peek() == 'X')
            {
                Base16();
            }
            else
            {
                Base10();
            }
        }

        private static void Base10()
        {
            while (IsDigit(Peek()))
            {
                Advance();
            }

            try
            {
                if (_source[_start] == '0' && _current != _start + 1)
                {
                    ThrowException($"Invalid leading '0'");
                }

                AddToken(TokenType.NUMBER, ushort.Parse(_source.Extract(_start, _current), NumberStyles.Integer, CultureInfo.InvariantCulture));
            }
            catch (OverflowException e)
            {
                ThrowException("Value was too large for U16", e);
            }
        }

        private static void Base16()
        {
            // get past "0x"
            Advance();

            if (!IsHex(Peek()))
            {
                ThrowException("Missing digits for hex value");
            }

            while (IsHex(Peek()))
            {
                Advance();
            }

            try
            {
                AddToken(TokenType.NUMBER, ushort.Parse(_source.Extract(_start + 2, _current), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
            }
            catch (OverflowException e)
            {
                ThrowException("Value was too large for U16", e);
            }
        }

        private static void Identifier()
        {
            while (IsExtendedAlpha(Peek()))
            {
                Advance();
            }
        }

        private static bool IsAlpha(char c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');

        private static bool IsExtendedAlpha(char c) => IsAlpha(c) || IsDigit(c) || c == '_';

        private static bool IsCharacter(char c) => c >= 0x20 && c <= 0x7e;

        private static bool IsDigit(char c) => c >= '0' && c <= '9';

        private static bool IsHex(char c) => IsDigit(c) || ((c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'));

        private static bool IsAtEnd(int n = 0) => _current + n >= _source.Length;

        private static char Peek(int n = 0) => IsAtEnd(n) ? '\0' : _source[_current + n];

        private static char Advance() => _source[_current++];

        private static void AddToken(TokenType type, object literal = null)
        {
            var lexeme = _source.Extract(_start, _current);

            _tokens.Add(new Token(_line, type, lexeme, literal));
        }

        private static void ThrowException(string message, Exception e = null)
        {
            throw new ScanningException($"{message} on line {_line}.", e);
        }
    }
}
