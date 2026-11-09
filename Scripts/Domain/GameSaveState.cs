using Newtonsoft.Json;
using System.Collections.Generic;

namespace PursualRPG.Scripts.Domain
{
    public class TurnContent
    {
        [JsonProperty("role")]
        [System.Text.Json.Serialization.JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonProperty("parts")]
        [System.Text.Json.Serialization.JsonPropertyName("parts")]
        public List<Dictionary<string, string>> Parts { get; set; }
    }

    public class GameSaveState
    {
        /// <summary>Schema version for forward/backward compatibility checks.</summary>
        [JsonProperty("schema_version")]
        [System.Text.Json.Serialization.JsonPropertyName("schema_version")]
        public int SchemaVersion { get; set; } = 1;

        [JsonProperty("player")]
        [System.Text.Json.Serialization.JsonPropertyName("player")]
        public Player PlayerData { get; set; }

        [JsonProperty("scenario")]
        [System.Text.Json.Serialization.JsonPropertyName("scenario")]
        public Scenario ActiveScenario { get; set; }

        [JsonProperty("raw_ui_log")]
        [System.Text.Json.Serialization.JsonPropertyName("raw_ui_log")]
        public string RawUILog { get; set; } = string.Empty;

        [JsonProperty("chat_history")]
        [System.Text.Json.Serialization.JsonPropertyName("chat_history")]
        public List<TurnContent> ConversationHistory { get; set; } = new();

        /// <summary>Rolling summary of older turns for long-term memory injection.</summary>
        [JsonProperty("memory_summary")]
        [System.Text.Json.Serialization.JsonPropertyName("memory_summary")]
        public string MemorySummary { get; set; } = string.Empty;

        /// <summary>
        /// Migrate handles forward compatibility. If a save's SchemaVersion is higher
        /// than the current app version, loading is aborted by the caller.
        /// </summary>
        public bool Migrate(int appSchemaVersion, out string errorMessage)
        {
            errorMessage = null;
            if (SchemaVersion > appSchemaVersion)
            {
                errorMessage = $"Save file is from a newer version of the game (save v{SchemaVersion}, app v{appSchemaVersion}). Please update the app.";
                return false;
            }
            // Future migration steps go here (e.g., v1 -> v2 field renames).
            return true;
        }
    }
}
