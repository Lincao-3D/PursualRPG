using Godot;
using System;
using System.Collections.Generic;
using PursualRPG.Scripts.Domain;

namespace PursualRPG.Scripts.AI
{
    public partial class AIMessageBroker : Node
    {
        [Export] private LLMClient _llmClient;
        [Export] public float TokensPerSecond { get; set; } = 45.0f;

        private readonly Queue<char> _tokenQueue = new();
        private double _tokenAccumulator;
        private bool _isGenerating;

        private Action<string> _pendingCompletion;
        private string _pendingResponse;

        public Scenario ActiveScenario { get; set; } =
            Scenario.DefaultScenario;

        public delegate void TextTokenStreamHandler(string token);

        public event TextTokenStreamHandler OnTokenStreamed;
        
        public override void _Ready()
        {
            if (_llmClient == null)
            {
                _llmClient = GetNodeOrNull<LLMClient>("LLMClient");
            }
        }
        
        public override void _Process(double delta)
        {
            if (_tokenQueue.Count > 0)
            {
                _tokenAccumulator += delta * TokensPerSecond;

                while (_tokenAccumulator >= 1.0 &&
                       _tokenQueue.Count > 0)
                {
                    _tokenAccumulator -= 1.0;
                    OnTokenStreamed?.Invoke(
                        _tokenQueue.Dequeue().ToString());
                }
            }

            if (_isGenerating &&
                _tokenQueue.Count == 0 &&
                _pendingCompletion != null)
            {
                _isGenerating = false;

                var completion = _pendingCompletion;
                var response = _pendingResponse;

                _pendingCompletion = null;
                _pendingResponse = null;

                completion.Invoke(response);
            }
        }

        public void SendMessageAsync(
            string message,
            Action<string> onComplete)
        {
            SendMessageAsync(message, string.Empty, onComplete);
        }

        // Full 3-argument implementation with conversation context injection
        public async void SendMessageAsync(
            string message,
            string chatHistoryContext,
            Action<string> onComplete)
        {
            if (_isGenerating || _llmClient == null)
                return;

            _isGenerating = true;
            _tokenAccumulator = 0.0;

            string systemPrompt =
                ActiveScenario?.SystemPrompt ??
                Scenario.DefaultScenario.SystemPrompt;

            // Ensure LLM receives contextual awareness by prefixing recent dialogue context if desired,
            // or passing the player's message directly with system instructions.
            // FIX: Combine previous chat transcript context with the new user message
            string contextualMessage = string.IsNullOrEmpty(chatHistoryContext) 
                ? message 
                : $"[Previous Conversation History]:\n{chatHistoryContext}\n\n[Current Player Input]: {message}";

            string response = await _llmClient.GenerateContentAsync(
                systemPrompt,
                contextualMessage);

            _isGenerating = false;
            
            // Directly invoke completion without fake token queue lag
            onComplete?.Invoke(response);
        }

        public bool TryParseResponse(
            string rawInput,
            out AIResponse response)
        {
            return AIMessageParser.TryParseResponse(
                rawInput,
                out response);
        }

        public bool ValidateAndDeserializePayload(
            string rawJson,
            out AIResponse response)
        {
            return TryParseResponse(rawJson, out response);
        }
    }
}