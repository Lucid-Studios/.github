using System.Collections.Generic;

namespace Oan.Core.Meaning
{
    public enum RiskBand
    {
        SAFE,
        AMBIGUOUS,
        HARD_STOP
    }

    public class RiskBandAssessment
    {
        public RiskBand Band { get; set; }
        public List<string> ContributingSpanIds { get; set; } = new List<string>();
        public string Explanation { get; set; } = string.Empty;
        public float Uncertainty { get; set; } // 0..1
    }
}
