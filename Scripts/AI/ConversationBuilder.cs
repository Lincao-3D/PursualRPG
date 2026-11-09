using System.Collections.Generic;
using System.Linq;
using System.Text;
using PursualRPG.Scripts.Domain;

namespace PursualRPG.Scripts.AI
{
    /// <summary>
    /// Builds a clean, normalized conversation payload for the Gemini API.
    /// - Takes last N turns (default 24)
    /// - Ensures first turn is always "user"
    /// - Merges consecutive same-role turns with \n\n
    /// - Optionally prepends a memory summary as an extra user context message
    /// </summary>
    public static class ConversationBuilder
    {
        public const int DefaultWindowSize = 24;

        public static List<TurnContent> BuildPayload(
            IReadOnlyList<TurnContent> history,
            string memorySummary = "",
            int windowSize = DefaultWindowSize)
        {
            // Take last N turns
            var window = history.Count > windowSize
                ? history.Skip(history.Count - windowSize).ToList()
                : history.ToList();

            // Merge consecutive same-role turns
            var merged = new List<TurnContent>();
            foreach (var turn in window)
            {
                if (merged.Count > 0 && merged.Last().Role == turn.Role)
                {
                    // Append to the last turn's text
                    var last = merged.Last();
                    string lastText = last.Parts?.FirstOrDefault()?
                        .GetValueOrDefault("text", "") ?? "";
                    string newText = turn.Parts?.FirstOrDefault()?
                        .GetValueOrDefault("text", "") ?? "";
                    last.Parts = new List<Dictionary<string, string>>
                    {
                        new() { { "text", lastText + "\n\n" + newText } }
                    };
                }
                else
                {
                    merged.Add(new TurnContent
                    {
                        Role = turn.Role,
                        Parts = turn.Parts != null
                            ? new List<Dictionary<string, string>>(turn.Parts)
                            : new List<Dictionary<string, string>>()
                    });
                }
            }

            // Ensure first turn is "user" (Gemini requires user first)
            if (merged.Count > 0 && merged[0].Role != "user")
                merged.RemoveAt(0);

            // If still starts with model, prepend an empty user context turn
            if (merged.Count == 0 || merged[0].Role != "user")
            {
                merged.Insert(0, new TurnContent
                {
                    Role = "user",
                    Parts = new List<Dictionary<string, string>>
                    {
                        new() { { "text", "[New adventure begins]" } }
                    }
                });
            }

            // Inject memory summary as a system-context user message at the front if available
            if (!string.IsNullOrWhiteSpace(memorySummary))
            {
                merged.Insert(0, new TurnContent
                {
                    Role = "user",
                    Parts = new List<Dictionary<string, string>>
                    {
                        new() { { "text", $"[Chronicle Memory – Summary of earlier events]:\n{memorySummary}" } }
                    }
                });
                // Insert a model ack so the roles alternate correctly
                merged.Insert(1, new TurnContent
                {
                    Role = "model",
                    Parts = new List<Dictionary<string, string>>
                    {
                        new() { { "text", "Understood. I'll keep this chronicle in mind as we continue." } }
                    }
                });
            }

            return merged;
        }
    }
}
