using System.Text.Json;
using System.Text.RegularExpressions;

namespace EngramGovernance.Services;

public sealed class SymbolicConstructorGuidanceService
{
    private readonly object _loadGate = new();
    private bool _loaded;
    private string _fallbackSymbol = "@.@";
    private int _maxAssignableCharacters = 5120;
    private HashSet<string> _reservedTokens = new(StringComparer.Ordinal);
    private HashSet<char> _reservedCharacters = [];

    public SymbolicGuidanceEvaluation Evaluate(string symbolicTrace)
    {
        EnsureLoaded();

        var trace = string.IsNullOrWhiteSpace(symbolicTrace) ? _fallbackSymbol : symbolicTrace.Trim();
        var usedFallback = string.IsNullOrWhiteSpace(symbolicTrace);
        var wasTruncated = false;

        if (trace.Length > _maxAssignableCharacters)
        {
            trace = trace[.._maxAssignableCharacters];
            wasTruncated = true;
        }

        var reservedCollisionCount = CountReservedCollisions(trace);

        var constructorTag =
            $"fallback:{_fallbackSymbol}|max:{_maxAssignableCharacters}|reserved_collisions:{reservedCollisionCount}" +
            (wasTruncated ? "|truncated:true" : "|truncated:false");

        return new SymbolicGuidanceEvaluation(
            trace,
            constructorTag,
            reservedCollisionCount,
            usedFallback,
            wasTruncated);
    }

    private int CountReservedCollisions(string trace)
    {
        var collisions = 0;

        foreach (var character in trace)
        {
            if (_reservedCharacters.Contains(character))
            {
                collisions += 1;
            }
        }

        foreach (var token in _reservedTokens)
        {
            if (token.Length <= 1)
            {
                continue;
            }

            collisions += Regex.Matches(trace, Regex.Escape(token), RegexOptions.CultureInvariant).Count;
        }

        return collisions;
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

            LoadFromGel();
            _loaded = true;
        }
    }

    private void LoadFromGel()
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

            if (!root.TryGetProperty("engram_type", out var typeElement) ||
                typeElement.ValueKind != JsonValueKind.String ||
                !string.Equals(typeElement.GetString(), "atlas_symbolic_constructor_guidance", StringComparison.Ordinal))
            {
                continue;
            }

            if (!root.TryGetProperty("guidance", out var guidanceElement) ||
                guidanceElement.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            if (guidanceElement.TryGetProperty("assignment_rules", out var assignmentRules) &&
                assignmentRules.ValueKind == JsonValueKind.Object)
            {
                if (assignmentRules.TryGetProperty("fallback_symbol", out var fallbackSymbolElement) &&
                    fallbackSymbolElement.ValueKind == JsonValueKind.String)
                {
                    var fallback = fallbackSymbolElement.GetString();
                    if (!string.IsNullOrWhiteSpace(fallback))
                    {
                        _fallbackSymbol = fallback;
                    }
                }

                if (assignmentRules.TryGetProperty("max_assignable_characters", out var maxCharsElement) &&
                    maxCharsElement.ValueKind == JsonValueKind.Number &&
                    maxCharsElement.TryGetInt32(out var maxChars) &&
                    maxChars > 0)
                {
                    _maxAssignableCharacters = maxChars;
                }

                if (assignmentRules.TryGetProperty("reserved", out var reservedElement) &&
                    reservedElement.ValueKind == JsonValueKind.Object)
                {
                    ReadReservedCollections(reservedElement);
                }
            }

            if (guidanceElement.TryGetProperty("Reserved", out var reservedArray) &&
                reservedArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in reservedArray.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String)
                    {
                        AddReservedToken(item.GetString());
                    }
                }
            }

            return;
        }
    }

    private void ReadReservedCollections(JsonElement reservedElement)
    {
        foreach (var property in reservedElement.EnumerateObject())
        {
            if (property.Value.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var item in property.Value.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    AddReservedToken(item.GetString());
                }
            }
        }
    }

    private void AddReservedToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return;
        }

        _reservedTokens.Add(token);
        if (token.Length == 1)
        {
            _reservedCharacters.Add(token[0]);
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

        throw new FileNotFoundException("Unable to locate public_root/GEL.ndjson for symbolic constructor guidance.");
    }
}

public sealed record SymbolicGuidanceEvaluation(
    string NormalizedTrace,
    string ConstructorTag,
    int ReservedCollisionCount,
    bool UsedFallback,
    bool WasTruncated);
