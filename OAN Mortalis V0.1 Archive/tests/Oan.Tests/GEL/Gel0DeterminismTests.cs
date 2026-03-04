using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Oan.Place.GEL;
using Oan.SoulFrame.Atlas;
using Xunit;

namespace Oan.Tests.GEL
{
    public class Gel0DeterminismTests : IDisposable
    {
        private readonly string _tempDir;
        private readonly string _srcDir;

        public Gel0DeterminismTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            _srcDir = Path.Combine(_tempDir, "source");
            
            Directory.CreateDirectory(_srcDir);
            
            // Create dummy AtlasSource
            CreateDummyAtlasSource(_srcDir);
        }

        private void CreateDummyAtlasSource(string path)
        {
            foreach (var f in AtlasSourceLoader.RequiredFiles)
            {
               if (f == "RootAtlas.json") continue;
               File.WriteAllBytes(Path.Combine(path, f), Encoding.UTF8.GetBytes("{}"));
            }
            
            string json = @"{ ""roots"": [ 
                { ""root"": ""move"", ""variants"": [""move"", ""-ing"", ""re-""] },
                { ""root"": ""test"", ""variants"": [""test"", ""testing"", ""un-tested""] }
            ] }";
            File.WriteAllBytes(Path.Combine(path, "RootAtlas.json"), Encoding.UTF8.GetBytes(json));
        }

        [Fact]
        public void AtlasPack_Build_IsDeterministic_BitEqual()
        {
            string out1 = Path.Combine(_tempDir, "out1");
            string out2 = Path.Combine(_tempDir, "out2");
            Directory.CreateDirectory(out1);
            Directory.CreateDirectory(out2);

            var source = AtlasSourceLoader.Load(_srcDir);

            // Run 1
            var b1 = new AtlasPackBuilder();
            var pack1 = b1.Build(source);
            AtlasPackEmitter.Emit(pack1, out1);
            
            // Run 2
            var b2 = new AtlasPackBuilder();
            var pack2 = b2.Build(source);
            AtlasPackEmitter.Emit(pack2, out2);

            // Compare bit-for-bit
            var files1 = Directory.GetFiles(out1).OrderBy(f => f).ToList();
            var files2 = Directory.GetFiles(out2).OrderBy(f => f).ToList();
            
            Assert.Equal(files1.Count, files2.Count);
            Assert.True(files1.Count >= 2); // atlaspack.json, atlaspack.manifest.json
            
            for (int i = 0; i < files1.Count; i++)
            {
                byte[] b1Bytes = File.ReadAllBytes(files1[i]);
                byte[] b2Bytes = File.ReadAllBytes(files2[i]);
                Assert.Equal(b1Bytes, b2Bytes);
            }
        }

        [Fact]
        public void AtlasPackSha256_IsStable()
        {
            var source = AtlasSourceLoader.Load(_srcDir);
            var b1 = new AtlasPackBuilder();
            var pack1 = b1.Build(source);
            
            Assert.NotNull(pack1.Manifest.AtlasPackSha256);
            Assert.Matches("^[a-f0-9]{64}$", pack1.Manifest.AtlasPackSha256);
            
            var b2 = new AtlasPackBuilder();
            var pack2 = b2.Build(source);
            Assert.Equal(pack1.Manifest.AtlasPackSha256, pack2.Manifest.AtlasPackSha256);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, true);
        }
    }
}
