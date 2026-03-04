using System.Threading.Tasks;
using Oan.Place.Abstractions;

namespace Oan.Place.GOA.Service
{
    public class GoaServiceModule : IPlaceModule
    {
        public string ModuleId => "GOA.Service";
        public string Description => "Goal/Planning Layer (Online Service)";
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
