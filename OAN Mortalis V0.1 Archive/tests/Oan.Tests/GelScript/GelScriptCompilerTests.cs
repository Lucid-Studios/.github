using System;
using System.Linq;
using Oan.Place.GelScript;
using Oan.Place.Llm.BridgeIr; // Referenced for validation
using Xunit;

namespace Oan.Tests.GelScript
{
    public class GelScriptCompilerTests
    {
        [Fact]
        public void Compile_VarAndMoveTo_ProducesValidBridgeIr()
        {
            var compiler = new GelScriptCompiler();
            string source = @"
var x = 10
var y = 20
MoveTo(x, y)
";
            var result = compiler.Compile(source);

            Assert.True(result.Success, "Compilation failed: " + string.Join(", ", result.Errors));
            Assert.Single(result.BridgeIrOps);

            string ir = result.BridgeIrOps[0];
            // Format should be (oan.intent (id "...") (sli "...") (kind "MoveTo") (args (x "10") (y "20"))) or similar
            Assert.StartsWith("(oan.intent", ir);
            Assert.Contains("(kind \"MoveTo\")", ir);
            Assert.Contains("(x \"10\")", ir);
            Assert.Contains("(y \"20\")", ir);

            // Validation via Parser
            var parser = new BridgeIrParser(ir);
            var parsedObj = parser.Parse();
            Assert.NotNull(parsedObj);
            var intent = Assert.IsType<ParsedIntent>(parsedObj);
            Assert.Equal("MoveTo", intent.Kind);
            // x and y are treated as args in the IR I generated: (args (x "10") (y "20"))
            // BridgeIrParser maps args to Parameters, and specific keys x/y to X/Y properties if they exist?
            // Let's check BridgeIrParser source again:
            // "if (key == "x" ... result.X = dvx;"
            
            Assert.Equal(10.0, intent.X);
            Assert.Equal(20.0, intent.Y);
        }

        [Fact]
        public void Compile_Say_WithQuotedString()
        {
            var compiler = new GelScriptCompiler();
            string source = "Say(\"Hello World\")";
            var result = compiler.Compile(source);
            
            Assert.True(result.Success);
            string ir = result.BridgeIrOps[0];
            Assert.Contains("(kind \"Say\")", ir);
            Assert.Contains("(text \"Hello World\")", ir);

            var parser = new BridgeIrParser(ir);
            var intent = (ParsedIntent)parser.Parse();
            Assert.Equal("Say", intent.Kind);
            Assert.Equal("Hello World", intent.Parameters["text"]);
        }
        
        [Fact]
        public void Compile_UnknownFunction_DefaultsToArgs()
        {
            var compiler = new GelScriptCompiler();
            string source = "UnknownFunc(123, \"abc\")";
            var result = compiler.Compile(source);

            Assert.True(result.Success);
            string ir = result.BridgeIrOps[0];
            
            // Should produce (kind "UnknownFunc") (args (arg0 "123") (arg1 "abc"))
            Assert.Contains("(kind \"UnknownFunc\")", ir);
            Assert.Contains("(arg0 \"123\")", ir);
            Assert.Contains("(arg1 \"abc\")", ir);

            // Note: BridgeIrParser validation checks _validKinds ("MoveTo", "Say", "Emote", "LookAt", "Interact", "Stop")
            // So parsing this *should* throw UNSUPPORTED_KIND.
            // This confirms our compiler is generic, but the runtime parser is strict.
            
            var parser = new BridgeIrParser(ir);
            var ex = Assert.Throws<BridgeIrException>(() => parser.Parse());
            Assert.Equal(BridgeIrErrorCode.UNSUPPORTED_KIND, ex.ReasonCode);
        }
    }
}

