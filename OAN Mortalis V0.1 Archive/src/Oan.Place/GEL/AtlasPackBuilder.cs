using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Oan.SoulFrame.Atlas;

namespace Oan.Place.GEL
{
    public class AtlasPackBuilder
    {
        private AtlasPack _pack = new();
        private HashSet<string> _discoveredPrefixes = new(StringComparer.Ordinal);
        private HashSet<string> _discoveredSuffixes = new(StringComparer.Ordinal);

        public AtlasPack Build(AtlasSource source)
        {
            _pack = new AtlasPack();
            _discoveredPrefixes.Clear();
            _discoveredSuffixes.Clear();

            // 1. Initialize Manifest basic data
            _pack.Manifest.SourceFileHashes = new Dictionary<string, string>(source.FileHashes);

            // 2. Process Roots and Variants
            var parsedRoots = source.ParsedRootAtlas?.Roots ?? new List<RootEntryModel>();
            var unsupportedRaw = new HashSet<string>();
            var unsupportedNorm = new HashSet<string>();

            foreach (var inputRoot in parsedRoots)
            {
                var rootEntry = new RootEntry
                {
                    Root = inputRoot.Root,
                    Rid = $"R:{inputRoot.Root}"
                };

                for (int i = 0; i < inputRoot.Variants.Count; i++)
                {
                    string norm = inputRoot.Variants[i];
                    string orig = inputRoot.RawVariants[i];

                    if (TryParseVariant(norm, orig, out var spec))
                    {
                        foreach (var p in spec.Prefixes) _discoveredPrefixes.Add(p);
                        foreach (var s in spec.Suffixes) _discoveredSuffixes.Add(s);
                        
                        UpdateVid(rootEntry.Rid, spec);
                        rootEntry.Variants.Add(spec);
                    }
                    else
                    {
                        unsupportedNorm.Add(norm);
                        unsupportedRaw.Add(orig);
                    }
                }
                
                // Sort variants deterministically
                rootEntry.Variants = rootEntry.Variants.OrderBy(v => v.Vid, StringComparer.Ordinal).ToList();
                _pack.Roots.Add(rootEntry);
            }

            // 3. Build Ops
            _pack.Prefixes = _discoveredPrefixes.OrderBy(p => p, StringComparer.Ordinal)
                                               .Select(p => new PrefixOp { Pid = $"P:{p}", Text = p })
                                               .ToList();
            _pack.Suffixes = _discoveredSuffixes.OrderBy(s => s, StringComparer.Ordinal)
                                               .Select(s => new SuffixOp { Sid = $"S:{s}", Text = s })
                                               .ToList();

            // 4. Fill Manifest Counts & Unsupported
            _pack.Manifest.Counts.Roots = _pack.Roots.Count;
            _pack.Manifest.Counts.PrefixOps = _pack.Prefixes.Count;
            _pack.Manifest.Counts.SuffixOps = _pack.Suffixes.Count;
            _pack.Manifest.Counts.VariantSpecs = _pack.Roots.Sum(r => r.Variants.Count);
            _pack.Manifest.Counts.UnsupportedCount = unsupportedNorm.Count;

            _pack.Manifest.UnsupportedVariants = unsupportedNorm.Zip(unsupportedRaw, (n, o) => new UnsupportedVariant { RawNormalized = n, RawOriginal = o })
                .OrderBy(v => v.RawNormalized, StringComparer.Ordinal).ToList();

            // 5. Final Sorting of Roots
            _pack.Roots = _pack.Roots.OrderBy(r => r.Rid, StringComparer.Ordinal).ToList();

            // 6. Canonical Hashing
            string canonical = CanonicalJson.Serialize(_pack);
            _pack.Manifest.AtlasPackSha256 = ComputeSha256(canonical);

            return _pack;
        }

        private bool TryParseVariant(string norm, string orig, out VariantSpec spec)
        {
            spec = new VariantSpec();
            spec.RawNormalized = norm;
            spec.RawOriginal = orig;

            if (string.IsNullOrEmpty(norm))
            {
                spec.Flags.Add("IDENTITY");
                return true;
            }

            var parts = norm.Split(',').Select(p => p.Trim()).Where(p => !string.IsNullOrEmpty(p)).ToList();
            if (parts.Count == 0) return false;

            var pList = new List<string>();
            var sList = new List<string>();

            foreach (var part in parts)
            {
                if (part.EndsWith("-") && !part.StartsWith("-"))
                {
                    pList.Add(part.Substring(0, part.Length - 1));
                }
                else if (part.StartsWith("-") && !part.EndsWith("-") && !part.StartsWith("--"))
                {
                    sList.Add(part.Substring(1));
                }
                else
                {
                    return false;
                }
            }

            spec.Prefixes = pList.Distinct().OrderBy(x => x, StringComparer.Ordinal).ToList();
            spec.Suffixes = sList.Distinct().OrderBy(x => x, StringComparer.Ordinal).ToList();

            if (spec.Prefixes.Any(p => !IsValidToken(p)) || spec.Suffixes.Any(s => !IsValidToken(s)))
            {
                return false;
            }

            return true;
        }

        private bool IsValidToken(string t)
        {
            foreach (char c in t)
            {
                if (!char.IsLower(c) && !char.IsDigit(c) && c != '.' && c != '_') return false;
            }
            return true;
        }

        private void UpdateVid(string rid, VariantSpec spec)
        {
             var pJson = CanonicalJson.Serialize(spec.Prefixes);
             var sJson = CanonicalJson.Serialize(spec.Suffixes);
             var fJson = CanonicalJson.Serialize(spec.Flags);
             string payload = rid + "|" + pJson + "|" + sJson + "|" + fJson;
             spec.Vid = ComputeSha256(payload);
        }

        private string ComputeSha256(string content)
        {
             byte[] bytes = Encoding.UTF8.GetBytes(content);
             using (var sha = SHA256.Create())
             {
                 return Convert.ToHexString(sha.ComputeHash(bytes)).ToLowerInvariant();
             }
        }
    }
}
