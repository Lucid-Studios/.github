using System.Threading.Tasks;
using Oan.Place.Abstractions;

namespace Oan.Place.GEL.Service
{
    public class GelServiceModule : IPlaceModule
    {
        public string ModuleId => "GEL.Service";
        public string Description => "Geometric/Spatial Environmental Layer (Online Service)";
        public bool IsOnline => true;

        public Task InitializeAsync(IHostRegistry registry)
        {
            // Register GEL capabilities here in future
            return Task.CompletedTask;
        }

        public Task ShutdownAsync()
        {
            return Task.CompletedTask;
        }
    }
}
