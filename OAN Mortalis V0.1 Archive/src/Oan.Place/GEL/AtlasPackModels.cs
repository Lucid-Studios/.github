using System;
using System.Collections.Generic;

namespace Oan.Place.GEL
{
    /// <summary>
    /// AtlasPack (Atlas₀): The consolidated canonical dataset for GEL structural factor algebra.
    /// </summary>
    public class AtlasPack
    {
        public AtlasPackManifest Manifest { get; set; } = new();
        public List<PrefixOp> Prefixes { get; set; } = new();
        public List<SuffixOp> Suffixes { get; set; } = new();
        public List<RootEntry> Roots { get; set; } = new();
        public AtlasPackNf Nf { get; set; } = new();
    }

    public class AtlasPackManifest
    {
        public string PackVersion { get; set; } = "0.1.0";
        public string TieBreakRulesVersion { get; set; } = "TB-0.1.0";
        
        public Dictionary<string, string> SourceFileHashes { get; set; } = new();
        public string AtlasPackSha256 { get; set; } = string.Empty;
        
        public PackCounts Counts { get; set; } = new();
        
        public List<UnsupportedVariant> UnsupportedVariants { get; set; } = new();
    }

    public class PackCounts
    {
        public int Roots { get; set; }
        public int PrefixOps { get; set; }
        public int SuffixOps { get; set; }
        public int VariantSpecs { get; set; }
        public int UnsupportedCount { get; set; }
    }

    public class UnsupportedVariant
    {
        public string RawOriginal { get; set; } = string.Empty;
        public string RawNormalized { get; set; } = string.Empty;
    }

    public class AtlasPackNf
    {
        public string BuildRulesVersion { get; set; } = "NF-0.1.0";
        public string TieBreakRulesVersion { get; set; } = "TB-0.1.0";
        public List<string> ActiveRules { get; set; } = new() 
        { 
            "VALIDATION_EXACT_MATCH", 
            "LEXICOGRAPHIC_TIE_BREAK" 
        };
    }

    public class PrefixOp
    {
        public string Pid { get; set; } = string.Empty; // P:<text>
        public string Text { get; set; } = string.Empty; // normalized
        public string? Glyph { get; set; }
    }

    public class SuffixOp
    {
        public string Sid { get; set; } = string.Empty; // S:<text>
        public string Text { get; set; } = string.Empty; // normalized
        public string? Glyph { get; set; }
    }

    public class RootEntry
    {
        public string Rid { get; set; } = string.Empty; // R:<root>
        public string Root { get; set; } = string.Empty; // normalized
        public List<VariantSpec> Variants { get; set; } = new();
    }

    public class VariantSpec
    {
        public string Vid { get; set; } = string.Empty; // deterministic hash
        public string RawOriginal { get; set; } = string.Empty; // EXACT as read
        public string RawNormalized { get; set; } = string.Empty; // NFC+Lower
        public List<string> Prefixes { get; set; } = new(); // sorted pid/text? specified "prefixes[]"
        public List<string> Suffixes { get; set; } = new(); // sorted
        public List<string> Flags { get; set; } = new(); // e.g. IDENTITY
    }
}
