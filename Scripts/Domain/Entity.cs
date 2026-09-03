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

        public virtual void ApplyDamage(Entity target, float damage, DamageType damageType = DamageType.Bludgeoning, Combat combat = null)
        {
            Health -= (int)damage;
            if (Health <= 0) Die(target, damage);
        }

        public virtual void Die(Entity target, float damage) => Dead = true;
        public virtual void Heal(int healAmount) => Health = Math.Min(MaxHealth, Health + healAmount);
        public float CalculateDamage(float damage) => damage * (100f / (100f + Armor));

        public int GetSpellDamageMod()
        {
            int intel = Attributes.GetValueOrDefault(CharacterAttrib.Intelligence, 10);
            return (intel - 10) / 2;
        }

        public int GetSpellDifficultClass() => 10 + GetSpellDamageMod();

        public int AttribTest(CharacterAttrib attrib)
        {
            int val = Attributes.GetValueOrDefault(attrib, 10);
            int mod = (val - 10) / 2;
            return new Random().Next(1, 21) + mod;
        }

        public DamageType GetDamageType() => DamageType.Bludgeoning;

        public bool ApplyEffect(EffectEnum effectEnum, int duration)
        {
            var existing = Effects.Find(e => e.Name == effectEnum.ToString());
            if (existing != null)
            {
                if (existing.Stackable) existing.Duration += duration;
                return true;
            }
            Effects.Add(new Effect
            {
                Name = effectEnum.ToString(),
                Duration = duration
            });
            return true;
        }

        public virtual (bool passed, int result, int damage) AttackTarget(Entity target, bool spell = false, int spellDamage = 0, int? rawD20 = null)
        {
            int d20 = rawD20 ?? new Random().Next(1, 21);
            int mod = spell ? GetSpellDamageMod() : (Attributes.GetValueOrDefault(CharacterAttrib.Dexterity, 10) - 10) / 2;
            int result = d20 + mod;
            bool passed = result >= target.Dodge || d20 == 20;
            int damage = 0;
            if (passed)
            {
                int baseDmg = spell ? (spellDamage + GetSpellDamageMod()) : BaseDamage;
                if (d20 == 20) baseDmg *= CritMultiplier;
                damage = (int)target.CalculateDamage(baseDmg);
            }
            return (passed, result, damage);
        }
    }
}