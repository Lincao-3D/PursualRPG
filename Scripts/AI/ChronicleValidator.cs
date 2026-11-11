using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PursualRPG.Scripts.AI
{
    public static class ChronicleValidator
    {
        private static readonly HashSet<string> SupportedOperations = new(StringComparer.OrdinalIgnoreCase)
        {
            "damage", "spell_attack", "guided_spell", "weapon_attack", "bonus_damage",
            "heal", "restore_mana", "apply_effect", "thunder_wave", "sleep_song"
        };
        private static readonly HashSet<string> SupportedAttributes = new(StringComparer.OrdinalIgnoreCase)
        {
            "str", "strength", "dex", "dexterity", "con", "constitution",
            "int", "intelligence", "wis", "wisdom", "cha", "charisma"
        };

        public static bool Validate(ChroniclePack pack, out List<string> errors)
        {
            errors = new List<string>();
            if (pack == null)
            {
                errors.Add("Chronicle pack is null or structurally invalid JSON.");
                return false;
            }

            if (pack.Schema != 1)
                errors.Add($"Unsupported chronicle schema version '{pack.Schema}'.");
            if (pack.Meta == null)
            {
                errors.Add("Meta block is required.");
            }
            else
            {
                if (!IsValidId(pack.Meta.Id))
                    errors.Add("Meta ID must be a non-empty lowercase ID containing only letters, numbers, '_' or '-'.");
                if (string.IsNullOrWhiteSpace(pack.Meta.Version))
                    errors.Add("Meta version is required.");
                if (string.IsNullOrWhiteSpace(pack.Meta.DefaultLocale))
                    errors.Add("Meta default_locale is required.");
            }

            if (pack.Rules == null)
            {
                errors.Add("Rules block is required.");
            }
            else if (pack.Rules.Attributes == null || pack.Rules.Attributes.Count == 0)
            {
                errors.Add("Rules block must define at least one attribute.");
            }
            if (pack.Rules != null && pack.Rules.Limits == null)
                errors.Add("Rules limits must be an object.");
            foreach (string attribute in pack.Rules?.Attributes ?? Enumerable.Empty<string>())
                if (!SupportedAttributes.Contains(attribute))
                    errors.Add($"Rules reference unsupported attribute '{attribute}'.");

            ValidateIds(pack.Races, "Races", errors, race => race?.Id);
            ValidateIds(pack.Classes, "Classes", errors, characterClass => characterClass?.Id);
            ValidateIds(pack.Skills, "Skills", errors, skill => skill?.Id);
            ValidateIds(pack.Monsters, "Monsters", errors, monster => monster?.Id);
            ValidateIds(pack.Items, "Items", errors, item => item?.Id);
            ValidateIds(pack.Effects, "Effects", errors, effect => effect?.Id);

            if (pack.Races == null || pack.Races.Count == 0) errors.Add("Chronicle must contain at least one Race.");
            if (pack.Classes == null || pack.Classes.Count == 0) errors.Add("Chronicle must contain at least one Class.");
            if (pack.Skills == null || pack.Skills.Count == 0) errors.Add("Chronicle must contain at least one Skill.");
            if (pack.Monsters == null || pack.Monsters.Count == 0) errors.Add("Chronicle must contain at least one Monster.");
            if (pack.Items == null || pack.Items.Count == 0) errors.Add("Chronicle must contain at least one Item.");
            if (pack.Effects == null) errors.Add("Effects must be an array, even when empty.");
            if (pack.Flaws == null) errors.Add("Flaws must be an array, even when empty.");
            if (pack.Assets == null) errors.Add("Assets must be an object.");

            var classIds = new HashSet<string>(
                (pack.Classes ?? new List<ClassDto>()).Where(c => c != null).Select(c => c.Id),
                StringComparer.OrdinalIgnoreCase);
            var skillIds = new HashSet<string>(
                (pack.Skills ?? new List<SkillDto>()).Where(s => s != null).Select(s => s.Id),
                StringComparer.OrdinalIgnoreCase);
            var effectIds = new HashSet<string>(
                (pack.Effects ?? new List<StatusEffectDto>()).Where(e => e != null).Select(e => e.Id),
                StringComparer.OrdinalIgnoreCase);

            if (pack.Items != null)
            {
                var numericIds = new HashSet<int>();
                foreach (var item in pack.Items.Where(item => item != null))
                {
                    if (item.NumericId <= 0)
                        errors.Add($"Item '{item.Id}' must have a positive numeric_id.");
                    else if (!numericIds.Add(item.NumericId))
                        errors.Add($"Items contain duplicate numeric_id '{item.NumericId}'.");
                    ValidateTextKeys(pack, item.NameKey, item.DescKey, $"Item '{item.Id}'", errors);
                    ValidateEffects(item.Effects, effectIds, $"Item '{item.Id}'", errors);
                }
            }

            if (pack.Races != null)
            {
                var attributes = new HashSet<string>(
                    pack.Rules?.Attributes ?? new List<string>(),
                    StringComparer.OrdinalIgnoreCase);
                foreach (var race in pack.Races.Where(race => race != null))
                {
                    ValidateTextKeys(pack, race.NameKey, race.DescKey, $"Race '{race.Id}'", errors);
                    if (race.Bonuses == null)
                    {
                        errors.Add($"Race '{race.Id}' must define a bonuses object.");
                        continue;
                    }
                    foreach (string attribute in race.Bonuses.Keys)
                        if (!attributes.Contains(attribute))
                            errors.Add($"Race '{race.Id}' references undefined attribute '{attribute}'.");
                }
            }

            if (pack.Classes != null)
            {
                foreach (var characterClass in pack.Classes.Where(characterClass => characterClass != null))
                {
                    ValidateTextKeys(pack, characterClass.NameKey, characterClass.DescKey, $"Class '{characterClass.Id}'", errors);
                    if (characterClass.Skills == null)
                    {
                        errors.Add($"Class '{characterClass.Id}' must define a skills array.");
                        continue;
                    }
                    foreach (string skillId in characterClass.Skills)
                        if (!skillIds.Contains(skillId))
                            errors.Add($"Class '{characterClass.Id}' references unknown skill '{skillId}'.");
                }
            }

            if (pack.Skills != null)
            {
                foreach (var skill in pack.Skills.Where(skill => skill != null))
                {
                    ValidateTextKeys(pack, skill.NameKey, skill.DescKey, $"Skill '{skill.Id}'", errors);
                    if (skill.Classes == null)
                        errors.Add($"Skill '{skill.Id}' must define a classes array.");
                    foreach (string classId in skill.Classes ?? new List<string>())
                        if (!classIds.Contains(classId))
                            errors.Add($"Skill '{skill.Id}' references unknown class '{classId}'.");
                    if (skill.Effects == null)
                        errors.Add($"Skill '{skill.Id}' must define an effects array.");
                    ValidateEffects(skill.Effects, effectIds, $"Skill '{skill.Id}'", errors);
                }
            }

            if (pack.Monsters != null)
                foreach (var monster in pack.Monsters.Where(monster => monster != null))
                {
                    ValidateTextKeys(pack, monster.NameKey, monster.DescKey, $"Monster '{monster.Id}'", errors);
                    foreach (string attribute in monster.Attributes?.Keys ?? Enumerable.Empty<string>())
                        if (pack.Rules?.Attributes == null
                            || !pack.Rules.Attributes.Contains(attribute, StringComparer.OrdinalIgnoreCase))
                            errors.Add($"Monster '{monster.Id}' references undefined attribute '{attribute}'.");
                    if (monster.Attributes == null)
                        errors.Add($"Monster '{monster.Id}' must define an attributes object.");
                }

            string defaultLocale = pack.Meta?.DefaultLocale;
            if (pack.Strings == null
                || string.IsNullOrWhiteSpace(defaultLocale)
                || !pack.Strings.TryGetValue(defaultLocale, out var defaultStrings)
                || defaultStrings == null)
            {
                errors.Add($"Strings dictionary is missing the default locale ('{defaultLocale}').");
            }
            else
            {
                ValidateTextKey(defaultStrings, pack.Scenario?.SystemPromptKey, "Scenario system prompt", errors);
                ValidateTextKey(defaultStrings, pack.Scenario?.OpeningKey, "Scenario opening", errors);
                ValidateTextKey(defaultStrings, pack.Scenario?.DebugSystemPromptKey, "Scenario debug system prompt", errors);
                ValidateTextKey(defaultStrings, pack.Scenario?.DebugOpeningKey, "Scenario debug opening", errors);
            }

            return errors.Count == 0;
        }

        private static void ValidateIds<T>(IEnumerable<T> entries, string category, List<string> errors, Func<T, string> getId)
        {
            if (entries == null) return;
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in entries)
            {
                string id = getId(entry);
                if (!IsValidId(id))
                {
                    errors.Add($"{category} contains an entry with an invalid ID.");
                    continue;
                }
                if (!ids.Add(id))
                    errors.Add($"{category} contains duplicate ID '{id}'.");
            }
        }

        private static void ValidateEffects(
            IEnumerable<EffectDefinition> effects,
            HashSet<string> effectIds,
            string owner,
            List<string> errors)
        {
            foreach (var effect in effects ?? Enumerable.Empty<EffectDefinition>())
            {
                if (effect == null)
                {
                    errors.Add($"{owner} contains a null effect.");
                    continue;
                }
                if (!SupportedOperations.Contains(effect.Op))
                    errors.Add($"{owner} uses unsupported effect operation '{effect.Op}'.");
                if (string.Equals(effect.Op, "apply_effect", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(effect.Op, "sleep_song", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(effect.Op, "thunder_wave", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(effect.Effect))
                {
                    if (!effectIds.Contains(effect.Effect))
                        errors.Add($"{owner} references unknown status effect '{effect.Effect}'.");
                }
            }
        }

        private static void ValidateTextKeys(ChroniclePack pack, string nameKey, string descKey, string owner, List<string> errors)
        {
            string locale = pack.Meta?.DefaultLocale;
            if (pack.Strings == null || string.IsNullOrWhiteSpace(locale) || !pack.Strings.TryGetValue(locale, out var strings))
                return;
            ValidateTextKey(strings, nameKey, $"{owner} name", errors);
            ValidateTextKey(strings, descKey, $"{owner} description", errors);
        }

        private static void ValidateTextKey(Dictionary<string, string> strings, string key, string owner, List<string> errors)
        {
            if (strings == null || string.IsNullOrWhiteSpace(key) || !strings.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
                errors.Add($"{owner} is missing localized text for key '{key}'.");
        }

        private static bool IsValidId(string id) =>
            !string.IsNullOrWhiteSpace(id) && Regex.IsMatch(id, "^[a-z0-9][a-z0-9_-]*$", RegexOptions.CultureInvariant);
    }
}
