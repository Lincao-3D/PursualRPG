using System;
using System.Collections.Generic;
using Godot;

public enum SkillEnum
{
    AccurateAttack,
    RecklessAttack,
    MagicMissile,
    GuidedMagicMissile,
    ThunderWave,
    SleepSong,
    HealingWords,
    CureWounds,
    DivineSmith
}

public class Skill
{
    public string Name { get; set; }
    public int MinLevel { get; set; }
    public int Cost { get; set; }
    public List<CharacterClassEnum> Classes { get; set; } = new();
    public string Description { get; set; }
    public SkillEnum Enum { get; set; }
    public bool IsCombat { get; set; } = true;
    public bool IsTargeted { get; set; } = false;
    public bool SkipTurn { get; set; } = true;
    public bool Passive { get; set; } = false;
    
    // Delegate matching execute behavior with optional raw d20 roll
    public Action<Entity, Entity, Combat, int?> ExecuteFunc { get; set; }

    public void Execute(Entity player, Entity target, Combat combat = null, int? rawD20 = null)
    {
        ExecuteFunc?.Invoke(player, target, combat, rawD20);
    }
}

public static class SkillActions
{
    public static void SpellAttack(Entity player, Entity target, Combat combat = null, int damage = 5, DamageType damageType = DamageType.Magical, int? rawD20 = null)
    {
        var (passed, result, finalDamage) = player.AttackTarget(target, spell: true, spellDamage: damage, rawD20: rawD20);
        if (passed)
        {
            target.ApplyDamage(player, finalDamage, damageType, combat);
        }
    }

    public static void ThunderWave(Entity player, Entity target, Combat combat, int? rawD20 = null)
    {
        if (combat == null) return;

        int baseDamage = 5 + player.GetSpellDamageMod();

        foreach (var enemy in combat.Enemies)
        {
            int damage = baseDamage;
            int result = enemy.AttribTest(CharacterAttrib.Constitution);
            
            if (result >= player.GetSpellDifficultClass())
            {
                damage = baseDamage / 2;
            }
            else
            {
                enemy.ApplyEffect(EffectEnum.Stunned, 1);
            }
            
            enemy.ApplyDamage(player, damage, DamageType.Bludgeoning, combat);
        }
    }

    public static void Attack(Entity player, Entity target, Combat combat, int extraDamage = 0, DamageType? extraDamageType = null, int? rawD20 = null)
    {
        var (passed, result, damage) = player.AttackTarget(target, rawD20: rawD20);
        if (passed)
        {
            target.ApplyDamage(player, damage, player.GetDamageType(), combat);
            if (extraDamage > 0 && extraDamageType.HasValue)
            {
                target.ApplyDamage(player, extraDamage, extraDamageType.Value, combat);
            }
        }
    }

    public static void SleepSong(Entity player, Entity target, Combat combat, int? rawD20 = null)
    {
        int result = rawD20 ?? (GD.RandRange(1, 10) + GD.RandRange(1, 10));
        if (result >= target.Health)
        {
            target.ApplyEffect(EffectEnum.Stunned, 10);
        }
    }
}

public static class SkillFactoryRegistry
{
    public static readonly Dictionary<SkillEnum, Skill> SkillFactory = new()
    {
        {
            SkillEnum.AccurateAttack,
            new Skill
            {
                Name = "Accurate Attack",
                MinLevel = 1,
                Cost = 0,
                Classes = new() { CharacterClassEnum.Rogue, CharacterClassEnum.Warrior },
                Description = "Skip your turn. Your next attack has advantage",
                IsCombat = true,
                SkipTurn = true,
                ExecuteFunc = (player, target, combat, rawD20) => player.ApplyEffect(EffectEnum.Aiming, 1),
                Enum = SkillEnum.AccurateAttack
            }
        },
        {
            SkillEnum.RecklessAttack,
            new Skill
            {
                Name = "Reckless Attack",
                MinLevel = 1,
                Cost = 0,
                Classes = new() { CharacterClassEnum.Barbarian, CharacterClassEnum.Warrior },
                Description = "Your next attack has advantage, but the enemies too",
                IsCombat = true,
                SkipTurn = false,
                ExecuteFunc = (player, target, combat, rawD20) => player.ApplyEffect(EffectEnum.Reckless, 1),
                Enum = SkillEnum.RecklessAttack
            }
        },
        {
            SkillEnum.MagicMissile,
            new Skill
            {
                Name = "Magic Missile",
                MinLevel = 1,
                Cost = 5,
                Classes = new() { CharacterClassEnum.Mage },
                Description = "Make an attack roll against an enemy using the intelligence modifier, if you hit, deal 5 + intelligence modifier damage",
                IsCombat = true,
                SkipTurn = true,
                ExecuteFunc = (player, target, combat, rawD20) => SkillActions.SpellAttack(player, target, combat, rawD20: rawD20),
                Enum = SkillEnum.MagicMissile
            }
        },
        {
            SkillEnum.GuidedMagicMissile,
            new Skill
            {
                Name = "Guided Magic Missile",
                MinLevel = 1,
                Cost = 5,
                Classes = new() { CharacterClassEnum.Mage },
                Description = "Deal 1 + intelligence modifier damage without attack",
                IsCombat = true,
                SkipTurn = true,
                ExecuteFunc = (player, target, combat, rawD20) => target.ApplyDamage(1 + player.GetSpellDamageMod(), DamageType.Magical, combat),
                Enum = SkillEnum.GuidedMagicMissile
            }
        },
        {
            SkillEnum.HealingWords,
            new Skill
            {
                Name = "Healing Words",
                MinLevel = 1,
                Cost = 5,
                Classes = new() { CharacterClassEnum.Bard, CharacterClassEnum.Cleric, CharacterClassEnum.Paladin },
                IsCombat = true,
                Description = "Cure 20 + spell mod life",
                ExecuteFunc = (player, target, combat, rawD20) => player.Heal(20 + player.GetSpellDamageMod()),
                Enum = SkillEnum.HealingWords
            }
        },
        {
            SkillEnum.CureWounds,
            new Skill
            {
                Name = "Cure Wounds",
                MinLevel = 1,
                Cost = 10,
                Classes = new() { CharacterClassEnum.Bard, CharacterClassEnum.Cleric, CharacterClassEnum.Paladin, CharacterClassEnum.Mage },
                IsCombat = true,
                Description = "Cure 50 + spell mod life. Skips turn",
                ExecuteFunc = (player, target, combat, rawD20) => player.Heal(50 + player.GetSpellDamageMod()),
                Enum = SkillEnum.CureWounds
            }
        },
        {
            SkillEnum.DivineSmith,
            new Skill
            {
                Name = "Divine Smith",
                MinLevel = 1,
                Cost = 10,
                Classes = new() { CharacterClassEnum.Paladin },
                Description = "Attack and Give 5 sacred damage if pass",
                IsCombat = true,
                SkipTurn = true,
                ExecuteFunc = (player, target, combat, rawD20) => SkillActions.Attack(player, target, combat, extraDamage: 5, extraDamageType: DamageType.Sacred, rawD20: rawD20),
                Enum = SkillEnum.DivineSmith
            }
        },
        {
            SkillEnum.ThunderWave,
            new Skill
            {
                Name = "Thunder Wave",
                MinLevel = 1,
                Cost = 10,
                Classes = new() { CharacterClassEnum.Mage, CharacterClassEnum.Bard },
                Description = "Deal 5 + intelligence modifier damage to all enemies. Everyone makes a constitution test, if they fail, they are stunned, if pass, take half of damage",
                IsCombat = true,
                SkipTurn = true,
                ExecuteFunc = SkillActions.ThunderWave,
                Enum = SkillEnum.ThunderWave
            }
        },
        {
            SkillEnum.SleepSong,
            new Skill
            {
                Name = "Sleep Song",
                MinLevel = 1,
                Cost = 10,
                Classes = new() { CharacterClassEnum.Bards }, // Adjust if needed
                Description = "Roll two d10s, if the result is greater than or equal to the target's total health, they will be stunned for 10 rounds",
                ExecuteFunc = SkillActions.SleepSong,
                Enum = SkillEnum.SleepSong
            }
        }
    };
}