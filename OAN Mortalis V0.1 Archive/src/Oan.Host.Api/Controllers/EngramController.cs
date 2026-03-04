using Microsoft.AspNetCore.Mvc;
using Oan.AgentiCore.Engrams;
using Oan.Core.Engrams;
using Oan.Core.Meaning;
using System.Collections.Generic;

namespace Oan.Host.Api.Controllers
{
    [ApiController]
    [Route("v1/engram")]
    public class EngramController : ControllerBase
    {
        private readonly EngramFormationService _formationService;
        private readonly EngramStore _store;

        public EngramController(EngramFormationService formationService, EngramStore store)
        {
            _formationService = formationService;
            _store = store;
        }

        [HttpPost("form")]
        public ActionResult<EngramBlock> FormEngram([FromBody] FormEngramRequest request)
        {
            var context = new FormationContext
            {
                PolicyVersion = request.PolicyVersion,
                Tick = request.Tick,
                SessionId = request.SessionId,
                OperatorId = request.OperatorId,
                AgentProfileId = request.AgentProfileId,
                
                RootId = request.RootId,
                OpalRootId = request.OpalRootId,
                PreviousOpalEngramId = request.PreviousOpalEngramId,
                ParentEngramIds = request.ParentEngramIds ?? new List<string>(),

                FrameLock = request.FrameLock,
                Spans = request.Spans ?? new List<MeaningSpan>(),
                
                KnowingMode = request.KnowingMode,
                MetabolicRegime = request.MetabolicRegime,
                ResolutionMode = request.ResolutionMode,

                Speculative = request.IsSpeculative,
                RoleBound = request.IsRoleBound,
                SharedEligible = request.IsSharedEligible,
                IdentityLocal = request.IsIdentityLocal,

                EvidenceRefs = request.EvidenceRefs ?? new List<EngramRef>()
            };

            try
            {
                var block = _formationService.FormEngram(context);
                return Ok(block);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public ActionResult<EngramBlock> GetEngram(string id)
        {
            var block = _store.GetById(id);
            if (block == null) return NotFound();
            return Ok(block);
        }
    }

    public class FormEngramRequest
    {
        public required string SessionId { get; set; }
        public required string OperatorId { get; set; }
        public required long Tick { get; set; }
        public required string PolicyVersion { get; set; }
        public required FrameLock FrameLock { get; set; }
        public List<MeaningSpan>? Spans { get; set; }
        public bool IsSpeculative { get; set; }
        public bool IsRoleBound { get; set; }
        public bool IsSharedEligible { get; set; }
        public bool IsIdentityLocal { get; set; }
        
        public required string RootId { get; set; }
        public required string OpalRootId { get; set; }
        public string? AgentProfileId { get; set; }
        public List<string>? ParentEngramIds { get; set; }
        public List<EngramRef>? EvidenceRefs { get; set; }
        public string? PreviousOpalEngramId { get; set; }

        public KnowingMode KnowingMode { get; set; } = KnowingMode.Propositional;
        public MetabolicRegime MetabolicRegime { get; set; } = MetabolicRegime.Exploration;
        public ResolutionMode ResolutionMode { get; set; } = ResolutionMode.Normal;
    }
}
