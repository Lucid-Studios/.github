using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Oan.Core.Events;
using Oan.Core.Governance;
using Oan.SoulFrame.Atlas;

namespace Oan.SoulFrame.Identity
{
    public static class TheaterIdentityService
    {
        public static void Test() { }
        
        public static string ComputeRunIdDeterministic(string sessionId, string scenarioName, string operatorId, long genesisTick)
        {
            // Input: [SessionId][Scenario][Operator][Tick]
            byte[] input = CanonicalBytes(
                sessionId, 
                scenarioName, 
                operatorId, 
                genesisTick.ToString());
            
            return HashBytes(input);
        }

        public static string ComputeRootAtlasHash()
        {
            var entries = RootAtlasRegistry.GetAllEntries();
            // Layout: for each entry [Handle][Kind][Channel][Partition][Mirror][Policy][Mode1][Mode2]...
            var fieldList = new List<string?>();
            
            foreach (var entry in entries)
            {
                fieldList.Add(entry.Handle);
                fieldList.Add(entry.IntentKind);
                fieldList.Add(entry.Address.Channel.ToString());
                fieldList.Add(entry.Address.Partition.ToString());
                fieldList.Add(entry.Address.Mirror.ToString());
                fieldList.Add(entry.PolicyVersion);
                
                var sortedModes = entry.RequiredSatModes.OrderBy(m => m).ToList();
                foreach (var mode in sortedModes) fieldList.Add(mode.ToString());
            }

            return HashBytes(CanonicalBytes(fieldList.ToArray()));
        }

        public static string ComputeTheaterId(string runId, string rootAtlasHash, string theaterPolicyVersion, long genesisTick)
        {
            // Input: [RunId][RootAtlasHash][Policy][Tick]
            byte[] input = CanonicalBytes(
                runId, 
                rootAtlasHash, 
                theaterPolicyVersion, 
                genesisTick.ToString());
            
            return HashBytes(input);
        }

        public static EngrammitizedEvent BuildEngrammitizedEvent(
            string theaterId, 
            string? parentTip, 
            string normalFormKey, 
            IEnumerable<string> witnessEventIds, 
            long tick)
        {
            var sortedIds = witnessEventIds.OrderBy(id => id, StringComparer.Ordinal).ToList();
            
            // New Tip is SHA256(ParentTip + NormalFormKey + SortedWitnessIds)
            // Input: [ParentTip][NFK][Id1][Id2]...
            
            var tipFields = new List<string?> { parentTip, normalFormKey };
            tipFields.AddRange(sortedIds);
            
            byte[] tipInput = CanonicalBytes(tipFields.ToArray());
            string newTip = HashBytes(tipInput);

            return new EngrammitizedEvent
            {
                TheaterId = theaterId,
                ParentTip = parentTip,
                NewTip = newTip,
                NormalFormKey = normalFormKey,
                WitnessEventIds = sortedIds,
                Tick = tick
            };
        }

        public static byte[] CanonicalBytes(params string?[] fields)
        {
            var parts = new List<byte[]>();
            int totalLength = 0;

            foreach (var field in fields)
            {
                if (field == null)
                {
                    // Length: -1 (4 bytes)
                    byte[] lenBytes = BitConverter.GetBytes((int)-1);
                    if (BitConverter.IsLittleEndian) Array.Reverse(lenBytes);
                    parts.Add(lenBytes);
                    totalLength += 4;
                }
                else
                {
                    byte[] utf8 = Encoding.UTF8.GetBytes(field);
                    // Length: N (4 bytes)
                    byte[] lenBytes = BitConverter.GetBytes((int)utf8.Length);
                    if (BitConverter.IsLittleEndian) Array.Reverse(lenBytes);
                    
                    parts.Add(lenBytes);
                    parts.Add(utf8);
                    totalLength += 4 + utf8.Length;
                }
            }

            byte[] result = new byte[totalLength];
            int offset = 0;
            foreach (var part in parts)
            {
                Buffer.BlockCopy(part, 0, result, offset, part.Length);
                offset += part.Length;
            }
            return result;
        }

        public static byte[] ComputeNormalFormKeyInput(string policyVersion, string intentKind, string sliHandle, IEnumerable<string> witnessTypes)
        {
            var sortedWitnesses = witnessTypes.OrderBy(w => w, StringComparer.Ordinal).ToArray();
            var list = new List<string?>
            {
                policyVersion,
                intentKind,
                sliHandle
            };
            list.AddRange(sortedWitnesses);

            return CanonicalBytes(list.ToArray());
        }

        public static string HashBytes(byte[] input)
        {
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(input);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}
