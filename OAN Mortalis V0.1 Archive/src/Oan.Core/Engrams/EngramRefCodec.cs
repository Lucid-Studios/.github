using System;

namespace Oan.Core.Engrams
{
    public static class EngramRefCodec
    {
        private static readonly char[] InvalidChars = { ':', '\n', '|' };

        public static void Validate(EngramRef r)
        {
            if (r == null) throw new ArgumentNullException(nameof(r));

            if (string.IsNullOrWhiteSpace(r.Relationship))
                throw new ArgumentException("EngramRef Relationship cannot be null or whitespace.", nameof(r.Relationship));

            if (string.IsNullOrWhiteSpace(r.TargetId))
                throw new ArgumentException("EngramRef TargetId cannot be null or whitespace.", nameof(r.TargetId));

            if (r.Relationship.IndexOfAny(InvalidChars) >= 0)
                throw new ArgumentException($"EngramRef Relationship '{r.Relationship}' contains invalid characters (':', '\\n', '|').", nameof(r.Relationship));

            if (r.TargetId.IndexOfAny(InvalidChars) >= 0)
                throw new ArgumentException($"EngramRef TargetId '{r.TargetId}' contains invalid characters (':', '\\n', '|').", nameof(r.TargetId));
        }

        public static string Format(EngramRef r)
        {
            Validate(r);
            // Format strictly as "Relationship:TargetId"
            return $"{r.Relationship}:{r.TargetId}";
        }
    }
}
