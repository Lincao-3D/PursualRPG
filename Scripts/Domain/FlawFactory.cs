using System;
using System.Collections.Generic;

namespace PursualRPG.Scripts.Domain
{
    public static class FlawFactory
    {
        public static readonly IReadOnlyList<string> Defects = new[]
        {
            "Acrophobia (Severe fear of heights)",
            "Pathological Liar",
            "Kleptomania (Compulsive urge to steal minor items)",
            "Hubris (Excessive pride and overconfidence)",
            "Deeply Superstitious",
            "Chronic Impatience",
            "Easily Distracted by Shiny Objects",
            "Vengeful Grudge-Holder",
            "Speaks in the Third Person",
            "Compulsive Gambler",
            "Magic Averse (Distrusts and fears spellcasters)",
            "Anti-Authority (Deep distrust of guards and leaders)",
            "The Braggart (Exaggerates every minor achievement)",
            "Parsimonious Hoarder (Keeps useless junk)",
            "Terrible Sense of Direction",
            "Jumpy & Easily Startled",
            "Tactless & Brutally Blunt",
            "Soft Spot for Strays (Easily manipulated emotionally)",
            "Paranoid (Convinced a minor curse follows them)",
            "Stubborn Refusal to Ask for Help",
            "Gluttonous (Obsessed with food and drink)",
            "Chronic Insomniac (Cranky when tired)",
            "Oblivious to Sarcasm",
            "Secret Coward (Brave until cornered alone)"
        };

        private static readonly Random _rng = new();

        public static string GetRandomFlaw(Random rng = null)
        {
            var random = rng ?? _rng;
            return Defects[random.Next(Defects.Count)];
        }
    }
}
