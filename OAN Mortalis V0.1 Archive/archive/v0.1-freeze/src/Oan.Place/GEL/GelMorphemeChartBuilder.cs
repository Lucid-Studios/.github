using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Oan.Place.GEL
{
    public static class GelMorphemeChartReasonCode
    {
        public const string ATLAS_SOURCE_NOT_FOUND = "ATLAS_SOURCE_NOT_FOUND";
        public const string ATLAS_SOURCE_EMPTY_FILE = "ATLAS_SOURCE_EMPTY_FILE";
        public const string ATLAS_SOURCE_INVALID_JSON = "ATLAS_SOURCE_INVALID_JSON";
        public const string ATLAS_SOURCE_SCHEMA_MISMATCH = "ATLAS_SOURCE_SCHEMA_MISMATCH";
        public const string GEL_CANON_EMPTY_SURFACE = "GEL_CANON_EMPTY_SURFACE";
        public const string GEL_SYMBOL_KIND_CONFLICT = "GEL_SYMBOL_KIND_CONFLICT";
        public const string GEL_SURFACE_COLLISION_SAME_KIND = "GEL_SURFACE_COLLISION_SAME_KIND";
        public const string GEL_VARIANT_WITHOUT_BASE = "GEL_VARIANT_WITHOUT_BASE";
        public const string GEL_EMPTY_CORE = "GEL_EMPTY_CORE";
        public const string GEL_RESOLVE_AMBIGUOUS = "GEL_RESOLVE_AMBIGUOUS";
        public const string GEL_RESOLVE_NOT_FOUND = "GEL_RESOLVE_NOT_FOUND";
    }

    public class GelMorphemeChartBuilder
    {
        public static string NormalizeSurface(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) throw new InvalidOperationException(GelMorphemeChartReasonCode.GEL_CANON_EMPTY_SURFACE);

            // 1. Unicode NFC
            string norm = raw.Normalize(NormalizationForm.FormC);

            // 2. Trim whitespace
            norm = norm.Trim();

            // 3. Lower-case invariant
            norm = norm.ToLowerInvariant();

            // 4. Record hyphen markers for intent (but strip from canonical surface)
            // Stripping logic per §3.1: leading/trailing hyphen markers record intent but not in chart.
            // "collapse internal whitespace to single space"
            // "collapse multiple hyphens to single hyphen"
            
            // Internal whitespace
            norm = System.Text.RegularExpressions.Regex.Replace(norm, @"\s+", " ");
            
            // Multiple hyphens
            norm = System.Text.RegularExpressions.Regex.Replace(norm, @"-+", "-");

            // Strip leading/trailing hyphens
            norm = norm.Trim('-');

            if (string.IsNullOrEmpty(norm)) throw new InvalidOperationException(GelMorphemeChartReasonCode.GEL_CANON_EMPTY_SURFACE);

            return norm;
        }

        public RawAtlasSources LoadSources(string directoryPath)
        {
            var sources = new RawAtlasSources();
            var files = new[]
            {
                "BaseSymbolCodex.jsonl",
                "RootAtlas.json",
                "RootIndex.json",
                "Roots.json",
                "SuffixIndex.json",
                "SymbolicIndex.json"
            };

            foreach (var filename in files)
            {
                string fullPath = Path.Combine(directoryPath, filename);
                if (!File.Exists(fullPath)) throw new FileNotFoundException(GelMorphemeChartReasonCode.ATLAS_SOURCE_NOT_FOUND, filename);

                byte[] bytes = File.ReadAllBytes(fullPath);
                if (bytes.Length == 0) throw new InvalidOperationException(GelMorphemeChartReasonCode.ATLAS_SOURCE_EMPTY_FILE + ": " + filename);

                sources.FileHashes[filename] = ComputeSha256(bytes);
                sources.FileSizes[filename] = bytes.Length;

                try
                {
                    if (filename == "BaseSymbolCodex.jsonl")
                    {
                        var lines = File.ReadAllLines(fullPath);
                        foreach (var line in lines)
                        {
                            if (string.IsNullOrWhiteSpace(line)) continue;
                            var entry = JsonSerializer.Deserialize<BaseSymbolEntry>(line);
                            if (entry != null) sources.BaseSymbolCodex.Add(entry);
                        }
                    }
                    else
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        using var doc = JsonDocument.Parse(bytes);
                        JsonElement el = doc.RootElement.Clone();

                        if (filename == "RootAtlas.json") sources.RootAtlas = el;
                        else if (filename == "RootIndex.json") sources.RootIndex = el;
                        else if (filename == "Roots.json") 
                        {
                            if (el.TryGetProperty("roots", out var r)) sources.Roots.Roots = r.Clone();
                            else throw new Exception("Missing 'roots' property");
                        }
                        else if (filename == "SuffixIndex.json") sources.SuffixIndex = el;
                        else if (filename == "SymbolicIndex.json") 
                        {
                            if (el.TryGetProperty("prefixes", out var p)) sources.SymbolicIndex.Prefixes = p.Clone();
                            if (el.TryGetProperty("roots", out var rt)) sources.SymbolicIndex.Roots = rt.Clone();
                        }
                    }
                }
                catch (JsonException ex)
                {
                    throw new InvalidOperationException($"{GelMorphemeChartReasonCode.ATLAS_SOURCE_INVALID_JSON}: {filename} - {ex.Message}");
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"{GelMorphemeChartReasonCode.ATLAS_SOURCE_SCHEMA_MISMATCH}: {filename} - {ex.GetType().Name}: {ex.Message}");
                }
            }

            return sources;
        }

        public GelMorphemeChartDraft BuildDraft(RawAtlasSources sources)
        {
            var draft = new GelMorphemeChartDraft();
            
            // Reset resolver state for this build
            SymbolByKindSurface.Clear();
            ExplicitSymbolMap.Clear();

            // 1. Seed from SymbolicIndex.json (Prefixes and Roots)
            SeedFromSymbolicIndex(draft, sources.SymbolicIndex);

            // 2. Enrich from SuffixIndex.json
            if (sources.SuffixIndex.ValueKind == JsonValueKind.Object)
            {
                foreach (var top in sources.SuffixIndex.EnumerateObject())
                {
                    ExtractSymbolicEntries(draft, top.Value, Kind.Suffix, top.Name);
                }
            }

            // 3. Enrich Roots from Roots.json and RootIndex.json
            if (sources.Roots.Roots.ValueKind == JsonValueKind.Object)
            {
                foreach (var entry in sources.Roots.Roots.EnumerateObject())
                {
                    ExtractSymbolicEntries(draft, entry.Value, Kind.Root, entry.Name);
                }
            }
            if (sources.RootIndex.ValueKind == JsonValueKind.Object)
            {
                foreach (var top in sources.RootIndex.EnumerateObject())
                {
                    ExtractSymbolicEntries(draft, top.Value, Kind.Root, top.Name);
                }
            }

            // 4. Overlay variants from RootAtlas.json
            OverlayVariants(draft, sources.RootAtlas);

            if (draft.SymKind.Count == 0) throw new InvalidOperationException(GelMorphemeChartReasonCode.GEL_EMPTY_CORE);

            ActiveDraft = draft;
            return draft;
        }

        public string Resolve(Kind kind, string surface)
        {
            string norm = NormalizeSurface(surface);
            if (SymbolByKindSurface.TryGetValue((kind, norm), out string? sym)) return sym;
            throw new InvalidOperationException($"{GelMorphemeChartReasonCode.GEL_RESOLVE_NOT_FOUND}: {kind} {norm}");
        }

        public string Assemble(string? prefix, string root, string? suffix)
        {
            string p = prefix != null ? Resolve(Kind.Prefix, prefix) : "";
            string r = Resolve(Kind.Root, root);
            string s = suffix != null ? Resolve(Kind.Suffix, suffix) : "";
            return p + r + s;
        }

        public (string packPath, string manifestPath) Emit(string outputDir, RawAtlasSources sources, GelMorphemeChartDraft draft)
        {
            string packContent = SerializeCanonical(draft);
            string chartHash = ComputeSha256(Encoding.UTF8.GetBytes(packContent));
            
            string packName = "gel.morpheme.chart.v1.pack";
            string manifestName = "gel.morpheme.chart.v1.manifest";
            
            string packPath = Path.Combine(outputDir, packName);
            string manifestPath = Path.Combine(outputDir, manifestName);
            
            File.WriteAllText(packPath, packContent);
            
            var manifestLines = new List<string>
            {
                "=== GEL MORPHEME CHART V1 MANIFEST ===",
                $"ChartHash: {chartHash}",
                $"Timestamp: {DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ} (UTC)",
                $"TotalSymbols: {draft.SymKind.Count}",
                "Sources:",
            };
            
            foreach (var kv in sources.FileHashes)
            {
                manifestLines.Add($"- {kv.Key} | Size: {sources.FileSizes[kv.Key]} | SHA256: {kv.Value}");
            }
            
            File.WriteAllLines(manifestPath, manifestLines);
            
            return (packPath, manifestPath);
        }

        private Dictionary<(Kind Kind, string Surface), string> SymbolByKindSurface = new();

        public GelMorphemeChartDraft? ActiveDraft { get; private set; }
        private Dictionary<string, Kind> ExplicitSymbolMap = new();

        public (string? prefix, string root, string? suffix) Factor(string nf)
        {
            if (ActiveDraft == null) throw new InvalidOperationException("No active draft to factor against.");

            // Real Factor: Greedy match R, then check P and S.
            var roots = ActiveDraft.SymKind.Where(kv => kv.Value == Kind.Root).OrderByDescending(kv => kv.Key.Length);
            foreach(var rootSym in roots)
            {
                int rIdx = nf.IndexOf(rootSym.Key);
                if (rIdx == -1) continue;

                string pPart = nf.Substring(0, rIdx);
                string sPart = nf.Substring(rIdx + rootSym.Key.Length);

                string? pSym = string.IsNullOrEmpty(pPart) ? null : pPart;
                string? sSym = string.IsNullOrEmpty(sPart) ? null : sPart;

                // Validate pSym is a Prefix and sSym is a Suffix
                if (pSym != null && (!ActiveDraft.SymKind.TryGetValue(pSym, out var pk) || pk != Kind.Prefix)) continue;
                if (sSym != null && (!ActiveDraft.SymKind.TryGetValue(sSym, out var sk) || sk != Kind.Suffix)) continue;

                return (pSym, rootSym.Key, sSym);
            }

            throw new InvalidOperationException(GelMorphemeChartReasonCode.GEL_RESOLVE_NOT_FOUND + ": " + nf);
        }

        public string SerializeCanonical(GelMorphemeChartDraft draft)
        {
            var entries = draft.SymKind.Select(kv => new MorphemeEntry
            {
                Id = kv.Key,
                Kind = kv.Value,
                Surfaces = draft.Surfaces.GetValueOrDefault(kv.Key)?.ToList() ?? new List<string>(),
                Variants = draft.Variants.GetValueOrDefault(kv.Key)?.ToList() ?? new List<string>(),
                Visibility = draft.Visibility.GetValueOrDefault(kv.Key) ?? new VisibilityMeta()
            })
            .OrderBy(e => e.Kind)
            .ThenBy(e => e.Id)
            .ToList();

            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            return JsonSerializer.Serialize(entries, options);
        }

        private void SeedFromSymbolicIndex(GelMorphemeChartDraft draft, SymbolicIndexJson index)
        {
            // Prefixes
            if (index.Prefixes.ValueKind == JsonValueKind.Object)
            {
                foreach (var topLetter in index.Prefixes.EnumerateObject())
                {
                    if (topLetter.Value.ValueKind != JsonValueKind.Object) continue;
                    foreach (var entry in topLetter.Value.EnumerateObject())
                    {
                        if (entry.Value.ValueKind == JsonValueKind.Object)
                        {
                            ExtractSymbolicEntries(draft, entry.Value, Kind.Prefix, entry.Name);
                        }
                    }
                }
            }

            // Roots
            if (index.Roots.ValueKind == JsonValueKind.Object)
            {
                foreach (var topLetter in index.Roots.EnumerateObject())
                {
                    if (topLetter.Value.ValueKind != JsonValueKind.Object) continue;
                    foreach (var entry in topLetter.Value.EnumerateObject())
                    {
                        if (entry.Value.ValueKind == JsonValueKind.Object)
                        {
                            ExtractSymbolicEntries(draft, entry.Value, Kind.Root, entry.Name);
                        }
                    }
                }
            }
        }

        private void ExtractSymbolicEntries(GelMorphemeChartDraft draft, JsonElement el, Kind kind, string surface)
        {
            if (el.ValueKind != JsonValueKind.Object) return;

            if (el.TryGetProperty("symbol", out var sym))
            {
                if (sym.ValueKind == JsonValueKind.String)
                {
                    RegisterMorpheme(draft, surface, sym.GetString() ?? "", kind);
                }
            }
            else
            {
                // If no symbol, treat props as nested sub-keys (e.g. "prefixes": { "a": { "anti": { "symbol": "X" } } })
                foreach (var prop in el.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.Object)
                    {
                        ExtractSymbolicEntries(draft, prop.Value, kind, prop.Name);
                    }
                }
            }
        }

        private void EnrichSuffixes(GelMorphemeChartDraft draft, JsonElement suffixIndex)
        {
            if (suffixIndex.ValueKind != JsonValueKind.Object) return;
            foreach (var top in suffixIndex.EnumerateObject())
            {
                if (top.Value.ValueKind != JsonValueKind.Object) continue;
                foreach (var entry in top.Value.EnumerateObject())
                {
                    if (entry.Value.ValueKind != JsonValueKind.Object) continue;
                    if (entry.Value.TryGetProperty("symbol", out var sym))
                    {
                        RegisterMorpheme(draft, entry.Name, sym.GetString() ?? "", Kind.Suffix);
                    }
                    else
                    {
                        string surface = NormalizeSurface(entry.Name);
                        RegisterMorpheme(draft, surface, "s_" + surface, Kind.Suffix);
                    }
                }
            }
        }

        private void EnrichRoots(GelMorphemeChartDraft draft, JsonElement rootsJson, JsonElement rootIndex)
        {
            // rootsJson passed in from sources.Roots.Roots is ALREADY the "roots" object from Roots.json
            // Process Roots.json
            if (rootsJson.ValueKind == JsonValueKind.Object)
            {
                foreach (var entry in rootsJson.EnumerateObject())
                {
                    if (entry.Value.ValueKind == JsonValueKind.Object && entry.Value.TryGetProperty("symbol", out var sym))
                    {
                        RegisterMorpheme(draft, entry.Name, sym.GetString() ?? "", Kind.Root);
                    }
                    else
                    {
                        string surface = NormalizeSurface(entry.Name);
                        RegisterMorpheme(draft, surface, "r_" + surface, Kind.Root);
                    }
                }
            }

            if (rootIndex.ValueKind == JsonValueKind.Object)
            {
                foreach (var top in rootIndex.EnumerateObject())
                {
                    if (top.Value.ValueKind != JsonValueKind.Object) continue;
                    foreach (var entry in top.Value.EnumerateObject())
                    {
                        if (entry.Value.ValueKind == JsonValueKind.Object && entry.Value.TryGetProperty("symbol", out var sym))
                        {
                            RegisterMorpheme(draft, entry.Name, sym.GetString() ?? "", Kind.Root);
                        }
                        else
                        {
                            string surface = NormalizeSurface(entry.Name);
                            RegisterMorpheme(draft, surface, "r_" + surface, Kind.Root);
                        }
                    }
                }
            }
        }

        private void OverlayVariants(GelMorphemeChartDraft draft, JsonElement rootAtlas)
        {
            if (rootAtlas.ValueKind != JsonValueKind.Object) return;
            foreach (var entry in rootAtlas.EnumerateObject())
            {
                if (entry.Value.ValueKind != JsonValueKind.Object) continue;
                string rootSurface = NormalizeSurface(entry.Name);
                
                // Find symbol for this root
                string? symbolId = null;
                // Ambiguity check: if SAME surface exists for multiple symbols of kind ROOT, we have a problem.
                // But typically RootAtlas is keyed by the canonical surface.
                var matching = draft.SymbolByKindSurface.Where(kv => kv.Key.Kind == Kind.Root && kv.Key.Surface == rootSurface).ToList();
                if (matching.Count > 1) throw new InvalidOperationException(GelMorphemeChartReasonCode.GEL_RESOLVE_AMBIGUOUS + ": " + rootSurface);
                if (matching.Count == 1) symbolId = matching[0].Value;

                if (symbolId == null)
                {
                    // This variant set has no declared root in SymbolicIndex/Roots.json
                    // For structural resilience, we skip rather than fail.
                    continue; 
                }

                if (entry.Value.TryGetProperty("variants", out var vars) && vars.ValueKind == JsonValueKind.Array)
                {
                    foreach (var v in vars.EnumerateArray())
                    {
                        if (v.ValueKind != JsonValueKind.String) continue;
                        string vStr = v.GetString() ?? "";
                        if (vStr == "") continue; // skip base marker
                        
                        // User says: s_ for suffixes, but variants are usually appended to roots.
                        // We record them as variants for the symbol.
                        if (!draft.Variants.ContainsKey(symbolId)) draft.Variants[symbolId] = new SortedSet<string>();
                        draft.Variants[symbolId].Add(NormalizeSurface(vStr));
                    }
                }
            }
        }

        private void RegisterMorpheme(GelMorphemeChartDraft draft, string rawSurface, string symbolId, Kind kind)
        {
            string surface = NormalizeSurface(rawSurface);
            
            // Disambiguation Rule: If explicit symbolId is already taken by a different kind, disambiguate.
            string finalId = symbolId;
            if (ExplicitSymbolMap.TryGetValue(symbolId, out var existingKind) && existingKind != kind)
            {
                // Disambiguate by appending kind
                finalId = $"{symbolId}_{kind}";
            }
            else
            {
                ExplicitSymbolMap[symbolId] = kind;
            }

            // Conflict 1: Symbol ID reused for different kinds (after disambiguation)
            if (draft.SymKind.TryGetValue(finalId, out var k) && k != kind)
            {
                 throw new InvalidOperationException($"{GelMorphemeChartReasonCode.GEL_SYMBOL_KIND_CONFLICT}: Symbol {finalId} is {k} and {kind}");
            }
            draft.SymKind[finalId] = kind;

            // Register surface
            if (!draft.Surfaces.ContainsKey(finalId)) draft.Surfaces[finalId] = new SortedSet<string>();
            draft.Surfaces[finalId].Add(surface);

            // Conflict 2: Surface collision SAME kind mapping to different symbols
            var key = (kind, surface);
            if (draft.SymbolByKindSurface.TryGetValue(key, out var existingSym) && existingSym != finalId)
            {
                throw new InvalidOperationException($"{GelMorphemeChartReasonCode.GEL_SURFACE_COLLISION_SAME_KIND}: {kind} {surface} maps to {existingSym} and {finalId}");
            }
            draft.SymbolByKindSurface[key] = finalId;
            
            // Mirror back to local state for Resolve/Assemble
            SymbolByKindSurface[key] = finalId;
        }

        public void PrintReport(RawAtlasSources sources)
        {
            Console.WriteLine("=== BuilderReport ===");
            foreach (var kv in sources.FileHashes)
            {
                Console.WriteLine($"File: {kv.Key} | Size: {sources.FileSizes[kv.Key]} | SHA256: {kv.Value}");
            }

            Console.WriteLine($"Raw Counts (Estimated):");
            Console.WriteLine($"- BaseSymbolCodex: {sources.BaseSymbolCodex.Count} entries");
            
            // Helper for JsonElement count
            int GetCount(JsonElement el) 
            {
                if (el.ValueKind != JsonValueKind.Object) return 0;
                int count = 0;
                foreach (var _ in el.EnumerateObject()) count++;
                return count;
            }

            Console.WriteLine($"- RootAtlas: {GetCount(sources.RootAtlas)} roots");
            Console.WriteLine($"- RootIndex: {GetCount(sources.RootIndex)} top-keys");
            Console.WriteLine($"- Roots.json: {GetCount(sources.Roots.Roots)} top-keys");
            Console.WriteLine($"- SuffixIndex: {GetCount(sources.SuffixIndex)} top-keys");
            Console.WriteLine($"- SymbolicIndex.Prefixes: {GetCount(sources.SymbolicIndex.Prefixes)} top-keys");
            Console.WriteLine($"- SymbolicIndex.Roots: {GetCount(sources.SymbolicIndex.Roots)} top-keys");
        }

        private string ComputeSha256(byte[] bytes)
        {
            using (var sha = SHA256.Create())
            {
                return Convert.ToHexString(sha.ComputeHash(bytes)).ToLowerInvariant();
            }
        }
    }
}
