using System.Threading.Tasks;
using Oan.Place.Abstractions;

namespace Oan.Place.GOA.Self
{
    public class GoaSelfModule : IPlaceModule
    {
        public string ModuleId => "GOA.Self";
        public string Description => "Goal/Planning Layer (Local/DLL)";
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
