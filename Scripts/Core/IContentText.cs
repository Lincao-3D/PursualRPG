using System;
using System.Collections.Generic;

namespace PursualRPG.Scripts.Core
{
    /// <summary>
    /// Temporary interface for Chronicle Content Strings (races, classes, skills, items, etc.).
    /// Keeps domain content text decoupled from Godot Engine/UI translation tables
    /// ahead of the planned game model monolithization.
    /// </summary>
    public interface IContentText
    {
        string Get(string id, params object[] args);
    }

    /// <summary>
    /// Temporary dictionary-backed singleton implementation for IContentText.
    /// </summary>
    public class ContentTextService : IContentText
    {
        public static ContentTextService Instance { get; } = new ContentTextService();

        private readonly Dictionary<string, string> _contentStrings = new(StringComparer.OrdinalIgnoreCase);

        public ContentTextService()
        {
            RegisterDefaultContentStrings();
        }

        public string Get(string id, params object[] args)
        {
            if (string.IsNullOrEmpty(id)) return string.Empty;

            string text = _contentStrings.TryGetValue(id, out var val) ? val : id;
            if (args != null && args.Length > 0)
            {
                try
                {
                    return string.Format(text, args);
                }
                catch
                {
                    return text;
                }
            }
            return text;
        }

        public void Register(string id, string text)
        {
            _contentStrings[id] = text;
        }

        private void RegisterDefaultContentStrings()
        {
            // Races
            _contentStrings["race.human.name"] = "Human";
            _contentStrings["race.human.desc"] = "Versatile and ambitious, adaptable to any path.";
            _contentStrings["race.elf.name"] = "Elf";
            _contentStrings["race.elf.desc"] = "Graceful and attuned to magic and nature.";
            _contentStrings["race.dwarf.name"] = "Dwarf";
            _contentStrings["race.dwarf.desc"] = "Resilient and sturdy warriors with enduring traditions.";

            // Classes
            _contentStrings["class.warrior.name"] = "Warrior";
            _contentStrings["class.warrior.desc"] = "Masters of martial combat and brute strength.";
            _contentStrings["class.paladin.name"] = "Paladin";
            _contentStrings["class.paladin.desc"] = "Holy champions upholding divine oaths and sacred fury.";
            _contentStrings["class.rogue.name"] = "Rogue";
            _contentStrings["class.rogue.desc"] = "Stealthy combatants specializing in precise strikes.";
            _contentStrings["class.mage.name"] = "Mage";
            _contentStrings["class.mage.desc"] = "Scholars of the arcane manipulating reality.";
            _contentStrings["class.cleric.name"] = "Cleric";
            _contentStrings["class.cleric.desc"] = "Healers and smiters channeling deity power.";
            _contentStrings["class.bard.name"] = "Bard";
            _contentStrings["class.bard.desc"] = "Artistic souls inspiring allies with melodic lore.";
            _contentStrings["class.barbarian.name"] = "Barbarian";
            _contentStrings["class.barbarian.desc"] = "Fierce combatants drawing power from untamed rage.";

            // Basic Items
            _contentStrings["item.healing_potion.name"] = "Healing Potion";
            _contentStrings["item.healing_potion.desc"] = "Restores 15 Hit Points when consumed.";
            _contentStrings["item.mana_elixir.name"] = "Mana Elixir";
            _contentStrings["item.mana_elixir.desc"] = "Restores 10 Mana points.";
            _contentStrings["item.fire_bomb.name"] = "Fire Bomb";
            _contentStrings["item.fire_bomb.desc"] = "Deals 20 explosive fire damage to a target.";
            _contentStrings["item.ancient_coin.name"] = "Ancient Coin";
            _contentStrings["item.ancient_coin.desc"] = "A relic coin that glows with faint magical light.";
        }
    }

    /// <summary>
    /// Static convenience accessor for IContentText.
    /// </summary>
    public static class ContentText
    {
        public static string Get(string id, params object[] args) => ContentTextService.Instance.Get(id, args);
    }
}
