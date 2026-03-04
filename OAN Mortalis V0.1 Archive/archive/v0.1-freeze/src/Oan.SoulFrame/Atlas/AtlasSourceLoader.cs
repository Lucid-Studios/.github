using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Oan.SoulFrame.Atlas
{
    public class AtlasSource
    {
        public Dictionary<string, string> FileHashes { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, byte[]> RawBytes { get; set; } = new Dictionary<string, byte[]>();
        public RootAtlasModel? ParsedRootAtlas { get; set; }
    }

    public class RootAtlasModel
    {
        public List<RootEntryModel> Roots { get; set; } = new();
    }

    public class RootEntryModel
    {
        public string Root { get; set; } = string.Empty;
        public List<string> Variants { get; set; } = new(); // Normalized
        public List<string> RawVariants { get; set; } = new(); // Original (un-normalized)
    }

    public static class AtlasSourceLoader
    {
        public static readonly string[] RequiredFiles = new[]
        {
            "RootAtlas.json",
            "RootIndex.json",
            "Roots.json",
            "SuffixIndex.json",
            "SymbolicIndex.json",
            "BaseSymbolCodex.jsonl"
        };

        public static AtlasSource Load(string directoryPath)
        {
            var source = new AtlasSource();

            foreach (var filename in RequiredFiles)
            {
                string path = Path.Combine(directoryPath, filename);
                if (!File.Exists(path))
                {
                    // Allow missing optional files? Prompt implied "Ensure these exist... as applicable".
                    // For now, consistent with previous behavior, throw if missing.
                    throw new FileNotFoundException($"Required Atlas source file not found: {filename}");
                }

                byte[] bytes = File.ReadAllBytes(path);
                source.RawBytes[filename] = bytes;
                source.FileHashes[filename] = ComputeSha256(bytes);

                if (filename == "RootAtlas.json")
                {
                    try
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var model = JsonSerializer.Deserialize<RootAtlasModel>(bytes, options);
                        if (model != null)
                        {
                            // Normalize in-place
                            NormalizeModel(model);
                            source.ParsedRootAtlas = model;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Wrap or log?
                        throw new InvalidOperationException("Failed to parse RootAtlas.json", ex);
                    }
                }
            }

            return source;
        }

        private static void NormalizeModel(RootAtlasModel model)
        {
            if (model.Roots != null)
            {
                foreach (var entry in model.Roots)
                {
                    entry.Root = Normalize(entry.Root);
                    if (entry.Variants != null)
                    {
                        entry.RawVariants = new List<string>(entry.Variants);
                        for (int i = 0; i < entry.Variants.Count; i++)
                        {
                            entry.Variants[i] = Normalize(entry.Variants[i]);
                        }
                    }
                }
            }
        }

        public static string Normalize(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text.Trim().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        }

        public static string ComputeSha256(byte[] bytes)
        {
            using (var sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(bytes);
                return Convert.ToHexString(hash).ToLowerInvariant();
            }
        }
    }
}
