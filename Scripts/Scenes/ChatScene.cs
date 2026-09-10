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
        private TypewriterLabel _streamPreview;

        public override void _Ready()
        {
            if (MessageBroker == null) MessageBroker = GetNodeOrNull<AIMessageBroker>("MessageBroker");

            _historyText = GetNode<RichTextLabel>("VBoxContainer/ScrollContainer/HistoryText");
            _userInput = GetNode<LineEdit>("VBoxContainer/HBoxContainer/UserInput");
            _submitButton = GetNode<Button>("VBoxContainer/HBoxContainer/SubmitButton");
            _combatButton = GetNode<Button>("CombatButton");
            _characterSheetPanel = GetNode<CharacterSheetPanel>("CharacterSheetPanel");
            _characterSheetButton = GetNode<Button>("CharacterSheetButton");
            _streamPreview = GetNode<TypewriterLabel>("StreamPreview");

            _submitButton.Text = Tr("BTN_SUBMIT");
            _combatButton.Text = "ENTER COMBAT";
            _characterSheetButton.Text = Tr("BTN_CHARACTER_SHEET");

            FontService.ApplyFont(_historyText, FontType.ChatReading);
            FontService.ApplyFont(_userInput, FontType.ChatReading);
            FontService.ApplyFont(_streamPreview, FontType.ChatReading);

            _submitButton.BindAudioAndFont(FontType.SecondaryButton);
            _combatButton.BindAudioAndFont(FontType.SecondaryButton);
            _characterSheetButton.BindAudioAndFont(FontType.SecondaryButton);

            _submitButton.Pressed += OnSubmitPressed;
            _userInput.TextSubmitted += OnTextSubmitted;
            _combatButton.Pressed += OnCombatButtonPressed;
            _characterSheetButton.Pressed += ToggleCharacterSheet;

            if (MessageBroker != null) MessageBroker.OnTokenStreamed += OnTokenStreamed;

            _combatButton.Visible = false;
            _combatButton.Disabled = true;
            _streamPreview.Visible = false;

            _scenario = Scenario.DefaultScenario;
            if (MessageBroker != null) MessageBroker.ActiveScenario = _scenario;

            _characterSheetPanel.Initialize(GameManager.Instance.CurrentPlayer);
            InitializeChatHistory();
        }

        private void InitializeChatHistory()
        {
            _historyText.Text = $"DM:\n{_scenario.InitialMessage}\n";
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

        private void OnSubmitPressed() => ProcessInput(_userInput.Text);
        private void OnTextSubmitted(string text) => ProcessInput(text);

        private void ProcessInput(string text)
        {
            text = text.Trim();
            if (string.IsNullOrEmpty(text)) return;

            if (_savingMode)
            {
                GameManager.Instance.SaveGame();
                AppendLog($"\n[color=gray][System: {string.Format(Tr("MSG_GAME_SAVED"), text)}][/color]\n");
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
            AppendLog($"\n[color=green]{Tr("TXT_PLAYER")}:[/color]\n{text}\n[color=yellow]{Tr("TXT_DM")}:[/color]\n");
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
                _savingMode = true;
                AppendLog($"\n{Tr("MSG_ENTER_SAVE_NAME")}\n");
            }
        }

        private void OnTokenStreamed(string token)
        {
            _streamPreview.Visible = true;
            _streamPreview.Text += token;
        }

        private void ToggleCharacterSheet()
        {
            if (GameManager.Instance.CurrentPlayer == null) return;
            _characterSheetPanel.Initialize(GameManager.Instance.CurrentPlayer);
            _characterSheetPanel.Toggle();
        }

        private void SendToLLM(string prompt)
        {
            if (MessageBroker == null) return;
            _streamPreview.Text = string.Empty;
            _streamPreview.Visible = true;

            MessageBroker.SendMessageAsync(prompt, response => {
                CallDeferred(nameof(FinishStreamingResponse), response);
            });
        }

        private void FinishStreamingResponse(string responseText)
        {
            _streamPreview.Visible = false;
            _streamPreview.Text = string.Empty;
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
            _combatButton.Visible = true;
            _combatButton.Disabled = false;
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
            _historyText.AppendText(message);
        }
    }
}