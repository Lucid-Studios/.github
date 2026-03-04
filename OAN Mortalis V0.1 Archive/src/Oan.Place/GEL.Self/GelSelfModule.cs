using System.Threading.Tasks;
using Oan.Place.Abstractions;

namespace Oan.Place.GEL.Self
{
    public class GelSelfModule : IPlaceModule
    {
        public string ModuleId => "GEL.Self";
        public string Description => "Geometric/Spatial Environmental Layer (Local/DLL)";
        public bool IsOnline => false;

        public Task InitializeAsync(IHostRegistry registry)
        {
            return Task.CompletedTask;
        }

        public Task ShutdownAsync()
        {
            return Task.CompletedTask;
        }
    }
}
