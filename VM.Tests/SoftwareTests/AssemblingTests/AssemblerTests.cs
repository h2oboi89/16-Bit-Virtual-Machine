using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VM.Software.Assembling;

namespace VM.Tests.SoftwareTests.AssemblingTests
{
    [TestFixture]
    public class AssemblerTests
    {
        [Test]
        public void ValidProgram_Assembles()
        {
            var source = "NOP";

            var binary = Assembler.Assemble(source);

            Assert.That(binary, Is.EqualTo(new byte[] { 0x00 }));
        }

        [Test]
        public void NullSource_ThrowsException()
        {
            Assert.That(() => Assembler.Assemble(null), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("source"));
        }

        [Test]
        public void ScanningException_ThrowsException()
        {
            var source = "BORK";

            Assert.That(() => Assembler.Assemble(source), Throws.InstanceOf<AssemblingException>()
                .With.Message.EqualTo("[1] Error at BORK: Expected LABEL or INSTRUCTION."));
        }

        [Test]
        public void ParsingException_ThrowsException()
        {
            var source = "INC BORK";

            Assert.That(() => Assembler.Assemble(source), Throws.InstanceOf<AssemblingException>()
                .With.Message.EqualTo("[1] Error at BORK: Expected register."));
        }

        [Test]
        public void DuplicateLabels_ThrowsException()
        {
            var source = string.Join(Environment.NewLine, new string[]
            {
                "START:",
                "START:",
                "NOP"
            });

            Assert.That(() => Assembler.Assemble(source), Throws.InstanceOf<AssemblingException>()
               .With.Message.EqualTo("Label 'START' is already defined."));
        }
    }
}
