using System;
using System.Collections.Generic;
using System.Linq;
using Oan.Core.Engrams;
using Oan.AgentiCore.Engrams.Data;

namespace Oan.AgentiCore.Engrams
{
    public class EngramProjectionService
    {
        public EngramDto ToDto(EngramBlock block)
        {
            if (block == null) throw new ArgumentNullException(nameof(block));

            return new EngramDto
            {
                EngramId = block.EngramId,
                Hash = block.Hash,
                Header = block.Header, // Records are immutable, safe to share
                Factors = block.Factors, // Immutable list
                Refs = block.Refs // Immutable list
            };
        }

        public IReadOnlyList<EngramDto> ToDto(IEnumerable<EngramBlock> blocks)
        {
            return blocks.Select(ToDto).ToList();
        }
    }
}
