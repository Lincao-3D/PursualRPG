using Godot;

namespace PursualRPG.Scripts.Scenes
{
    public partial class MainMenuScene : Control
    {
        private Button _newGameButton;
        private Button _continueButton;
        private Button _optionsButton;

        public override void _Ready()
        {
            _newGameButton = GetNode<Button>("VBoxContainer/NewGameButton");
            _continueButton = GetNode<Button>("VBoxContainer/ContinueButton");
            _optionsButton = GetNode<Button>("VBoxContainer/OptionsButton");

            _newGameButton.Pressed += OnNewGamePressed;
            _continueButton.Pressed += OnContinuePressed;
            _optionsButton.Pressed += OnOptionsPressed;

            CheckForSaves();
        }

        private void CheckForSaves()
        {
            bool hasSave = FileAccess.FileExists("user://savegame.json");
            _continueButton.Visible = hasSave;
        }

        private void OnNewGamePressed() => GetTree().ChangeSceneToFile("res://Scenes/CharacterCreatorScene.tscn");
        private void OnContinuePressed() => GetTree().ChangeSceneToFile("res://Scenes/ChatScene.tscn");
        private void OnOptionsPressed() => GetTree().ChangeSceneToFile("res://Scenes/OptionsScene.tscn");
    }
}