using System;
using System.Collections.Generic;
using System.Linq;
using Oan.AgentiCore.Engrams;
using Oan.Core.Engrams;
using Oan.Core.Meaning;

using Oan.AgentiCore.Engrams.Data;

namespace Oan.Host.Cli
{
    public static class EngramCommands
    {
        public static void Handle(string[] args, EngramFormationService formationService, EngramStore store, EngramQueryService queryService, EngramProjectionService projectionService)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: engram <form|get|query> [args...]");
                return;
            }

            var subCommand = args[1];

            if (subCommand == "form")
            {
                // Simple arg parsing
                string sessionId = "cli-session";
                string operatorId = "cli-op";
                string policyVersion = "v1-cli";
                string? agentProfileId = null;
                string rootId = "root-000";
                string opalRootId = "opal-000";
                bool speculative = false;
                bool role = false;
                bool shared = false;
                bool local = true;
                string goal = "Default Goal";
                List<string> constraints = new List<string>();

                for(int i=2; i<args.Length; i++) 
                { 
                    if(args[i] == "--session" && i+1 < args.Length) sessionId = args[i+1];
                    if(args[i] == "--operator" && i+1 < args.Length) operatorId = args[i+1];
                    if(args[i] == "--agent" && i+1 < args.Length) agentProfileId = args[i+1];
                    if(args[i] == "--root" && i+1 < args.Length) rootId = args[i+1];
                    if(args[i] == "--opal" && i+1 < args.Length) opalRootId = args[i+1];
                    if(args[i] == "--speculative" && i+1 < args.Length) bool.TryParse(args[i+1], out speculative);
                    if(args[i] == "--role" && i+1 < args.Length) bool.TryParse(args[i+1], out role);
                    if(args[i] == "--shared" && i+1 < args.Length) bool.TryParse(args[i+1], out shared);
                    if(args[i] == "--local" && i+1 < args.Length) bool.TryParse(args[i+1], out local);
                    if(args[i] == "--goal" && i+1 < args.Length) goal = args[i+1];
                }

                var context = new FormationContext
                {
                    PolicyVersion = policyVersion,
                    Tick = DateTime.UtcNow.Ticks,
                    SessionId = sessionId,
                    OperatorId = operatorId,
                    AgentProfileId = agentProfileId,
                    RootId = rootId,
                    OpalRootId = opalRootId,
                    PreviousOpalEngramId = null,
                    
                    FrameLock = new FrameLock { Goal = goal, Mode = FrameMode.Clarify, Constraints = constraints },
                    Spans = new List<MeaningSpan>(),
                    
                    Speculative = speculative,
                    RoleBound = role,
                    SharedEligible = shared,
                    IdentityLocal = local,
                    
                    ParentEngramIds = new List<string>(),
                    EvidenceRefs = new List<EngramRef>()
                };

                try
                {
                    var block = formationService.FormEngram(context);

                    Console.WriteLine($"Engram Formed: {block.EngramId}");
                    Console.WriteLine($"Hash: {block.Hash}");
                    Console.WriteLine($"Channel: {block.Header.Channel}");
                    Console.WriteLine($"Reason: {block.Header.RoutingReason}");
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Error forming engram: {ex.Message}");
                }
            }
            else if (subCommand == "get")
            {
                string id = "";
                // Use args[2] as id if valid
                if (args.Length > 2) id = args[2];

                var block = queryService.GetById(id);
                if (block != null)
                {
                    var dto = projectionService.ToDto(block);
                    Console.WriteLine($"Engram Found: {dto.EngramId}");
                    Console.WriteLine($"Channel: {dto.Header.Channel}");
                    Console.WriteLine($"Tick: {dto.Header.Tick}");
                    Console.WriteLine($"Factors: {dto.Factors.Count}");
                }
                else
                {
                     Console.WriteLine("Engram not found.");
                }
            }
            else if (subCommand == "query")
            {
                string? root = null;
                string? session = null;
                EngramChannel? channel = null;
                int limit = 10;
                
                for(int i=2; i<args.Length; i++) 
                {
                    if(args[i] == "--root" && i+1 < args.Length) root = args[i+1];
                    if(args[i] == "--session" && i+1 < args.Length) session = args[i+1];
                    if(args[i] == "--channel" && i+1 < args.Length) 
                    {
                         if(Enum.TryParse<EngramChannel>(args[i+1], out var c)) channel = c;
                    }
                    if(args[i] == "--limit" && i+1 < args.Length) int.TryParse(args[i+1], out limit);
                }

                IEnumerable<Oan.Core.Engrams.EngramBlock> blocks = Array.Empty<Oan.Core.Engrams.EngramBlock>();

                if (root != null) 
                    blocks = queryService.QueryByRootId(root, limit);
                else if (session != null)
                    blocks = queryService.QueryBySessionId(session, limit);
                else if (channel != null)
                    blocks = queryService.QueryByChannel(channel.Value, limit);
                else 
                    Console.WriteLine("Please specify --root, --session, or --channel");

                var dtos = projectionService.ToDto(blocks);

                foreach(var b in dtos)
                {
                    Console.WriteLine($"{b.Header.Tick} | {b.EngramId} | {b.Header.Channel}");
                }
            }
            else
            {
                Console.WriteLine("Unknown subheading. Use 'form', 'get', or 'query'.");
            }
        }
    }
}
