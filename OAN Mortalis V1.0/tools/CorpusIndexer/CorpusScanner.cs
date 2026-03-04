using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace CorpusIndexer;

internal sealed class CorpusScanner
{
    private static readonly Regex HeadingRegex = new(@"^\s*#+\s*(.+?)\s*$", RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly string[] KeywordSeeds =
    [
        "sli",
        "engram",
        "cme",
        "gel",
        "cryptic",
        "identity continuity",
        "symbolic packet",
        "routing",
        "soulframe",
        "agenticore",
        "cradletek"
    ];

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".md", ".txt", ".rst", ".json", ".xml", ".yaml", ".yml", ".tex", ".cfg", ".ini", ".csv"
    };

    public IReadOnlyList<ScannedDocument> Scan(string corpusRoot)
    {
        var documents = new List<ScannedDocument>();

        foreach (var file in EnumerateCandidateFiles(corpusRoot))
        {
            string content;
            try
            {
                content = File.ReadAllText(file);
            }
            catch
            {
                // Ignore unreadable files; scanner stays deterministic over readable set.
                continue;
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                continue;
            }

            var hash = ComputeSha256(content);
            var title = DetectTitle(content, hash);
            var keywords = DetectKeywords(content);

            documents.Add(new ScannedDocument(title, hash, keywords, content));
        }

        return documents
            .OrderBy(d => d.Hash, StringComparer.Ordinal)
            .ThenBy(d => d.Title, StringComparer.Ordinal)
            .ToList();
    }

    private static IEnumerable<string> EnumerateCandidateFiles(string corpusRoot)
    {
        if (!Directory.Exists(corpusRoot))
        {
            return [];
        }

        return Directory
            .EnumerateFiles(corpusRoot, "*", SearchOption.AllDirectories)
            .Where(path => AllowedExtensions.Contains(Path.GetExtension(path)))
            .OrderBy(path => path, StringComparer.Ordinal);
    }

    private static string DetectTitle(string content, string hash)
    {
        var heading = HeadingRegex.Match(content);
        if (heading.Success)
        {
            return NormalizeTitle(heading.Groups[1].Value);
        }

        foreach (var line in content.Split('\n'))
        {
            var trimmed = line.Trim();
            if (!string.IsNullOrWhiteSpace(trimmed))
            {
                return NormalizeTitle(trimmed);
            }
        }

        return $"Document-{hash[..12]}";
    }

    private static string NormalizeTitle(string value)
    {
        var clean = value.Replace('\r', ' ').Replace('\t', ' ').Trim();
        return clean.Length <= 120 ? clean : clean[..120];
    }

    private static IReadOnlyList<string> DetectKeywords(string content)
    {
        var lowered = content.ToLowerInvariant();
        return KeywordSeeds
            .Where(seed => lowered.Contains(seed, StringComparison.Ordinal))
            .Select(seed => seed.Replace(' ', '-'))
            .OrderBy(seed => seed, StringComparer.Ordinal)
            .ToList();
    }

    private static string ComputeSha256(string content)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
