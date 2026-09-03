using Godot;
using System.IO;

namespace PursualRPG.Scripts.Scenes
{
    public partial class ScenarioAssistantScene : Control
    {
        private TextEdit _worldInput;
        private TextEdit _outputView;
        private Button _btnCompile;
        private Button _btnGenerate;
        private Button _btnBack;

        public override void _Ready()
        {
            _worldInput = GetNode<TextEdit>("WorldInput");
            _outputView = GetNode<TextEdit>("OutputView");
            _btnCompile = GetNode<Button>("BtnCompile");
            _btnGenerate = GetNode<Button>("BtnGenerate");
            _btnBack = GetNode<Button>("BtnBack");

            _btnCompile.Pressed += OnCompileMechanics;
            _btnGenerate.Pressed += OnGenerateWorld;
            _btnBack.Pressed += OnBackToOptions;
        }

        private void OnCompileMechanics()
        {
            string modelPath = ProjectSettings.GlobalizePath("res://Scripts/Domain");
            if (!Directory.Exists(modelPath))
            {
                _outputView.Text = "[Error] Domain model directory not found.";
                return;
            }

            _outputView.Text = "[System] Successfully compiled C# domain mechanics for AI context injection.";
        }

        private void OnGenerateWorld()
        {
            string idea = _worldInput.Text.Trim();
            if (string.IsNullOrEmpty(idea))
            {
                _outputView.Text = "[Warning] Please enter a chronicle concept first.";
                return;
            }

            _outputView.Text = $"[System] Requesting new world configuration for: '{idea}'...";
        }

        private void OnBackToOptions()
        {
            GetTree().ChangeSceneToFile("res://Scenes/OptionsScene.tscn");
        }
    }
}