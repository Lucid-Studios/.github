using CradleTek.Memory.Models;

namespace CradleTek.Memory.Interfaces;

public interface IRootOntologicalCleaver
{
    Task<OntologicalCleaverResult> CleaveAsync(string inputText, CancellationToken cancellationToken = default);
}
