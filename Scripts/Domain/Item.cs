using System;
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
        public bool IsUsable { get; set; }
        
        public Action<Player, Entity, Combat> OnUse { get; set; }

        public GenericItem(int id, string name, string description, int value, bool useless = false, bool isUsable = false, Action<Player, Entity, Combat> onUse = null)
        {
            Id = id;
            Name = name;
            Description = description;
            Value = value;
            Useless = useless;
            IsUsable = isUsable;
            OnUse = onUse;
        }

        public virtual void Use(Player user, Entity target, Combat combat)
        {
            OnUse?.Invoke(user, target, combat);
        }
    }

    public class HealingPotion : GenericItem
    {
        public int Potency { get; set; }
        
        public HealingPotion(int id, int potency, int value) 
            : base(id, "Healing Potion", "Restores health", value, useless: false, isUsable: true)
        {
            Potency = potency;
            OnUse = (user, target, combat) =>
            {
                user.Heal(Potency);
                combat?.LogMessage($"{user.Name} drank a Healing Potion and recovered {Potency} HP!");
            };
        }
    }

    public static class ItemFactoryRegistry
    {
        public static readonly Dictionary<int, GenericItem> Items = new()
        {
            { 1, new HealingPotion(1, 25, 15) },
            { 2, new GenericItem(2, "Mana Elixir", "Restores 20 Mana", 20, false, true, (user, target, combat) => {
                user.Mana = Math.Min(user.MaxMana, user.Mana + 20);
                combat?.LogMessage($"{user.Name} drank a Mana Elixir and restored 20 Mana!");
            }) },
            { 3, new GenericItem(3, "Fire Bomb", "Deals 15 Fire damage to an enemy", 30, false, true, (user, target, combat) => {
                if (target != null)
                {
                    target.ApplyDamage(user, 15, DamageType.Fire, combat);
                    combat?.LogMessage($"{user.Name} threw a Fire Bomb at {target.Name} for 15 Fire damage!");
                }
            }) },
            { 4, new GenericItem(4, "Ancient Coin", "A shiny old coin", 50, true) }
        };

        public static GenericItem GetItem(int id)
        {
            if (Items.TryGetValue(id, out var item)) return item;
            return new GenericItem(id, $"Unknown Item #{id}", "An unidentified object.", 0, true);
        }
    }
}