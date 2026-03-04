using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Oan.Place.GEL;
using Oan.SoulFrame.Atlas;
using Xunit;

namespace Oan.Tests.GEL
{
    public class Gel0VariantTests
    {
        [Fact]
        public void VariantParsing_EdgeCases()
        {
            var cases = new Dictionary<string, (bool Supported, string[]? P, string[]? S, bool Identity)>
            {
                { "", (true, new string[0], new string[0], true) },
                { "-ing", (true, null, new[]{"ing"}, false) },
                { "un-", (true, new[]{"un"}, null, false) },
                { "un-, -able", (true, new[]{"un"}, new[]{"able"}, false) },
                { "anti-, un-", (true, new[]{"anti", "un"}, null, false) }, 
                { "-able, -ness", (true, null, new[]{"able", "ness"}, false) }, 
                { "un-, -able, -ness", (true, new[]{"un"}, new[]{"able", "ness"}, false) },
                { "un-, ", (true, new[]{"un"}, null, false) }, 
                { ", -able", (true, null, new[]{"able"}, false) }, 
                { "un-, , -able", (true, new[]{"un"}, new[]{"able"}, false) }, 
                { " un- ,  -able ", (true, new[]{"un"}, new[]{"able"}, false) }, 
                
                // Unsupported
                { "un", (false, null, null, false) }, 
                { "un-able", (false, null, null, false) }, 
                { " ", (true, new string[0], new string[0], true) }, 
                { "-", (false, null, null, false) }, 
                { "--ing", (false, null, null, false) }, 
                { "un--able", (false, null, null, false) }, 
                { "un- -able", (false, null, null, false) }, 
                
                { "un-, un-", (true, new[]{"un"}, null, false) },
                { "-able, -able", (true, null, new[]{"able"}, false) },
                
                { ", , ", (false, null, null, false) }, 
                { "hello-world", (false, null, null, false) },
                
                { "café-", (true, new[]{"café"}, null, false) }, 
                { "UN-", (true, new[]{"un"}, null, false) }, 
            };

            foreach (var kvp in cases)
            {
                RunCase(kvp.Key, kvp.Value.Supported, kvp.Value.P, kvp.Value.S, kvp.Value.Identity);
            }
        }

        private void RunCase(string raw, bool expectSuccess, string[]? expectedP, string[]? expectedS, bool expectIdentity)
        {
            var source = new AtlasSource();
            var parsed = new RootAtlasModel();
            
            // Simulator: Loader would provide rawOriginal as well.
            string norm = AtlasSourceLoader.Normalize(raw);
            
            parsed.Roots.Add(new RootEntryModel 
            { 
                Root = "test", 
                Variants = new List<string> { norm },
                RawVariants = new List<string> { raw }
            });
            
            source.ParsedRootAtlas = parsed;

            var builder = new AtlasPackBuilder();
            var pack = builder.Build(source);
            
            var root = pack.Roots.FirstOrDefault();
            Assert.NotNull(root);
            
            if (expectSuccess)
            {
                Assert.Empty(pack.Manifest.UnsupportedVariants);
                Assert.Single(root.Variants);
                var spec = root.Variants[0];
                
                if (expectIdentity)
                {
                    Assert.Contains("IDENTITY", spec.Flags);
                }
                else
                {
                    if (expectedP != null) Assert.Equal(expectedP.OrderBy(x=>x).ToArray(), spec.Prefixes.ToArray());
                    if (expectedS != null) Assert.Equal(expectedS.OrderBy(x=>x).ToArray(), spec.Suffixes.ToArray());
                }
            }
            else
            {
                Assert.Single(pack.Manifest.UnsupportedVariants);
                Assert.Empty(root.Variants);
            }
        }
    }
}
