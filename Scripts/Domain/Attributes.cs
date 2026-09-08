using System;
using System.Collections.Generic;

namespace PursualRPG.Scripts.Domain
{
    public enum CharacterAttrib
    {
        Strength,
        Dexterity,
        Constitution,
        Intelligence,
        Wisdom,
        Charisma
    }

    public enum CharacterExpertise
    {
        Acrobatics, AnimalHandling, Arcana, Athletics, Deception, History,
        Insight, Intimidation, Investigation, Medicine, Nature, Perception,
        Performance, Persuasion, Religion, SleightOfHand, Stealth, Survival
    }

    public static class AttributeUtils
    {
        public static CharacterAttrib GetAssociatedAbility(CharacterExpertise expertise)
        {
            return expertise switch
            {
                CharacterExpertise.Athletics => CharacterAttrib.Strength,
                CharacterExpertise.Acrobatics or CharacterExpertise.SleightOfHand or CharacterExpertise.Stealth => CharacterAttrib.Dexterity,
                CharacterExpertise.Arcana or CharacterExpertise.History or CharacterExpertise.Investigation or CharacterExpertise.Nature or CharacterExpertise.Religion => CharacterAttrib.Intelligence,
                CharacterExpertise.AnimalHandling or CharacterExpertise.Insight or CharacterExpertise.Medicine or CharacterExpertise.Perception or CharacterExpertise.Survival => CharacterAttrib.Wisdom,
                CharacterExpertise.Deception or CharacterExpertise.Performance or CharacterExpertise.Persuasion => CharacterAttrib.Charisma,
                CharacterExpertise.Intimidation => CharacterAttrib.Strength,
                _ => CharacterAttrib.Strength
            };
        }

        public static List<int> RollFourD6DropLowestSet()
        {
            var rand = new Random();
            List<int> rolls;
            do
            {
                rolls = new List<int>();
                for (int i = 0; i < 6; i++)
                {
                    var dices = new int[4];
                    for (int d = 0; d < 4; d++) dices[d] = rand.Next(1, 7);
                    Array.Sort(dices);
                    Array.Reverse(dices);
                    rolls.Add(dices[0] + dices[1] + dices[2]);
                }
            } while (HasDuplicates(rolls));
            rolls.Sort((a, b) => b.CompareTo(a));
            return rolls;
        }

        private static bool HasDuplicates(List<int> list)
        {
            var set = new HashSet<int>(list);
            return set.Count < list.Count;
        }

        public static List<int> RollAttribs(int count = 6)
        {
            var rand = new Random();
            var rolls = new List<int>();
            for (int i = 0; i < count; i++)
            {
                var dices = new int[4];
                for (int d = 0; d < 4; d++) dices[d] = rand.Next(1, 7);
                Array.Sort(dices);
                Array.Reverse(dices);
                rolls.Add(dices[0] + dices[1] + dices[2]);
            }
            return rolls;
        }

        public static Dictionary<CharacterAttrib, int> RandomAttribs()
        {
            var roll = RollAttribs();
            return new Dictionary<CharacterAttrib, int>
            {
                { CharacterAttrib.Strength, roll[0] },
                { CharacterAttrib.Dexterity, roll[1] },
                { CharacterAttrib.Constitution, roll[2] },
                { CharacterAttrib.Intelligence, roll[3] },
                { CharacterAttrib.Wisdom, roll[4] },
                { CharacterAttrib.Charisma, roll[5] }
            };
        }

        public static Dictionary<CharacterAttrib, int> FillMissingAttribs(Dictionary<CharacterAttrib, int> initialDict)
        {
            var randDict = RandomAttribs();
            foreach (CharacterAttrib attrib in Enum.GetValues(typeof(CharacterAttrib)))
            {
                if (!initialDict.ContainsKey(attrib))
                {
                    initialDict[attrib] = randDict[attrib];
                }
            }
            return initialDict;
        }
    }
}