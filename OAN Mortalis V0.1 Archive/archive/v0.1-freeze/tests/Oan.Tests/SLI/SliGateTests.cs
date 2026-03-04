using System.Collections.Generic;
using System.Linq;
using Oan.Core;
using Oan.Core.Events;
using Oan.Core.Governance;
using Oan.SoulFrame;
using Oan.SoulFrame.SLI;
using Oan.Ledger;
using Oan.Runtime;
using Xunit;

namespace Oan.Tests.SLI
{
    public class SliGateTests
    {
        private readonly WorldState _world;
        private readonly EventLog _ledger;
        private readonly SoulFrameSession _session;
        private readonly SliGateService _gate;

        public SliGateTests()
        {
            _world = new WorldState();
            _ledger = new EventLog();
            _session = new SoulFrameSession("test-session", "tester");
            _gate = new SliGateService();
        }

        [Fact]
        public void NoHandle_NoAction_Denied()
        {
            var intent = new Intent { SourceAgentId = "a1", AgentProfileId = "a1", Action = "Move", SliHandle = null };
            var result = _gate.Resolve(intent, _session, SatMode.Baseline, 0);

            Assert.False(result.Allowed);
            Assert.Equal("SLI.HANDLE.MISSING", result.ReasonCode);
            Assert.NotNull(result.PolicyVersion);
        }

        [Fact]
        public void UnknownHandle_Denied()
        {
            var intent = new Intent { SourceAgentId = "a1", AgentProfileId = "a1", Action = "Move", SliHandle = "invalid/handle" };
            var result = _gate.Resolve(intent, _session, SatMode.Baseline, 0);

            Assert.False(result.Allowed);
            Assert.Equal("SLI.HANDLE.UNKNOWN", result.ReasonCode);
        }

        [Fact]
        public void HandleExists_ButPartitionNotMounted_Denied()
        {
            // public/goa/objective.propose is on GOA partition
            var intent = new Intent { SourceAgentId = "a1", AgentProfileId = "a1", Action = "ProposeGoal", SliHandle = "public/goa/objective.propose" };
            
            // Session has NO partitions mounted
            var result = _gate.Resolve(intent, _session, SatMode.Baseline, 0);

            Assert.False(result.Allowed);
            Assert.Equal("MOUNT_NOT_PRESENT", result.ReasonCode);
        }

        [Fact]
        public void Cryptic_PrivateSatGate_Allowed_MaskingAppliedTrue()
        {
            // private/crypticgel/ref.store is Private/GEL/Cryptic, requires SatMode.Gate or higher
            var intent = new Intent { SourceAgentId = "a1", AgentProfileId = "a1", Action = "StoreRef", SliHandle = "private/crypticgel/ref.store" };
            
            _session.Mounts.TryAddMount(new MountEntry
            {
                Address = new SliAddress(SliChannel.Private, SliPartition.GEL, SliMirror.Cryptic),
                MountId = "m-gel-cryptic",
                PolicyVersion = "v1",
                SatCeiling = SatMode.Stronger,
                RequiresHitlForElevation = false,
                CreatedTick = 0
            });

            var result = _gate.Resolve(intent, _session, SatMode.Gate, 0);

            Assert.True(result.Allowed);
            Assert.True(result.MaskingApplied);
            Assert.Equal(SliMirror.Cryptic, result.ResolvedAddress.Mirror);
        }

        [Fact]
        public void Cryptic_PrivateButSatNotGate_Denied()
        {
            var intent = new Intent { SourceAgentId = "a1", AgentProfileId = "a1", Action = "StoreRef", SliHandle = "private/crypticgel/ref.store" };
            _session.Mounts.TryAddMount(new MountEntry
            {
                Address = new SliAddress(SliChannel.Private, SliPartition.GEL, SliMirror.Cryptic),
                MountId = "m-gel-cryptic",
                PolicyVersion = "v1",
                SatCeiling = SatMode.Stronger,
                RequiresHitlForElevation = false,
                CreatedTick = 0
            });

            // Using Baseline mode
            var result = _gate.Resolve(intent, _session, SatMode.Baseline, 0);

            Assert.False(result.Allowed);
            Assert.Equal("SLI.SAT_MODE.INSUFFICIENT", result.ReasonCode);
        }

        [Fact]
        public void Determinism_SameInput_SameResult()
        {
            var intent = new Intent { SourceAgentId = "a1", AgentProfileId = "a1", Action = "Move", SliHandle = "public/oan/move.commit" };
            _session.Mounts.TryAddMount(new MountEntry
            {
                Address = new SliAddress(SliChannel.Public, SliPartition.OAN, SliMirror.Standard),
                MountId = "m-oan-std",
                PolicyVersion = "v1",
                SatCeiling = SatMode.Standard,
                RequiresHitlForElevation = false,
                CreatedTick = 0
            });

            var res1 = _gate.Resolve(intent, _session, SatMode.Standard, 0);
            var res2 = _gate.Resolve(intent, _session, SatMode.Standard, 0);

            Assert.Equal(res1.Allowed, res2.Allowed);
            Assert.Equal(res1.ReasonCode, res2.ReasonCode);
            Assert.Equal(res1.ResolvedAddress, res2.ResolvedAddress);
        }

        [Fact]
        public void Ledger_DecisionEvents_Appended()
        {
            // This test verifies the IntentProcessor integration
            var processor = new Oan.Runtime.IntentProcessor(_world, _session, _ledger, _gate);
            _session.AddToRoster("a1");
            _session.SetTheaterMode(TheaterMode.Prime, "test-theater");
            _session.Apply(new AgentActivationChangedEvent { ToAgentProfileId = "a1", WorldTick = 0, SoulFrameSessionId = "s", OperatorId = "o", Reason = "r" });
            _session.Mounts.TryAddMount(new MountEntry
            {
                Address = new SliAddress(SliChannel.Public, SliPartition.OAN, SliMirror.Standard),
                MountId = "m-oan-std",
                PolicyVersion = "v1",
                SatCeiling = SatMode.Standard,
                RequiresHitlForElevation = false,
                CreatedTick = 0
            });

            var intent = new Intent { SourceAgentId = "a1", AgentProfileId = "a1", Action = "Move", SliHandle = "public/oan/move.commit" };
            
            // 1. Allow case
            processor.EvaluateIntent(intent);
            
            // 2. Deny case (Unknown handle)
            var badIntent = new Intent { SourceAgentId = "a1", AgentProfileId = "a1", Action = "Move", SliHandle = "unknown" };
            processor.EvaluateIntent(badIntent);

            var decisions = _ledger.GetEvents().Where(e => e.Type == "SliDecision").ToList();
            Assert.Equal(2, decisions.Count);

            var allowPayload = decisions[0].Payload as SliDecisionEvent;
            Assert.True(allowPayload!.Allowed);
            
            var denyPayload = decisions[1].Payload as SliDecisionEvent;
            Assert.False(denyPayload!.Allowed);
        }
    }
}
