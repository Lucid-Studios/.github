using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Oan.Tests.Architecture
{
    public class ArchitectureTests
    {
        [Fact]
        public void Oan_Core_Should_Not_Depend_On_Any_Other_Oan_Project()
        {
            var assembly = typeof(Oan.Core.Entity).Assembly;
            var dependencies = assembly.GetReferencedAssemblies();
            
            foreach (var dep in dependencies)
            {
                if (dep.Name == null) continue;
                Assert.False(dep.Name.StartsWith("Oan.") && dep.Name != "Oan.Core", 
                    $"Oan.Core must not reference {dep.Name}");
            }
        }

        [Fact]
        public void Oan_AgentiCore_Should_Only_Depend_On_Core()
        {
            var assembly = typeof(Oan.AgentiCore.IdentityKernel).Assembly;
            VerifyDependencies(assembly, "Oan.Core");
        }

        [Fact]
        public void Oan_SoulFrame_Should_Only_Depend_On_Core()
        {
            var assembly = typeof(Oan.SoulFrame.SoulFrameSession).Assembly;
            VerifyDependencies(assembly, "Oan.Core");
        }
        
        [Fact]
        public void Oan_Place_Abstractions_Should_Only_Depend_On_Core()
        {
            var assembly = typeof(Oan.Place.Abstractions.IPlaceModule).Assembly;
            VerifyDependencies(assembly, "Oan.Core");
        }

        [Fact]
        public void Oan_CradleTek_Should_Not_Depend_On_Host()
        {
            var assembly = typeof(Oan.CradleTek.HostRegistry).Assembly;
            var dependencies = assembly.GetReferencedAssemblies();

            foreach (var dep in dependencies)
            {
                 if (dep.Name == null) continue;
                 Assert.False(dep.Name.StartsWith("Oan.Host"), 
                    $"Oan.CradleTek must NOT reference composition root {dep.Name}");
            }
        }

        private void VerifyDependencies(Assembly assembly, params string[] allowedOanAssemblies)
        {
            var dependencies = assembly.GetReferencedAssemblies();
            foreach (var dep in dependencies)
            {
                if (dep.Name != null && dep.Name.StartsWith("Oan.") && dep.Name != assembly.GetName().Name)
                {
                    Assert.Contains(dep.Name, allowedOanAssemblies);
                }
            }
        }
    }
}
