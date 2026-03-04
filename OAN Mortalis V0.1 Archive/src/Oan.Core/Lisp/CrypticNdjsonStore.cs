using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Oan.Core.Lisp
{
    /// <summary>
    /// Append-only NDJSON cryptic store (v0.1).
    /// One emission per line, canonical JSON, '\n' newline.
    /// </summary>
    public sealed class CrypticNdjsonStore : ICrypticStore
    {
        private readonly string _path;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public CrypticNdjsonStore(string path)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
            _path = path;
        }

        public async Task<string> AppendAsync(CrypticEmission emission, CancellationToken ct = default)
        {
            if (emission == null) throw new ArgumentNullException(nameof(emission));

            // 1) Canonicalize emission (single source of truth for bit-stability and tier casing)
            string json = CrypticCanonicalizer.SerializeEmission(emission);

            // 2) Append line (NDJSON)
            // Use UTF8 w/o BOM; newline must be '\n'
            byte[] bytes = Encoding.UTF8.GetBytes(json + "\n");

            // Ensure directory exists
            string? dir = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

            await _lock.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                // Append-only
                using (var fs = new FileStream(_path, FileMode.Append, FileAccess.Write, FileShare.Read, 4096, useAsync: true))
                {
                    await fs.WriteAsync(bytes, 0, bytes.Length, ct).ConfigureAwait(false);
                    await fs.FlushAsync(ct).ConfigureAwait(false);
                }
            }
            finally
            {
                _lock.Release();
            }

            // 3) Return canonical pointer (CGoA/<hash>)
            return CrypticPointerHelper.ComputeCGoAPtr(emission);
        }
    }
}
