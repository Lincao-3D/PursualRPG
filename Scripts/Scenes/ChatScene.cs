using Godot;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks; // Fixes "The name 'Task' does not exist"
using PursualRPG.Scripts.Domain; // Fixes "DamageType does not exist"
using PursualRPG.Scripts.AI;
using PursualRPG.Scripts.Core; // Fixes Core namespace issue

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

            // Load Scenario (matching Python initial instructions & initial_message)
            _scenario = Scenario.DefaultScenario;
            InitializeChatHistory();
        }

        private void InitializeChatHistory()
        {
            _historyText.Text = $"DM:\n{_scenario.InitialMessage}\n";
        }

        // Resolves the Split() and 'char' indexing errors with a native typewriter effect
        public async Task DisplayChatCardAsync(AIPayload card)
        {
            // Translating character names/roles using Tr()
            _historyText.AppendText($"\n[color=yellow]{Tr(card.Speaker)}:[/color] ");
            
            // In C#, iterate directly over the string natively without Split("")
            foreach (char c in card.Text)
            {
                _historyText.AppendText(c.ToString());
                await ToSignal(GetTree().CreateTimer(TextSpeed), SceneTreeTimer.SignalName.Timeout);
            }

            _userInput.Visible = !card.IsNarrative;
            if (_submitButton != null) _submitButton.Visible = !card.IsNarrative;
            
            if (card.Choices != null && card.Choices.Length > 0)
            {
                _historyText.AppendText($"\n\n[b]{Tr("TXT_YOUR_OPTIONS")}:[/b]\n");
                for (int i = 0; i < card.Choices.Length; i++)
                {
                    // Tr() wraps the dynamic AI choices if they match predefined keys, 
                    // or renders the raw text directly if there is no key match.
                    _historyText.AppendText($"[color=cyan]{i + 1}[/color]. {Tr(card.Choices[i])}\n");
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
            var parts = commandText[1:].Split(' ', StringSplitOptions.RemoveEmptyEntries);
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
                // Fallback for testing without active broker node export
                await Task.Delay(500);
                ReceiveLLMResponse("The shadows grow longer as you ponder your next move. ```json\n{ \"tool\": \"none\" }\n```");
            }
        }

        private void ReceiveLLMResponse(string responseText)
        {
            ExtractAndApplyJsonPayloads(responseText);
            responseText = Regex.Replace(responseText, @"```json\s*.*?\s*```", "", RegexOptions.Singleline);
            AppendLog($"DM: {responseText}\n");
        }

        private void ExtractAndApplyJsonPayloads(string text)
        {
            var matches = Regex.Matches(text, @"```json\s*(.*?)\s*```", RegexOptions.Singleline);
            foreach (Match match in matches)
            {
                try
                {
                    string jsonStr = match.Groups[1].Value;
                    var json = new Json();
                    if (json.Parse(jsonStr) == Error.Ok)
                    {
                        var dict = json.Data.AsGodotDictionary();
                        if (dict.ContainsKey("tool"))
                        {
                            string toolName = dict["tool"].AsString();
                            ExecuteToolAction(toolName, dict);
                        }
                    }
                }
                catch (Exception e)
                {
                    GD.PrintErr($"Failed to parse JSON tool payload: {e.Message}");
                }
            }
        }

        private void ExecuteToolAction(string toolName, Godot.Collections.Dictionary data)
        {
            if (toolName == "initialize_combat")
            {
                var enemies = new List<Entity> { MonsterFactory.GetFactories()[EnemyEnum.Skeleton] };
                _eminentCombat = new Combat(this, enemies, fleeable: true);
                WaitCombatConfirm(_eminentCombat);
            }
            else if (toolName == "reward_player")
            {
                int gold = data.ContainsKey("gold") ? (int)data["gold"].AsInt64() : 10;
                int xp = data.ContainsKey("xp") ? (int)data["xp"].AsInt64() : 50;
                if (GameManager.Instance.CurrentPlayer != null)
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
            _combatButton.Visible = false; // Trigger transition directly or via button
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