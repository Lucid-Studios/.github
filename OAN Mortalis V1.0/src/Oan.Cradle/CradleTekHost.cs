using System;
using System.Threading.Tasks;
using Oan.Common;
using Oan.SoulFrame;

namespace Oan.Cradle
{
    /// <summary>
    /// The headless runtime container and orchestrator.
    /// </summary>
    public sealed class CradleTekHost
    {
        private readonly StoreRegistry _stores;
        private readonly StackManager _stackManager;
        private bool _isInitialized;

        public CradleTekHost(StoreRegistry stores)
        {
            _stores = stores ?? throw new ArgumentNullException(nameof(stores));
            _stackManager = new StackManager(_stores);
        }

        public bool PublicAvailable => _stores.PublicAvailable;
        public bool CrypticAvailable => _stores.CrypticAvailable;
        public bool SpineAvailable { get; private set; } = true;

        /// <summary>
        /// Initializes the host and mounts all logic components.
        /// </summary>
        public async Task InitializeAsync()
        {
            // Sprint 1: Basic structure.
            _isInitialized = true;
            await Task.CompletedTask;
        }

        /// <summary>
        /// Orchestrates a deterministic evaluation.
        /// </summary>
        public async Task<EvaluateEnvelope> EvaluateAsync(string agentId, string theaterId, object input)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("Host not initialized.");

            var runtime = _stackManager.CreateStack(agentId, theaterId);

            // Safe-fail ladder enforcement:
            // 1. Spine check
            if (!SpineAvailable)
            {
                runtime.SoulFrame.Freeze();
            }

            // 2. SoulFrame state check
            if (runtime.SoulFrame.State == SoulFrameState.Halt)
                throw new InvalidOperationException("System in HARD HALT state.");

            if (runtime.SoulFrame.State == SoulFrameState.Frozen)
            {
                return new EvaluateEnvelope
                {
                    AgentId = agentId,
                    TheaterId = theaterId,
                    Decision = "FREEZE",
                    Note = "System is FROZEN. Evaluation blocked."
                };
            }

            // 3. Execution
            try 
            {
                return await runtime.EvaluateAsync(input);
            }
            catch (Exception ex)
            {
                // If evaluation itself fails catastrophically
                runtime.SoulFrame.HardHalt();
                throw new InvalidOperationException("Critical evaluation failure. Transitioning to HARD HALT.", ex);
            }
        }

        public void ReportSpineFailure()
        {
            SpineAvailable = false;
        }
    }
}
