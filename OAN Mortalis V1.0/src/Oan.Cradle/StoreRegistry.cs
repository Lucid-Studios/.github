using Oan.Common;

namespace Oan.Cradle
{
    /// <summary>
    /// Structured, immutable container for all system stores.
    /// </summary>
    public sealed class StoreRegistry
    {
        public ITelemetrySink GovernanceTelemetry { get; }
        public ITelemetrySink StorageTelemetry { get; }

        public IPublicPlaneStores Public { get; }
        public ICrypticPlaneStores Cryptic { get; }

        public bool PublicAvailable { get; }
        public bool CrypticAvailable { get; }

        public StoreRegistry(
            ITelemetrySink governanceTelemetry,
            ITelemetrySink storageTelemetry,
            IPublicPlaneStores publicStores,
            bool publicAvailable,
            ICrypticPlaneStores crypticStores,
            bool crypticAvailable)
        {
            GovernanceTelemetry = governanceTelemetry;
            StorageTelemetry = storageTelemetry;
            Public = publicStores;
            PublicAvailable = publicAvailable;
            Cryptic = crypticStores;
            CrypticAvailable = crypticAvailable;
        }
    }
}
