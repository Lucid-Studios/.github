using System;
using System.Collections.Generic;
using Xunit;
using Oan.Core.Lisp;

namespace Oan.Tests.Lisp
{
    public class LispHasherTests
    {
        [Fact]
        public void Hash_CanonicalString_IsLowerHex()
        {
            // canonicalJson = "{\"op\":\"seal\"}"
            var json = "{\"op\":\"seal\"}";
            var hash = LispHasher.Sha256HexUtf8(json);

            Assert.Equal(64, hash.Length);
            Assert.Matches("^[0-9a-f]{64}$", hash);
        }

        [Fact]
        public void HashForm_UsesCanonicalizer()
        {
            // Create LispForm op="seal", empty args
            var form = new LispForm { op = "seal", args = new Dictionary<string, object>() };
            var expectedJson = "{\"op\":\"seal\"}";
            
            var hash = LispHasher.HashForm(form);
            var expectedHash = LispHasher.Sha256HexUtf8(expectedJson);

            Assert.Equal(expectedHash, hash);
        }

        [Fact]
        public void HashReceipt_OmitsNullNotes()
        {
            var receipt = new TransformReceipt
            {
                id = "IA",
                version = "1",
                in_hash = "h1",
                out_hash = "h2",
                rationale_code = "SAFE_EXPANSION",
                notes = null
            };

            // Expected keys ordinal: id, in_hash, out_hash, rationale_code, version
            var expectedJson = "{\"id\":\"IA\",\"in_hash\":\"h1\",\"out_hash\":\"h2\",\"rationale_code\":\"SAFE_EXPANSION\",\"version\":\"1\"}";
            
            var hash = LispHasher.HashReceipt(receipt);
            var expectedHash = LispHasher.Sha256HexUtf8(expectedJson);

            Assert.Equal(expectedHash, hash);
        }

        [Fact]
        public void HashReceipt_IncludesNotesWhenPresent()
        {
            var receiptWithNotes = new TransformReceipt
            {
                id = "IA",
                version = "1",
                in_hash = "h1",
                out_hash = "h2",
                rationale_code = "SAFE_EXPANSION",
                notes = "x"
            };

            // Expected keys ordinal: id, in_hash, notes (n), out_hash (o), rationale_code, version
            var expectedJson = "{\"id\":\"IA\",\"in_hash\":\"h1\",\"notes\":\"x\",\"out_hash\":\"h2\",\"rationale_code\":\"SAFE_EXPANSION\",\"version\":\"1\"}";
            
            var hash = LispHasher.HashReceipt(receiptWithNotes);
            var expectedHash = LispHasher.Sha256HexUtf8(expectedJson);

            // Verify difference from the one without notes
            var receiptNoNotes = new TransformReceipt
            {
                id = "IA",
                version = "1",
                in_hash = "h1",
                out_hash = "h2",
                rationale_code = "SAFE_EXPANSION"
            };
            
            Assert.Equal(expectedHash, hash);
            Assert.NotEqual(LispHasher.HashReceipt(receiptNoNotes), hash);
        }

        [Fact]
        public void HashReceiptChain_PreservesOrder()
        {
            var hashesA = new List<string> { "aa", "bb" };
            var hashesB = new List<string> { "bb", "aa" };

            var hashA = LispHasher.HashReceiptChain(hashesA);
            var hashB = LispHasher.HashReceiptChain(hashesB);

            Assert.NotEqual(hashA, hashB);
        }

        [Fact]
        public void HashReceipt_MandatoryFields_Required()
        {
            var receipt = new TransformReceipt { id = "" }; // missing others
            Assert.Throws<InvalidOperationException>(() => LispHasher.HashReceipt(receipt));
        }

        [Fact]
        public void HashChain_Alias_MatchesReceiptChain()
        {
            var hashes = new List<string> { "aa", "bb" };
            Assert.Equal(LispHasher.HashReceiptChain(hashes), LispHasher.HashChain(hashes));
        }

        [Fact]
        public void HashReceiptSet_HashesAllInOrder()
        {
            var r1 = new TransformReceipt { id="IA", version="1", in_hash="h1", out_hash="h2", rationale_code="OK" };
            var r2 = new TransformReceipt { id="CA", version="1", in_hash="h2", out_hash="h3", rationale_code="OK" };
            var set = new List<TransformReceipt> { r1, r2 };

            var hashes = LispHasher.HashReceiptSet(set);
            
            Assert.Equal(2, hashes.Count);
            Assert.Equal(LispHasher.HashReceipt(r1), hashes[0]);
            Assert.Equal(LispHasher.HashReceipt(r2), hashes[1]);
        }

        [Fact]
        public void HashReceiptChain_UsesColonJoin()
        {
            var hashes = new List<string> { "aa", "bb" };
            var expectedHash = LispHasher.Sha256HexUtf8("aa:bb");

            var hash = LispHasher.HashReceiptChain(hashes);

            Assert.Equal(expectedHash, hash);
        }
    }
}
