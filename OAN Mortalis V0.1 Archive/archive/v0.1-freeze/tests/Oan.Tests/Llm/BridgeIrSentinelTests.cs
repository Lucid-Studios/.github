using System;
using System.Collections.Generic;
using System.IO;
using Oan.Core;
using Oan.Place.Llm.BridgeIr;
using Xunit;

namespace Oan.Tests.Llm
{
    public class BridgeIrSentinelTests
    {
        private readonly string _fixturesDir;

        public BridgeIrSentinelTests()
        {
            // Base path for fixtures assuming execution from project root or test bin
            _fixturesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Llm", "Fixtures");
            
            // If running via dotnet test, we might be in bin/Debug/net8.0/
            // Let's also check if we can find it relative to current directory
            if (!Directory.Exists(_fixturesDir))
            {
                _fixturesDir = Path.Combine(Directory.GetCurrentDirectory(), "tests", "Oan.Tests", "Llm", "Fixtures");
            }
            
            // Fallback for direct absolute path during implementation
            if (!Directory.Exists(_fixturesDir))
            {
                _fixturesDir = @"d:\Unity Projects\Game Design OAN\OAN Mortalis\tests\Oan.Tests\Llm\Fixtures\";
            }
        }

        [Fact]
        public void Golden_Parse_Valid_MoveTo()
        {
            string path = Path.Combine(_fixturesDir, "move_to_valid.ir.txt");
            string ir = File.ReadAllText(path);
            
            var parser = new BridgeIrParser(ir);
            var parsed = (ParsedIntent)parser.Parse();

            Assert.Equal("llm-stub-1", parsed.Id);
            Assert.Equal("public/oan/move.commit", parsed.SliHandle);
            Assert.Equal("MoveTo", parsed.Kind);
            Assert.Equal(50.0, parsed.X);
            Assert.Equal(-10.0, parsed.Y);
        }

        [Fact]
        public void Golden_Compile_Valid_MoveTo()
        {
            string path = Path.Combine(_fixturesDir, "move_to_valid.ir.txt");
            string ir = File.ReadAllText(path);
            
            var parser = new BridgeIrParser(ir);
            var parsed = (ParsedIntent)parser.Parse();
            var intent = BridgeIrCompiler.Compile(parsed, "sentinel-model");

            Assert.Equal("MoveTo", intent.Action);
            Assert.Equal("public/oan/move.commit", intent.SliHandle);
            
            // Assert TargetPosition nesting
            Assert.True(intent.Parameters.ContainsKey("TargetPosition"));
            var pos = intent.Parameters["TargetPosition"] as Dictionary<string, object>;
            Assert.NotNull(pos);
            Assert.Equal(50.0, pos["X"]);
            Assert.Equal(-10.0, pos["Y"]);
        }

        [Fact]
        public void Golden_Parse_Prose_Rejected_WithStableCode()
        {
            string path = Path.Combine(_fixturesDir, "move_to_invalid_prose.txt");
            string ir = File.ReadAllText(path);
            
            var parser = new BridgeIrParser(ir);
            var ex = Assert.Throws<BridgeIrException>(() => parser.Parse());
            Assert.Equal(BridgeIrErrorCode.PROSE_INPUT, ex.ReasonCode);
        }

        [Fact]
        public void Golden_UnknownField_Rejected_WithStableCode()
        {
            string ir = "(oan.intent (id \"1\") (sli \"h\") (kind \"MoveTo\") (hacker \"x\"))";
            var parser = new BridgeIrParser(ir);
            var ex = Assert.Throws<BridgeIrException>(() => parser.Parse());
            Assert.Equal(BridgeIrErrorCode.UNKNOWN_FIELD, ex.ReasonCode);
        }

        [Fact]
        public void Golden_NaN_Rejected_WithStableCode()
        {
            string ir = "(oan.intent (id \"1\") (sli \"h\") (kind \"MoveTo\") (args (x NaN) (y 0)))";
            var parser = new BridgeIrParser(ir);
            var ex = Assert.Throws<BridgeIrException>(() => parser.Parse());
            Assert.Equal(BridgeIrErrorCode.NAN_OR_INF, ex.ReasonCode);
        }
    }
}
