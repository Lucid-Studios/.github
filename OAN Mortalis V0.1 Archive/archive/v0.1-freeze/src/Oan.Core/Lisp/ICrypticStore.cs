using System.Threading;
using System.Threading.Tasks;

namespace Oan.Core.Lisp
{
    /// <summary>
    /// Append-only cryptic storage. Deterministic record emission.
    /// </summary>
    public interface ICrypticStore
    {
        /// <summary>
        /// Appends an emission to storage and returns its canonical pointer (e.g., "cGoA/<hash>").
        /// Must be append-only.
        /// </summary>
        Task<string> AppendAsync(CrypticEmission emission, CancellationToken ct = default);
    }
}
