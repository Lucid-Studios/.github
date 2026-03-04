using System.Security.Cryptography;
using System.Text;
using SoulFrame.Identity.Models;
using SoulFrameModel = SoulFrame.Identity.Models.SoulFrame;

namespace SoulFrame.Identity.Services;

public sealed class SoulFrameFactory
{
    public SoulFrameModel Create(
        string cmeId,
        string opalEngramId,
        string operatorBondReference,
        RuntimePolicy runtimePolicy = RuntimePolicy.Default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(opalEngramId);
        ArgumentException.ThrowIfNullOrWhiteSpace(operatorBondReference);

        var soulFrameId = CreateDeterministicGuid($"{cmeId}|{opalEngramId}");

        return new SoulFrameModel
        {
            SoulFrameId = soulFrameId,
            CMEId = cmeId.Trim(),
            OpalEngramId = opalEngramId.Trim(),
            CreationTimestamp = DateTime.UtcNow,
            RuntimePolicy = runtimePolicy,
            OperatorBondReference = operatorBondReference.Trim(),
            SelfGelReference = $"selfgel:{opalEngramId.Trim()}",
            cSelfGelReference = $"cselfgel:{opalEngramId.Trim()}"
        };
    }

    public SoulFrameState CreateInitialState(SoulFrameModel soulFrame)
    {
        ArgumentNullException.ThrowIfNull(soulFrame);
        return new SoulFrameState
        {
            SoulFrameId = soulFrame.SoulFrameId,
            LastActiveTimestamp = soulFrame.CreationTimestamp,
            RuntimeAttachmentState = RuntimeAttachmentState.Detached,
            IntegrityHash = ComputeIntegrityHash(soulFrame)
        };
    }

    public static string ComputeIntegrityHash(SoulFrameModel soulFrame)
    {
        var payload = string.Join("|",
            soulFrame.SoulFrameId.ToString("D"),
            soulFrame.CMEId,
            soulFrame.OpalEngramId,
            soulFrame.RuntimePolicy.ToString(),
            soulFrame.OperatorBondReference,
            soulFrame.SelfGelReference,
            soulFrame.cSelfGelReference);

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static Guid CreateDeterministicGuid(string seed)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(seed));
        Span<byte> guidBytes = stackalloc byte[16];
        hash.AsSpan(0, 16).CopyTo(guidBytes);
        return new Guid(guidBytes);
    }
}
