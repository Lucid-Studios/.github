using System;
using System.Linq;
using System.Reflection;
using Xunit;
using Oan.Runtime;
using Oan.Core.Engrams;

namespace Oan.Tests.Architecture
{
    public class ArchitectureHardeningTests
    {
        [Fact]
        public void IntentProcessor_MustNotReference_RecallSurface()
        {
            var intentProcessorType = typeof(IntentProcessor);
            var constructors = intentProcessorType.GetConstructors();

            foreach (var ctor in constructors)
            {
                foreach (var param in ctor.GetParameters())
                {
                    // Check if parameter type implements IRecallSurface
                    if (typeof(IRecallSurface).IsAssignableFrom(param.ParameterType))
                    {
                        Assert.Fail($"IntentProcessor constructor parameter '{param.Name}' implement IRecallSurface. This violates the write-side isolation boundary.");
                    }

                    // Check if parameter type IS IRecallSurface
                    if (param.ParameterType == typeof(IRecallSurface))
                    {
                        Assert.Fail($"IntentProcessor constructor parameter '{param.Name}' is IRecallSurface. This violates the write-side isolation boundary.");
                    }
                }
            }
        }

        [Fact]
        public void OanRuntime_MustNotReference_AgentiCore()
        {
            // Verify that Oan.Runtime assembly does not reference Oan.AgentiCore
            var runtimeAssembly = typeof(IntentProcessor).Assembly;
            var referencedAssemblies = runtimeAssembly.GetReferencedAssemblies();

            var referencesAgentiCore = referencedAssemblies.Any(a => a.Name == "Oan.AgentiCore");
            
            Assert.False(referencesAgentiCore, "Oan.Runtime assembly must not reference Oan.AgentiCore. Intent processing must be isolated from Engram formation/storage mechanisms.");
        }
    }
}
