using System.Collections.Generic;

namespace PursualRPG.Scripts.Domain
{
    public class Player : Entity
    {
        public CharacterRace Race { get; set; }
        public CharacterClass Clazz { get; set; }
        public int Gold { get; set; } = 0;
        public int Xp { get; set; } = 0;
        public int MaxMana { get; set; }
        public int Mana { get; set; }
        public int Proficiency { get; set; } = 2;
        public Dictionary<int, int> Inventory { get; set; } = new();

        public Player(string name, CharacterClass clazz, CharacterRace race, Dictionary<CharacterAttrib, int> attributes)
            : base(name, clazz.GetInitialLife(attributes.GetValueOrDefault(CharacterAttrib.Constitution, 10)), 0, 10, 1, RaceUtils.SumAttrib(attributes, race))
        {
            Race = race;
            Clazz = clazz;
            MaxMana = clazz.GetInitialMana(Attributes.GetValueOrDefault(CharacterAttrib.Intelligence, 10));
            Mana = MaxMana;
        }

        public void GiveItem(int itemId, int quantity)
        {
            if (Inventory.ContainsKey(itemId)) Inventory[itemId] += quantity;
            else Inventory[itemId] = quantity;
        }

        public void Rest()
        {
            Health = MaxHealth;
            Mana = MaxMana;
        }
    }
}