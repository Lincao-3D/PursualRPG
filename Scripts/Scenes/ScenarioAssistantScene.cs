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
        [Export] public TextEdit WorldInput;
        [Export] public RichTextLabel OutputLabel;
        [Export] public Button BtnUnderstand;
        [Export] public Button BtnCompile;
        [Export] public Button BtnGenerate;
        [Export] public Button CopyButton;
        [Export] public Button BackButton;

        private LLMClient _llmClient;

        public override void _Ready()
        {
            // Node Fallback Resolution
            WorldInput ??= GetNode<TextEdit>("WorldInput");
            OutputLabel ??= GetNode<RichTextLabel>("OutputLabel");
            BtnUnderstand ??= GetNode<Button>("ActionFooter/BtnUnderstand");
            BtnCompile ??= GetNode<Button>("ActionFooter/BtnCompile");
            BtnGenerate ??= GetNode<Button>("ActionFooter/BtnGenerate");
            CopyButton ??= GetNodeOrNull<Button>("ActionFooter/BtnCopyClipboard");
            BackButton ??= GetNode<Button>("ActionFooter/BtnBack");

            _llmClient = GetNodeOrNull<LLMClient>("LLMClient") ?? new LLMClient();
            if (_llmClient.GetParent() == null) AddChild(_llmClient);

            // Labels and Typography
            if (BtnUnderstand != null) BtnUnderstand.Text = "Understand";
            if (BtnCompile != null) BtnCompile.Text = Tr("BTN_COMPILE");
            if (BtnGenerate != null) BtnGenerate.Text = Tr("BTN_GENERATE");
            if (BackButton != null) BackButton.Text = Tr("BTN_BACK");
            if (CopyButton != null) CopyButton.Text = "Copy Output";

            if (WorldInput != null) FontService.ApplyFont(WorldInput, FontType.ChatReading, 14);
            if (OutputLabel != null) FontService.ApplyFont(OutputLabel, FontType.ChatReading, 14);

            BtnUnderstand?.BindAudioAndFont(FontType.SecondaryButton, 13);
            BtnCompile?.BindAudioAndFont(FontType.SecondaryButton, 13);
            BtnGenerate?.BindAudioAndFont(FontType.SecondaryButton, 13);
            BackButton?.BindAudioAndFont(FontType.SecondaryButton, 13);
            CopyButton?.BindAudioAndFont(FontType.SecondaryButton, 13);

            // Button Wiring
            if (BtnUnderstand != null) BtnUnderstand.Pressed += OnUnderstandScenario;
            if (BtnCompile != null) BtnCompile.Pressed += OnCompileMechanics;
            if (BtnGenerate != null) BtnGenerate.Pressed += () => _ = OnGenerateWorldAsync();
            if (CopyButton != null) CopyButton.Pressed += OnCopyPressed;
            if (BackButton != null) BackButton.Pressed += OnBackPressed;
        }

        private void OnBackPressed()
        {
            GameManager.Instance.ChangeScene("res://Scenes/OptionsScene.tscn");
        }

        private async void OnCopyPressed()
        {
            if (OutputLabel == null || CopyButton == null) return;

            // Copy BBCode/Raw Text to System Clipboard
            DisplayServer.ClipboardSet(OutputLabel.Text);

            string originalText = CopyButton.Text;
            CopyButton.Text = "Copied to Clipboard!";
            CopyButton.Disabled = true;

            await ToSignal(GetTree().CreateTimer(2.0f), SceneTreeTimer.SignalName.Timeout);

            if (GodotObject.IsInstanceValid(CopyButton))
            {
                CopyButton.Text = originalText;
                CopyButton.Disabled = false;
            }
        }

        public void DisplayGeneratedWorld(string jsonResult)
        {
            if (OutputLabel == null) return;

            OutputLabel.Text = "[b]World Generation Steps:[/b]\n" +
                               "1. [color=cyan]Core Concept generated.[/color]\n" +
                               "2. [color=cyan]Factions aligned.[/color]\n" +
                               "3. [color=cyan]JSON Payload Ready:[/color]\n\n" + 
                               jsonResult;
        }

        private void OnUnderstandScenario()
        {
            var active = Scenario.DefaultScenario;
            if (OutputLabel == null) return;

            OutputLabel.Text = "[b]Scenario Guidelines:[/b]\n" +
                               "1. [color=cyan][SYSTEM PROMPT][/color]\n" + active.SystemPrompt + "\n\n" +
                               "2. [color=cyan][INITIAL MESSAGE][/color]\n" + active.InitialMessage;
        }

        private void OnCompileMechanics()
        {
            string domainPath = ProjectSettings.GlobalizePath("res://Scripts/Domain");
            if (!Directory.Exists(domainPath))
            {
                if (OutputLabel != null) OutputLabel.Text = Tr("ERR_DOMAIN_DIRECTORY_NOT_FOUND");
                return;
            }

            var files = Directory.GetFiles(domainPath, "*.cs");
            var summary = new System.Text.StringBuilder();
            summary.AppendLine("[b]=== DOMAIN MECHANICS SUMMARY ===[/b]");
            summary.AppendLine($"Scanned {files.Length} domain files in /Scripts/Domain:\n");

            int index = 1;
            foreach (var file in files)
            {
                string filename = Path.GetFileName(file);
                string content = File.ReadAllText(file);

                var enums = Regex.Matches(content, @"enum\s+([A-Za-z0-9_]+)").Select(m => m.Groups[1].Value);
                var classes = Regex.Matches(content, @"class\s+([A-Za-z0-9_]+)").Select(m => m.Groups[1].Value);

                summary.AppendLine($"{index}. [color=cyan]{filename}[/color]");
                if (classes.Any()) summary.AppendLine($"   - Classes: {string.Join(", ", classes)}");
                if (enums.Any()) summary.AppendLine($"   - Enums:   {string.Join(", ", enums)}");
                index++;
            }

            if (OutputLabel != null) OutputLabel.Text = summary.ToString();
        }

        private async Task OnGenerateWorldAsync()
        {
            if (WorldInput == null || OutputLabel == null || BtnGenerate == null) return;

            string concept = WorldInput.Text.Trim();
            if (string.IsNullOrEmpty(concept))
            {
                OutputLabel.Text = Tr("WARN_ENTER_SCENARIO_CONCEPT");
                return;
            }

            BtnGenerate.Disabled = true;
            BtnGenerate.Text = "Generating...";
            OutputLabel.Text = "[color=yellow][Requesting chronicle generation from LLM...][/color]";

            try
            {
                string metaSystemPrompt =
                    "You are an expert tabletop RPG scenario designer. " +
                    "Given a user's chronicle concept, output a valid JSON object with exactly two string fields:\n" +
                    "1. \"systemPrompt\": Detailed rules and personality instructions for the DM AI.\n" +
                    "2. \"initialMessage\": The opening narrative scene string presented to the player.";

                string rawResponse = await _llmClient.GenerateContentAsync(metaSystemPrompt, concept);
                DisplayGeneratedWorld(rawResponse);
            }
            catch (Exception ex)
            {
                OutputLabel.Text = $"[color=red][Error generating world: {ex.Message}][/color]";
                GD.PrintErr($"[ScenarioAssistant] LLM Generation Exception: {ex}");
            }
            finally
            {
                BtnGenerate.Disabled = false;
                BtnGenerate.Text = Tr("BTN_GENERATE");
            }
        }
    }
}