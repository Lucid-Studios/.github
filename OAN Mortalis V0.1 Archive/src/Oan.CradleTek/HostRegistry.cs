using System;
using System.Collections.Generic;
using Oan.Place.Abstractions;

namespace Oan.CradleTek
{
    public class HostRegistry : IHostRegistry
    {
        public const string GEL0_SUBSTRATE = "GEL0_SUBSTRATE";

        private readonly Dictionary<string, object> _capabilities = new Dictionary<string, object>();
        private readonly List<IPlaceModule> _modules = new List<IPlaceModule>();

        public void RegisterCapability(string capabilityName, object implementation)
        {
            if (_capabilities.ContainsKey(capabilityName))
            {
                Console.WriteLine($"[HostRegistry] Warning: Overwriting capability {capabilityName}");
                _capabilities[capabilityName] = implementation;
            }
            else
            {
                _capabilities.Add(capabilityName, implementation);
            }
        }

        public T GetCapability<T>(string capabilityName)
        {
            if (_capabilities.TryGetValue(capabilityName, out var impl) && impl is T typedImpl)
            {
                return typedImpl;
            }
            return default!;
        }

        public async Task LoadModuleAsync(IPlaceModule module)
        {
            Console.WriteLine($"[HostRegistry] Initializing Module: {module.ModuleId} ({module.Description})");
            await module.InitializeAsync(this);
            _modules.Add(module);
        }

        public async Task ShutdownAsync()
        {
            foreach (var module in _modules)
            {
                await module.ShutdownAsync();
            }
        }
    }
}
