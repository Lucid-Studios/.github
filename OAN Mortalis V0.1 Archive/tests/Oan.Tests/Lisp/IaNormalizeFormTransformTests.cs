using System.Collections.Generic;
using Xunit;
using Oan.Core.Lisp;

namespace Oan.Tests.Lisp
{
    public class IaNormalizeFormTransformTests
    {
        [Fact]
        public void Apply_NoMutation_WhenInputChanges()
        {
            var transform = new IaNormalizeFormTransform();
            var input = new LispForm { op = "  read  " };
            
            var output = transform.Apply(input);

            Assert.NotSame(input, output);
            Assert.Equal("  read  ", input.op);
            Assert.Equal("read", output.op);
        }

        [Fact]
        public void Apply_NormalizesOp_TrimsAndDefaultsToNop()
        {
            var transform = new IaNormalizeFormTransform();

            Assert.Equal("read", transform.Apply(new LispForm { op = "  read  " }).op);
            Assert.Equal("nop", transform.Apply(new LispForm { op = "" }).op);
            Assert.Equal("nop", transform.Apply(new LispForm { op = "   " }).op);
        }

        [Fact]
        public void Apply_Args_EnsuresNonNull()
        {
            var transform = new IaNormalizeFormTransform();
            var input = new LispForm { op = "test", args = null! }; // Bypass compiler for test

            var output = transform.Apply(input);

            Assert.NotNull(output.args);
            Assert.Empty(output.args);
        }

        [Fact]
        public void Apply_Meta_EmptyToNull()
        {
            var transform = new IaNormalizeFormTransform();
            var input = new LispForm 
            { 
                op = "test", 
                meta = new Dictionary<string, object>() 
            };

            var output = transform.Apply(input);

            Assert.Null(output.meta);
            Assert.NotNull(input.meta);
            Assert.Empty(input.meta);
        }

        [Fact]
        public void Apply_WhenNewInstanceCreated_DoesNotAliasArgsDictionary()
        {
            var transform = new IaNormalizeFormTransform();
            var input = new LispForm { op = "  read  ", args = new Dictionary<string, object> { ["k"] = 1 } };

            var output = transform.Apply(input);

            Assert.NotSame(input, output);
            Assert.NotSame(input.args, output.args);
            Assert.Equal(1, (int)output.args["k"]);
        }

        [Fact]
        public void Apply_ReturnsSameReference_IfNoChangesNeeded()
        {
            var transform = new IaNormalizeFormTransform();
            var input = new LispForm { op = "read", args = new Dictionary<string, object>() };

            var output = transform.Apply(input);

            Assert.Same(input, output);
        }
    }
}
