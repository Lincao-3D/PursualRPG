using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using PursualRPG.Scripts.Core;
using PursualRPG.Scripts.Domain;

namespace PursualRPG.Scripts.AI
{
    public class ChronicleMeta
    {
        [JsonProperty("id")] public string Id { get; set; } = "default";
        [JsonProperty("version")] public string Version { get; set; } = "1.0.0";
        [JsonProperty("title")] public Dictionary<string, string> Title { get; set; } = new();
        [JsonProperty("default_locale")] public string DefaultLocale { get; set; } = "en_US";
        [JsonProperty("author")] public string Author { get; set; } = "Unknown";
    }

    public class ChronicleRules
    {
        [JsonProperty("attributes")] public List<string> Attributes { get; set; } = new();
        [JsonProperty("attribute_modifier")] public string AttributeModifierFormula { get; set; } = string.Empty;
        [JsonProperty("base_speed")] public int BaseSpeed { get; set; }
        [JsonProperty("ac_base")] public int AcBase { get; set; }
        [JsonProperty("xp_per_level")] public int XpPerLevel { get; set; }
        [JsonProperty("limits")] public Dictionary<string, int> Limits { get; set; } = new();
    }

    public class EffectDefinition
    {
        [JsonProperty("op")] public string Op { get; set; } = string.Empty;
        [JsonProperty("amount")] public int Amount { get; set; }
        [JsonProperty("damage")] public int Damage { get; set; }
        [JsonProperty("type")] public string Type { get; set; } = "physical";
        [JsonProperty("effect")] public string Effect { get; set; } = string.Empty;
        [JsonProperty("duration")] public int Duration { get; set; }
        [JsonProperty("add_spell_mod")] public bool AddSpellMod { get; set; }
        [JsonProperty("targets")] public string Targets { get; set; } = string.Empty;
    }

    public class RaceDto
    {
        [JsonProperty("id")] public string Id { get; set; } = string.Empty;
        [JsonProperty("bonuses")] public Dictionary<string, int> Bonuses { get; set; } = new();
        [JsonProperty("speed")] public int Speed { get; set; } = 30;
        [JsonProperty("name_key")] public string NameKey { get; set; } = string.Empty;
        [JsonProperty("desc_key")] public string DescKey { get; set; } = string.Empty;
    }

    public class ClassDto
    {
        [JsonProperty("id")] public string Id { get; set; } = string.Empty;
        [JsonProperty("initial_life")] public int InitialLife { get; set; } = 100;
        [JsonProperty("initial_mana")] public int InitialMana { get; set; } = 100;
        [JsonProperty("skills")] public List<string> Skills { get; set; } = new();
        [JsonProperty("name_key")] public string NameKey { get; set; } = string.Empty;
        [JsonProperty("desc_key")] public string DescKey { get; set; } = string.Empty;
    }

    public class SkillDto
    {
        [JsonProperty("id")] public string Id { get; set; } = string.Empty;
        [JsonProperty("legacy_enum")] public string LegacyEnum { get; set; } = string.Empty;
        [JsonProperty("cost")] public int Cost { get; set; }
        [JsonProperty("min_level")] public int MinLevel { get; set; } = 1;
        [JsonProperty("classes")] public List<string> Classes { get; set; } = new();
        [JsonProperty("is_combat")] public bool IsCombat { get; set; }
        [JsonProperty("is_targeted")] public bool IsTargeted { get; set; }
        [JsonProperty("skip_turn")] public bool SkipTurn { get; set; }
        [JsonProperty("passive")] public bool Passive { get; set; }
        [JsonProperty("effects")] public List<EffectDefinition> Effects { get; set; } = new();
        [JsonProperty("name_key")] public string NameKey { get; set; } = string.Empty;
        [JsonProperty("desc_key")] public string DescKey { get; set; } = string.Empty;
    }

    public class ItemDto
    {
        [JsonProperty("id")] public string Id { get; set; } = string.Empty;
        [JsonProperty("numeric_id")] public int NumericId { get; set; }
        [JsonProperty("value")] public int Value { get; set; }
        [JsonProperty("usable")] public bool Usable { get; set; }
        [JsonProperty("useless")] public bool Useless { get; set; }
        [JsonProperty("effects")] public List<EffectDefinition> Effects { get; set; } = new();
        [JsonProperty("name_key")] public string NameKey { get; set; } = string.Empty;
        [JsonProperty("desc_key")] public string DescKey { get; set; } = string.Empty;
    }

    public class MonsterDto
    {
        [JsonProperty("id")] public string Id { get; set; } = string.Empty;
        [JsonProperty("legacy_enum")] public string LegacyEnum { get; set; } = string.Empty;
        [JsonProperty("name_key")] public string NameKey { get; set; } = string.Empty;
        [JsonProperty("desc_key")] public string DescKey { get; set; } = string.Empty;
        [JsonProperty("hp")] public int Hp { get; set; } = 10;
        [JsonProperty("armor")] public int Armor { get; set; }
        [JsonProperty("dodge")] public int Dodge { get; set; }
        [JsonProperty("base_damage")] public int BaseDamage { get; set; } = 5;
        [JsonProperty("attributes")] public Dictionary<string, int> Attributes { get; set; } = new();
        [JsonProperty("category")] public string Category { get; set; } = "humanoid";
        [JsonProperty("xp")] public int Xp { get; set; } = 10;
        [JsonProperty("gold")] public int Gold { get; set; } = 2;
        [JsonProperty("image")] public string Image { get; set; } = string.Empty;
    }

    public class ScenarioDto
    {
        [JsonProperty("system_prompt_key")] public string SystemPromptKey { get; set; } = string.Empty;
        [JsonProperty("opening_key")] public string OpeningKey { get; set; } = string.Empty;
        [JsonProperty("debug_system_prompt_key")] public string DebugSystemPromptKey { get; set; } = string.Empty;
        [JsonProperty("debug_opening_key")] public string DebugOpeningKey { get; set; } = string.Empty;
    }

    public class StatusEffectDto
    {
        [JsonProperty("id")] public string Id { get; set; } = string.Empty;
        [JsonProperty("stackable")] public bool Stackable { get; set; } = true;
        [JsonProperty("skip_turn")] public bool SkipTurn { get; set; }
        [JsonProperty("positive")] public bool Positive { get; set; }
    }

    public class ChroniclePack
    {
        [JsonProperty("schema")] public int Schema { get; set; } = 1;
        [JsonProperty("meta")] public ChronicleMeta Meta { get; set; } = new();
        [JsonProperty("rules")] public ChronicleRules Rules { get; set; } = new();
        [JsonProperty("races")] public List<RaceDto> Races { get; set; } = new();
        [JsonProperty("classes")] public List<ClassDto> Classes { get; set; } = new();
        [JsonProperty("skills")] public List<SkillDto> Skills { get; set; } = new();
        [JsonProperty("items")] public List<ItemDto> Items { get; set; } = new();
        [JsonProperty("monsters")] public List<MonsterDto> Monsters { get; set; } = new();
        [JsonProperty("flaws")] public List<string> Flaws { get; set; } = new();
        [JsonProperty("effects")] public List<StatusEffectDto> Effects { get; set; } = new();
        [JsonProperty("scenario")] public ScenarioDto Scenario { get; set; } = new();
        [JsonProperty("strings")] public Dictionary<string, Dictionary<string, string>> Strings { get; set; } = new();
        [JsonProperty("assets")] public Dictionary<string, Dictionary<string, string>> Assets { get; set; } = new();
    }

    public static class ChronicleRegistry
    {
        public static ChroniclePack Current { get; private set; } = new();

        public static void LoadChronicle(string jsonPath)
        {
            if (!Godot.FileAccess.FileExists(jsonPath))
            {
                GD.PushError($"[ChronicleRegistry] Chronicle file not found at {jsonPath}");
                return;
            }

            try
            {
                using var file = Godot.FileAccess.Open(jsonPath, Godot.FileAccess.ModeFlags.Read);
                if (file == null)
                {
                    GD.PushError($"[ChronicleRegistry] Could not open chronicle file: {jsonPath}");
                    return;
                }

                string json = file.GetAsText();
                if (!TryLoadChronicleJson(json, out var errors))
                    GD.PushError($"[ChronicleRegistry] Invalid chronicle pack: {string.Join("; ", errors)}");
            }
            catch (Exception ex)
            {
                GD.PushError($"[ChronicleRegistry] Failed to parse chronicle pack: {ex.Message}");
            }
        }

        public static bool TryLoadChronicleJson(string json, out System.Collections.Generic.List<string> errors)
        {
            errors = new System.Collections.Generic.List<string>();
            try
            {
                var pack = JsonConvert.DeserializeObject<ChroniclePack>(json);
                if (!ChronicleValidator.Validate(pack, out errors))
                    return false;

                Current = pack;
                GD.Print($"[ChronicleRegistry] Successfully loaded chronicle pack: {Current.Meta.Id} (v{Current.Meta.Version})");
                return true;
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to parse chronicle pack: {ex.Message}");
                return false;
            }
        }

        public static RaceDto GetRace(string id) =>
            Current.Races.FirstOrDefault(r => string.Equals(r.Id, id, StringComparison.OrdinalIgnoreCase));

        public static ClassDto GetClass(string id) =>
            Current.Classes.FirstOrDefault(c => string.Equals(c.Id, id, StringComparison.OrdinalIgnoreCase));

        public static SkillDto GetSkill(string id) =>
            Current.Skills.FirstOrDefault(s =>
                string.Equals(s.Id, id, StringComparison.OrdinalIgnoreCase)
                || string.Equals(s.LegacyEnum, id, StringComparison.OrdinalIgnoreCase));

        public static ItemDto GetItem(string id) =>
            Current.Items.FirstOrDefault(i =>
                string.Equals(i.Id, id, StringComparison.OrdinalIgnoreCase)
                || string.Equals(i.NumericId.ToString(), id, StringComparison.Ordinal));

        public static ItemDto GetItem(int numericId) =>
            Current.Items.FirstOrDefault(i => i.NumericId == numericId);

        public static MonsterDto GetMonster(string id) =>
            Current.Monsters.FirstOrDefault(m =>
                string.Equals(m.Id, id, StringComparison.OrdinalIgnoreCase)
                || string.Equals(m.LegacyEnum, id, StringComparison.OrdinalIgnoreCase));

        public static SkillDto GetSkillByLegacyId(string id) =>
            Current.Skills.FirstOrDefault(s => string.Equals(s.LegacyEnum, id, StringComparison.OrdinalIgnoreCase));

        public static CharacterClass CreateClass(string id)
        {
            var dto = GetClass(id);
            return dto == null ? null : new CharacterClass(dto.Id, dto.InitialLife, dto.InitialMana, dto.NameKey, dto.DescKey);
        }

        public static Skill CreateSkill(string id)
        {
            var dto = GetSkill(id);
            return dto == null ? null : new Skill(dto);
        }

        public static List<Skill> GetSkillsForClass(string classId) =>
            Current.Skills
                .Where(skill => skill.Classes.Contains(classId, StringComparer.OrdinalIgnoreCase))
                .Select(skill => new Skill(skill))
                .ToList();

        public static GenericItem CreateItem(int numericId)
        {
            var dto = GetItem(numericId);
            return dto == null ? null : new GenericItem(dto);
        }

        public static Entity CreateMonster(string id)
        {
            var dto = GetMonster(id);
            if (dto == null)
                return null;

            var attributes = new Dictionary<CharacterAttrib, int>();
            foreach (var attribute in dto.Attributes)
            {
                if (RaceUtils.TryParseAttribute(attribute.Key, out var parsed))
                    attributes[parsed] = attribute.Value;
            }

            Enum.TryParse(dto.Category, true, out EntityCategory category);
            string displayName = ContentText.Get(dto.NameKey);
            if (string.IsNullOrEmpty(displayName) || displayName == dto.NameKey)
                displayName = dto.Id.Replace('_', ' ');
            return new Entity(dto.Id, displayName, dto.Hp, dto.Armor, dto.Dodge, dto.BaseDamage, attributes, category)
            {
                XpValue = dto.Xp,
                GoldValue = dto.Gold,
                Image = dto.Image
            };
        }

        public static Scenario CreateScenario(bool debug = false)
        {
            var scenario = Current?.Scenario;
            if (scenario == null)
                return new Scenario(string.Empty, string.Empty);

            string systemKey = debug ? scenario.DebugSystemPromptKey : scenario.SystemPromptKey;
            string openingKey = debug ? scenario.DebugOpeningKey : scenario.OpeningKey;
            return new Scenario(ContentText.Get(systemKey), ContentText.Get(openingKey));
        }
    }
}