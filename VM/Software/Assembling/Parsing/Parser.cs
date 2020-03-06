using System;
using System.Collections.Generic;
using VM.Software.Assembling.Scanning;

namespace VM.Software.Assembling.Parsing
{
    public static class Parser
    {
        private static List<Token> _tokens;
        private static int _current = 0;

        public static IEnumerable<Statement> Parse(IEnumerable<Token> tokens)
        {
            _tokens = new List<Token>(tokens);
            _current = 0;

            var statements = new List<Statement>();

            while (!IsAtEnd)
            {
                statements.Add(Statement());
            }

            return statements;
        }

        #region Utility Methods
        private static bool Match(params TokenType[] tokenTypes)
        {
            foreach (var tokenType in tokenTypes)
            {
                if (Check(tokenType))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool Check(TokenType tokenType) => IsAtEnd ? false : Peek.Type == tokenType;

        private static Token Advance()
        {
            if (!IsAtEnd) _current++;
            return PreviousToken;
        }

        private static bool IsAtEnd => Peek.Type == TokenType.EOF;

        private static Token Peek => _tokens[_current];

        private static Token PreviousToken => _tokens[_current - 1];

        private static Token Consume(TokenType tokenType, string message)
        {
            if (Check(tokenType))
            {
                return Advance();
            }
            else
            {
                throw Error(Peek, message);
            }
        }

        private static ParsingException Error(Token token, string message)
        {
            var where = token.Type == TokenType.EOF ? "end" : token.Lexeme;

            return new ParsingException($"[{token.Line}] Error at {where}: {message}.");
        }
        #endregion

        private static Statement Statement()
        {
            if (Peek.Type == TokenType.LABEL) return new LabelStatement(Advance().Lexeme);
            if (Peek.Type == TokenType.INSTRUCTION) return InstructionStatement();

            throw Error(Peek, $"Unexpected token {Peek}, expected {TokenType.LABEL} or {TokenType.INSTRUCTION}");
        }

        private static Statement InstructionStatement()
        {
            var instruction = (Instruction)Advance().Literal;

            switch (instruction)
            {
                case Instruction.NOP:
                case Instruction.HALT:
                case Instruction.RESET:
                    return new InstructionStatement(instruction);
                case Instruction.JUMP:
                case Instruction.JLT:
                case Instruction.JGT:
                case Instruction.JE:
                case Instruction.JNE:
                case Instruction.JNZ:
                    return new InstructionStatement(instruction, JumpTarget());
                case Instruction.INC:
                case Instruction.DEC:
                case Instruction.NOT:
                case Instruction.JUMPR:
                case Instruction.CMPZ:
                case Instruction.JLTR:
                case Instruction.JGTR:
                case Instruction.JER:
                case Instruction.JNER:
                case Instruction.JZR:
                case Instruction.JNZR:
                case Instruction.PUSH:
                case Instruction.POP:
                case Instruction.PEEK:
                    return new InstructionStatement(instruction, Register());
                case Instruction.MOVE:
                case Instruction.LDRR:
                case Instruction.LBRR:
                case Instruction.STRR:
                case Instruction.SBRR:
                case Instruction.ADD:
                case Instruction.SUB:
                case Instruction.MUL:
                case Instruction.DIV:
                case Instruction.AND:
                case Instruction.OR:
                case Instruction.XOR:
                case Instruction.SRLR:
                case Instruction.SRRR:
                case Instruction.CMP:
                    return new InstructionStatement(instruction, Register(), Register());
                case Instruction.SRL:
                case Instruction.SRR:
                    return new InstructionStatement(instruction, Register(), U8());
                case Instruction.STRA:
                case Instruction.SBRA:
                    return new InstructionStatement(instruction, Register(), U16());
                case Instruction.RET:
                    return new InstructionStatement(instruction, U8());
                case Instruction.CALL:
                    return new InstructionStatement(instruction, U8(), JumpTarget());
                case Instruction.LBVR:
                case Instruction.SBVR:
                case Instruction.CALLR:
                    return new InstructionStatement(instruction, U8(), Register());
                case Instruction.SBVA:
                    return new InstructionStatement(instruction, U8(), U16());
                case Instruction.LDVR:
                case Instruction.LDAR:
                case Instruction.LBAR:
                case Instruction.STVR:
                    return new InstructionStatement(instruction, U16(), Register());
                case Instruction.STVA:
                    return new InstructionStatement(instruction, U16(), U16());
                default:
                    throw new NotImplementedException($"Unknown instruction: {instruction}.");
            }
        }

        // FUTURE: All places that accept a number (u8 or u16) can be raw or identifier
        // Value : u8, 16, variable
        // JumpTarget : can be u16 or label name (could also be defined as u8 in variable form)

        private static Argument JumpTarget()
        {
            if (Match(TokenType.NUMBER))
            {
                return new Argument((ushort)Advance().Literal, sizeof(ushort));
            }

            if (Match(TokenType.IDENTIFIER))
            {
                return new Argument(0, sizeof(ushort), true);
            }

            throw Error(Peek, "Expected U16 or label name.");
        }

        private static Argument Register()
        {
            var register = Consume(TokenType.REGISTER, "Expected register.");

            return new Argument((ushort)register.Literal, sizeof(Register));
        }

        private static Argument U8()
        {
            if (!Match(TokenType.NUMBER, TokenType.CHARACTER))
            {
                throw Error(Peek, "Expected U8 or character.");
            }

            var number = Advance();

            var value = (ushort)number.Literal;

            if (value > byte.MaxValue)
            {
                throw Error(number, $"{value} too large for U8.");
            }

            return new Argument(value, sizeof(byte));
        }

        private static Argument U16()
        {
            if (!Match(TokenType.NUMBER, TokenType.CHARACTER))
            {
                throw Error(Peek, "Expected U16 or character.");
            }

            return new Argument((ushort)Advance().Literal, sizeof(ushort));
        }
    }
}
