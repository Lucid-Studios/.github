using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Oan.Place.GEL
{
    // DTO for BaseSymbolCodex.jsonl (line-by-line)
    public class BaseSymbolEntry
    {
        [JsonPropertyName("variant")]
        public string Variant { get; set; } = string.Empty;

        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = string.Empty;
    }

    // DTO for RootAtlas.json
    public class RootAtlasEntry
    {
        [JsonPropertyName("root")]
        public string Root { get; set; } = string.Empty;

        [JsonPropertyName("variants")]
        public List<string> Variants { get; set; } = new();
    }

    // DTO for elements in RootIndex, Roots, SuffixIndex
    public class SymbolicEntry
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = string.Empty;
    }

    public class RootsJson
    {
        [JsonPropertyName("roots")]
        public JsonElement Roots { get; set; }
    }

    public class SymbolicIndexJson
    {
        [JsonPropertyName("prefixes")]
        public JsonElement Prefixes { get; set; }

        [JsonPropertyName("roots")]
        public JsonElement Roots { get; set; }
    }

    public enum Kind
    {
        Prefix,
        Root,
        Suffix
    }

    public sealed record VisibilityMeta(string MinChannel = "public", string MinMirror = "base", string MinMode = "standard");

    public class MorphemeEntry
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("kind")]
        public Kind Kind { get; set; }

        [JsonPropertyName("surfaces")]
        public List<string> Surfaces { get; set; } = new();

        [JsonPropertyName("variants")]
        public List<string> Variants { get; set; } = new();

        [JsonPropertyName("visibility")]
        public VisibilityMeta Visibility { get; set; } = new();
    }

    public class GelMorphemeChartDraft
    {
        public Dictionary<string, Kind> SymKind { get; set; } = new();
        public Dictionary<string, SortedSet<string>> Surfaces { get; set; } = new();
        public Dictionary<string, SortedSet<string>> Variants { get; set; } = new();
        public Dictionary<string, VisibilityMeta> Visibility { get; set; } = new();
        public Dictionary<(Kind Kind, string Surface), string> SymbolByKindSurface { get; set; } = new();
    }

    public class RawAtlasSources
    {
        public List<BaseSymbolEntry> BaseSymbolCodex { get; set; } = new();
        public JsonElement RootAtlas { get; set; }
        public JsonElement RootIndex { get; set; }
        public RootsJson Roots { get; set; } = new();
        public JsonElement SuffixIndex { get; set; }
        public SymbolicIndexJson SymbolicIndex { get; set; } = new();

        public Dictionary<string, string> FileHashes { get; set; } = new();
        public Dictionary<string, long> FileSizes { get; set; } = new();
    }
}
