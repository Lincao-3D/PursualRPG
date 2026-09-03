using Godot;
using System.Text;
using System.Threading.Tasks;

namespace PursualRPG.Scripts.AI
{
    public partial class LLMClient : Node
    {
        private HttpRequest _httpRequest;
        private string _apiKey;
        private string _modelName = "gemini-2.5-flash"; // Or configured model

        public override void _Ready()
        {
            _httpRequest = new HttpRequest();
            AddChild(_httpRequest);
            _apiKey = System.Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? "";
        }

        public async Task<string> GenerateContentAsync(string systemPrompt, string userMessage)
        {
            string url = $"https://generativelanguage.googleapis.com/v1beta/models/{_modelName}:generateContent?key={_apiKey}";
            
            var payload = new
            {
                system_instruction = new { parts = new[] { new { text = systemPrompt } } },
                contents = new[] { new { role = "user", parts = new[] { new { text = userMessage } } } }
            };

            string jsonPayload = Json.Stringify(payload);
            string[] headers = new[] { "Content-Type: application/json" };

            var error = _httpRequest.Request(url, headers, HttpClient.Method.Post, jsonPayload);
            if (error != Error.Ok)
            {
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
                                return parts[0].AsGodotDictionary()["text"].AsString();
                            }
                        }
                    }
                }
            }
            return $"[API Error Code: {responseCode}]";
        }
    }
}