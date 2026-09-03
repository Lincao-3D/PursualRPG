using Godot;
using System;
using System.Collections.Generic;

namespace PursualRPG.Scripts.AI
{
    public partial class AIMessageBroker : Node
    {
        [Export] private LLMClient _llmClient;
        private Queue<char> _tokenQueue = new();
        private bool _isGenerating = false;

        public delegate void TextTokenStreamHandler(string token);
        public event TextTokenStreamHandler OnTokenStreamed;

        public async void SendMessageAsync(string message, System.Action<string> onComplete)
        {
            if (_isGenerating) return;
            _isGenerating = true;

            string response = await _llmClient.GenerateContentAsync("You are an RPG Game Master.", message);
            
            // Queue response for smooth typewriter stream simulation
            foreach (char c in response)
            {
                _tokenQueue.Enqueue(c);
            }

            _isGenerating = false;
            onComplete?.Invoke(response);
        }

        public bool ValidateAndDeserializePayload(string rawJson, out Godot.Collections.Dictionary parsedData)
        {
            parsedData = null;
            var json = new Json();
            if (json.Parse(rawJson) == Error.Ok)
            {
                parsedData = json.Data.AsGodotDictionary();
                return true;
            }
            return false;
        }
    }
}