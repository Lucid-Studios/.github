using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Oan.Place.GEL
{
    public static class Gel0ReasonCode
    {
        public const string OK = "GEL0.OK";
        public const string UNKNOWN_PREFIX = "GEL0.UNKNOWN_PREFIX";
        public const string UNKNOWN_SUFFIX = "GEL0.UNKNOWN_SUFFIX";
        public const string UNKNOWN_ROOT = "GEL0.UNKNOWN_ROOT";
        public const string VARIANT_NOT_DECLARED = "GEL0.VARIANT_NOT_DECLARED";
        public const string AMBIGUOUS_TIE_BREAK = "GEL0.AMBIGUOUS_TIE_BREAK";
        public const string INVALID_TOKEN_CHARS = "GEL0.INVALID_TOKEN_CHARS";
        public const string NFC_REQUIRED = "GEL0.NFC_REQUIRED";
        public const string EMPTY_TRIPLE_INVALID = "GEL0.EMPTY_TRIPLE_INVALID";
        public const string DUPLICATE_FACTOR_DETECTED = "GEL0.DUPLICATE_FACTOR_DETECTED";
        public const string INVALID_INTEGRITY = "GEL0.INVALID_INTEGRITY";
        public const string INVALID_FORMAT = "GEL0.INVALID_FORMAT";
    }

    public interface IGel0Substrate
    {
        string Normalize(string[] prefixes, string root, string[] suffixes);
        (bool ok, string reasonCode) Validate(string[] prefixes, string root, string[] suffixes);
    }

    public class Gel0Substrate : IGel0Substrate
    {
        private AtlasPackManifest? _manifest;
        private HashSet<string> _knownPrefixes = new(StringComparer.Ordinal);
        private HashSet<string> _knownSuffixes = new(StringComparer.Ordinal);
        private Dictionary<string, List<VariantSpec>> _rootVariants = new(StringComparer.Ordinal); // Root -> Variants

        public void Mount(string artifactsPath)
        {
            string packPath = Path.Combine(artifactsPath, "atlaspack.json");
            if (!File.Exists(packPath)) throw new FileNotFoundException("AtlasPack not found", packPath);
            
            byte[] bytes = File.ReadAllBytes(packPath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            
            AtlasPack? pack;
            try
            {
                pack = JsonSerializer.Deserialize<AtlasPack>(bytes, options);
            }
            catch (JsonException)
            {
                throw new InvalidOperationException(Gel0ReasonCode.INVALID_FORMAT);
            }
            
            if (pack == null) throw new InvalidOperationException(Gel0ReasonCode.INVALID_FORMAT);

            // Integrity Handshake
            string recorded = pack.Manifest.AtlasPackSha256;
            pack.Manifest.AtlasPackSha256 = string.Empty;
            string recomputed = ComputeSha256(Encoding.UTF8.GetBytes(CanonicalJson.Serialize(pack)));
            
            if (recorded != recomputed)
            {
                throw new InvalidOperationException(Gel0ReasonCode.INVALID_INTEGRITY);
            }

            _manifest = pack.Manifest;
            _manifest.AtlasPackSha256 = recorded; // Restore

            foreach (var p in pack.Prefixes) _knownPrefixes.Add(p.Text);
            foreach (var s in pack.Suffixes) _knownSuffixes.Add(s.Text);
            foreach (var r in pack.Roots)
            {
                _rootVariants[r.Root] = r.Variants;
            }
        }

        public string Normalize(string[] prefixes, string root, string[] suffixes)
        {
            var pSorted = prefixes.Select(p => p.Normalize(NormalizationForm.FormC).ToLowerInvariant()).OrderBy(p => p, StringComparer.Ordinal).ToArray();
            var sSorted = suffixes.Select(s => s.Normalize(NormalizationForm.FormC).ToLowerInvariant()).OrderBy(s => s, StringComparer.Ordinal).ToArray();
            var rNorm = root.Normalize(NormalizationForm.FormC).ToLowerInvariant();

            string pStr = pSorted.Length == 0 ? "[]" : "[" + string.Join(",", pSorted) + "]";
            string sStr = sSorted.Length == 0 ? "[]" : "[" + string.Join(",", sSorted) + "]";
            
            return $"gel0|p={pStr}|r={rNorm}|s={sStr}";
        }

        public (bool ok, string reasonCode) Validate(string[] prefixes, string root, string[] suffixes)
        {
             var rNorm = root.Normalize(NormalizationForm.FormC).ToLowerInvariant();
             if (!_rootVariants.ContainsKey(rNorm)) return (false, Gel0ReasonCode.UNKNOWN_ROOT);

             var pSorted = prefixes.Select(p => p.Normalize(NormalizationForm.FormC).ToLowerInvariant()).OrderBy(p => p, StringComparer.Ordinal).ToList();
             var sSorted = suffixes.Select(s => s.Normalize(NormalizationForm.FormC).ToLowerInvariant()).OrderBy(s => s, StringComparer.Ordinal).ToList();

             foreach(var p in pSorted) if (!_knownPrefixes.Contains(p)) return (false, Gel0ReasonCode.UNKNOWN_PREFIX);
             foreach(var s in sSorted) if (!_knownSuffixes.Contains(s)) return (false, Gel0ReasonCode.UNKNOWN_SUFFIX);

             var variants = _rootVariants[rNorm];
             
             bool match = false;
             foreach(var v in variants)
             {
                 if (v.Prefixes.SequenceEqual(pSorted, StringComparer.Ordinal) && 
                     v.Suffixes.SequenceEqual(sSorted, StringComparer.Ordinal))
                 {
                     match = true;
                     break;
                 }
             }

             if (!match) return (false, Gel0ReasonCode.VARIANT_NOT_DECLARED);

             return (true, Gel0ReasonCode.OK);
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
