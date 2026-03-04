using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Oan.Place.GEL;
using Xunit;

namespace Oan.Tests.GEL
{
    public class GelMorphemeChartBuilderTests
    {
        private readonly Xunit.Abstractions.ITestOutputHelper _output;
        private const string AtlasDirPath = @"d:\Unity Projects\Game Design OAN\OAN Mortalis\src\Oan.SoulFrame\Atlas\Roots";

        public GelMorphemeChartBuilderTests(Xunit.Abstractions.ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void NormalizeSurface_FollowsStrictRules()
        {
            // Unicode NFC + lowercase
            Assert.Equal("\u00e1", GelMorphemeChartBuilder.NormalizeSurface("A\u0301")); // Á -> á (U+00E1)
            
            // Trim
            Assert.Equal("test", GelMorphemeChartBuilder.NormalizeSurface("  test  "));
            
            // Hyphen stripping (leading/trailing)
            Assert.Equal("able", GelMorphemeChartBuilder.NormalizeSurface("-able"));
            Assert.Equal("anti", GelMorphemeChartBuilder.NormalizeSurface("anti-"));
            Assert.Equal("un-able", GelMorphemeChartBuilder.NormalizeSurface("-un-able-")); // Internal hyphens preserved
            
            // Collapse multiple hyphens
            Assert.Equal("a-b", GelMorphemeChartBuilder.NormalizeSurface("a---b"));
            
            // Collapse whitespace
            Assert.Equal("a b", GelMorphemeChartBuilder.NormalizeSurface("a   b"));
            
            // Empty / Whitespace only -> Fail
            Assert.Throws<InvalidOperationException>(() => GelMorphemeChartBuilder.NormalizeSurface("   "));
            Assert.Throws<InvalidOperationException>(() => GelMorphemeChartBuilder.NormalizeSurface("-"));
        }

        [Fact]
        public void LoadSources_ProducesDeterministicHashesAndCounts()
        {
            var builder = new GelMorphemeChartBuilder();
            var sources = builder.LoadSources(AtlasDirPath);

            Assert.NotNull(sources);
            Assert.True(sources.FileHashes.Count == 6);
            Assert.True(sources.FileSizes.Count == 6);

            // Basic presence checks for DTOs
            Assert.NotEmpty(sources.BaseSymbolCodex);
            Assert.Equal(JsonValueKind.Object, sources.RootAtlas.ValueKind);
            Assert.Equal(JsonValueKind.Object, sources.RootIndex.ValueKind);
            Assert.Equal(JsonValueKind.Object, sources.Roots.Roots.ValueKind);
            Assert.Equal(JsonValueKind.Object, sources.SuffixIndex.ValueKind);
            Assert.Equal(JsonValueKind.Object, sources.SymbolicIndex.Prefixes.ValueKind);
            
            builder.PrintReport(sources); 
        }

        [Fact]
        public void LoadSources_FailsOnMissingFile()
        {
            var builder = new GelMorphemeChartBuilder();
            Assert.Throws<FileNotFoundException>(() => builder.LoadSources(@"C:\NonExistentPath"));
        }

        [Fact]
        public void LoadSources_FailsOnInvalidJson()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);
            
            // Create dummy files but one is invalid JSON
            File.WriteAllText(Path.Combine(tempPath, "BaseSymbolCodex.jsonl"), "{}");
            File.WriteAllText(Path.Combine(tempPath, "RootAtlas.json"), "invalid{json}");
            File.WriteAllText(Path.Combine(tempPath, "RootIndex.json"), "{}");
            File.WriteAllText(Path.Combine(tempPath, "Roots.json"), "{\"roots\":{}}");
            File.WriteAllText(Path.Combine(tempPath, "SuffixIndex.json"), "{}");
            File.WriteAllText(Path.Combine(tempPath, "SymbolicIndex.json"), "{}");

            var builder = new GelMorphemeChartBuilder();
            var ex = Assert.Throws<InvalidOperationException>(() => builder.LoadSources(tempPath));
            Assert.Contains(GelMorphemeChartReasonCode.ATLAS_SOURCE_INVALID_JSON, ex.Message);

            Directory.Delete(tempPath, true);
        }

        [Fact]
        public void BuildDraft_And_Seal_ProducesNonEmptyAndStableHash()
        {
            var builder = new GelMorphemeChartBuilder();
            var sources = builder.LoadSources(AtlasDirPath);
            
            // First run
            var draft1 = builder.BuildDraft(sources);
            string pack1 = builder.SerializeCanonical(draft1);
            using var sha = SHA256.Create();
            string hash1 = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(pack1)));

            Assert.NotEmpty(pack1);
            Assert.True(draft1.SymKind.Count > 0);

            // Second run (stability)
            var draft2 = builder.BuildDraft(sources);
            string pack2 = builder.SerializeCanonical(draft2);
            string hash2 = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(pack2)));

            Assert.Equal(hash1, hash2);

            // Round-trip invariants (3 test tuples)
            // We'll pick 3 symbols if they exist
            if (draft1.SymKind.Count >= 3)
            {
                var keys = draft1.SymKind.Keys.Take(3).ToList();
                foreach(var k in keys)
                {
                    var kind = draft1.SymKind[k];
                    // Assemble requires surface, but we used Symbol ID as surface in Seed if "symbol" not found.
                    // Actually Seed logic uses surface from JSON property Name.
                    string surface = draft1.Surfaces[k].First();
                    
                    if (kind == Kind.Root)
                    {
                        string nf = builder.Assemble(null, surface, null);
                        var (p, r, s) = builder.Factor(nf);
                        Assert.Null(p);
                        Assert.Equal(k, r);
                        Assert.Null(s);
                    }
                }
            }

            // Emit check
            string tempOut = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempOut);
            var (pPath, mPath) = builder.Emit(tempOut, sources, draft1);
            
            Assert.True(File.Exists(pPath));
            Assert.True(File.Exists(mPath));
            
            Directory.Delete(tempOut, true);
        }
    }
}
