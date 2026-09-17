using System;
using System.Globalization;

namespace PursualRPG.Scripts.Core
{
    public static class RandomNamesFactory
    {
        private static readonly Random _rng = new Random();

        // Syllable pools tailored for a high-fantasy aesthetic
        private static readonly string[] _prefixes = 
        {
            "Ar", "Bel", "Cor", "Dor", "El", "Fael", "Gal", "Hald", 
            "Il", "Kel", "Mor", "Nar", "Orn", "Quel", "Row", "Syl", "Thor", "Val"
        };

        private static readonly string[] _middles = 
        {
            "an", "bern", "ar", "en", "hen", "or", "ul", "ith", "oth", "and", "ast", "iml", "enor"
        };

        private static readonly string[] _suffixes = 
        {
            "a", "alt", "nan", "orn", "ion", "or", "as", "is", "us", "eth", "wen", "mir", "dor", "mar", "ian", "eus", "omoth",
        };

        /// <p>Generates a random procedural fantasy name using a mix of syllables.</p>
        public static string GenerateName()
        {
            // Randomly choose whether to use a 2-syllable or 3-syllable structure
            bool useMiddle = _rng.Next(2) == 0;

            string name = _prefixes[_rng.Next(_prefixes.Length)];

            if (useMiddle)
            {
                name += _middles[_rng.Next(_middles.Length)];
            }

            name += _suffixes[_rng.Next(_suffixes.Length)];

            // Ensure proper capitalization (e.g., "Aronthiel")
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name.ToLower());
        }
    }
}