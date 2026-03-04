using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Oan.SoulFrame.Atlas;
using Oan.Place.GEL;
using Xunit;

namespace Oan.Tests.GEL
{
    public class AtlasPackTests
    {
        private const string AtlasDirPath = @"d:\Unity Projects\Game Design OAN\OAN Mortalis\src\Oan.SoulFrame\Atlas\Roots";
        private const string ArtifactPath = @"d:\Unity Projects\Game Design OAN\OAN Mortalis\artifacts\atlaspack";

        [Fact]
        public void AtlasSource_Immutable()
        {
            var source = AtlasSourceLoader.Load(AtlasDirPath);
            var initialHashes = new Dictionary<string, string>(source.FileHashes);

            var builder = new AtlasPackBuilder();
            builder.Build(source);

            var finalHashes = source.FileHashes;
            foreach (var kvp in initialHashes)
            {
                Assert.Equal(kvp.Value, finalHashes[kvp.Key]);
            }
        }

        [Fact]
        public void AtlasPack_Build_IsDeterministic()
        {
            var source = AtlasSourceLoader.Load(AtlasDirPath);
            var builder1 = new AtlasPackBuilder();
            var pack1 = builder1.Build(source);
            string json1 = CanonicalJson.Serialize(pack1);

            var builder2 = new AtlasPackBuilder();
            var pack2 = builder2.Build(source);
            string json2 = CanonicalJson.Serialize(pack2);

            Assert.Equal(json1, json2);
            Assert.Equal(pack1.Manifest.AtlasPackSha256, pack2.Manifest.AtlasPackSha256);
        }

        [Fact]
        public void AtlasPack_Hash_IsStable()
        {
            var source = AtlasSourceLoader.Load(AtlasDirPath);
            var builder = new AtlasPackBuilder();
            var pack = builder.Build(source);
            
            Assert.NotNull(pack.Manifest.AtlasPackSha256);
            Assert.Matches("^[a-f0-9]{64}$", pack.Manifest.AtlasPackSha256);
        }

        [Fact]
        public void NF_Normalization_IsStable()
        {
            var source = AtlasSourceLoader.Load(AtlasDirPath);
            var builder = new AtlasPackBuilder();
            var pack = builder.Build(source);

            foreach (var root in pack.Roots)
            {
                foreach (var variant in root.Variants)
                {
                    var sortedP = variant.Prefixes.OrderBy(x => x, StringComparer.Ordinal).ToList();
                    var sortedS = variant.Suffixes.OrderBy(x => x, StringComparer.Ordinal).ToList();
                    
                    Assert.Equal(sortedP, variant.Prefixes);
                    Assert.Equal(sortedS, variant.Suffixes);
                }
            }
        }

        [Fact]
        public void Full_Emission_Test()
        {
            var source = AtlasSourceLoader.Load(AtlasDirPath);
            var builder = new AtlasPackBuilder();
            var pack = builder.Build(source);
            
            AtlasPackEmitter.Emit(pack, ArtifactPath);
            
            Assert.True(File.Exists(Path.Combine(ArtifactPath, "atlaspack.json")));
            Assert.True(File.Exists(Path.Combine(ArtifactPath, "atlaspack.manifest.json")));
        }
    }
}
