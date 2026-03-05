using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using EngramGovernance.Models;
using OAN.Core.Telemetry;
using Telemetry.GEL;

namespace EngramGovernance.Services;

public sealed class LedgerWriter
{
    private readonly ConcurrentDictionary<string, List<OEDecisionEntry>> _decisionChains = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, object> _chainLocks = new(StringComparer.Ordinal);
    private readonly List<RootEngramRecord> _rootEngramLedger = [];
    private readonly List<ConstructorEngramRecord> _constructorEngramLedger = [];
    private readonly object _rootLedgerLock = new();
    private readonly object _constructorLedgerLock = new();
    private readonly GelTelemetryAdapter _telemetry;

    public LedgerWriter(GelTelemetryAdapter telemetry)
    {
        _telemetry = telemetry;
    }

    public async Task<EngramRecord> AppendDecisionAsync(
        OEDecisionEntry decisionEntry,
        string storagePointer,
        string decisionSpline,
        string symbolicTrace,
        EngramCompassState compassState,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(decisionEntry);
        ArgumentException.ThrowIfNullOrWhiteSpace(storagePointer);
        ArgumentException.ThrowIfNullOrWhiteSpace(decisionSpline);
        ArgumentException.ThrowIfNullOrWhiteSpace(symbolicTrace);
        ArgumentNullException.ThrowIfNull(compassState);

        var chain = _decisionChains.GetOrAdd(decisionEntry.CMEId, _ => []);
        var chainLock = _chainLocks.GetOrAdd(decisionEntry.CMEId, _ => new object());
        OEDecisionEntry immutableEntry;
        long ledgerIndex;

        lock (chainLock)
        {
            immutableEntry = new OEDecisionEntry
            {
                DecisionId = decisionEntry.DecisionId,
                CMEId = decisionEntry.CMEId,
                SoulFrameId = decisionEntry.SoulFrameId,
                ContextId = decisionEntry.ContextId,
                Classification = decisionEntry.Classification,
                BodyHash = decisionEntry.BodyHash,
                Timestamp = decisionEntry.Timestamp
            };

            chain.Add(immutableEntry);
            ledgerIndex = chain.Count - 1L;
        }

        var timestamp = DateTime.UtcNow;
        ITelemetryEvent telemetryEvent = new GovernanceTelemetryEvent
        {
            EventHash = HashHex($"{immutableEntry.DecisionId:D}|ledger-commit-success|{immutableEntry.CMEId}|{ledgerIndex}"),
            Timestamp = timestamp
        };

        await _telemetry.AppendAsync(telemetryEvent, "ledger-commit-success", cancellationToken).ConfigureAwait(false);

        return new EngramRecord
        {
            DecisionEntry = immutableEntry,
            StoragePointer = storagePointer,
            LedgerIndex = ledgerIndex,
            DecisionSpline = decisionSpline,
            SymbolicTrace = symbolicTrace,
            CompassState = compassState
        };
    }

    public IReadOnlyList<OEDecisionEntry> GetDecisionChainSnapshot(string cmeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        if (!_decisionChains.TryGetValue(cmeId, out var chain))
        {
            return [];
        }

        var chainLock = _chainLocks.GetOrAdd(cmeId, _ => new object());
        lock (chainLock)
        {
            return chain.ToList();
        }
    }

    public async Task<long> AppendRootEngramAsync(
        RootEngramRecord rootEngram,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rootEngram);

        long ledgerIndex;
        lock (_rootLedgerLock)
        {
            _rootEngramLedger.Add(rootEngram);
            ledgerIndex = _rootEngramLedger.Count - 1L;
        }

        ITelemetryEvent telemetryEvent = new GovernanceTelemetryEvent
        {
            EventHash = HashHex($"{rootEngram.SymbolicId}|root-engram-commit|{ledgerIndex}"),
            Timestamp = DateTime.UtcNow
        };

        await _telemetry.AppendAsync(telemetryEvent, "root-engram-commit", cancellationToken).ConfigureAwait(false);
        return ledgerIndex;
    }

    public async Task<long> AppendConstructorEngramAsync(
        ConstructorEngramRecord constructorEngram,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(constructorEngram);

        long ledgerIndex;
        lock (_constructorLedgerLock)
        {
            _constructorEngramLedger.Add(constructorEngram);
            ledgerIndex = _constructorEngramLedger.Count - 1L;
        }

        ITelemetryEvent telemetryEvent = new GovernanceTelemetryEvent
        {
            EventHash = HashHex($"{constructorEngram.RootReference}|constructor-engram-commit|{ledgerIndex}"),
            Timestamp = DateTime.UtcNow
        };

        await _telemetry.AppendAsync(telemetryEvent, "constructor-engram-commit", cancellationToken).ConfigureAwait(false);
        return ledgerIndex;
    }

    public IReadOnlyList<RootEngramRecord> GetRootEngramLedgerSnapshot()
    {
        lock (_rootLedgerLock)
        {
            return _rootEngramLedger.ToList();
        }
    }

    public IReadOnlyList<ConstructorEngramRecord> GetConstructorEngramLedgerSnapshot()
    {
        lock (_constructorLedgerLock)
        {
            return _constructorEngramLedger.ToList();
        }
    }

    private static string HashHex(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }
}

internal sealed class GovernanceTelemetryEvent : ITelemetryEvent
{
    public required string EventHash { get; init; }
    public required DateTime Timestamp { get; init; }
}
