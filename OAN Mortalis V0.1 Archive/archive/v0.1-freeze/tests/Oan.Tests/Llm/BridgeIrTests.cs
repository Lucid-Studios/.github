using System;
using Oan.Place.Llm.BridgeIr;
using Xunit;

namespace Oan.Tests.Llm
{
    public class BridgeIrTests
    {
        [Fact]
        public void Parse_Rejects_Prose()
        {
            var parser = new BridgeIrParser("hello world");
            var ex = Assert.Throws<BridgeIrException>(() => parser.Parse());
            Assert.Equal(BridgeIrErrorCode.PROSE_INPUT, ex.ReasonCode);
        }

        [Fact]
        public void Parses_MoveTo_Valid()
        {
            string ir = "(oan.intent (id \"test-1\") (sli \"public/oan/move.commit\") (kind \"MoveTo\") (args (x 10.5) (y -20.0)))";
            var parser = new BridgeIrParser(ir);
            var parsed = (ParsedIntent)parser.Parse();

            Assert.Equal("test-1", parsed.Id);
            Assert.Equal("public/oan/move.commit", parsed.SliHandle);
            Assert.Equal("MoveTo", parsed.Kind);
            Assert.Equal(10.5, parsed.X);
            Assert.Equal(-20.0, parsed.Y);
        }

        [Fact]
        public void Rejects_UnknownKind()
        {
            string ir = "(oan.intent (id \"test-1\") (sli \"public/oan/move.commit\") (kind \"Attack\") (args (x 10)))";
            var parser = new BridgeIrParser(ir);
            var ex = Assert.Throws<BridgeIrException>(() => parser.Parse());
            Assert.Equal(BridgeIrErrorCode.UNSUPPORTED_KIND, ex.ReasonCode);
        }

        [Fact]
        public void Rejects_DuplicateFields()
        {
            string ir = "(oan.intent (id \"1\") (id \"2\") (sli \"h\") (kind \"MoveTo\"))";
            var parser = new BridgeIrParser(ir);
            var ex = Assert.Throws<BridgeIrException>(() => parser.Parse());
            Assert.Equal(BridgeIrErrorCode.DUPLICATE_FIELD, ex.ReasonCode);
        }

        [Fact]
        public void Rejects_NaN()
        {
             string ir = "(oan.intent (id \"1\") (sli \"h\") (kind \"MoveTo\") (args (x NaN) (y 0)))";
             var parser = new BridgeIrParser(ir);
             var ex = Assert.Throws<BridgeIrException>(() => parser.Parse());
             Assert.Equal(BridgeIrErrorCode.NAN_OR_INF, ex.ReasonCode);
        }
    }
}
