using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace PursualRPG.Scripts.AI;

public sealed class ChatChoice
{
    [JsonPropertyName("textKey")]
    public string TextKey { get; init; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; init; } = string.Empty;
}

public sealed class ChatMessage
{
    [JsonPropertyName("speakerKey")]
    public string SpeakerKey { get; init; } = "CHAT_DM";

    [JsonPropertyName("text")]
    public string Text { get; init; } = string.Empty;

    [JsonPropertyName("isNarrative")]
    public bool IsNarrative { get; init; }

    [JsonPropertyName("choices")]
    public IReadOnlyList<ChatChoice> Choices { get; init; } = Array.Empty<ChatChoice>();
}

public sealed class AIToolCommand
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("arguments")]
    public IReadOnlyDictionary<string, JsonElement> Arguments { get; init; } = new Dictionary<string, JsonElement>();
}

public sealed class AIResponse
{
    [JsonPropertyName("messages")]
    public IReadOnlyList<ChatMessage> Messages { get; init; } = Array.Empty<ChatMessage>();

    [JsonPropertyName("toolCommands")]
    public IReadOnlyList<AIToolCommand> ToolCommands { get; init; } = Array.Empty<AIToolCommand>();
}

public static class AIMessageParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true
    };

    public static bool TryParseResponse(string rawInput, out AIResponse response)
    {
        response = new AIResponse();
        if (string.IsNullOrWhiteSpace(rawInput)) return false;

        try
        {
            string cleaned = rawInput.Trim();
            var match = Regex.Match(cleaned, @"```json\s*(.*?)\s*```", RegexOptions.Singleline);
            string jsonCandidate = match.Success ? match.Groups[1].Value : cleaned;

            if (jsonCandidate.StartsWith("{") && jsonCandidate.EndsWith("}"))
            {
                using var doc = JsonDocument.Parse(jsonCandidate);
                var root = doc.RootElement;

                List<ChatMessage> messages = new();
                List<AIToolCommand> tools = new();

                if (root.TryGetProperty("messages", out var msgElement) && msgElement.ValueKind == JsonValueKind.Array)
                {
                    var parsedResponse = JsonSerializer.Deserialize<AIResponse>(jsonCandidate, JsonOptions);
                    if (parsedResponse != null)
                    {
                        response = parsedResponse;
                        return true;
                    }
                }

                if (root.TryGetProperty("tool", out var toolProp) || root.TryGetProperty("name", out toolProp))
                {
                    string toolName = toolProp.GetString() ?? "";
                    Dictionary<string, JsonElement> args = new();

                    if (root.TryGetProperty("arguments", out var argsElement) && argsElement.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var prop in argsElement.EnumerateObject())
                        {
                            args[prop.Name] = prop.Value.Clone();
                        }
                    }
                    else
                    {
                        foreach (var prop in root.EnumerateObject())
                        {
                            if (prop.Name != "tool" && prop.Name != "name")
                            {
                                args[prop.Name] = prop.Value.Clone();
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(toolName))
                    {
                        tools.Add(new AIToolCommand { Name = toolName, Arguments = args });
                    }
                }

                string textWithoutJson = Regex.Replace(cleaned, @"```json\s*.*?\s*```", "", RegexOptions.Singleline).Trim();
                if (!string.IsNullOrEmpty(textWithoutJson))
                {
                    messages.Add(new ChatMessage
                    {
                        SpeakerKey = "CHAT_DM",
                        Text = textWithoutJson,
                        IsNarrative = false
                    });
                }

                response = new AIResponse { Messages = messages, ToolCommands = tools };
                return true;
            }

            response = new AIResponse
            {
                Messages = new List<ChatMessage>
                {
                    new ChatMessage { SpeakerKey = "CHAT_DM", Text = cleaned, IsNarrative = false }
                }
            };
            return true;
        }
        catch
        {
            response = new AIResponse
            {
                Messages = new List<ChatMessage>
                {
                    new ChatMessage { SpeakerKey = "CHAT_DM", Text = rawInput, IsNarrative = false }
                }
            };
            return false;
        }
    }
}
