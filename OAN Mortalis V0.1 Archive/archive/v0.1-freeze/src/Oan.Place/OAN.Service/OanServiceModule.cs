using System.Threading.Tasks;
using Oan.Place.Abstractions;

namespace Oan.Place.OAN.Service
{
    public class OanServiceModule : IPlaceModule
    {
        public string ModuleId => "OAN.Service";
        public string Description => "Ontological/Semantic Layer (Online Service)";
        public bool IsOnline => true;

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
