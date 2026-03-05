using EngramGovernance.Models;
using EngramGovernance.Services;

namespace AgentiCore.Services;

public sealed class EngramCommitService
{
    private readonly StewardAgent _stewardAgent;

    public EngramCommitService(StewardAgent stewardAgent)
    {
        _stewardAgent = stewardAgent;
    }

    public async Task CommitAsync(
        string cmeId,
        Guid soulFrameId,
        Guid contextId,
        string cognitionBody,
        IReadOnlyDictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(cognitionBody);

        var metadataBuffer = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["source"] = "AgentiCore",
            ["context"] = contextId.ToString("D"),
            ["captured_at"] = DateTime.UtcNow.ToString("O")
        };

        if (metadata is not null)
        {
            foreach (var pair in metadata)
            {
                if (!string.IsNullOrWhiteSpace(pair.Key) && !string.IsNullOrWhiteSpace(pair.Value))
                {
                    metadataBuffer[pair.Key] = pair.Value;
                }
            }
        }

        var candidate = new EngramCandidate
        {
            CandidateId = Guid.NewGuid(),
            CMEId = cmeId,
            SoulFrameId = soulFrameId,
            ContextId = contextId,
            CognitionBody = cognitionBody,
            Metadata = metadataBuffer,
            Timestamp = DateTime.UtcNow
        };

        await _stewardAgent.ProcessCandidateAsync(candidate, cancellationToken).ConfigureAwait(false);
    }
}
