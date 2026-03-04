using System.Threading.Tasks;

namespace Oan.Common
{
    /// <summary>
    /// Surface for public plane storage operations.
    /// </summary>
    public interface IPublicPlaneStores
    {
        Task AppendToGoAAsync(string engramHash, object payload);
        Task AppendToGELAsync(string engramHash, object payload);
    }

    /// <summary>
    /// Surface for cryptic plane storage operations.
    /// </summary>
    public interface ICrypticPlaneStores
    {
        Task AppendToCGoAAsync(string engramHash, object payload);
    }
}
