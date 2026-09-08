using Godot;
using PursualRPG.Scripts.Core;

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

			_newGameButton.Text = Tr("BTN_PLAY");
			_continueButton.Text = Tr("BTN_CONTINUE");
			_optionsButton.Text = Tr("BTN_OPTIONS");

			_newGameButton.BindAudioAndFont(FontType.DefaultMenu, 24);
			_continueButton.BindAudioAndFont(FontType.DefaultMenu, 24);
			_optionsButton.BindAudioAndFont(FontType.DefaultMenu, 24);

			_newGameButton.Pressed += () => GameManager.Instance.ChangeScene("res://Scenes/CharacterCreatorScene.tscn");
			_continueButton.Pressed += () => GameManager.Instance.ChangeScene("res://Scenes/ChatScene.tscn");
			_optionsButton.Pressed += () => GameManager.Instance.ChangeScene("res://Scenes/OptionsScene.tscn");

			_continueButton.Visible = GameManager.Instance.SaveExists();

            // Trigger dungeon synth music theme with vocal stab
            SynthAudioServer.Instance?.PlayDungeonSynthTheme();
		}
	}
}