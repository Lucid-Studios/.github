using System.Threading.Tasks;
using Oan.Place.Abstractions;

namespace Oan.Place.OAN.Self
{
    public class OanSelfModule : IPlaceModule
    {
        public string ModuleId => "OAN.Self";
        public string Description => "Ontological/Semantic Layer (Local/DLL)";
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
