namespace Oan.Core.Engrams
{
    /// <summary>
    /// Marker interface for services that provide read-only recall of Engrams.
    /// Used to enforce architectural boundaries (e.g., IntentProcessor must not depend on this).
    /// </summary>
    public interface IRecallSurface
    {
    }
}
