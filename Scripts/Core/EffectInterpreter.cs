using Godot;
using System;
using System.Collections.Generic;
using PursualRPG.Scripts.Domain;
using PursualRPG.Scripts.AI;

namespace PursualRPG.Scripts.Core
{
    /// <summary>
    /// Evaluates the declarative Effect DSL defined in chronicle.v1.json.
    /// Replaces hardcoded C# Actions/Funcs (SkillActions.cs) for Skills and Items.
    /// </summary>
    public static class EffectInterpreter
    {
        public static void ExecuteEffects(List<EffectDefinition> effects, Entity source, Entity target, Combat combat = null, int? rawD20 = null)
        {
            if (effects == null || effects.Count == 0) return;

            bool lastAttackHit = false;
            foreach (var effect in effects)
            {
                // Map JSON DamageType string to Enum safely
                DamageType dmgType = DamageType.Bludgeoning;
                if (!string.IsNullOrEmpty(effect.Type) && Enum.TryParse(effect.Type, true, out DamageType parsedType))
                {
                    dmgType = parsedType;
                }

                switch (effect.Op.ToLower())
                {
                    case "damage":
                        ResolveTarget(effect, source, target)?.ApplyDamage(source, effect.Amount, dmgType, combat);
                        break;

                    case "spell_attack":
                        if (source == null || target == null) break;
                        var (passed, _, finalDmg) = source.AttackTarget(target, spell: true, spellDamage: effect.Damage, rawD20: rawD20);
                        lastAttackHit = passed;
                        if (passed) target.ApplyDamage(source, finalDmg, dmgType, combat);
                        break;

                    case "guided_spell":
                        if (source == null || target == null) break;
                        target.ApplyDamage(source, effect.Damage + source.GetSpellDamageMod(), dmgType, combat);
                        break;

                    case "weapon_attack":
                        if (source == null || target == null) break;
                        var (wPassed, _, wDamage) = source.AttackTarget(target, rawD20: rawD20);
                        lastAttackHit = wPassed;
                        if (wPassed) target.ApplyDamage(source, wDamage, source.GetDamageType(), combat);
                        break;

                    case "bonus_damage":
                        if (target == null || !lastAttackHit) break;
                        target.ApplyDamage(source, effect.Damage, dmgType, combat);
                        break;

                    case "heal":
                        target = ResolveTarget(effect, source, target, defaultToSource: true);
                        int healAmt = effect.Amount + (effect.AddSpellMod ? source?.GetSpellDamageMod() ?? 0 : 0);
                        target?.Heal(healAmt);
                        break;

                    case "restore_mana":
                        target = ResolveTarget(effect, source, target, defaultToSource: true);
                        if (target is Player p)
                        {
                            p.Mana = Math.Min(p.MaxMana, p.Mana + effect.Amount);
                        }
                        break;

                    case "apply_effect":
                        target = ResolveTarget(effect, source, target, defaultToSource: true);
                        target?.ApplyEffect(effect.Effect, effect.Duration);
                        break;

                    case "thunder_wave":
                        if (combat == null || source == null) break;
                        int twBase = effect.Damage + source.GetSpellDamageMod();
                        foreach (var enemy in combat.Enemies)
                        {
                            int dmg = twBase;
                            if (enemy.AttribTest(CharacterAttrib.Constitution) >= source.GetSpellDifficultClass())
                                dmg /= 2;
                            else if (!string.IsNullOrWhiteSpace(effect.Effect))
                                enemy.ApplyEffect(effect.Effect, effect.Duration > 0 ? effect.Duration : 1);
                                
                            enemy.ApplyDamage(source, dmg, dmgType, combat);
                        }
                        break;

                    case "sleep_song":
                        if (target == null) break;
                        int roll = rawD20 ?? (GD.RandRange(1, 10) + GD.RandRange(1, 10));
                        if (roll >= target.Health)
                            target.ApplyEffect(effect.Effect, effect.Duration);
                        break;

                    default:
                        GD.PushWarning($"[EffectInterpreter] Unknown operation '{effect.Op}' encountered.");
                        break;
                }
            }
        }

        private static Entity ResolveTarget(
            EffectDefinition effect,
            Entity source,
            Entity target,
            bool defaultToSource = false)
        {
            if (string.Equals(effect.Targets, "self", StringComparison.OrdinalIgnoreCase))
                return source;
            if (string.Equals(effect.Targets, "enemy", StringComparison.OrdinalIgnoreCase))
                return target;
            return defaultToSource ? source : target;
        }
    }
}