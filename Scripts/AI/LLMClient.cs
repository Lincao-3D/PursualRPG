using Godot;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace PursualRPG.Scripts.AI
{
    public partial class LLMClient : Node
    {
        private HttpRequest _httpRequest;
        private string _apiKey;
        private string _modelName = "gemini-3.1-flash-lite";

        // B.1: Internal turn list
        private readonly List<(string role, string text)> _turns = new();

        public override void _Ready()
        {
            _httpRequest = new HttpRequest();
            AddChild(_httpRequest);
            _apiKey = System.Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? "";
            
            if (string.IsNullOrEmpty(_apiKey))
            {
                GD.PushWarning("GEMINI_API_KEY environment variable not found. Trying local config...");
                LoadFromLocalConfig();
            }

            if (string.IsNullOrEmpty(_apiKey))
            {
                GD.PushError("Critical: AI model settings could not be loaded! Requests will fail.");
            }
            else
            {
                GD.Print("LLM settings loaded successfully.");
            }
        }

        private void LoadFromLocalConfig()
        {
            var config = new ConfigFile();
            Error err = config.Load("res://sc_config.cfg");
            if (err == Error.Ok)
            {
                _apiKey = (string)config.GetValue("api", "GEMINI_API_KEY", "");
            }
        }

        // B.1: Clean state for new games
        public void ResetConversation() => _turns.Clear();

        // B.3: Inject history once
        public void SeedConversationHistory(string strippedHistory)
        {
            _turns.Clear();
            _turns.Add(("user", $"[Recap of prior session]:\n{strippedHistory}"));
            // Insert an artificial model turn so the subsequent player input doesn't trigger a 400 Bad Request (Gemini requires alternating roles)
            _turns.Add(("model", "Understood. I am ready to continue the campaign based on this history."));
        }

        // B.1: Stateful multi-turn generation
        public async Task<string> GenerateContentAsync(string systemPrompt, string userMessage)
        {
            _turns.Add(("user", userMessage));

            string url = $"https://generativelanguage.googleapis.com/v1beta/models/{_modelName}:generateContent?key={_apiKey}";
            
            var payload = new
            {
                system_instruction = new { parts = new[] { new { text = systemPrompt } } },
                contents = _turns.Select(t => new { role = t.role, parts = new[] { new { text = t.text } } }).ToArray()
            };

            string jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);
            string[] headers = new[] { "Content-Type: application/json" };

            var error = _httpRequest.Request(url, headers, HttpClient.Method.Post, jsonPayload);
            if (error != Error.Ok)
            {
                _turns.RemoveAt(_turns.Count - 1); // Pop user turn on fail
                return "[Error: Failed to dispatch request]";
            }

            var response = await ToSignal(_httpRequest, HttpRequest.SignalName.RequestCompleted);
            int responseCode = (int)((long)response[1]);
            byte[] body = (byte[])response[3];
            string responseString = Encoding.UTF8.GetString(body);

            if (responseCode == 200)
            {
                var json = new Json();
                if (json.Parse(responseString) == Error.Ok)
                {
                    var data = json.Data.AsGodotDictionary();
                    if (data.ContainsKey("candidates"))
                    {
                        var candidates = data["candidates"].AsGodotArray();
                        if (candidates.Count > 0)
                        {
                            var candidate = candidates[0].AsGodotDictionary();
                            var content = candidate["content"].AsGodotDictionary();
                            var parts = content["parts"].AsGodotArray();
                            if (parts.Count > 0)
                            {
                                string resultText = parts[0].AsGodotDictionary()["text"].AsString();
                                _turns.Add(("model", resultText)); // Append model reply
                                return resultText;
                            }
                        }
                    }
                }
            }
            
            // Pop the last user turn if the API call failed so retries don't stack duplicates
            if (_turns.Count > 0 && _turns.Last().role == "user")
            {
                _turns.RemoveAt(_turns.Count - 1);
            }
            return $"[API Error Code: {responseCode}]";
        }
    }
}