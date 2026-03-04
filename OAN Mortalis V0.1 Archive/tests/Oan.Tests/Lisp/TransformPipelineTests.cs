using System;
using System.Collections.Generic;
using Xunit;
using Oan.Core.Lisp;

namespace Oan.Tests.Lisp
{
    public class TransformPipelineTests
    {
        private class MockTransform : IFormTransform
        {
            public string id { get; set; } = "MOCK";
            public string version { get; set; } = "1";
            public string rationale_code { get; set; } = "TEST";
            public Func<LispForm, LispForm> Logic { get; set; } = f => f;

            public LispForm Apply(LispForm input) => Logic(input);
        }

        [Fact]
        public void Run_EmptyPipeline_ProducesEmptyReceipts_StableChainHash()
        {
            var pipeline = new TransformPipeline(new List<IFormTransform>());
            var input = new LispForm { op = "nop" };
            
            var result = pipeline.Run(input);

            Assert.Same(input, result.sealed_form);
            Assert.Empty(result.receipts);
            Assert.Empty(result.receipt_hashes);
            
            string expectedEmptyChain = LispHasher.HashReceiptChain(Array.Empty<string>());
            Assert.Equal(expectedEmptyChain, result.chain_hash);
            Assert.Equal(LispHasher.HashForm(input), result.final_form_hash);
        }

        [Fact]
        public void Run_TwoTransforms_ProducesTwoReceipts_OrderSensitive()
        {
            var t1 = new MockTransform 
            { 
                id = "T1", 
                Logic = f => new LispForm { op = "b" } 
            };
            var t2 = new MockTransform 
            { 
                id = "T2", 
                Logic = f => new LispForm { op = "c" } 
            };

            var input = new LispForm { op = "a" };
            
            var pipeline12 = new TransformPipeline(new List<IFormTransform> { t1, t2 });
            var res12 = pipeline12.Run(input);

            var pipeline21 = new TransformPipeline(new List<IFormTransform> { t2, t1 });
            var res21 = pipeline21.Run(input);

            Assert.Equal(2, res12.receipts.Count);
            Assert.Equal(2, res12.receipt_hashes.Count);
            Assert.NotEqual(res12.chain_hash, res21.chain_hash);
            
            Assert.Equal("c", res12.sealed_form.op);
            // res21: a -> (T2 adds 'c' to a? No, T2 replaces op with 'c')
            // T2: a -> c
            // T1: c -> b
            Assert.Equal("b", res21.sealed_form.op);
        }

        [Fact]
        public void Run_ReceiptHashesMatchReceiptContent()
        {
            var t1 = new MockTransform { id = "STEP_1" };
            var pipeline = new TransformPipeline(new List<IFormTransform> { t1 });
            var result = pipeline.Run(new LispForm { op = "start" });

            Assert.Single(result.receipts);
            var expectedHash = LispHasher.HashReceipt(result.receipts[0]);
            Assert.Equal(expectedHash, result.receipt_hashes[0]);
        }

        [Fact]
        public void Run_FinalFormHashMatches()
        {
            var t1 = new MockTransform { id = "T", Logic = f => new LispForm { op = "end" } };
            var pipeline = new TransformPipeline(new List<IFormTransform> { t1 });
            var result = pipeline.Run(new LispForm { op = "start" });

            Assert.Equal("end", result.sealed_form.op);
            Assert.Equal(LispHasher.HashForm(result.sealed_form), result.final_form_hash);
        }
        [Fact]
        public void Run_IaNormalizeTransform_ProducesValidReceipt()
        {
            var transform = new IaNormalizeFormTransform();
            var pipeline = new TransformPipeline(new List<IFormTransform> { transform });
            var input = new LispForm { op = "  read  " };

            var result = pipeline.Run(input);

            Assert.Single(result.receipts);
            var receipt = result.receipts[0];

            Assert.Equal("IA_NORMALIZE_FORM", receipt.id);
            Assert.Equal("IA_CANONICAL_SHAPE", receipt.rationale_code);
            Assert.Equal("read", result.sealed_form.op);

            // Verify hash stability
            Assert.Equal(LispHasher.HashForm(input), receipt.in_hash);
            Assert.Equal(LispHasher.HashForm(result.sealed_form), receipt.out_hash);
            Assert.Equal(LispHasher.HashForm(result.sealed_form), result.final_form_hash);
            
            var expectedChain = LispHasher.HashReceiptChain(new[] { LispHasher.HashReceipt(receipt) });
            Assert.Equal(expectedChain, result.chain_hash);
        }
    }
}
