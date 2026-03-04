using System;
using System.IO;
using System.Text.Json;

namespace Oan.Place.GEL
{
    public static class AtlasPackEmitter
    {
        public static void Emit(AtlasPack pack, string outputDirectory)
        {
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            // 1. Emit consolidated pack
            string packJson = CanonicalJson.Serialize(pack);
            File.WriteAllText(Path.Combine(outputDirectory, "atlaspack.json"), packJson);

            // 2. Emit manifest only (slice)
            string manifestJson = CanonicalJson.Serialize(pack.Manifest);
            File.WriteAllText(Path.Combine(outputDirectory, "atlaspack.manifest.json"), manifestJson);
        }
    }
}
