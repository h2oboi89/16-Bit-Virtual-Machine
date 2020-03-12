using NUnit.Framework;
using VM.Software.Assembling.Scanning;

namespace VM.Tests.SoftwareTests.AssemblingTests.ScanningTests
{
    [TestFixture]
    public class TokenTests
    {
        [Test]
        public void Constructor_DefaultLiteral()
        {
            var token = new Token(1, TokenType.EOF, string.Empty);

            Assert.That(token.Line, Is.EqualTo(1));
            Assert.That(token.Type, Is.EqualTo(TokenType.EOF));
            Assert.That(token.Lexeme, Is.EqualTo(string.Empty));
            Assert.That(token.Literal, Is.Null);
        }

        [Test]
        public void Constructor_Literal()
        {
            var token = new Token(10, TokenType.NUMBER, "0x10", (ushort)0x10);

            Assert.That(token.Line, Is.EqualTo(10));
            Assert.That(token.Type, Is.EqualTo(TokenType.NUMBER));
            Assert.That(token.Lexeme, Is.EqualTo("0x10"));
            Assert.That(token.Literal, Is.EqualTo(0x10));
        }

        [Test]
        public void ToString_EmptyLexemeAndNullLiteral()
        {
            var token = new Token(314, TokenType.EOF, string.Empty);

            Assert.That(token.ToString(), Is.EqualTo("314 : EOF '' []"));
        }

        [Test]
        public void ToString_HexValue()
        {
            var token = new Token(42, TokenType.NUMBER, "0x10", (ushort)0x10);

            Assert.That(token.ToString(), Is.EqualTo("42 : NUMBER '0x10' [0x0010]"));
        }

        [Test]
        public void ToString_NonHexValue()
        {
            var token = new Token(69, TokenType.NUMBER, "10", (ushort)10);

            Assert.That(token.ToString(), Is.EqualTo("69 : NUMBER '10' [10]"));
        }

        [Test]
        public void Equals_NullToken_ReturnsFalse()
        {
            var token = new Token(15, TokenType.REGISTER, "$R0", Register.R0);

            Assert.That(token.Equals(null), Is.False);
        }

        [Test]
        public void Equals_SameObject_ReturnsTrue()
        {
            var token = new Token(15, TokenType.REGISTER, "$R0", Register.R0);

            Assert.That(token.Equals(token), Is.True);
        }

        [Test]
        public void Equals_DifferentType_ReturnsFalse()
        {
            var token = new Token(15, TokenType.REGISTER, "$R0", Register.R0);
            var other = new Token(15, TokenType.INSTRUCTION, "HALT", Instruction.HALT);

            Assert.That(token.Equals(other), Is.False);
        }

        [Test]
        public void Equals_DifferentLexeme_ReturnsFalse()
        {
            var token = new Token(15, TokenType.REGISTER, "$R0", Register.R0);
            var other = new Token(15, TokenType.REGISTER, "$R1", Register.R1);

            Assert.That(token.Equals(other), Is.False);
        }

        [Test]
        public void Equals_DifferentLine_ReturnsFalse()
        {
            var token = new Token(15, TokenType.REGISTER, "$R0", Register.R0);
            var other = new Token(16, TokenType.REGISTER, "$R0", Register.R0);

            Assert.That(token.Equals(other), Is.False);
        }

        [Test]
        public void Equals_BothLiteralsAreNull_ReturnsTrue()
        {
            var token = new Token(314, TokenType.EOF, string.Empty);
            var other = new Token(314, TokenType.EOF, string.Empty);

            Assert.That(token.Equals(other), Is.True);
        }

        [Test]
        public void Equals_OtherLiteralIsNull_ReturnsFalse()
        {
            var token = new Token(314, TokenType.EOF, string.Empty, string.Empty);
            var other = new Token(314, TokenType.EOF, string.Empty);

            Assert.That(token.Equals(other), Is.False);
        }

        [Test]
        public void EqualsOperator_BothSidesNull_ReturnsTrue()
        {
            Token token = null;
            Token other = null;

            Assert.That(token == other, Is.True);
        }

        [Test]
        public void EqualsOperator_RHSNull_ReturnsFalse()
        {
            Token token = new Token(1, TokenType.EOF, string.Empty);
            Token other = null;

            Assert.That(token == other, Is.False);
        }

        [Test]
        public void EqualsOperator_LHSNull_ReturnsFalse()
        {
            Token token = null;
            Token other = new Token(1, TokenType.EOF, string.Empty);

            Assert.That(token == other, Is.False);
        }

        [Test]
        public void EqualsOperator_BothSidesNotNull_ReturnsEquals()
        {
            var token = new Token(15, TokenType.REGISTER, "$R0", Register.R0);
            var other = token;

            Assert.That(token == null, Is.False);
            Assert.That(token == other, Is.True);
            Assert.That(token == new Token(15, TokenType.INSTRUCTION, "HALT", Instruction.HALT), Is.False);
            Assert.That(token == new Token(15, TokenType.REGISTER, "$R1", Register.R1), Is.False);
        }

        [Test]
        public void NotEqualsOperator_ReturnsNotEqualsOperator()
        {
            var token = new Token(15, TokenType.REGISTER, "$R0", Register.R0);
            var other = token;

            Assert.That(token != null, Is.True);
            Assert.That(token != other, Is.False);
            Assert.That(token != new Token(15, TokenType.INSTRUCTION, "HALT", Instruction.HALT), Is.True);
            Assert.That(token != new Token(15, TokenType.REGISTER, "$R1", Register.R1), Is.True);
        }

        [Test]
        public void GetHashCode_EqualObjects_AreEqual()
        {
            var token = new Token(15, TokenType.REGISTER, "$R0", Register.R0);
            var other = new Token(15, TokenType.REGISTER, "$R0", Register.R0);

            Assert.That(token.GetHashCode() == token.GetHashCode(), Is.True);
            Assert.That(token.GetHashCode() == other.GetHashCode(), Is.True);
        }

        [Test]
        public void GetHashCode_UnequalObjects_AreNotEqual()
        {
            var token = new Token(15, TokenType.REGISTER, "$R0", Register.R0);
            var other = new Token(16, TokenType.REGISTER, "$R1", Register.R1);

            Assert.That(token.GetHashCode() == other.GetHashCode(), Is.False);
        }
    }
}
