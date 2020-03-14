using NUnit.Framework;
using System;
using System.Text;
using VM.Software.Assembling.Scanning;

namespace VM.Tests.SoftwareTests.AssemblingTests.ScanningTests
{
    [TestFixture]
    public class ScannerTests
    {
        static readonly Token EOF_TOKEN_1 = new Token(1, TokenType.EOF, string.Empty);

        private void AssertEmptyInput(string source, int line = 1)
        {
            var expected = new Token[] { new Token(line, TokenType.EOF, string.Empty) };

            Assert.That(Scanner.Scan(source), Is.EqualTo(expected));
        }

        [Test]
        public void NullSource_ThrowsException()
        {
            Assert.That(() => Scanner.Scan(null), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("source"));
        }

        [Test]
        public void Whitespace_IsIgnored()
        {
            AssertEmptyInput(" \r\t\n \r\t", 2);
        }

        [Test]
        public void Comment_Simple_IsIgnored()
        {
            AssertEmptyInput("; this is a comment");
        }

        [Test]
        public void Comment_WithNewline_IsIgnored()
        {
            AssertEmptyInput("; this is a comment\n", 2);
        }

        [Test]
        public void Section_Simple()
        {
            var source = ".SECTION";

            var expected = new Token[] {
                new Token(1, TokenType.SECTION,".SECTION"),
                EOF_TOKEN_1
            };

            Assert.That(Scanner.Scan(source), Is.EqualTo(expected));
        }

        [Test]
        public void Section_WithUnderscore()
        {
            var source = ".SECTION_123abc";

            var expected = new Token[] {
                new Token(1, TokenType.SECTION,".SECTION_123abc"),
                EOF_TOKEN_1
            };

            Assert.That(Scanner.Scan(source), Is.EqualTo(expected));
        }

        [Test]
        public void Character()
        {
            for (char i = ' '; i <= '~'; i++)
            {
                var source = $"'{i}'";

                var expected = new Token[] {
                    new Token(1, TokenType.CHARACTER, $"'{i}'", i),
                    EOF_TOKEN_1
                };

                Assert.That(Scanner.Scan(source), Is.EqualTo(expected));
            }
        }

        [Test]
        public void Character_InvalidCharacter_ThrowsException()
        {
            for (byte i = 0; i < ' '; i++)
            {
                var source = Encoding.ASCII.GetString(new byte[] { (byte)'\'', i, (byte)'\'' });

                Assert.That(() => Scanner.Scan(source), Throws.InstanceOf<ScanningException>()
                    .With.Message.EqualTo($"Unexpected non-printable ASCII character '{(char)i}' on line 1."));
            }

            for (byte i = 0x7f; i < 0x80; i++)
            {
                var source = Encoding.ASCII.GetString(new byte[] { (byte)'\'', i, (byte)'\'' });

                Assert.That(() => Scanner.Scan(source), Throws.InstanceOf<ScanningException>()
                    .With.Message.EqualTo($"Unexpected non-printable ASCII character '{(char)i}' on line 1."));
            }
        }

        [Test]
        public void Character_MissingClosingQuote_ThrowsException()
        {
            var source = "'a ";

            Assert.That(() => Scanner.Scan(source), Throws.InstanceOf<ScanningException>()
                .With.Message.EqualTo("Expected ''' but got ' ' on line 1."));
        }

        [Test]
        public void Register_ValidName()
        {
            var source = "$R0";

            var expected = new Token[] {
                new Token(1, TokenType.REGISTER,"$R0", Register.R0),
                EOF_TOKEN_1
            };

            Assert.That(Scanner.Scan(source), Is.EqualTo(expected));
        }

        [Test]
        public void Register_InvalidName_ThrowsException()
        {
            var source = "$Invalid_Register";

            Assert.That(() => Scanner.Scan(source), Throws.InstanceOf<ScanningException>()
                .With.Message.EqualTo("Unknown VM.Register 'Invalid_Register' on line 1."));
        }

        [Test]
        public void Number_Base10_LeadingZero_ThrowsException()
        {
            var source = "01234";

            Assert.That(() => Scanner.Scan(source), Throws.InstanceOf<ScanningException>()
                .With.Message.EqualTo("Invalid leading '0' on line 1."));
        }

        [Test]
        public void Number_Base10_IsTokenized()
        {
            var source = "1234 5678 90";

            var expected = new Token[] {
                new Token(1, TokenType.NUMBER, "1234", (ushort)1234),
                new Token(1, TokenType.NUMBER, "5678", (ushort)5678),
                new Token(1, TokenType.NUMBER, "90", (ushort)90),
                EOF_TOKEN_1
            };

            Assert.That(Scanner.Scan(source), Is.EqualTo(expected));
        }

        [Test]
        public void Number_Base10_Overflow_ThrowsException()
        {
            var source = "65536";

            Assert.That(() => Scanner.Scan(source), Throws.InstanceOf<ScanningException>()
                .With.Message.EqualTo("Value was too large for U16 on line 1."));
        }

        [Test]
        public void Number_Base16_IsTokenized()
        {
            var source = "0x1234 0xabef 0xABEF";

            var expected = new Token[] {
                new Token(1, TokenType.NUMBER, "0x1234", (ushort)0x1234),
                new Token(1, TokenType.NUMBER, "0xabef", (ushort)0xabef),
                new Token(1, TokenType.NUMBER, "0xABEF", (ushort)0xabef),
                EOF_TOKEN_1
            };

            Assert.That(Scanner.Scan(source), Is.EqualTo(expected));
        }

        [Test]
        public void Number_Base16_Overflow_ThrowsException()
        {
            var source = "0x10000";

            Assert.That(() => Scanner.Scan(source), Throws.InstanceOf<ScanningException>()
                .With.Message.EqualTo("Value was too large for U16 on line 1."));
        }

        [Test]
        public void Number_Base16_NoDigits_ThrowsException()
        {
            var source = "0x";

            Assert.That(() => Scanner.Scan(source), Throws.InstanceOf<ScanningException>()
                .With.Message.EqualTo("Missing digits for hex value on line 1."));
        }

        public void Instruction_Valid()
        {
            var source = "HALT";

            var expected = new Token[] {
                new Token(1, TokenType.INSTRUCTION, "HALT", Instruction.HALT),
                EOF_TOKEN_1
            };

            Assert.That(Scanner.Scan(source), Is.EqualTo(expected));
        }

        [Test]
        public void Label_Simple()
        {
            var source = "LABEL:";

            var expected = new Token[] {
                new Token(1, TokenType.LABEL, "LABEL:"),
                EOF_TOKEN_1
            };

            Assert.That(Scanner.Scan(source), Is.EqualTo(expected));
        }

        [Test]
        public void Label_WithUnderscore()
        {
            var source = "LABEL_123abc:";

            var expected = new Token[] {
                new Token(1, TokenType.LABEL, "LABEL_123abc:"),
                EOF_TOKEN_1
            };

            Assert.That(Scanner.Scan(source), Is.EqualTo(expected));
        }

        [Test]
        public void Identifier_Simple()
        {
            var source = "IDENTIFIER";

            var expected = new Token[] {
                new Token(1, TokenType.IDENTIFIER, "IDENTIFIER"),
                EOF_TOKEN_1
            };

            Assert.That(Scanner.Scan(source), Is.EqualTo(expected));
        }

        [Test]
        public void Identifier_WithUnderscore()
        {
            var source = "IDENTIFIER_123abc";

            var expected = new Token[] {
                new Token(1, TokenType.IDENTIFIER, "IDENTIFIER_123abc"),
                EOF_TOKEN_1
            };

            Assert.That(Scanner.Scan(source), Is.EqualTo(expected));
        }

        [Test]
        public void InvalidCharacters_ThrowException()
        {
            var unexpectedCharacters = new char[]
            {
                '`', '~', '!', '@', '#', '%', '^', '&', '*', '(', ')', '-', '_', '=', '+',
                '[', ']', '{', '}', '\\', '|', ':', '"', ',', '<', '>', '/', '?'
            };

            foreach (var unexpectedCharacter in unexpectedCharacters)
            {
                Assert.That(() => Scanner.Scan(unexpectedCharacter.ToString()), Throws.InstanceOf<ScanningException>()
               .With.Message.EqualTo($"Unexpected character '{unexpectedCharacter}' on line 1."));
            }
        }

        [Test]
        public void SimpleProgam()
        {
            var source = string.Join(Environment.NewLine, new string[] {
                ".CODE",
                "   ; Write '#' to entire console",
                "   LDVR	0xf000	$R0",
                "   LDVR	0xf7d0	$R1",
                "",
                "LOOP:",
                "   SBVR    '#'     $R0",
                "   INC     $R0",
                "   CMP     $R0     $R1",
                "   JNE     LOOP",
                "",
                "END:",
                "   HALT",
                "" });

            var expected = new Token[]
            {
                new Token(1, TokenType.SECTION, ".CODE"),

                new Token(3, TokenType.INSTRUCTION, "LDVR", Instruction.LDVR),
                new Token(3, TokenType.NUMBER, "0xf000", (ushort)0xf000),
                new Token(3, TokenType.REGISTER, "$R0", Register.R0),

                new Token(4, TokenType.INSTRUCTION, "LDVR", Instruction.LDVR),
                new Token(4, TokenType.NUMBER, "0xf7d0", (ushort)0xf7d0),
                new Token(4, TokenType.REGISTER, "$R1", Register.R1),

                new Token(6, TokenType.LABEL, "LOOP:"),

                new Token(7, TokenType.INSTRUCTION, "SBVR", Instruction.SBVR),
                new Token(7, TokenType.CHARACTER, "'#'", '#'),
                new Token(7, TokenType.REGISTER, "$R0", Register.R0),

                new Token(8, TokenType.INSTRUCTION, "INC", Instruction.INC),
                new Token(8, TokenType.REGISTER, "$R0", Register.R0),

                new Token(9, TokenType.INSTRUCTION, "CMP", Instruction.CMP),
                new Token(9, TokenType.REGISTER, "$R0", Register.R0),
                new Token(9, TokenType.REGISTER, "$R1", Register.R1),

                new Token(10, TokenType.INSTRUCTION, "JNE", Instruction.JNE),
                new Token(10, TokenType.IDENTIFIER, "LOOP"),

                new Token(12, TokenType.LABEL, "END:"),

                new Token(13, TokenType.INSTRUCTION, "HALT", Instruction.HALT),

                new Token(14, TokenType.EOF, string.Empty)
            };

            Assert.That(Scanner.Scan(source), Is.EqualTo(expected));
        }
    }
}
