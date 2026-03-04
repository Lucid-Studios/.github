using System;

namespace Oan.AgentiCore
{
    /// <summary>
    /// Represents an active participant in the CME framework.
    /// Can be a User, an NPC, or a Process.
    /// Pure Identity/Memory state. No direct symbolic processing.
    /// </summary>
    public class EngramAgent
    {
        public string AgentId { get; private set; }
        public IdentityKernel Kernel { get; private set; }
        
        // Agent state
        public float EnergyBudget { get; private set; } // "Agency Cost"
        
        public EngramAgent(string id, IdentityKernel kernel)
        {
            AgentId = id;
            Kernel = kernel;
            EnergyBudget = 100f; 
        }

        public bool ConsumeBudget(float amount)
        {
            if (EnergyBudget >= amount)
            {
                EnergyBudget -= amount;
                return true;
            }
            return false;
        }
    }
}
