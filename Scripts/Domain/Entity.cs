using System;
using System.Collections.Generic;

namespace PursualRPG.Scripts.Domain
{
    public enum EntityCategory { Humanoid, Undead }
    public enum DamageType { Fire, Cold, Electric, Poison, Corrosive, Slashing, Bludgeoning, Piercing, Magical, Psychic, Sacred, Profane, True }

    public class Entity
    {
        public string Name { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public float Armor { get; set; }
        public int Dodge { get; set; }
        public int BaseDamage { get; set; }
        public int CritMultiplier { get; set; } = 2;
        public bool Dead { get; set; } = false;
        public Dictionary<CharacterAttrib, int> Attributes { get; set; }
        public List<Effect> Effects { get; set; } = new();
        public EntityCategory Category { get; set; }

        public Entity(string name, int health, float armor, int dodge, int baseDamage, Dictionary<CharacterAttrib, int> attributes, EntityCategory category = EntityCategory.Humanoid)
        {
            Name = name;
            Health = health;
            MaxHealth = health;
            Armor = armor;
            Dodge = dodge;
            BaseDamage = baseDamage;
            Attributes = AttributeUtils.FillMissingAttribs(attributes);
            Category = category;
        }

        public virtual void ApplyDamage(Entity target, float damage, DamageType damageType)
        {
            Health -= (int)damage;
            if (Health <= 0) Die(target, damage);
        }

        public virtual void Die(Entity target, float damage) => Dead = true;
        public virtual void Heal(int healAmount) => Health = Math.Min(MaxHealth, Health + healAmount);
        public float CalculateDamage(float damage) => damage * (100f / (100f + Armor));
    }
}