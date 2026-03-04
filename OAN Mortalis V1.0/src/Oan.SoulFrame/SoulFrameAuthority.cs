using System;
using Oan.Common;

namespace Oan.SoulFrame
{
    /// <summary>
    /// Current state of the SoulFrame enforcement boundary.
    /// </summary>
    public enum SoulFrameState
    {
        Operational,
        Frozen,
        Quarantined,
        Halt
    }

    /// <summary>
    /// The final authority boundary for OAN Mortalis.
    /// Manages state enforcement and safe-fail logic.
    /// </summary>
    public sealed class SoulFrameAuthority
    {
        private readonly ITelemetrySink _governanceTelemetry;

        public SoulFrameAuthority(ITelemetrySink governanceTelemetry)
        {
            _governanceTelemetry = governanceTelemetry ?? throw new ArgumentNullException(nameof(governanceTelemetry));
            State = SoulFrameState.Operational;
        }

        public SoulFrameState State { get; private set; }

        public bool IsOperational => State == SoulFrameState.Operational;

        /// <summary>
        /// Transitions to a Freeze state. Blocks evaluation but allows telemetry.
        /// </summary>
        public void Freeze()
        {
            TransitionTo(SoulFrameState.Frozen);
        }

        /// <summary>
        /// Transitions to a Quarantine state. Isolation mode.
        /// </summary>
        public void Quarantine()
        {
            TransitionTo(SoulFrameState.Quarantined);
        }

        /// <summary>
        /// Hard halt. Critical failure. No further operations allowed.
        /// </summary>
        public void HardHalt()
        {
            TransitionTo(SoulFrameState.Halt);
        }

        /// <summary>
        /// Authorizes a store write based on the current state.
        /// </summary>
        public bool IsWriteAuthorized(string plane, bool isIncident)
        {
            switch (State)
            {
                case SoulFrameState.Operational:
                    return true;
                case SoulFrameState.Frozen:
                    // C2 Forensic Freeze: Allows ONLY incident-only writes to Cryptic.
                    return plane == "Cryptic" && isIncident;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Verifies lineage continuity within a plane.
        /// </summary>
        public void VerifyLineage(string currentTip, string expectedTip)
        {
            if (currentTip != expectedTip)
            {
                throw new InvalidOperationException($"Lineage integrity violation: Expected {expectedTip}, found {currentTip}");
            }
        }

        /// <summary>
        /// Validates a promotion receipt against historical policy.
        /// </summary>
        public void ValidatePromotion(PromotionReceipt receipt)
        {
            if (State != SoulFrameState.Operational)
                throw new InvalidOperationException("Promotions allowed ONLY in Operational state.");

            if (receipt == null) throw new ArgumentNullException(nameof(receipt));

            // Policy-bound validation (Mock for now, will be SLI gated in Phase D)
            if (receipt.ResultingStandardHash == "none")
                throw new InvalidOperationException("Invalid PromotionReceipt: Missing resulting hash.");
        }

        private async void TransitionTo(SoulFrameState newState)
        {
            if (State == SoulFrameState.Halt) return;

            var oldState = State;
            State = newState;

            await _governanceTelemetry.EmitAsync(new
            {
                v = "1.0",
                type = "SoulFrameStateTransition",
                old_state = oldState.ToString(),
                new_state = newState.ToString(),
                timestamp = DateTimeOffset.UtcNow
            });
        }
    }
}
