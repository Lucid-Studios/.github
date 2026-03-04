using System;
using System.Linq;
using Oan.Core;
using Oan.Core.Governance;
using Oan.SoulFrame.Atlas;

namespace Oan.SoulFrame.SLI
{
    public class SliGateService
    {
        public const string ERR_HANDLE_MISSING = "SLI.HANDLE.MISSING";
        public const string ERR_HANDLE_UNKNOWN = "SLI.HANDLE.UNKNOWN";
        public const string ERR_MOUNT_NOT_PRESENT = "MOUNT_NOT_PRESENT";
        public const string ERR_SAT_MODE_INSUFFICIENT = "SLI.SAT_MODE.INSUFFICIENT";
        public const string ERR_CRYPTIC_PUBLIC_DENY = "SLI.CRYPTIC.PUBLIC_DENY";

        private readonly ISliTelemetrySink _telemetry;

        public SliGateService(ISliTelemetrySink? telemetry = null)
        {
            _telemetry = telemetry ?? new NullSliTelemetrySink();
        }

        public SliResolutionResult Resolve(Intent intent, SoulFrameSession session, SatMode currentSatMode, long tick, string runId = "none")
        {
            if (intent == null) throw new ArgumentNullException(nameof(intent));
            if (session == null) throw new ArgumentNullException(nameof(session));

            SliResolutionResult result;
            var entry = RootAtlasRegistry.Get(intent.SliHandle);

            // 1. Validate handle exists in registry
            if (string.IsNullOrWhiteSpace(intent.SliHandle))
            {
                result = Deny(intent.SliHandle, ERR_HANDLE_MISSING, "No policy version", SliMirror.Standard);
            }
            else if (entry == null)
            {
                result = Deny(intent.SliHandle, ERR_HANDLE_UNKNOWN, "No policy version", SliMirror.Standard);
            }
            else
            {
                // Intermediate checks for telemetry
                bool mountPresent = session.Mounts.IsMounted(entry.Address, out var mount);
                bool satSatisfied = entry.RequiredSatModes.Contains(currentSatMode);
                bool crypticRequested = entry.Address.Mirror == SliMirror.Cryptic;
                bool crypticBaselineAudit = crypticRequested && currentSatMode == SatMode.Baseline;
                bool crypticPublicDeny = crypticRequested && entry.Address.Channel == SliChannel.Public;

                bool isBootstrapHandle = intent.SliHandle == "sys/admin/mount.commit" || 
                                         intent.SliHandle == "sys/admin/sat.elevate.request" ||
                                         intent.SliHandle == "sys/admin/theater.transition" ||
                                         intent.SliHandle == "sys/admin/formation.promote";

                if (!mountPresent && !isBootstrapHandle)
                {
                    result = Deny(intent.SliHandle, ERR_MOUNT_NOT_PRESENT, entry.PolicyVersion, entry.Address.Mirror);
                }
                else if (!satSatisfied || crypticBaselineAudit)
                {
                    result = Deny(intent.SliHandle, ERR_SAT_MODE_INSUFFICIENT, entry.PolicyVersion, entry.Address.Mirror);
                }
                else if (crypticPublicDeny)
                {
                    result = Deny(intent.SliHandle, ERR_CRYPTIC_PUBLIC_DENY, entry.PolicyVersion, entry.Address.Mirror);
                }
                else
                {
                    // Success
                    result = new SliResolutionResult
                    {
                        Allowed = true,
                        ReasonCode = "ADMISSIBLE",
                        PolicyVersion = entry.PolicyVersion,
                        Handle = entry.Handle,
                        ResolvedAddress = entry.Address,
                        SatModeAtDecision = currentSatMode,
                        MaskingApplied = crypticRequested
                    };
                }
            }

            EmitTelemetry(runId, tick, intent, session, currentSatMode, entry, result);

            return result;
        }

        private void EmitTelemetry(string runId, long tick, Intent intent, SoulFrameSession session, SatMode mode, SliHandleEntry? entry, SliResolutionResult result)
        {
            bool mountPresent = entry != null && session.Mounts.IsMounted(entry.Address, out var mount);

            _telemetry.Append(new SliTelemetryRecord
            {
                RunId = runId,
                Tick = tick,
                SessionId = session.SessionId,
                OperatorId = session.OperatorId,
                ActiveSatMode = mode.ToString(),
                MountedPartitions = session.Mounts.GetActiveMounts().Select(m => m.Address.Partition.ToString()).Distinct().OrderBy(s => s, StringComparer.Ordinal).ToArray(),
                RequestedHandle = intent.SliHandle ?? "null",
                RequestedKind = intent.Action,
                ResolvedAddress = entry == null ? "None" : $"{entry.Address.Channel}/{entry.Address.Partition}/{entry.Address.Mirror}",
                PartitionMounted = mountPresent,
                SatSatisfied = entry != null && entry.RequiredSatModes.Contains(mode),
                CrypticRequested = entry != null && entry.Address.Mirror == SliMirror.Cryptic,
                MaskingApplied = result.MaskingApplied,
                Allowed = result.Allowed,
                ReasonCode = result.ReasonCode,
                PolicyVersion = result.PolicyVersion,
                MountPresent = mountPresent,
                MountId = (entry != null && session.Mounts.IsMounted(entry.Address, out var m)) ? m?.MountId : null
            });
        }

        private SliResolutionResult Deny(string? handle, string code, string policy, SliMirror mirror)
        {
            return new SliResolutionResult
            {
                Allowed = false,
                ReasonCode = code,
                PolicyVersion = policy,
                Handle = handle,
                ResolvedAddress = new SliAddress(SliChannel.Public, SliPartition.OAN, mirror),
                SatModeAtDecision = SatMode.Baseline,
                MaskingApplied = false
            };
        }
    }
}
