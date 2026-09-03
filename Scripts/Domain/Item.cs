using System.Collections.Generic;

namespace PursualRPG.Scripts.Domain
{
    public enum EquipSlot { LeftHand, RightHand, Head, Armor, Feet, Ring }

    public class GenericItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Value { get; set; }
        public bool Useless { get; set; }

        public GenericItem(int id, string name, string description, int value, bool useless)
        {
            Id = id;
            Name = name;
            Description = description;
            Value = value;
            Useless = useless;
        }
    }

    public class HealingPotion : GenericItem
    {
        public int Potency { get; set; }
        public HealingPotion(int id, int potency, int value) : base(id, "Healing Potion", "Heals player", value, false)
        {
            Potency = potency;
        }
    }
}