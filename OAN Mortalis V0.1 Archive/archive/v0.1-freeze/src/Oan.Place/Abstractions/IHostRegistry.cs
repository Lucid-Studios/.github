using System;

namespace Oan.Place.Abstractions
{
    /// <summary>
    /// Registry interface provided by CradleTek for modules to register capabilities.
    /// </summary>
    public interface IHostRegistry
    {
        void RegisterCapability(string capabilityName, object implementation);
        T GetCapability<T>(string capabilityName);
    }
}
