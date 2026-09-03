using System.Collections.Generic;

namespace PursualRPG.Scripts.Domain
{
    public enum CharacterClassEnum { Warrior, Paladin, Rogue, Mage, Cleric, Bard, Barbarian }

    public class CharacterClass
    {
        public string Name { get; set; }
        public int InitialLife { get; set; }
        public int InitialMana { get; set; }

        public CharacterClass(string name, int life, int mana)
        {
            Name = name;
            InitialLife = life;
            InitialMana = mana;
        }

        public int GetInitialLife(int constitution) => InitialLife + (GetMod(constitution) * 10);
        public int GetInitialMana(int intelligence) => InitialMana + (GetMod(intelligence) * 10);

        private int GetMod(int attrVal) => (attrVal - 10) / 2;
    }

    public static class ClassFactoryMap
    {
        public static readonly Dictionary<CharacterClassEnum, CharacterClass> ClassFactory = new()
        {
            { CharacterClassEnum.Warrior, new CharacterClass("Warrior", 100, 100) },
            { CharacterClassEnum.Barbarian, new CharacterClass("Barbarian", 120, 80) },
            { CharacterClassEnum.Paladin, new CharacterClass("Paladin", 80, 120) },
            { CharacterClassEnum.Cleric, new CharacterClass("Cleric", 80, 120) },
            { CharacterClassEnum.Mage, new CharacterClass("Mage", 60, 140) },
            { CharacterClassEnum.Bard, new CharacterClass("Bard", 80, 120) },
            { CharacterClassEnum.Rogue, new CharacterClass("Rogue", 80, 120) }
        };
    }
}