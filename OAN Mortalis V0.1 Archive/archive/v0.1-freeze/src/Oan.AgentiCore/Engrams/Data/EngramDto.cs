using System.Collections.Generic;
using Oan.Core.Engrams;

namespace Oan.AgentiCore.Engrams.Data
{
    public record EngramDto
    {
        public required string EngramId { get; init; }
        public required string Hash { get; init; }
        public required EngramBlockHeader Header { get; init; }
        public required IReadOnlyList<EngramFactor> Factors { get; init; }
        public required IReadOnlyList<string> Refs { get; init; }
    }
}
