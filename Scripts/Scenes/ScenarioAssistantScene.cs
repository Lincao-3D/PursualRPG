using Godot;
using System.IO;
using PursualRPG.Scripts.Core;

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

            _btnCompile.Text = Tr("BTN_COMPILE");
            _btnGenerate.Text = Tr("BTN_GENERATE");
            _btnBack.Text = Tr("BTN_BACK");

            FontService.ApplyFont(_worldInput, FontType.ChatReading);
            FontService.ApplyFont(_outputView, FontType.ChatReading);
            _btnCompile.BindAudioAndFont(FontType.SecondaryButton);
            _btnGenerate.BindAudioAndFont(FontType.SecondaryButton);
            _btnBack.BindAudioAndFont(FontType.SecondaryButton);

            _btnCompile.Pressed += OnCompileMechanics;
            _btnGenerate.Pressed += OnGenerateWorld;
            _btnBack.Pressed += () => GameManager.Instance.ChangeScene("res://Scenes/OptionsScene.tscn");
        }

        private void OnCompileMechanics()
        {
            string modelPath = ProjectSettings.GlobalizePath("res://Scripts/Domain");
            _outputView.Text = Directory.Exists(modelPath) ? Tr("MSG_DOMAIN_COMPILED") : Tr("ERR_DOMAIN_DIRECTORY_NOT_FOUND");
        }

        private void OnGenerateWorld()
        {
            string idea = _worldInput.Text.Trim();
            _outputView.Text = string.IsNullOrEmpty(idea) ? Tr("WARN_ENTER_SCENARIO_CONCEPT") : string.Format(Tr("MSG_REQUESTING_WORLD_CONFIGURATION"), idea);
        }
    }
}