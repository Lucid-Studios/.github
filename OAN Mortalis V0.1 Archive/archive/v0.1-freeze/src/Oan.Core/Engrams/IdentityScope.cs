using System;

namespace Oan.Core.Engrams
{
    public enum IdentityScope
    {
        Intrinsic,
        Contextual
    }

    public enum CanonicalProfile
    {
        Intrinsic,
        Contextual
    }

    public record TransportDescriptor
    {
        public required string SourceFormationLevel { get; init; }
        public required string TargetFormationLevel { get; init; }
        public required string SourceTheaterMode { get; init; }
        public required string TargetTheaterMode { get; init; } // Often same as Source
        public bool IsCrossCradle { get; init; }
        public bool IsBindingAttempt { get; init; }
        public bool IsPromotion { get; init; }
    }

    public static class IdentityScopeMatrix
    {
        public static IdentityScope Resolve(TransportDescriptor descriptor)
        {
            // 1. If IsBindingAttempt == true → return Contextual
            if (descriptor.IsBindingAttempt) return IdentityScope.Contextual;

            // 2. If IsPromotion == true → return Contextual
            if (descriptor.IsPromotion) return IdentityScope.Contextual;

            // 3. If IsCrossCradle == true → return Intrinsic
            if (descriptor.IsCrossCradle) return IdentityScope.Intrinsic;

            // 4. Otherwise → return Intrinsic
            return IdentityScope.Intrinsic;

            // 5. If descriptor incomplete (checked via required properties at compile/runtime instantiation)
        }

        public static CanonicalProfile MapToProfile(IdentityScope scope)
        {
             return scope switch
             {
                 IdentityScope.Intrinsic => CanonicalProfile.Intrinsic,
                 IdentityScope.Contextual => CanonicalProfile.Contextual,
                 _ => throw new ArgumentOutOfRangeException(nameof(scope), scope, null)
             };
        }
    }
}
