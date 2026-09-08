using Godot;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PursualRPG.Scripts.AI;
using PursualRPG.Scripts.Core;
using PursualRPG.Scripts.Domain;

namespace PursualRPG.Scripts.Scenes
{
    public partial class ScenarioAssistantScene : Control
    {
        private TextEdit _worldInput;
        private TextEdit _outputView;
        private Button _btnUnderstand;
        private Button _btnCompile;
        private Button _btnGenerate;
        private Button _btnBack;

        // Deferred Phase 6.4 Buttons
        private Button _btnAgenticPrompt;
        private Button _btnCopyClipboard;
        private Button _btnSaveFile;

        private LLMClient _llmClient;

        public override void _Ready()
        {
            _worldInput = GetNode<TextEdit>("WorldInput");
            _outputView = GetNode<TextEdit>("OutputView");
            _btnUnderstand = GetNode<Button>("BtnUnderstand");
            _btnCompile = GetNode<Button>("BtnCompile");
            _btnGenerate = GetNode<Button>("BtnGenerate");
            _btnBack = GetNode<Button>("BtnBack");

            _llmClient = GetNodeOrNull<LLMClient>("LLMClient") ?? new LLMClient();
            if (_llmClient.GetParent() == null) AddChild(_llmClient);

            // Bind Deferred Phase 6.4 Buttons if present in tree
            _btnAgenticPrompt = GetNodeOrNull<Button>("BtnAgenticPrompt");
            _btnCopyClipboard = GetNodeOrNull<Button>("BtnCopyClipboard");
            _btnSaveFile = GetNodeOrNull<Button>("BtnSaveFile");

            _btnUnderstand.Text = "Understand";
            _btnCompile.Text = Tr("BTN_COMPILE");
            _btnGenerate.Text = Tr("BTN_GENERATE");
            _btnBack.Text = Tr("BTN_BACK");

            FontService.ApplyFont(_worldInput, FontType.ChatReading);
            FontService.ApplyFont(_outputView, FontType.ChatReading);

            _btnUnderstand.BindAudioAndFont(FontType.SecondaryButton);
            _btnCompile.BindAudioAndFont(FontType.SecondaryButton);
            _btnGenerate.BindAudioAndFont(FontType.SecondaryButton);
            _btnBack.BindAudioAndFont(FontType.SecondaryButton);

            if (_btnAgenticPrompt != null) _btnAgenticPrompt.BindAudioAndFont(FontType.SecondaryButton);
            if (_btnCopyClipboard != null) _btnCopyClipboard.BindAudioAndFont(FontType.SecondaryButton);
            if (_btnSaveFile != null) _btnSaveFile.BindAudioAndFont(FontType.SecondaryButton);

            _btnUnderstand.Pressed += OnUnderstandScenario;
            _btnCompile.Pressed += OnCompileMechanics;
            _btnGenerate.Pressed += () => _ = OnGenerateWorldAsync();
            _btnBack.Pressed += () => GameManager.Instance.ChangeScene("res://Scenes/OptionsScene.tscn");
        }

        private void OnUnderstandScenario()
        {
            var active = Scenario.DefaultScenario;
            _outputView.Text = $"=== CURRENT SCENARIO CONFIGURATION ===\n\n" +
                               $"[SYSTEM PROMPT]\n{active.SystemPrompt}\n\n" +
                               $"[INITIAL MESSAGE]\n{active.InitialMessage}";
        }

        private void OnCompileMechanics()
        {
            string domainPath = ProjectSettings.GlobalizePath("res://Scripts/Domain");
            if (!Directory.Exists(domainPath))
            {
                _outputView.Text = Tr("ERR_DOMAIN_DIRECTORY_NOT_FOUND");
                return;
            }

            var files = Directory.GetFiles(domainPath, "*.cs");
            var summary = new System.Text.StringBuilder();
            summary.AppendLine("=== DOMAIN MECHANICS SUMMARY ===");
            summary.AppendLine($"Scanned {files.Length} domain files in /Scripts/Domain:\n");

            foreach (var file in files)
            {
                string filename = Path.GetFileName(file);
                string content = File.ReadAllText(file);

                var enums = Regex.Matches(content, @"enum\s+([A-Za-z0-9_]+)").Select(m => m.Groups[1].Value);
                var classes = Regex.Matches(content, @"class\s+([A-Za-z0-9_]+)").Select(m => m.Groups[1].Value);

                summary.AppendLine($"• {filename}");
                if (classes.Any()) summary.AppendLine($"   Classes: {string.Join(", ", classes)}");
                if (enums.Any()) summary.AppendLine($"   Enums:   {string.Join(", ", enums)}");
            }

            _outputView.Text = summary.ToString();
        }

        private async Task OnGenerateWorldAsync()
        {
            string concept = _worldInput.Text.Trim();
            if (string.IsNullOrEmpty(concept))
            {
                _outputView.Text = Tr("WARN_ENTER_SCENARIO_CONCEPT");
                return;
            }

            _btnGenerate.Disabled = true;
            _btnGenerate.Text = "Generating...";
            _outputView.Text = "[Requesting chronicle generation from LLM...]";

            string metaSystemPrompt =
                "You are an expert tabletop RPG scenario designer. " +
                "Given a user's chronicle concept, output a valid JSON object with exactly two string fields:\n" +
                "1. \"systemPrompt\": Detailed rules and personality instructions for the DM AI.\n" +
                "2. \"initialMessage\": The opening narrative scene string presented to the player.";

            string rawResponse = await _llmClient.GenerateContentAsync(metaSystemPrompt, concept);

            _outputView.Text = rawResponse;
            _btnGenerate.Disabled = false;
            _btnGenerate.Text = Tr("BTN_GENERATE");
        }
    }
}