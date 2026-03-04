using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using Oan.Core.Governance;

namespace Oan.SoulFrame.SLI
{
    public sealed class FileSliTelemetrySink : ISliTelemetrySink
    {
        private readonly string _filePath;

        public FileSliTelemetrySink(string filePath)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            
            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        public void Append(SliTelemetryRecord record)
        {
            File.AppendAllText(_filePath, Format(record) + "\n");
        }

        public void Append(DriverIngestionEvent evt)
        {
            File.AppendAllText(_filePath, Format(evt) + "\n");
        }

        public void Append(DriverCommitEvent evt)
        {
            File.AppendAllText(_filePath, Format(evt) + "\n");
        }

        public void Append(DriverSatElevationRequestEvent evt)
        {
            File.AppendAllText(_filePath, Format(evt) + "\n");
        }

        public void Append(DriverSatElevationOutcomeEvent evt)
        {
            File.AppendAllText(_filePath, Format(evt) + "\n");
        }

        /// <summary>
        /// Manual NDJSON formatter ensuring strict field ordering and determinism.
        /// </summary>
        public string Format(SliTelemetryRecord record)
        {
            var sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"Type\":\"SliResolve\",");
            sb.Append($"\"RunId\":\"{Escape(record.RunId)}\",");
            sb.Append($"\"Tick\":{record.Tick.ToString(CultureInfo.InvariantCulture)},");
            sb.Append($"\"SessionId\":\"{Escape(record.SessionId)}\",");
            sb.Append($"\"OperatorId\":\"{Escape(record.OperatorId)}\",");
            sb.Append($"\"ActiveSatMode\":\"{Escape(record.ActiveSatMode)}\",");
            
            sb.Append("\"MountedPartitions\":[");
            if (record.MountedPartitions != null)
            {
                for (int i = 0; i < record.MountedPartitions.Length; i++)
                {
                    sb.Append($"\"{Escape(record.MountedPartitions[i])}\"");
                    if (i < record.MountedPartitions.Length - 1) sb.Append(",");
                }
            }
            sb.Append("],");

            sb.Append($"\"RequestedHandle\":\"{Escape(record.RequestedHandle)}\",");
            sb.Append($"\"RequestedKind\":\"{Escape(record.RequestedKind)}\",");
            sb.Append($"\"ResolvedAddress\":\"{Escape(record.ResolvedAddress)}\",");
            sb.Append($"\"PartitionMounted\":{record.PartitionMounted.ToString().ToLowerInvariant()},");
            sb.Append($"\"SatSatisfied\":{record.SatSatisfied.ToString().ToLowerInvariant()},");
            sb.Append($"\"CrypticRequested\":{record.CrypticRequested.ToString().ToLowerInvariant()},");
            sb.Append($"\"MaskingApplied\":{record.MaskingApplied.ToString().ToLowerInvariant()},");
            sb.Append($"\"Allowed\":{record.Allowed.ToString().ToLowerInvariant()},");
            sb.Append($"\"ReasonCode\":\"{Escape(record.ReasonCode)}\",");
            sb.Append($"\"PolicyVersion\":\"{Escape(record.PolicyVersion)}\",");
            sb.Append($"\"MountPresent\":{record.MountPresent.ToString().ToLowerInvariant()},");
            sb.Append($"\"MountId\":{(record.MountId == null ? "null" : $"\"{Escape(record.MountId)}\"")},");
            sb.Append($"\"Notes\":{(record.Notes == null ? "null" : $"\"{Escape(record.Notes)}\"")}");
            sb.Append("}");
            return sb.ToString();
        }

        public string Format(DriverIngestionEvent evt)
        {
            var sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"Type\":\"Ingestion\",");
            sb.Append($"\"RunId\":\"{Escape(evt.RunId)}\",");
            sb.Append($"\"Tick\":{evt.Tick.ToString(CultureInfo.InvariantCulture)},");
            sb.Append($"\"Attempt\":{evt.Attempt.ToString(CultureInfo.InvariantCulture)},");
            sb.Append($"\"Outcome\":\"{evt.Outcome.ToString()}\",");
            
            sb.Append("\"MissingFields\":[");
            if (evt.MissingFields != null)
            {
                for (int i = 0; i < evt.MissingFields.Length; i++)
                {
                    sb.Append($"\"{Escape(evt.MissingFields[i])}\"");
                    if (i < evt.MissingFields.Length - 1) sb.Append(",");
                }
            }
            sb.Append("],");

            sb.Append($"\"ReasonCode\":{(evt.ReasonCode == null ? "null" : $"\"{Escape(evt.ReasonCode)}\"")},");
            
            sb.Append("\"Raw\":");
            if (evt.Raw == null) sb.Append("null");
            else
            {
                sb.Append("{");
                sb.Append($"\"Subject\":{(evt.Raw.Subject == null ? "null" : $"\"{Escape(evt.Raw.Subject)}\"")},");
                sb.Append($"\"Predicate\":{(evt.Raw.Predicate == null ? "null" : $"\"{Escape(evt.Raw.Predicate)}\"")},");
                sb.Append($"\"Scope\":{(evt.Raw.Scope == null ? "null" : $"\"{Escape(evt.Raw.Scope)}\"")}");
                sb.Append("}");
            }
            
            sb.Append("}");
            return sb.ToString();
        }

        public string Format(DriverCommitEvent evt)
        {
            var sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"Type\":\"Commit\",");
            sb.Append($"\"RunId\":\"{Escape(evt.RunId)}\",");
            sb.Append($"\"Tick\":{evt.Tick.ToString(CultureInfo.InvariantCulture)},");
            sb.Append($"\"IntentId\":\"{evt.IntentId.ToString()}\",");
            sb.Append($"\"Result\":\"{Escape(evt.Result)}\",");
            sb.Append($"\"ReasonCode\":{(evt.ReasonCode == null ? "null" : $"\"{Escape(evt.ReasonCode)}\"")}");
            sb.Append("}");
            return sb.ToString();
        }

        public string Format(DriverSatElevationRequestEvent evt)
        {
            var sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"Type\":\"SatElevationRequested\",");
            sb.Append($"\"RunId\":\"{Escape(evt.RunId)}\",");
            sb.Append($"\"Tick\":{evt.Tick.ToString(CultureInfo.InvariantCulture)},");
            sb.Append($"\"SessionId\":\"{Escape(evt.SessionId)}\",");
            sb.Append($"\"RequestedMode\":\"{Escape(evt.RequestedMode)}\",");
            sb.Append($"\"TargetAddress\":\"{Escape(evt.TargetAddress)}\",");
            sb.Append($"\"Reason\":{(evt.Reason == null ? "null" : $"\"{Escape(evt.Reason)}\"")}");
            sb.Append("}");
            return sb.ToString();
        }

        public string Format(DriverSatElevationOutcomeEvent evt)
        {
            var sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"Type\":\"SatElevationOutcome\",");
            sb.Append($"\"RunId\":\"{Escape(evt.RunId)}\",");
            sb.Append($"\"Tick\":{evt.Tick.ToString(CultureInfo.InvariantCulture)},");
            sb.Append($"\"Result\":\"{Escape(evt.Result)}\",");
            sb.Append($"\"ReasonCode\":{(evt.ReasonCode == null ? "null" : $"\"{Escape(evt.ReasonCode)}\"")},");
            sb.Append($"\"ResultingMode\":\"{Escape(evt.ResultingMode ?? "None")}\"");
            sb.Append("}");
            return sb.ToString();
        }

        private static string Escape(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}
