using System;
using System.Text.RegularExpressions;

namespace Oan.SoulFrame.SLI
{
    /// <summary>
    /// Represents a single unit of Symbolic Language Interconnect (SLI).
    /// Format: PREFIX:BASE^mode_op.suffix
    /// </summary>
    public class SliMorpheme
    {
        public string? Prefix { get; set; }
        public required string Base { get; set; }
        public string? Mode { get; set; }
        public string? Op { get; set; }
        public string? Suffix { get; set; }

        public string? FullString { get; set; }

        // Regex for: PREFIX:BASE^mode[_op].suffix
        // 1: Prefix, 2: Base, 3: Mode, 4: Op (optional), 5: Suffix (optional)
        // Fixed regex to allow optional op
        private static readonly Regex _parser = new Regex(
            @"^([A-Z0-9_]+):([A-Z0-9_]+)\^([a-z0-9_]+)(?:_([a-z0-9_]+))?(?:\.([a-z0-9_.-]+))?$",
            RegexOptions.Compiled
        );

        public static bool TryParse(string input, out SliMorpheme? morpheme)
        {
            morpheme = null;
            if (string.IsNullOrWhiteSpace(input)) return false;

            var match = _parser.Match(input);
            if (!match.Success) return false;

            morpheme = new SliMorpheme
            {
                FullString = input,
                Prefix = match.Groups[1].Value,
                Base = match.Groups[2].Value,
                Mode = match.Groups[3].Value,
                Op = match.Groups[4].Success ? match.Groups[4].Value : string.Empty,
                Suffix = match.Groups[5].Success ? match.Groups[5].Value : string.Empty
            };

            return true;
        }

        public override string ToString() => FullString ?? string.Empty;
    }
}
