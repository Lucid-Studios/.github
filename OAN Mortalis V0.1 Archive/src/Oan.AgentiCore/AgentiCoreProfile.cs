using System.Collections.Generic;

namespace Oan.AgentiCore
{
    /// <summary>
    /// Represents a specific instantiation of an IdentityKernel within a SoulFrame.
    /// Acts as the "Active Persona" or "Role" that can be switched into.
    /// </summary>
    public class AgentiCoreProfile
    {
        /// <summary>
        /// Unique ID for this profile instance (e.g., "admin-profile-01").
        /// </summary>
        public string AgentProfileId { get; set; }

        /// <summary>
        /// Reference to the immutable Identity Kernel (Engram).
        /// </summary>
        public string EngramId { get; set; }

        /// <summary>
        /// Specific invariants or capabilities enabled for this profile.
        /// </summary>
        public List<string> Invariants { get; set; } = new List<string>();

        public AgentiCoreProfile(string profileId, string engramId)
        {
            AgentProfileId = profileId;
            EngramId = engramId;
        }
    }
}
