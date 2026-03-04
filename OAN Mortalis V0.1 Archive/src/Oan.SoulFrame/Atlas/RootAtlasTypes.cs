using System.Collections.Generic;
using System; // Added for Guid

namespace Oan.SoulFrame.Atlas
{
    public class RootWord
    {
        public required string base_word { get; set; }
        public string? form { get; set; }
        public List<string> variants { get; set; } = new List<string>();
        public string? default_sli_handle { get; set; }
        public List<string> sli_handles { get; set; } = new List<string>();
        public List<Guid> ledgers { get; set; } = new List<Guid>();
        public List<string> tags { get; set; } = new List<string>();
    }

    public class Variant
    {
        public required string form { get; set; }
        public int count { get; set; }
    }

    /// <summary>
    /// Wrapper for the whole RootAtlasMaster.json object
    /// </summary>
    public class RootAtlasContainer
    {
        // The JSON structure is a dictionary of "word": { entry_data }
    }

    public interface IAtlasLoader
    {
        Dictionary<string, RootWord> Load(string jsonContent);
    }
}
