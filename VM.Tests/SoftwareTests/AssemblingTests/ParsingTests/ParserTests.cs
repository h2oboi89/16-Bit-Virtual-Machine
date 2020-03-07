using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using VM.Software.Assembling.Parsing;
using VM.Software.Assembling.Scanning;

namespace VM.Tests.SoftwareTests.AssemblingTests.ParsingTests
{
    [TestFixture]
    public class ParserTests
    {
        private IEnumerable<Statement> ScanAndParse(string source)
        {
            return Parser.Parse(Scanner.Scan(source));
        }

        private void TestValues(IEnumerable<(string input, string expected)> values)
        {
            foreach (var (input, expected) in values)
            {
                var actual = ScanAndParse(input).First().ToString();

                Assert.That(actual, Is.EqualTo(expected));
            }
        }

        #region Statements
        [Test]
        public void Label()
        {
            TestValues(new List<(string, string)> {
                ("LABEL:", "0x0000 : LABEL")
            });
        }

        [Test]
        public void Instruction_NoArguments()
        {
            TestValues(new List<(string, string)> {
                ("NOP",     "0x0000 : NOP"),
                ("HALT",    "0x0000 : HALT"),
                ("RESET",   "0x0000 : RESET")
            });
        }

        // TODO: All: unexpected tokens and other exceptions

        [Test]
        public void Instruction_JumpTarget()
        {
            TestValues(new List<(string, string)> {
                ("JUMP 0x1234", "0x0000 : JUMP 0x1234"),
                ("JUMP LABEL",  "0x0000 : JUMP LABEL (0x0000)"),

                ("JLT 0x1111",  "0x0000 : JLT 0x1111"),
                ("JLT ROUTINE", "0x0000 : JLT ROUTINE (0x0000)"),

                ("JGT 0x2222",  "0x0000 : JGT 0x2222"),
                ("JGT ROUTINE", "0x0000 : JGT ROUTINE (0x0000)"),

                ("JE 0x4444",   "0x0000 : JE 0x4444"),
                ("JE ROUTINE",  "0x0000 : JE ROUTINE (0x0000)"),

                ("JNE 0x5555",  "0x0000 : JNE 0x5555"),
                ("JNE ROUTINE", "0x0000 : JNE ROUTINE (0x0000)"),

                ("JZ 0x6666",   "0x0000 : JZ 0x6666"),
                ("JZ ROUTINE",  "0x0000 : JZ ROUTINE (0x0000)"),

                ("JNZ 0x7777",  "0x0000 : JNZ 0x7777"),
                ("JNZ ROUTINE", "0x0000 : JNZ ROUTINE (0x0000)"),
            });
        }

        [Test]
        public void Instruction_Register()
        {
            TestValues(new List<(string, string)> {
                ("INC $R0",     "0x0000 : INC 0x10"),
                ("DEC $R1",     "0x0000 : DEC 0x11"),
                ("NOT $R2",     "0x0000 : NOT 0x12"),
                ("JUMPR $R3",   "0x0000 : JUMPR 0x13"),
                ("CMPZ $R4",    "0x0000 : CMPZ 0x14"),
                ("JLTR $R5",    "0x0000 : JLTR 0x15"),
                ("JGTR $R6",    "0x0000 : JGTR 0x16"),
                ("JER $R7",     "0x0000 : JER 0x17"),
                ("JNER $R8",    "0x0000 : JNER 0x18"),
                ("JZR $R9",     "0x0000 : JZR 0x19"),
                ("JNZR $R10",   "0x0000 : JNZR 0x1A"),
                ("PUSH $R11",   "0x0000 : PUSH 0x1B"),
                ("POP $R12",    "0x0000 : POP 0x1C"),
                ("PEEK $R13",   "0x0000 : PEEK 0x1D"),
            });
        }

        [Test]
        public void Instruction_Register_Register()
        {
            TestValues(new List<(string, string)> {
                ("MOVE $S0 $T0",    "0x0000 : MOVE 0x20 0x30"),
                ("LDRR $S1 $T1",    "0x0000 : LDRR 0x21 0x31"),
                ("LBRR $S2 $T2",    "0x0000 : LBRR 0x22 0x32"),
                ("STRR $S3 $T3",    "0x0000 : STRR 0x23 0x33"),
                ("SBRR $S4 $T4",    "0x0000 : SBRR 0x24 0x34"),
                ("ADD $S5 $T5",     "0x0000 : ADD 0x25 0x35"),
                ("SUB $S6 $T6",     "0x0000 : SUB 0x26 0x36"),
                ("MUL $S7 $T7",     "0x0000 : MUL 0x27 0x37"),
                ("DIV $S8 $T8",     "0x0000 : DIV 0x28 0x38"),
                ("AND $S9 $T9",     "0x0000 : AND 0x29 0x39"),
                ("OR $S10 $T10",    "0x0000 : OR 0x2A 0x3A"),
                ("XOR $S11 $T11",   "0x0000 : XOR 0x2B 0x3B"),
                ("SRLR $S12 $T12",  "0x0000 : SRLR 0x2C 0x3C"),
                ("SRRR $S13 $T13",  "0x0000 : SRRR 0x2D 0x3D"),
                ("CMP $S14 $T14",   "0x0000 : CMP 0x2E 0x3E"),
            });
        }

        [Test]
        public void Instruction_Register_U8()
        {
            TestValues(new List<(string, string)> {
                ("SRL $R0 2",   "0x0000 : SRL 0x10 0x02"),
                ("SRR $R1 15",  "0x0000 : SRR 0x11 0x0F"),
            });
        }

        [Test]
        public void Instruction_Register_U16()
        {
            TestValues(new List<(string, string)> {
                ("STRA $R0 0xffff", "0x0000 : STRA 0x10 0xFFFF"),
                ("SBRA $R1 0x1234", "0x0000 : SBRA 0x11 0x1234"),
            });
        }

        [Test]
        public void Instruction_U8()
        {
            TestValues(new List<(string, string)>
            {
                ("RET 0", "0x0000 : RET 0x00")
            });
        }

        [Test]
        public void Instruction_U8_JumpTarget()
        {
            TestValues(new List<(string, string)>
            {
                ("CALL 0 0x1234",   "0x0000 : CALL 0x00 0x1234"),
                ("CALL 0 LABEL",    "0x0000 : CALL 0x00 LABEL (0x0000)"),
            });
        }

        [Test]
        public void Instruction_U8_Register()
        {
            TestValues(new List<(string, string)>
            {
                ("LBVR 0 $R0",      "0x0000 : LBVR 0x00 0x10"),
                ("LBVR '#' $R1",    "0x0000 : LBVR 0x23 0x11"),

                ("SBVR 255 $R2",    "0x0000 : SBVR 0xFF 0x12"),
                ("SBVR '!' $R3",    "0x0000 : SBVR 0x21 0x13"),

                ("CALLR 16 $R4",    "0x0000 : CALLR 0x10 0x14"),
                ("CALLR 0x16 $R5",  "0x0000 : CALLR 0x16 0x15"),
            });
        }

        [Test]
        public void Instruction_U8_U16()
        {
            TestValues(new List<(string, string)>
            {
                ("SBVA 0 0x1234",   "0x0000 : SBVA 0x00 0x1234"),
                ("SBVA '~' 0x5678", "0x0000 : SBVA 0x7E 0x5678"),
            });
        }

        [Test]
        public void Instruction_U16_Register()
        {
            TestValues(new List<(string, string)>
            {
                ("LDVR 0 $R0",      "0x0000 : LDVR 0x0000 0x10"),
                ("LDAR 0x1234 $R1", "0x0000 : LDAR 0x1234 0x11"),
                ("LBAR 0x5678 $R2", "0x0000 : LBAR 0x5678 0x12"),
                ("STVR 0x9abc $R3", "0x0000 : STVR 0x9ABC 0x13"),
            });
        }

        [Test]
        public void Instruction_U16_U16()
        {
            TestValues(new List<(string, string)>
            {
                ("STVA 0x0123 0x4567", "0x0000 : STVA 0x0123 0x4567"),
            });
        }
        #endregion

        #region Exceptions
        [Test]
        public void ExpectedStatement_MissingTokenAtEndOfFile_ThrowsException()
        {
            Assert.That(() => ScanAndParse("JUMP"), Throws.InstanceOf<ParsingException>()
                .With.Message.EqualTo("[1] Error at end: Expected U16 or label name."));
        }

        [Test]
        public void ExpectedStatement_UnexpectedToken_ThrowsException()
        {
            Assert.That(() => ScanAndParse("0"), Throws.InstanceOf<ParsingException>()
                .With.Message.EqualTo("[1] Error at 0: Expected LABEL or INSTRUCTION."));
        }

        [Test]
        public void InvalidJumpTarget_ThrowsException()
        {
            Assert.That(() => ScanAndParse("JUMP NOP"), Throws.InstanceOf<ParsingException>()
                .With.Message.EqualTo("[1] Error at NOP: Expected U16 or label name."));
        }

        [Test]
        public void ExpectedRegister_UnexpectedToken_ThrowsException()
        {
            Assert.That(() => ScanAndParse("JUMPR 255"), Throws.InstanceOf<ParsingException>()
                .With.Message.EqualTo("[1] Error at 255: Expected register."));
        }

        [Test]
        public void ValueTooLargeForU8()
        {
            Assert.That(() => ScanAndParse("RET 256"), Throws.InstanceOf<ParsingException>()
                .With.Message.EqualTo("[1] Error at 256: 256 too large for U8."));
        }

        [Test]
        public void ExpectedNumber_UnexpectedToken_ThrowsException()
        {
            Assert.That(() => ScanAndParse("RET LOOP"), Throws.InstanceOf<ParsingException>()
                .With.Message.EqualTo("[1] Error at LOOP: Expected U8 or character."));

            Assert.That(() => ScanAndParse("LDVR LOOP"), Throws.InstanceOf<ParsingException>()
                .With.Message.EqualTo("[1] Error at LOOP: Expected U16 or character."));
        }
        #endregion
    }
}
