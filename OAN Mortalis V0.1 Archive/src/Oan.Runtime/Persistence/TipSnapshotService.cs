using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Oan.Core.Events;
using Oan.SoulFrame.Identity;

namespace Oan.Runtime.Persistence
{
    public interface ISnapshotTelemetrySink
    {
        void Emit(object evt);
    }

    public class TipSnapshotService
    {
        private readonly ISnapshotTelemetrySink _sink;
        private readonly Oan.SoulFrame.Identity.OpalTipRegistry _registry;

        public TipSnapshotService(Oan.SoulFrame.Identity.OpalTipRegistry registry, ISnapshotTelemetrySink sink)
        {
            _registry = registry;
            _sink = sink;
        }

        public bool TryLoad(string snapshotPath, string currentRootAtlasHash, string currentPolicyVersion)
        {
            _sink.Emit(new TipSnapshotLoadAttemptedEvent { PathHash = Oan.SoulFrame.Identity.TheaterIdentityService.HashBytes(System.Text.Encoding.UTF8.GetBytes(snapshotPath)) });

            if (!File.Exists(snapshotPath))
            {
                _sink.Emit(new TipSnapshotMissingEvent());
                return false;
            }

            try
            {
                string json = File.ReadAllText(snapshotPath);
                var snapshot = OpalTipSnapshot.FromJson(json);

                if (snapshot == null)
                {
                    _sink.Emit(new TipSnapshotRejectedEvent { ReasonCode = "PARSE_ERROR" });
                    return false;
                }

                if (snapshot.Schema != "opal.tips.v0.2")
                {
                    _sink.Emit(new TipSnapshotRejectedEvent { ReasonCode = "SCHEMA_MISMATCH" });
                    return false;
                }

                if (snapshot.RootAtlasHash != currentRootAtlasHash)
                {
                    _sink.Emit(new TipSnapshotRejectedEvent { ReasonCode = "ROOT_ATLAS_MISMATCH" });
                    return false;
                }

                if (snapshot.PolicyVersion != currentPolicyVersion)
                {
                    _sink.Emit(new TipSnapshotRejectedEvent { ReasonCode = "POLICY_MISMATCH" });
                    return false;
                }

                _registry.LoadTipSnapshot(snapshot.Theaters);
                _sink.Emit(new TipSnapshotLoadedEvent { TheaterCount = snapshot.Theaters.Count });
                return true;
            }
            catch (Exception)
            {
                _sink.Emit(new TipSnapshotRejectedEvent { ReasonCode = "FATAL_ERROR" });
                return false;
            }
        }

        public void Save(string snapshotPath, string runId, string sessionId, string rootAtlasHash, string policyVersion)
        {
            var allTips = _registry.GetAllTips();
            var snapshot = new OpalTipSnapshot
            {
                Schema = "opal.tips.v0.2",
                RunId = runId,
                SessionId = sessionId,
                RootAtlasHash = rootAtlasHash,
                PolicyVersion = policyVersion,
                Theaters = allTips.Select(kvp => new OpalTheaterTip { TheaterId = kvp.Key, Tip = kvp.Value }).ToList()
            };

            string json = snapshot.ToDeterministicJson();
            File.WriteAllText(snapshotPath, json);
            _sink.Emit(new TipSnapshotWrittenEvent { TheaterCount = snapshot.Theaters.Count });
        }
    }

    // Telemetry Events are now in Oan.Core.Events
}
