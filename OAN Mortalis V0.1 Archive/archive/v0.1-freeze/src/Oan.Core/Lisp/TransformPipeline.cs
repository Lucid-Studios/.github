using System;
using System.Collections.Generic;

namespace Oan.Core.Lisp
{
    /// <summary>
    /// Result of a transform pipeline execution.
    /// </summary>
    public sealed class TransformPipelineResult
    {
        public LispForm sealed_form { get; set; } = new LispForm();
        public IReadOnlyList<TransformReceipt> receipts { get; set; } = new List<TransformReceipt>();
        public IReadOnlyList<string> receipt_hashes { get; set; } = new List<string>();
        public string chain_hash { get; set; } = string.Empty;
        public string final_form_hash { get; set; } = string.Empty;
    }

    /// <summary>
    /// Orchestrates a sequence of deterministic symbolic transforms.
    /// Produces a verifiable audit trail of receipts and hashes.
    /// </summary>
    public sealed class TransformPipeline
    {
        private readonly IReadOnlyList<IFormTransform> _transforms;

        public TransformPipeline(IReadOnlyList<IFormTransform> transforms)
        {
            if (transforms == null) throw new ArgumentNullException(nameof(transforms));
            
            foreach (var t in transforms)
            {
                if (t == null) throw new ArgumentException("Transform entry cannot be null.");
                if (string.IsNullOrEmpty(t.id)) throw new ArgumentException("Transform ID cannot be empty.");
                if (string.IsNullOrEmpty(t.version)) throw new ArgumentException("Transform Version cannot be empty.");
                if (string.IsNullOrEmpty(t.rationale_code)) throw new ArgumentException("Transform RationaleCode cannot be empty.");
            }

            _transforms = transforms;
        }

        /// <summary>
        /// Executes the pipeline on a Lisp form.
        /// </summary>
        public TransformPipelineResult Run(LispForm input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            LispForm current = input;
            var receipts = new List<TransformReceipt>();
            var receiptHashes = new List<string>();

            foreach (var transform in _transforms)
            {
                string inHash = LispHasher.HashForm(current);
                
                // Execute transform
                LispForm next = transform.Apply(current);
                if (next == null) throw new InvalidOperationException("TRANSFORM_RETURNED_NULL: " + transform.id);
                
                string outHash = LispHasher.HashForm(next);

                // Create deterministic receipt
                var receipt = new TransformReceipt
                {
                    id = transform.id,
                    version = transform.version,
                    rationale_code = transform.rationale_code,
                    in_hash = inHash,
                    out_hash = outHash,
                    notes = null // Always null in this sprint
                };

                string receiptHash = LispHasher.HashReceipt(receipt);

                receipts.Add(receipt);
                receiptHashes.Add(receiptHash);

                current = next;
            }

            string finalFormHash = LispHasher.HashForm(current);
            string chainHash = LispHasher.HashReceiptChain(receiptHashes);

            return new TransformPipelineResult
            {
                sealed_form = current,
                receipts = receipts,
                receipt_hashes = receiptHashes,
                chain_hash = chainHash,
                final_form_hash = finalFormHash
            };
        }
    }
}
