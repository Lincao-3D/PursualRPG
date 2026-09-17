using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using PursualRPG.Scripts.Domain;
using PursualRPG.Scripts.AI;
using PursualRPG.Scripts.UI;
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

        private CharacterSheetPanel _characterSheetPanel;
        private Button _characterSheetButton;
        private Button _saveButton;
        private Button _optionsButton;
        // tracking flags for LLM response streaming and display
        private bool _isDisplayingText = false;
        private bool _speedUpRequested = false;
        private TypewriterLabel _streamPreview;

        public override void _Ready()
        {
            if (MessageBroker == null) MessageBroker = GetNodeOrNull<AIMessageBroker>("MessageBroker");

            _historyText = GetNodeOrNull<RichTextLabel>("MarginContainer/VBoxContainer/ScrollContainer/HistoryText") ?? GetNode<RichTextLabel>("VBoxContainer/ScrollContainer/HistoryText");
            _userInput = GetNodeOrNull<LineEdit>("MarginContainer/VBoxContainer/HBoxContainer/UserInput") ?? GetNode<LineEdit>("VBoxContainer/HBoxContainer/UserInput");
            _submitButton = GetNodeOrNull<Button>("MarginContainer/VBoxContainer/HBoxContainer/SubmitButton") ?? GetNode<Button>("VBoxContainer/HBoxContainer/SubmitButton");
            _combatButton = GetNode<Button>("CombatButton");
            _characterSheetPanel = GetNode<CharacterSheetPanel>("CharacterSheetPanel");
            _characterSheetButton = GetNode<Button>("CharacterSheetButton");
            _saveButton = GetNodeOrNull<Button>("SaveButton");
            _optionsButton = GetNodeOrNull<Button>("OptionsButton");
            _streamPreview = GetNode<TypewriterLabel>("StreamPreview");

            _submitButton.Text = "Submit";
            _combatButton.Text = "ENTER COMBAT";
            if (_characterSheetButton != null) _characterSheetButton.Text = "Sheet";
            if (_saveButton != null) _saveButton.Text = "Save";
            if (_optionsButton != null) _optionsButton.Text = "Options";

            if (_historyText != null) FontService.ApplyFont(_historyText, FontType.ChatReading);
            if (_userInput != null) FontService.ApplyFont(_userInput, FontType.ChatReading);
            if (_streamPreview != null) FontService.ApplyFont(_streamPreview, FontType.ChatReading);

            _submitButton?.BindAudioAndFont(FontType.SecondaryButton);
            _combatButton?.BindAudioAndFont(FontType.SecondaryButton);
            _characterSheetButton?.BindAudioAndFont(FontType.SecondaryButton);
            _saveButton?.BindAudioAndFont(FontType.SecondaryButton);
            _optionsButton?.BindAudioAndFont(FontType.SecondaryButton);

            if (_submitButton != null) _submitButton.Pressed += OnSubmitPressed;
            if (_userInput != null) _userInput.TextSubmitted += OnTextSubmitted;
            if (_combatButton != null) _combatButton.Pressed += OnCombatButtonPressed;
            
            // Wire Sheet, Save, and Options buttons
            if (_characterSheetButton != null) _characterSheetButton.Pressed += ToggleCharacterSheet;
            if (_saveButton != null) _saveButton.Pressed += OnSaveButtonPressed;
            if (_optionsButton != null) _optionsButton.Pressed += () => GameManager.Instance.ChangeScene("res://Scenes/OptionsScene.tscn");

            if (MessageBroker != null) MessageBroker.OnTokenStreamed += OnTokenStreamed;

            if (_combatButton != null)
            {
                _combatButton.Visible = false;
                _combatButton.Disabled = true;
            }
            if (_streamPreview != null) _streamPreview.Visible = false;

            _scenario = Scenario.DefaultScenario;
            if (MessageBroker != null) MessageBroker.ActiveScenario = _scenario;

            // Initialize sheet once on ready without resetting toggle state afterwards
            if (_characterSheetPanel != null && GameManager.Instance.CurrentPlayer != null)
            {
                _characterSheetPanel.Initialize(GameManager.Instance.CurrentPlayer);
            }
            InitializeChatHistory();
        }

        private void InitializeChatHistory()
        {
            if (GameManager.Instance.CurrentPlayer != null && !string.IsNullOrEmpty(GameManager.Instance.CurrentPlayer.SavedChatHistory))
            {
                // Restore saved chat transcript if returning to an active game
                if (_historyText != null) _historyText.Text = GameManager.Instance.CurrentPlayer.SavedChatHistory;
            }
            else
            {
                if (_historyText != null) _historyText.Text = $"DM:\n{_scenario.InitialMessage}\n";
            }
        }
        public override void _Input(InputEvent @event)
        {
            if (_isDisplayingText && @event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                if (keyEvent.Keycode == Key.Space || keyEvent.Keycode == Key.Enter || keyEvent.Keycode == Key.KpEnter)
                {
                    _speedUpRequested = true;
                }
            }
        }
        private async Task ShowNotificationBarAsync(string text, float durationMs = 2000f)
        {
            var bar = new HorizontalUIBar();
            AddChild(bar);
            bar.Initialize(text, durationMs);
            while (!bar.IsDone)
            {
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            }
        }

        public async Task DisplayChatCardAsync(ChatMessage card)
        {
            if (_historyText == null) return;
            string speakerName = string.IsNullOrEmpty(card.SpeakerKey) ? "DM" : Tr(card.SpeakerKey);
            _historyText.AppendText($"\n[color=yellow]{speakerName}:[/color] ");

            _isDisplayingText = true;
            _speedUpRequested = false;

            // Stream text directly to main chat with dynamic speed up on Spacebar
            foreach (char c in card.Text)
            {
                _historyText.AppendText(c.ToString());
                float activeSpeed = _speedUpRequested ? 0.001f : TextSpeed;
                await ToSignal(GetTree().CreateTimer(activeSpeed), SceneTreeTimer.SignalName.Timeout);
            }

            _isDisplayingText = false;
            _speedUpRequested = false;

            if (_userInput != null) _userInput.Visible = !card.IsNarrative;
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

        private void OnSubmitPressed() { if (_userInput != null) ProcessInput(_userInput.Text); }
        private void OnTextSubmitted(string text) => ProcessInput(text);

        private void ProcessInput(string text)
        {
            text = text.Trim();
            if (string.IsNullOrEmpty(text)) return;

            if (_savingMode)
            {
                GameManager.Instance.SaveGame();
                AppendLog($"\n[color=gray][System: Game saved successfully!][/color]\n");
                _savingMode = false;
                _userInput?.Clear();
                return;
            }

            if (text.StartsWith("/"))
            {
                HandleCommand(text);
                _userInput?.Clear();
                return;
            }

            _userInput?.Clear();
            AppendLog($"\n[color=green]Player:[/color]\n{text}\n[color=yellow]DM:[/color]\n");
            SendToLLM(text);
        }

        private void HandleCommand(string commandText)
        {
            var parts = commandText[1..].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return;

            string command = parts[0].ToLowerInvariant();
            if (command == "quit" || command == "exit")
            {
                GameManager.Instance.ChangeScene("res://Scenes/MainMenuScene.tscn");
            }
            else if (command == "player" || command == "sheet")
            {
                ToggleCharacterSheet();
            }
            else if (command == "save")
            {
                OnSaveButtonPressed();
            }
        }

        private void OnSaveButtonPressed()
        {
            if (GameManager.Instance.CurrentPlayer != null && _historyText != null)
            {
                // Capture current chat history into player data before saving JSON
                GameManager.Instance.CurrentPlayer.SavedChatHistory = _historyText.Text;
            }
            
            GameManager.Instance.SaveGame();
            AppendLog($"\n[color=gray][System: Game progress and chat history saved!][/color]\n");
            _ = ShowNotificationBarAsync("Game Saved!", 2000f);
        }

        private void OnTokenStreamed(string token)
        {
            if (_streamPreview == null) return;
            _streamPreview.Visible = true;
            _streamPreview.Text += token;
        }

        private void ToggleCharacterSheet()
        {
            if (GameManager.Instance.CurrentPlayer == null || _characterSheetPanel == null) return;
            // Only toggle the sliding position without calling Initialize() again, preventing reset bugs
            _characterSheetPanel.Toggle();
        }

        private void SendToLLM(string prompt)
        {
            if (MessageBroker == null) return;
           // Clear out/hide any old preview boxes since we stream directly to chat now
            if (_streamPreview != null)
            {
                _streamPreview.Visible = false;
                _streamPreview.Text = string.Empty;
            }

            // Capture everything currently displayed in the chat history log as context
            string currentHistoryContext = _historyText != null ? _historyText.Text : string.Empty;

            MessageBroker.SendMessageAsync(prompt, currentHistoryContext, response => {
                CallDeferred(nameof(FinishStreamingResponse), response);
            });
        }

        private void FinishStreamingResponse(string responseText)
        {
            if (_streamPreview != null)
            {
                _streamPreview.Visible = false;
                _streamPreview.Text = string.Empty;
            }
            ReceiveLLMResponse(responseText);
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
                _eminentCombat = new Combat(GameManager.Instance.CurrentPlayer, enemies, fleeable: true);
                WaitCombatConfirm(_eminentCombat);
            }
            else if (command.Name == "reward_player")
            {
                int gold = 10;
                int xp = 50;

                if (command.Arguments.TryGetValue("gold", out var goldElem) && goldElem.ValueKind == JsonValueKind.Number) gold = goldElem.GetInt32();
                if (command.Arguments.TryGetValue("xp", out var xpElem) && xpElem.ValueKind == JsonValueKind.Number) xp = xpElem.GetInt32();

                if (GameManager.Instance?.CurrentPlayer != null)
                {
                    GameManager.Instance.CurrentPlayer.Gold += gold;
                    GameManager.Instance.CurrentPlayer.Xp += xp;
                }
                AppendLog($"\n[color=gold][System: Rewarded +{gold} Gold, +{xp} XP][/color]\n");
                _ = ShowNotificationBarAsync($"Rewarded: +{gold} Gold, +{xp} XP", 2000f);
            }
            else if (command.Name == "give_item" || command.Name == "give_items")
            {
                int itemId = 1;
                int qty = 1;

                if (command.Arguments.TryGetValue("item_id", out var idElem) && idElem.ValueKind == JsonValueKind.Number) itemId = idElem.GetInt32();
                if (command.Arguments.TryGetValue("quantity", out var qtyElem) && qtyElem.ValueKind == JsonValueKind.Number) qty = qtyElem.GetInt32();

                if (GameManager.Instance?.CurrentPlayer != null)
                {
                    GameManager.Instance.CurrentPlayer.GiveItem(itemId, qty);
                    var itemInfo = ItemFactoryRegistry.GetItem(itemId);
                    AppendLog($"\n[color=cyan][System: Received {itemInfo.Name} x{qty}][/color]\n");
                    _ = ShowNotificationBarAsync($"Received: {itemInfo.Name} x{qty}", 2000f);
                }
            }
        }

        public void WaitCombatConfirm(Combat combat)
        {
            if (_combatButton != null)
            {
                _combatButton.Visible = true;
                _combatButton.Disabled = false;
            }
            AppendLog("\n[color=red][System: An encounter has begun! Click 'ENTER COMBAT' to engage!][/color]\n");
            _ = ShowNotificationBarAsync("An encounter has begun!", 2500f);
        }

        private void OnCombatButtonPressed()
        {
            if (_eminentCombat == null) return;
            GameManager.Instance.ChangeScene("res://Scenes/CombatScene.tscn");
        }

        private void AppendLog(string message)
        {
            _historyText?.AppendText(message);
        }
    }
}