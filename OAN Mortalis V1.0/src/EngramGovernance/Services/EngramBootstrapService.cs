using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CradleTek.Host.Interfaces;
using CradleTek.Memory.Interfaces;
using CradleTek.Memory.Models;
using CradleTek.Memory.Services;
using EngramGovernance.Models;
using Telemetry.GEL;

namespace EngramGovernance.Services;

public sealed class EngramBootstrapService
{
    private readonly object _atlasLoadGate = new();
    private readonly IRootOntologicalCleaver _rootCleaver;
    private readonly LedgerWriter _ledgerWriter;
    private readonly IPublicStore _publicStore;
    private readonly GelTelemetryAdapter _telemetry;
    private readonly Dictionary<string, string> _rootSymbolByLexical = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _knownSymbolicTokens = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _generatedSymbols = new(StringComparer.OrdinalIgnoreCase);
    private bool _atlasLoaded;

    public EngramBootstrapService(
        LedgerWriter ledgerWriter,
        IPublicStore publicStore,
        GelTelemetryAdapter telemetry,
        IRootOntologicalCleaver? rootCleaver = null)
    {
        _ledgerWriter = ledgerWriter;
        _publicStore = publicStore;
        _telemetry = telemetry;
        _rootCleaver = rootCleaver ?? new RootAtlasOntologicalCleaver();
    }

    public async Task<EngramBootstrapResult> BootstrapAsync(
        EngramCandidate candidate,
        IReadOnlyList<string> sliTokens,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(sliTokens);
        EnsureAtlasLoaded();

        var tokenClassifications = new List<EngramTokenClassification>();
        var lexicalTokens = new List<string>();
        foreach (var rawToken in sliTokens.Where(t => !string.IsNullOrWhiteSpace(t)))
        {
            foreach (var token in ExpandToken(rawToken))
            {
                var normalized = NormalizeToken(token);
                if (string.IsNullOrWhiteSpace(normalized))
                {
                    continue;
                }

                if (IsLexicalToken(normalized))
                {
                    lexicalTokens.Add(normalized);
                    continue;
                }

                var symbolicHit = IsKnownSymbolicToken(token, normalized);
                tokenClassifications.Add(new EngramTokenClassification
                {
                    Token = token,
                    NormalizedToken = normalized,
                    Classification = symbolicHit
                        ? EngramLookupClassification.Known
                        : EngramLookupClassification.Absent,
                    ResolutionReason = symbolicHit ? "symbolic-index-hit" : "symbolic-index-miss"
                });
            }
        }

        var lexicalClassification = await ClassifyLexicalTokensAsync(lexicalTokens, cancellationToken).ConfigureAwait(false);
        tokenClassifications.AddRange(lexicalClassification.TokenClassifications);

        var contextDomain = InferContextDomain(candidate, sliTokens);
        var rootsCreated = new List<RootEngramRecord>();
        var constructorsCreated = new List<ConstructorEngramRecord>();

        foreach (var lexicalToken in lexicalClassification.AbsentLexicalTokens)
        {
            var symbolicId = AssignRootSymbol(lexicalToken);
            var rootRecord = new RootEngramRecord
            {
                SymbolicId = symbolicId,
                LexicalKey = lexicalToken,
                OntologicalClass = InferOntologicalClass(lexicalToken, contextDomain),
                GelDictionaryIndexPointer = $"gel://dictionary/{lexicalToken}",
                DiscoveryContext = contextDomain,
                Timestamp = DateTime.UtcNow
            };

            var rootLedgerIndex = await _ledgerWriter.AppendRootEngramAsync(rootRecord, cancellationToken).ConfigureAwait(false);
            await _publicStore.PublishPointerAsync(
                $"gel:root-engram:{symbolicId}:{rootLedgerIndex}",
                cancellationToken).ConfigureAwait(false);

            var rootTelemetry = BuildTelemetry("EngramCreated", "Root", symbolicId, contextDomain, ExtractTraceId(candidate.Metadata));
            await _telemetry.AppendAsync(rootTelemetry, "engram-root-created", cancellationToken).ConfigureAwait(false);

            var constructorRecord = BuildConstructorRecord(candidate, lexicalToken, symbolicId, contextDomain, sliTokens);
            var constructorLedgerIndex = await _ledgerWriter.AppendConstructorEngramAsync(constructorRecord, cancellationToken)
                .ConfigureAwait(false);
            await _publicStore.PublishPointerAsync(
                $"gel:constructor-engram:{symbolicId}:{constructorLedgerIndex}",
                cancellationToken).ConfigureAwait(false);

            var constructorTelemetry = BuildTelemetry(
                "EngramCreated",
                "Constructor",
                symbolicId,
                contextDomain,
                ExtractTraceId(candidate.Metadata));
            await _telemetry.AppendAsync(constructorTelemetry, "engram-constructor-created", cancellationToken).ConfigureAwait(false);

            rootsCreated.Add(rootRecord);
            constructorsCreated.Add(constructorRecord);
        }

        return new EngramBootstrapResult
        {
            TokenClassifications = tokenClassifications
                .OrderBy(classification => classification.NormalizedToken, StringComparer.Ordinal)
                .ThenBy(classification => classification.Token, StringComparer.Ordinal)
                .ToList(),
            RootEngramsCreated = rootsCreated,
            ConstructorEngramsCreated = constructorsCreated
        };
    }

    private async Task<(IReadOnlyList<EngramTokenClassification> TokenClassifications, IReadOnlyList<string> AbsentLexicalTokens)>
        ClassifyLexicalTokensAsync(
            IReadOnlyList<string> lexicalTokens,
            CancellationToken cancellationToken)
    {
        var uniqueTokens = lexicalTokens
            .Where(token => !string.IsNullOrWhiteSpace(token))
            .Select(NormalizeToken)
            .Where(token => !string.IsNullOrWhiteSpace(token))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (uniqueTokens.Count == 0)
        {
            return ([], []);
        }

        var result = await _rootCleaver.CleaveAsync(string.Join(' ', uniqueTokens), cancellationToken).ConfigureAwait(false);
        var byToken = result.Resolutions
            .GroupBy(resolution => resolution.NormalizedToken, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        var classifications = new List<EngramTokenClassification>();
        var absent = new List<string>();
        foreach (var token in uniqueTokens)
        {
            if (!byToken.TryGetValue(token, out var resolution))
            {
                classifications.Add(new EngramTokenClassification
                {
                    Token = token,
                    NormalizedToken = token,
                    Classification = EngramLookupClassification.Absent,
                    ResolutionReason = "no-resolution"
                });
                absent.Add(token);
                continue;
            }

            var classification = resolution.Classification switch
            {
                OntologicalCleaverClassification.Known => EngramLookupClassification.Known,
                OntologicalCleaverClassification.PartiallyKnown => EngramLookupClassification.Partial,
                _ => EngramLookupClassification.Absent
            };

            if (classification == EngramLookupClassification.Absent)
            {
                absent.Add(token);
            }

            classifications.Add(new EngramTokenClassification
            {
                Token = token,
                NormalizedToken = token,
                Classification = classification,
                ResolutionReason = resolution.ResolutionReason
            });
        }

        return (classifications, absent);
    }

    private ConstructorEngramRecord BuildConstructorRecord(
        EngramCandidate candidate,
        string lexicalToken,
        string symbolicId,
        string contextDomain,
        IReadOnlyList<string> sliTokens)
    {
        var symbolicStructure = ExtractSymbolicStructure(candidate, sliTokens);
        var constructorType = DetermineConstructorType(symbolicStructure);
        var predicateRules = BuildPredicateRules(symbolicStructure, symbolicId, lexicalToken);

        return new ConstructorEngramRecord
        {
            ConstructorType = constructorType,
            RootReference = symbolicId,
            SymbolicStructure = symbolicStructure,
            PredicateRules = predicateRules,
            ContextDomain = contextDomain,
            Provenance = ExtractMetadata(candidate.Metadata, "source", "EngramFormationProtocol"),
            Timestamp = DateTime.UtcNow
        };
    }

    private static ConstructorEngramType DetermineConstructorType(string symbolicStructure)
    {
        var depth = ComputeParenthesisDepth(symbolicStructure);
        if (depth >= 5)
        {
            return ConstructorEngramType.Master;
        }

        if (depth >= 4 || (symbolicStructure.Contains("⊗", StringComparison.Ordinal) && symbolicStructure.Contains("+", StringComparison.Ordinal)))
        {
            return ConstructorEngramType.Advanced;
        }

        if (symbolicStructure.Contains("=", StringComparison.Ordinal))
        {
            return ConstructorEngramType.Intermediate;
        }

        return ConstructorEngramType.Basic;
    }

    private static IReadOnlyList<string> BuildPredicateRules(string symbolicStructure, string symbolicId, string lexicalToken)
    {
        var predicates = new List<string>
        {
            $"anchor:{symbolicId}->{lexicalToken}"
        };

        if (symbolicStructure.Contains("=", StringComparison.Ordinal))
        {
            predicates.Add("preserve-equivalence-under-operation");
        }

        if (symbolicStructure.Contains("⊗", StringComparison.Ordinal) && symbolicStructure.Contains("+", StringComparison.Ordinal))
        {
            predicates.Add("distributive-candidate");
        }

        if (symbolicStructure.Contains("÷", StringComparison.Ordinal))
        {
            predicates.Add("inverse-operation");
        }

        if (predicates.Count == 1)
        {
            predicates.Add("contextual-anchor");
        }

        return predicates;
    }

    private static int ComputeParenthesisDepth(string symbolicStructure)
    {
        var depth = 0;
        var maxDepth = 0;
        foreach (var character in symbolicStructure)
        {
            if (character == '(')
            {
                depth++;
                maxDepth = Math.Max(maxDepth, depth);
                continue;
            }

            if (character == ')')
            {
                depth = Math.Max(0, depth - 1);
            }
        }

        return maxDepth;
    }

    private static string ExtractSymbolicStructure(EngramCandidate candidate, IReadOnlyList<string> sliTokens)
    {
        if (candidate.Metadata.TryGetValue("symbolic_trace", out var symbolicTrace) && !string.IsNullOrWhiteSpace(symbolicTrace))
        {
            return symbolicTrace;
        }

        if (sliTokens.Count > 0)
        {
            return string.Join(" ", sliTokens);
        }

        return "(= (unknown) (unknown))";
    }

    private EngramTelemetry BuildTelemetry(
        string eventType,
        string engramClass,
        string symbolId,
        string sourceContext,
        string cognitionTraceId)
    {
        var timestamp = DateTime.UtcNow;
        return new EngramTelemetry
        {
            EventType = eventType,
            EngramClass = engramClass,
            SymbolId = symbolId,
            SourceContext = sourceContext,
            CognitionTraceId = cognitionTraceId,
            EventHash = HashHex($"{eventType}|{engramClass}|{symbolId}|{sourceContext}|{cognitionTraceId}|{timestamp:O}"),
            Timestamp = timestamp
        };
    }

    private string AssignRootSymbol(string lexicalToken)
    {
        if (_rootSymbolByLexical.TryGetValue(lexicalToken, out var existing))
        {
            return existing;
        }

        if (_generatedSymbols.TryGetValue(lexicalToken, out var generated))
        {
            return generated;
        }

        var baseSymbol = $"⍙{MapLexemeGlyph(lexicalToken)}";
        if (!_knownSymbolicTokens.Contains(baseSymbol))
        {
            _knownSymbolicTokens.Add(baseSymbol);
            _generatedSymbols[lexicalToken] = baseSymbol;
            return baseSymbol;
        }

        var hash = HashHex(lexicalToken);
        for (var index = 0; index < 12; index += 2)
        {
            var candidate = $"{baseSymbol}{hash.Substring(index, 2)}";
            if (_knownSymbolicTokens.Add(candidate))
            {
                _generatedSymbols[lexicalToken] = candidate;
                return candidate;
            }
        }

        var fallback = $"{baseSymbol}{hash[..8]}";
        _knownSymbolicTokens.Add(fallback);
        _generatedSymbols[lexicalToken] = fallback;
        return fallback;
    }

    private bool IsKnownSymbolicToken(string token, string normalizedToken)
    {
        return _knownSymbolicTokens.Contains(token) || _knownSymbolicTokens.Contains(normalizedToken);
    }

    private static bool IsLexicalToken(string token)
    {
        if (token.Length == 0)
        {
            return false;
        }

        foreach (var character in token)
        {
            if (char.IsLetter(character) || character == '\'' || character == '-' || char.IsDigit(character))
            {
                continue;
            }

            return false;
        }

        return token.Any(char.IsLetter);
    }

    private static string NormalizeToken(string token)
    {
        return token.Trim().Trim('(', ')', '"', '\'', ',', '.', ';', ':').ToLowerInvariant();
    }

    private static IReadOnlyList<string> ExpandToken(string rawToken)
    {
        var token = rawToken.Trim();
        if (token.Length == 0)
        {
            return [];
        }

        var normalized = token.Trim('"');
        if (!normalized.Contains(' ', StringComparison.Ordinal))
        {
            return [token];
        }

        var parts = normalized
            .Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .ToList();

        if (parts.Count == 0)
        {
            return [token];
        }

        return parts;
    }

    private static string MapLexemeGlyph(string lexicalToken)
    {
        var normalized = NormalizeToken(lexicalToken);
        if (normalized.Length == 0)
        {
            return "x";
        }

        return normalized[0] switch
        {
            'l' => "ℓ",
            _ => normalized[0].ToString()
        };
    }

    private static string InferOntologicalClass(string lexicalToken, string contextDomain)
    {
        var token = NormalizeToken(lexicalToken);
        if (contextDomain.Equals("mathematics", StringComparison.OrdinalIgnoreCase))
        {
            return token.EndsWith("ity", StringComparison.Ordinal) || token.EndsWith("al", StringComparison.Ordinal)
                ? "mathematical_property"
                : "mathematical_concept";
        }

        if (contextDomain.Equals("logic", StringComparison.OrdinalIgnoreCase))
        {
            return "logical_atom";
        }

        return "runtime_concept";
    }

    private static string InferContextDomain(EngramCandidate candidate, IReadOnlyList<string> sliTokens)
    {
        if (candidate.Metadata.TryGetValue("context_domain", out var contextDomain) && !string.IsNullOrWhiteSpace(contextDomain))
        {
            return contextDomain.Trim().ToLowerInvariant();
        }

        var payload = string.Join(" ", sliTokens).ToLowerInvariant();
        if (payload.Contains("⊗", StringComparison.Ordinal) ||
            payload.Contains("÷", StringComparison.Ordinal) ||
            payload.Contains("=", StringComparison.Ordinal) ||
            payload.Contains("algebra", StringComparison.Ordinal))
        {
            return "mathematics";
        }

        if (payload.Contains("predicate", StringComparison.Ordinal) ||
            payload.Contains("⊨", StringComparison.Ordinal) ||
            payload.Contains("∀", StringComparison.Ordinal))
        {
            return "logic";
        }

        return "runtime";
    }

    private static string ExtractTraceId(IReadOnlyDictionary<string, string> metadata)
    {
        return ExtractMetadata(metadata, "trace_id", "unknown-trace");
    }

    private static string ExtractMetadata(IReadOnlyDictionary<string, string> metadata, string key, string fallback)
    {
        if (metadata.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return fallback;
    }

    private void EnsureAtlasLoaded()
    {
        if (_atlasLoaded)
        {
            return;
        }

        lock (_atlasLoadGate)
        {
            if (_atlasLoaded)
            {
                return;
            }

            LoadAtlasSymbols();
            _atlasLoaded = true;
        }
    }

    private void LoadAtlasSymbols()
    {
        var gelPath = ResolveGelPath();
        using var stream = File.OpenRead(gelPath);
        using var reader = new StreamReader(stream);
        string? line;

        while ((line = reader.ReadLine()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            using var document = JsonDocument.Parse(line);
            var root = document.RootElement;

            if (!root.TryGetProperty("engram_type", out var engramType) || engramType.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            switch (engramType.GetString())
            {
                case "atlas_index":
                    ReadAtlasIndex(root);
                    break;
                case "atlas_symbolic_index":
                    if (root.TryGetProperty("symbolic_index", out var symbolicIndex))
                    {
                        ReadSymbolicTokens(symbolicIndex);
                    }
                    break;
                case "atlas_symbolic_constructor_guidance":
                    ReadConstructorGuidance(root);
                    break;
            }
        }
    }

    private void ReadAtlasIndex(JsonElement root)
    {
        if (!root.TryGetProperty("index", out var indexElement) || indexElement.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        foreach (var letterNode in indexElement.EnumerateObject())
        {
            if (letterNode.Value.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            foreach (var tokenNode in letterNode.Value.EnumerateObject())
            {
                if (tokenNode.Value.ValueKind != JsonValueKind.Object ||
                    !tokenNode.Value.TryGetProperty("symbol", out var symbolNode) ||
                    symbolNode.ValueKind != JsonValueKind.String)
                {
                    continue;
                }

                var lexicalToken = NormalizeToken(tokenNode.Name);
                var symbol = symbolNode.GetString();
                if (!string.IsNullOrWhiteSpace(lexicalToken) && !string.IsNullOrWhiteSpace(symbol))
                {
                    _rootSymbolByLexical[lexicalToken] = symbol;
                    _knownSymbolicTokens.Add(symbol);
                }
            }
        }
    }

    private void ReadSymbolicTokens(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.NameEquals("symbol") && property.Value.ValueKind == JsonValueKind.String)
                {
                    var symbol = property.Value.GetString();
                    if (!string.IsNullOrWhiteSpace(symbol))
                    {
                        _knownSymbolicTokens.Add(symbol);
                    }
                }

                ReadSymbolicTokens(property.Value);
            }

            return;
        }

        if (element.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var item in element.EnumerateArray())
        {
            ReadSymbolicTokens(item);
        }
    }

    private void ReadConstructorGuidance(JsonElement root)
    {
        if (!root.TryGetProperty("guidance", out var guidance) || guidance.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        if (guidance.TryGetProperty("assignment_rules", out var assignmentRules) &&
            assignmentRules.ValueKind == JsonValueKind.Object &&
            assignmentRules.TryGetProperty("reserved", out var reservedNode) &&
            reservedNode.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in reservedNode.EnumerateObject())
            {
                if (property.Value.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var token in property.Value.EnumerateArray())
                {
                    if (token.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(token.GetString()))
                    {
                        _knownSymbolicTokens.Add(token.GetString()!);
                    }
                }
            }
        }

        if (guidance.TryGetProperty("Reserved", out var reservedArray) && reservedArray.ValueKind == JsonValueKind.Array)
        {
            foreach (var token in reservedArray.EnumerateArray())
            {
                if (token.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(token.GetString()))
                {
                    _knownSymbolicTokens.Add(token.GetString()!);
                }
            }
        }
    }

    private static string ResolveGelPath()
    {
        var candidates = new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory };

        foreach (var candidate in candidates)
        {
            var current = new DirectoryInfo(Path.GetFullPath(candidate));
            while (current is not null)
            {
                var gelPath = Path.Combine(current.FullName, "public_root", "GEL.ndjson");
                if (File.Exists(gelPath))
                {
                    return gelPath;
                }

                current = current.Parent;
            }
        }

        throw new FileNotFoundException("Unable to locate public_root/GEL.ndjson for engram bootstrap.");
    }

    private static string HashHex(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }
}
