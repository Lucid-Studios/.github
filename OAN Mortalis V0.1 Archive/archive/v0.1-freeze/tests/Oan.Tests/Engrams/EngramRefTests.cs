using System;
using Oan.Core.Engrams;
using Xunit;

namespace Oan.Tests.Engrams
{
    public class EngramRefTests
    {
        [Theory]
        [InlineData("ValidRel", "ValidId")]
        [InlineData("Link", "123")]
        [InlineData("Parent", "abc-def")]
        public void EngramRefCodec_AcceptsValidTokens(string rel, string id)
        {
            var r = new EngramRef { Relationship = rel, TargetId = id };
            var token = EngramRefCodec.Format(r);
            Assert.Equal($"{rel}:{id}", token);
        }

        [Theory]
        [InlineData("Bad:Rel", "Id")]
        [InlineData("Rel", "Bad:Id")]
        [InlineData("Bad\nRel", "Id")]
        [InlineData("Rel", "Bad\nId")]
        [InlineData("Bad|Rel", "Id")]
        [InlineData("Rel", "Bad|Id")]
        public void EngramRefCodec_RejectsInvalidChars(string rel, string id)
        {
            var r = new EngramRef { Relationship = rel, TargetId = id };
            Assert.Throws<ArgumentException>(() => EngramRefCodec.Format(r));
        }

        [Theory]
        [InlineData("", "Id")]
        [InlineData("Rel", "")]
        [InlineData("   ", "Id")]
        [InlineData("Rel", "   ")]
        public void EngramRefCodec_RejectsEmptyOrWhitespace(string rel, string id)
        {
            var r = new EngramRef { Relationship = rel, TargetId = id };
            Assert.Throws<ArgumentException>(() => EngramRefCodec.Format(r));
        }

        [Fact]
        public void EngramRefCodec_RejectsNullProperties()
        {
            // Test explicit nulls (which might not happen with C# 8 non-nullable without warnings, but runtime possible)
            var r1 = new EngramRef { Relationship = null!, TargetId = "Id" };
            Assert.Throws<ArgumentException>(() => EngramRefCodec.Format(r1));

            var r2 = new EngramRef { Relationship = "Rel", TargetId = null! };
            Assert.Throws<ArgumentException>(() => EngramRefCodec.Format(r2));
        }
    }
}
