using System;
using System.Threading.Tasks;

namespace Oan.Place.Abstractions
{
    /// <summary>
    /// Contract for a "Place" module (Tool) in the OAN ecosystem.
    /// Modules must register with CradleTek to be accessible.
    /// </summary>
    public interface IPlaceModule
    {
        string ModuleId { get; }
        string Description { get; }
        bool IsOnline { get; } // True = Service, False = Self (DLL)

        Task InitializeAsync(IHostRegistry registry);
        Task ShutdownAsync();
    }
}
