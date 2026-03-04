using System;
using System.Collections.Generic;
using System.Linq;

namespace Oan.SoulFrame.Atlas
{
    public class LexicalLookup
    {
        private readonly Dictionary<string, List<RootWord>> _variantMap;

        public LexicalLookup()
        {
            _variantMap = new Dictionary<string, List<RootWord>>();
        }

        public LexicalLookup(IEnumerable<RootWord> words) : this()
        {
            foreach (var word in words)
            {
                IndexWord(word);
            }
        }

        public void IndexWord(RootWord word)
        {
            if (!_variantMap.ContainsKey(word.base_word))
            {
                _variantMap[word.base_word] = new List<RootWord>();
            }
            _variantMap[word.base_word].Add(word);

            foreach (var variant in word.variants)
            {
                if (!_variantMap.ContainsKey(variant))
                {
                    _variantMap[variant] = new List<RootWord>();
                }
                _variantMap[variant].Add(word);
            }
        }

        public List<RootWord> Lookup(string term)
        {
            if (_variantMap.TryGetValue(term, out var words))
            {
                return words;
            }
            return new List<RootWord>();
        }

        public RootWord? FindExact(string term)
        {
            if (_variantMap.TryGetValue(term, out var words))
            {
                return words.FirstOrDefault(w => w.base_word == term);
            }
            return null;
        }

        // Simulating fuzzy search or SLI resolution
        public string ResolveSli(string term)
        {
            var match = FindExact(term);
            return match?.default_sli_handle ?? "SLI_UNKNOWN";
        }
    }
}
