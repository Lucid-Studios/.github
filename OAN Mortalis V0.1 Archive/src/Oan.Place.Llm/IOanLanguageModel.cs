using System.Threading;
using System.Threading.Tasks;

namespace Oan.Place.Llm
{
    public interface IOanLanguageModel
    {
        Task<string> ProposeAsync(string prompt, CancellationToken ct = default);
        string ModelId { get; }
    }
}
