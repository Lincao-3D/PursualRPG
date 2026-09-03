using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using PursualRPG.Scripts.Domain;
using PursualRPG.Scripts.AI;
using PursualRPG.Scripts.Core;

namespace PursualRPG.Scripts.Scenes
{
    public partial class ChatScene : Control
    {
        [Export] public AIMessageBroker MessageBroker;
        [Export] public float TextSpeed = 0.05f;
        
        private RichTextLabel _historyText;
        private LineEdit _userInput;
        private Button _submitButton;
        private Button _combatButton;
        
        private Combat _eminentCombat;
        private Scenario _scenario;
        private bool _savingMode = false;

        public override void _Ready()
        {
            _historyText = GetNode<RichTextLabel>("VBoxContainer/ScrollContainer/HistoryText");
            _userInput = GetNode<LineEdit>("VBoxContainer/HBoxContainer/UserInput");
            _submitButton = GetNode<Button>("VBoxContainer/HBoxContainer/SubmitButton");
            _combatButton = GetNode<Button>("CombatButton");

            _submitButton.Pressed += OnSubmitPressed;
            _userInput.TextSubmitted += OnTextSubmitted;
            _combatButton.Pressed += OnCombatButtonPressed;
            _combatButton.Visible = false;
            _combatButton.Disabled = true;

            _scenario = Scenario.DefaultScenario;
            InitializeChatHistory();
        }

        private void InitializeChatHistory()
        {
            _historyText.Text = $"DM:\n{_scenario.InitialMessage}\n";
        }

        public async Task DisplayChatCardAsync(ChatMessage card)
        {
            string speakerName = string.IsNullOrEmpty(card.SpeakerKey) ? "DM" : Tr(card.SpeakerKey);
            _historyText.AppendText($"\n[color=yellow]{speakerName}:[/color] ");
            
            foreach (char c in card.Text)
            {
                _historyText.AppendText(c.ToString());
                await ToSignal(GetTree().CreateTimer(TextSpeed), SceneTreeTimer.SignalName.Timeout);
            }

            _userInput.Visible = !card.IsNarrative;
            if (_submitButton != null) _submitButton.Visible = !card.IsNarrative;
            
            if (card.Choices != null && card.Choices.Count > 0)
            {
                _historyText.AppendText($"\n\n[b]{Tr("TXT_YOUR_OPTIONS")}:[/b]\n");
                for (int i = 0; i < card.Choices.Count; i++)
                {
                    var choice = card.Choices[i];
                    string choiceText = !string.IsNullOrEmpty(choice.TextKey) ? Tr(choice.TextKey) : choice.Value;
                    _historyText.AppendText($"[color=cyan]{i + 1}[/color]. {choiceText}\n");
                }
            }
        }

        private void OnSubmitPressed()
        {
            ProcessInput(_userInput.Text);
        }

        private void OnTextSubmitted(string text)
        {
            ProcessInput(text);
        }

        private void ProcessInput(string text)
        {
            text = text.Trim();
            if (string.IsNullOrEmpty(text)) return;

            if (_savingMode)
            {
                GameManager.Instance.SaveGame();
                AppendLog($"\n[System: Game saved as '{text}']\n");
                _savingMode = false;
                _userInput.Clear();
                return;
            }

            if (text.StartsWith("/"))
            {
                HandleCommand(text);
                _userInput.Clear();
                return;
            }

            _userInput.Clear();
            AppendLog($"\nPlayer:\n{text}\nDM:\n");
            SendToLLM(text);
        }

        private void HandleCommand(string commandText)
        {
            var parts = commandText[1..].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return;
            string cmd = parts[0].ToLower();

            if (cmd == "quit" || cmd == "exit")
            {
                GetTree().ChangeSceneToFile("res://Scenes/MainMenuScene.tscn");
            }
            else if (cmd == "player" || cmd == "sheet")
            {
                AppendLog("\nSystem:\nCharacter Sheet requested. Open panel via UI.\n");
            }
            else if (cmd == "save")
            {
                _savingMode = true;
                AppendLog("\nEnter save file name or identifier:\n");
            }
        }

        private async void SendToLLM(string prompt)
        {
            if (MessageBroker != null)
            {
                MessageBroker.SendMessageAsync(prompt, (response) =>
                {
                    CallDeferred(nameof(ReceiveLLMResponse), response);
                });
            }
            else
            {
                await Task.Delay(500);
                ReceiveLLMResponse("The shadows grow longer as you ponder your next move. ```json\n{ \"tool\": \"none\" }\n```");
            }
        }

        private void ReceiveLLMResponse(string responseText)
        {
            var broker = MessageBroker ?? new AIMessageBroker();
            if (broker.TryParseResponse(responseText, out var aiResponse))
            {
                foreach (var msg in aiResponse.Messages)
                {
                    _ = DisplayChatCardAsync(msg);
                }

                foreach (var toolCmd in aiResponse.ToolCommands)
                {
                    ExecuteToolAction(toolCmd);
                }
            }
            else
            {
                AppendLog($"DM: {responseText}\n");
            }
        }

        private void ExecuteToolAction(AIToolCommand command)
        {
            if (command.Name == "initialize_combat")
            {
                var enemies = new List<Entity> { MonsterFactory.GetFactories()[EnemyEnum.Skeleton] };
                _eminentCombat = new Combat(this, enemies, fleeable: true);
                WaitCombatConfirm(_eminentCombat);
            }
            else if (command.Name == "reward_player")
            {
                int gold = 10;
                int xp = 50;

                if (command.Arguments.TryGetValue("gold", out var goldElem))
                {
                    if (goldElem.ValueKind == JsonValueKind.Number) gold = goldElem.GetInt32();
                }
                if (command.Arguments.TryGetValue("xp", out var xpElem))
                {
                    if (xpElem.ValueKind == JsonValueKind.Number) xp = xpElem.GetInt32();
                }

                if (GameManager.Instance != null && GameManager.Instance.CurrentPlayer != null)
                {
                    GameManager.Instance.CurrentPlayer.Gold += gold;
                    GameManager.Instance.CurrentPlayer.Xp += xp;
                }
                AppendLog($"\n[System: Rewarded +{gold} Gold, +{xp} XP]\n");
            }
        }

        public void WaitCombatConfirm(Combat combat)
        {
            _submitButton.Visible = false;
            _userInput.Visible = false;
            _combatButton.Visible = false;
            GetTree().ChangeSceneToFile("res://Scenes/CombatScene.tscn");
        }

        private void OnCombatButtonPressed()
        {
            if (_eminentCombat != null)
            {
                GetTree().ChangeSceneToFile("res://Scenes/CombatScene.tscn");
            }
        }

        private void AppendLog(string message)
        {
            _historyText.Text += message;
        }
    }
}