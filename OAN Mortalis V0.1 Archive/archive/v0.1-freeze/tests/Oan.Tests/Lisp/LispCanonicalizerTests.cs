using System;
using System.Collections.Generic;
using Xunit;
using Oan.Core.Lisp;

namespace Oan.Tests.Lisp
{
    public class LispCanonicalizerTests
    {
        [Fact]
        public void Serialize_MinimalSeal_OmitsArgs()
        {
            // Input: new LispForm { op="seal", args=empty }
            var form = new LispForm 
            { 
                op = "seal", 
                args = new Dictionary<string, object>() 
            };

            var json = LispCanonicalizer.SerializeForm(form);

            // Expected EXACT: {"op":"seal"}
            Assert.Equal("{\"op\":\"seal\"}", json);
        }

        [Fact]
        public void Serialize_MoveOrdering_SortedOrdinal()
        {
            // Input args: {"dest":"lab","a":"x"}
            var form = new LispForm
            {
                op = "move",
                args = new Dictionary<string, object>
                {
                    { "dest", "lab" },
                    { "a", "x" }
                }
            };

            var json = LispCanonicalizer.SerializeForm(form);

            // Expected EXACT: {"args":{"a":"x","dest":"lab"},"op":"move"}
            // Note: "args" comes before "op" alphabetically.
            Assert.Equal("{\"args\":{\"a\":\"x\",\"dest\":\"lab\"},\"op\":\"move\"}", json);
        }

        [Fact]
        public void Serialize_NestedOrdering_Recursive()
        {
            // Input args: {"drift": {"s":1000000L,"v":1200000L}, "z": "y"}
            var form = new LispForm
            {
                op = "check",
                args = new Dictionary<string, object>
                {
                    { "drift", new Dictionary<string, object> { { "s", 1000000L }, { "v", 1200000L } } },
                    { "z", "y" }
                }
            };

            var json = LispCanonicalizer.SerializeForm(form);

            // Expected EXACT: {"args":{"drift":{"s":1000000,"v":1200000},"z":"y"},"op":"check"}
            Assert.Equal("{\"args\":{\"drift\":{\"s\":1000000,\"v\":1200000},\"z\":\"y\"},\"op\":\"check\"}", json);
        }

        [Fact]
        public void Serialize_BadScale_Throws()
        {
            // Input args drift: {"s", 100L}, {"v", 1L}
            var form = new LispForm
            {
                op = "check",
                args = new Dictionary<string, object>
                {
                    { "drift", new Dictionary<string, object> { { "s", 100L }, { "v", 1L } } }
                }
            };

            var ex = Assert.Throws<InvalidOperationException>(() => LispCanonicalizer.SerializeForm(form));
            Assert.Contains("BAD_SCALE", ex.Message);
        }
    }
}
