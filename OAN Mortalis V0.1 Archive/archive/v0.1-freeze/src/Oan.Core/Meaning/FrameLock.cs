using System;
using System.Collections.Generic;

namespace Oan.Core.Meaning
{
    public enum FrameMode
    {
        Clarify,
        Debate,
        Model,
        Plan,
        Explore
    }

    public class FrameLock
    {
        public required string Goal { get; set; }
        public FrameMode Mode { get; set; }
        public List<string> Constraints { get; set; } = new List<string>();
        public List<string> Assumptions { get; set; } = new List<string>();
        public bool IsSet { get; set; }
        public long SetTick { get; set; }
        public string? SetByOperatorId { get; set; }
    }
}
