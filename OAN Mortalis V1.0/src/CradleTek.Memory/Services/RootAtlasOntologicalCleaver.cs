using System.Text.Json;
using System.Text.RegularExpressions;
using CradleTek.Memory.Interfaces;
using CradleTek.Memory.Models;

namespace CradleTek.Memory.Services;

public sealed partial class RootAtlasOntologicalCleaver : IRootOntologicalCleaver
{
    private readonly object _loadGate = new();
    private bool _loaded;
    private readonly Dictionary<string, RootAtlasEntry> _entriesByRoot = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, RootAtlasEntry> _entriesByVariant = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _symbolsByRoot = new(StringComparer.OrdinalIgnoreCase);

    public Task<OntologicalCleaverResult> CleaveAsync(string inputText, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputText);
        cancellationToken.ThrowIfCancellationRequested();
        EnsureLoaded();

        var resolutions = new List<OntologicalTokenResolution>();
        var knownMap = new Dictionary<string, RootEngram>(StringComparer.Ordinal);
        var partiallyKnownMap = new Dictionary<string, RootEngram>(StringComparer.Ordinal);
        var unknown = new List<string>();
        var unknownSeen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var token in Tokenize(inputText))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var normalized = NormalizeToken(token);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                continue;
            }

            if (_entriesByRoot.TryGetValue(normalized, out var exactRoot))
            {
                var engram = BuildRootEngram(exactRoot);
                knownMap[engram.SymbolicId] = engram;
                resolutions.Add(new OntologicalTokenResolution
                {
                    Token = token,
                    NormalizedToken = normalized,
                    Classification = OntologicalCleaverClassification.Known,
                    RootEngram = engram,
                    ResolutionReason = "exact-root"
                });
                continue;
            }

            if (_entriesByVariant.TryGetValue(normalized, out var variantRoot))
            {
                var engram = BuildRootEngram(variantRoot);
                partiallyKnownMap[engram.SymbolicId] = engram;
                resolutions.Add(new OntologicalTokenResolution
                {
                    Token = token,
                    NormalizedToken = normalized,
                    Classification = OntologicalCleaverClassification.PartiallyKnown,
                    RootEngram = engram,
                    ResolutionReason = "variant-root"
                });
                continue;
            }

            if (TryNormalizeMorphology(normalized, out var morpheme) &&
                _entriesByVariant.TryGetValue(morpheme, out var morphemeRoot))
            {
                var engram = BuildRootEngram(morphemeRoot);
                partiallyKnownMap[engram.SymbolicId] = engram;
                resolutions.Add(new OntologicalTokenResolution
                {
                    Token = token,
                    NormalizedToken = normalized,
                    Classification = OntologicalCleaverClassification.PartiallyKnown,
                    RootEngram = engram,
                    ResolutionReason = "morph-normalized"
                });
                continue;
            }

            if (unknownSeen.Add(normalized))
            {
                unknown.Add(normalized);
            }

            resolutions.Add(new OntologicalTokenResolution
            {
                Token = token,
                NormalizedToken = normalized,
                Classification = OntologicalCleaverClassification.Unknown,
                RootEngram = null,
                ResolutionReason = "no-root-match"
            });
        }

        var total = resolutions.Count;
        var knownCount = resolutions.Count(r => r.Classification == OntologicalCleaverClassification.Known);
        var partiallyKnownCount = resolutions.Count(r => r.Classification == OntologicalCleaverClassification.PartiallyKnown);
        var unknownCount = resolutions.Count(r => r.Classification == OntologicalCleaverClassification.Unknown);

        var knownRatio = total == 0 ? 0d : (double)knownCount / total;
        var partiallyKnownRatio = total == 0 ? 0d : (double)partiallyKnownCount / total;
        var unknownRatio = total == 0 ? 0d : (double)unknownCount / total;
        var recognizedRatio = total == 0 ? 0d : (double)(knownCount + partiallyKnownCount) / total;

        var conceptDensity = recognizedRatio switch
        {
            >= 0.75 => "high",
            >= 0.45 => "moderate",
            _ => "low"
        };

        var contextStability = (unknownRatio, partiallyKnownRatio) switch
        {
            (<= 0.10, <= 0.40) => "stable",
            (<= 0.35, <= 0.65) => "transitional",
            _ => "volatile"
        };

        return Task.FromResult(new OntologicalCleaverResult
        {
            InputText = inputText,
            Resolutions = resolutions,
            Known = knownMap.Values.OrderBy(v => v.SymbolicId, StringComparer.Ordinal).ToList(),
            PartiallyKnown = partiallyKnownMap.Values.OrderBy(v => v.SymbolicId, StringComparer.Ordinal).ToList(),
            Unknown = unknown,
            Metrics = new OntologicalCleaverMetrics
            {
                KnownRatio = Math.Round(knownRatio, 6),
                PartiallyKnownRatio = Math.Round(partiallyKnownRatio, 6),
                UnknownRatio = Math.Round(unknownRatio, 6),
                ConceptDensity = conceptDensity,
                ContextStability = contextStability
            }
        });
    }

    private RootEngram BuildRootEngram(RootAtlasEntry entry)
    {
        var symbolId = _symbolsByRoot.TryGetValue(entry.RootTerm, out var symbol) && !string.IsNullOrWhiteSpace(symbol)
            ? $"ATLAS.SYM.{symbol}"
            : $"ATLAS.ROOT.{entry.RootTerm.ToUpperInvariant().Replace('-', '_')}";

        return new RootEngram
        {
            SymbolicId = symbolId,
            AtlasDomain = $"atlas.root.{entry.RootTerm[0]}",
            RootTerm = entry.RootTerm,
            VariantForms = entry.Variants.ToList(),
            FrequencyWeight = entry.FrequencyWeight,
            DictionaryPointer = $"atlas://root/{entry.RootTerm}"
        };
    }

    private void EnsureLoaded()
    {
        if (_loaded)
        {
            return;
        }

        lock (_loadGate)
        {
            if (_loaded)
            {
                return;
            }

            LoadPrimeGelRoots();
            _loaded = true;
        }
    }

    private void LoadPrimeGelRoots()
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

            if (TryReadRootAtlas(root))
            {
                continue;
            }

            if (root.TryGetProperty("engram_type", out var typeProperty) &&
                typeProperty.ValueKind == JsonValueKind.String &&
                typeProperty.GetString() == "atlas_symbolic_index" &&
                root.TryGetProperty("symbolic_index", out var symbolicIndex))
            {
                ReadSymbolicIndex(symbolicIndex);
            }
        }

        if (_entriesByRoot.Count == 0)
        {
            throw new InvalidOperationException("Prime GEL Root Atlas could not be loaded from public_root/GEL.ndjson.");
        }
    }

    private bool TryReadRootAtlas(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        // The prime root atlas line is the direct lexical object without an engram_type property.
        if (element.TryGetProperty("engram_type", out _))
        {
            return false;
        }

        foreach (var property in element.EnumerateObject())
        {
            if (property.Value.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            var rootTerm = NormalizeToken(property.Name);
            if (string.IsNullOrWhiteSpace(rootTerm))
            {
                continue;
            }

            var variants = new List<string> { rootTerm };
            var maxFrequency = 1d;

            if (property.Value.TryGetProperty("variants", out var variantsProperty) &&
                variantsProperty.ValueKind == JsonValueKind.Array)
            {
                foreach (var variant in variantsProperty.EnumerateArray())
                {
                    if (variant.ValueKind != JsonValueKind.String)
                    {
                        continue;
                    }

                    var rawVariant = variant.GetString();
                    if (string.IsNullOrWhiteSpace(rawVariant))
                    {
                        continue;
                    }

                    var term = ExtractVariantTerm(rawVariant);
                    if (!string.IsNullOrWhiteSpace(term))
                    {
                        variants.Add(term);
                    }

                    var frequency = ExtractVariantFrequency(rawVariant);
                    maxFrequency = Math.Max(maxFrequency, frequency);
                }
            }

            var entry = new RootAtlasEntry(
                rootTerm,
                variants.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(v => v, StringComparer.OrdinalIgnoreCase).ToArray(),
                maxFrequency);

            _entriesByRoot[rootTerm] = entry;
            foreach (var variant in entry.Variants)
            {
                _entriesByVariant[variant] = entry;
            }
        }

        return _entriesByRoot.Count > 0;
    }

    private void ReadSymbolicIndex(JsonElement symbolicIndex)
    {
        if (!symbolicIndex.TryGetProperty("roots", out var roots) ||
            roots.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        foreach (var letterNode in roots.EnumerateObject())
        {
            if (letterNode.Value.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            foreach (var rootNode in letterNode.Value.EnumerateObject())
            {
                if (rootNode.Value.ValueKind != JsonValueKind.Object ||
                    !rootNode.Value.TryGetProperty("symbol", out var symbolNode) ||
                    symbolNode.ValueKind != JsonValueKind.String)
                {
                    continue;
                }

                var rootTerm = NormalizeToken(rootNode.Name);
                var symbol = symbolNode.GetString();
                if (!string.IsNullOrWhiteSpace(rootTerm) && !string.IsNullOrWhiteSpace(symbol))
                {
                    _symbolsByRoot[rootTerm] = symbol;
                }
            }
        }
    }

    private static string ResolveGelPath()
    {
        var candidates = new[]
        {
            Directory.GetCurrentDirectory(),
            AppContext.BaseDirectory
        };

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

        throw new FileNotFoundException("Unable to locate public_root/GEL.ndjson for root engram resolution.");
    }

    private static IEnumerable<string> Tokenize(string input)
    {
        foreach (Match match in LexemeRegex().Matches(input))
        {
            if (match.Success && !string.IsNullOrWhiteSpace(match.Value))
            {
                yield return match.Value;
            }
        }
    }

    private static string NormalizeToken(string token)
    {
        var normalized = token.Trim().ToLowerInvariant();
        return normalized.Trim('\'', '"', '`', '.', ',', ';', ':', '!', '?', '(', ')', '[', ']', '{', '}');
    }

    private static bool TryNormalizeMorphology(string token, out string normalized)
    {
        normalized = string.Empty;
        if (token.Length < 4)
        {
            return false;
        }

        if (token.EndsWith("ies", StringComparison.Ordinal) && token.Length > 4)
        {
            normalized = token[..^3] + "y";
            return true;
        }

        if (token.EndsWith("ing", StringComparison.Ordinal) && token.Length > 5)
        {
            normalized = token[..^3];
            return true;
        }

        if (token.EndsWith("ed", StringComparison.Ordinal) && token.Length > 4)
        {
            normalized = token[..^2];
            return true;
        }

        if (token.EndsWith("es", StringComparison.Ordinal) && token.Length > 4)
        {
            normalized = token[..^2];
            return true;
        }

        if (token.EndsWith('s') && token.Length > 3)
        {
            normalized = token[..^1];
            return true;
        }

        return false;
    }

    private static string ExtractVariantTerm(string rawVariant)
    {
        var text = rawVariant.Trim();
        var index = text.LastIndexOf(" (", StringComparison.Ordinal);
        var term = index > 0 ? text[..index] : text;
        return NormalizeToken(term);
    }

    private static double ExtractVariantFrequency(string rawVariant)
    {
        var match = FrequencyRegex().Match(rawVariant);
        if (match.Success && double.TryParse(match.Groups["count"].Value, out var count))
        {
            return count;
        }

        return 1d;
    }

    [GeneratedRegex("[\\p{L}\\p{Mn}][\\p{L}\\p{Mn}'\\-]*", RegexOptions.Compiled)]
    private static partial Regex LexemeRegex();

    [GeneratedRegex("\\((?<count>\\d+)\\)\\s*$", RegexOptions.Compiled)]
    private static partial Regex FrequencyRegex();
}

internal sealed record RootAtlasEntry(
    string RootTerm,
    IReadOnlyList<string> Variants,
    double FrequencyWeight);
