using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        [Test]
        public void TestMethod()
        {
            var input = new Dictionary<string, byte[]>
            {
                { "LDVR 0xf000 $R0", new byte[] { 0x04, 0xf0, 0x00, 0x07 } },
                { "LDVR	0xf7d0 $R1", new byte[] { 0x04, 0xf7, 0xd0, 0x08 } },

                { "LOOP:",           new byte[] { 0x00, 0x00 } },    
                { "SBVR '#' $R0",    new byte[] { 0x0f, 0x23, 0x07 } },
                { "INC $R0",         new byte[] { 0x02, 0x07 } },
                { "CMP $R0 $R1",     new byte[] { 0x20, 0x07, 0x08 } },
                { "JNE LOOP",        new byte[] { 0x28, 0x00, 0x00 } },

                { "HALT",            new byte[] { 0x34 } }
            };

            foreach (var entry in input)
            {
                var actual = ScanAndParse(entry.Key).First().ToBytes();

                Assert.That(actual, Is.EqualTo(entry.Value));
            }

            var source = "LDVR 0xf000 $R0";

            var statements = ScanAndParse(source);

            Assert.That(statements.First().ToBytes(), Is.EqualTo(new byte[] { 0x04, 0xf0, 0x00, 0x07 }));
        }
    }
}
