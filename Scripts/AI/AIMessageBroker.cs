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

        public async void SendMessageAsync(
            string message,
            Action<string> onComplete)
        {
            if (_isGenerating || _llmClient == null)
                return;

            _isGenerating = true;
            _tokenAccumulator = 0.0;

            string systemPrompt =
                ActiveScenario?.SystemPrompt ??
                Scenario.DefaultScenario.SystemPrompt;

            string response = await _llmClient.GenerateContentAsync(
                systemPrompt,
                message);

            foreach (char character in response)
                _tokenQueue.Enqueue(character);

            _pendingResponse = response;
            _pendingCompletion = onComplete;
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