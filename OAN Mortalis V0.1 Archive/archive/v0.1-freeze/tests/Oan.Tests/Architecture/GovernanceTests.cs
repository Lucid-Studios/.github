using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Oan.Tests.Architecture
{
    public class GovernanceTests
    {
        [Fact]
        public void Repo_Should_Not_Contain_Unity_Remnants()
        {
            // Traverse up to solution root from execution path
            var cleanPath = Directory.GetCurrentDirectory();
            // Typically: .../tests/Oan.Tests/bin/Debug/net8.0
            // Go up 5 levels to reach solution root (safe check)
            var currentDir = new DirectoryInfo(cleanPath);
            var rootDir = currentDir.Parent?.Parent?.Parent?.Parent?.Parent;
            
            if (rootDir == null) return; // Should not happen in normal build

            // Just scan src/
            var srcDir = Path.Combine(rootDir.FullName, "src");
            if (!Directory.Exists(srcDir)) return; // Might be running in a different context

            var unityExtensions = new[] { ".meta", ".unity", ".prefab", ".mat", ".asset" };
            
            var files = Directory.GetFiles(srcDir, "*.*", SearchOption.AllDirectories)
                .Where(f => unityExtensions.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)));

            Assert.Empty(files);
        }
    }
}
