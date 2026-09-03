using System.Collections.Generic;

namespace PursualRPG.Scripts.Domain
{
    public enum CharacterRace { Human, Elf, Dwarf }

    public static class RaceUtils
    {
        public static Dictionary<CharacterAttrib, int> SumAttrib(Dictionary<CharacterAttrib, int> currentAttrib, CharacterRace race)
        {
            var result = new Dictionary<CharacterAttrib, int>(currentAttrib);
            var bonusMap = SkillsBonus.GetValueOrDefault(race, new());
            foreach (var kvp in bonusMap)
            {
                result[kvp.Key] = result.GetValueOrDefault(kvp.Key, 0) + kvp.Value;
            }
            return result;
        }

        private static readonly Dictionary<CharacterRace, Dictionary<CharacterAttrib, int>> SkillsBonus = new()
        {
            {
                CharacterRace.Human, new() {
                    { CharacterAttrib.Strength, 1 }, { CharacterAttrib.Dexterity, 1 },
                    { CharacterAttrib.Constitution, 1 }, { CharacterAttrib.Intelligence, 1 },
                    { CharacterAttrib.Wisdom, 1 }, { CharacterAttrib.Charisma, 1 }
                }
            },
            {
                CharacterRace.Elf, new() {
                    { CharacterAttrib.Strength, -1 }, { CharacterAttrib.Dexterity, 2 },
                    { CharacterAttrib.Constitution, -1 }, { CharacterAttrib.Intelligence, 1 },
                    { CharacterAttrib.Wisdom, 1 }, { CharacterAttrib.Charisma, 2 }
                }
            },
            {
                CharacterRace.Dwarf, new() {
                    { CharacterAttrib.Strength, 1 }, { CharacterAttrib.Constitution, 2 }, { CharacterAttrib.Wisdom, 1 }
                }
            }
        };
    }
}