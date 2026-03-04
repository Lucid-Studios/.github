using System;
using System.Collections.Generic;
using Oan.Core.Governance;

namespace Oan.SoulFrame.Atlas
{
    public static class RootAtlasRegistry
    {
        private static readonly Dictionary<string, SliHandleEntry> _entries = new Dictionary<string, SliHandleEntry>
        {
            {
                "public/goa/objective.propose",
                new SliHandleEntry
                {
                    Handle = "public/goa/objective.propose",
                    IntentKind = "ProposeGoal",
                    Address = new SliAddress(SliChannel.Public, SliPartition.GOA, SliMirror.Standard),
                    RequiredSatModes = new HashSet<SatMode> { SatMode.Baseline, SatMode.Gate, SatMode.Standard, SatMode.Stronger },
                    PolicyVersion = "sli.policy.v0.1"
                }
            },
            {
                "public/oan/move.commit",
                new SliHandleEntry
                {
                    Handle = "public/oan/move.commit",
                    IntentKind = "MoveTo",
                    Address = new SliAddress(SliChannel.Public, SliPartition.OAN, SliMirror.Standard),
                    RequiredSatModes = new HashSet<SatMode> { SatMode.Baseline, SatMode.Gate, SatMode.Standard, SatMode.Stronger },
                    PolicyVersion = "sli.policy.v0.1"
                }
            },
            {
                "private/crypticgel/ref.store",
                new SliHandleEntry
                {
                    Handle = "private/crypticgel/ref.store",
                    IntentKind = "StoreRef",
                    Address = new SliAddress(SliChannel.Private, SliPartition.GEL, SliMirror.Cryptic),
                    RequiredSatModes = new HashSet<SatMode> { SatMode.Gate, SatMode.Standard }, // Requires Gate or stronger
                    PolicyVersion = "sli.policy.v0.1"
                }
            },
            {
                "sys/admin/mount.commit",
                new SliHandleEntry
                {
                    Handle = "sys/admin/mount.commit",
                    IntentKind = "MountCapability",
                    Address = new SliAddress(SliChannel.Public, SliPartition.OAN, SliMirror.Standard),
                    RequiredSatModes = new HashSet<SatMode> { SatMode.Baseline, SatMode.Gate, SatMode.Standard, SatMode.Stronger }, // Relaxed for bootstrap
                    PolicyVersion = "sli.policy.v0.1"
                }
            },
            {
                "sys/admin/sat.elevate.request",
                new SliHandleEntry
                {
                    Handle = "sys/admin/sat.elevate.request",
                    IntentKind = "RequestSatElevation",
                    Address = new SliAddress(SliChannel.Public, SliPartition.OAN, SliMirror.Standard),
                    RequiredSatModes = new HashSet<SatMode> { SatMode.Baseline, SatMode.Gate, SatMode.Standard, SatMode.Stronger },
                    PolicyVersion = "sli.policy.v0.2"
                }
            },
            {
                "sys/admin/theater.transition",
                new SliHandleEntry
                {
                    Handle = "sys/admin/theater.transition",
                    IntentKind = "TheaterTransition",
                    Address = new SliAddress(SliChannel.Public, SliPartition.OAN, SliMirror.Standard),
                    RequiredSatModes = new HashSet<SatMode> { SatMode.Baseline, SatMode.Gate, SatMode.Standard, SatMode.Stronger },
                    PolicyVersion = "sli.policy.v0.2"
                }
            },
            {
                "sys/admin/formation.promote",
                new SliHandleEntry
                {
                    Handle = "sys/admin/formation.promote",
                    IntentKind = "FormationPromote",
                    Address = new SliAddress(SliChannel.Public, SliPartition.OAN, SliMirror.Standard),
                    RequiredSatModes = new HashSet<SatMode> { SatMode.Baseline, SatMode.Gate, SatMode.Standard, SatMode.Stronger }, // Can be called from any mode usually, but maybe restrict?
                    PolicyVersion = "sli.policy.v0.2"
                }
            }
        };

        public static IReadOnlyList<SliHandleEntry> GetAllEntries()
        {
            var list = new List<SliHandleEntry>(_entries.Values);
            list.Sort((a, b) => string.Compare(a.Handle, b.Handle, StringComparison.Ordinal));
            return list;
        }

        public static SliHandleEntry? Get(string? handle)
        {
            if (string.IsNullOrEmpty(handle)) return null;
            return _entries.TryGetValue(handle, out var entry) ? entry : null;
        }
    }
}
