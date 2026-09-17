using Godot;
using PursualRPG.Scripts.Core;

namespace PursualRPG.Scripts.Scenes
{
	public partial class MainMenuScene : Control
	{
		private Button _newGameButton;
		private Button _resumeButton;
		private Button _continueButton;
		private Button _optionsButton;

		public override void _Ready()
		{
			_newGameButton = GetNode<Button>("VBoxContainer/NewGameButton");
			_resumeButton = GetNodeOrNull<Button>("VBoxContainer/ResumeButton");
			_continueButton = GetNode<Button>("VBoxContainer/ContinueButton");
			_optionsButton = GetNode<Button>("VBoxContainer/OptionsButton");

			_newGameButton.Text = Tr("BTN_PLAY");
			if (_resumeButton != null) _resumeButton.Text = "Return to Active Game";
			_continueButton.Text = Tr("BTN_CONTINUE");
			_optionsButton.Text = Tr("BTN_OPTIONS");

			_newGameButton.BindAudioAndFont(FontType.DefaultMenu, 24);
			_continueButton.BindAudioAndFont(FontType.DefaultMenu, 24);
			_optionsButton.BindAudioAndFont(FontType.DefaultMenu, 24);

			_newGameButton.Pressed += () => GameManager.Instance.ChangeScene("res://Scenes/CharacterCreatorScene.tscn");
			
			if (_resumeButton != null)
			{
				bool hasActiveSession = GameManager.Instance.CurrentPlayer != null;
				_resumeButton.Visible = hasActiveSession;
				_resumeButton.Pressed += () => {
					if (GameManager.Instance.CurrentPlayer != null)
					{
						GameManager.Instance.ChangeScene("res://Scenes/ChatScene.tscn");
					}
				};
			}

			_continueButton.Pressed += () => {
				if (!GameManager.Instance.SaveExists()) return;
				GameManager.Instance.LoadGame();
				GameManager.Instance.ChangeScene("res://Scenes/ChatScene.tscn");
			};
			
			_optionsButton.Pressed += () => GameManager.Instance.ChangeScene("res://Scenes/OptionsScene.tscn");

			_continueButton.Visible = GameManager.Instance.SaveExists();

			// Diagnostic (a): Check if SynthAudioServer.Instance is non-null
			// GD.Print($"[Diagnostics] MainMenuScene._Ready -> SynthAudioServer.Instance is: {(SynthAudioServer.Instance != null ? "NON-NULL" : "NULL")}");
			
			SynthAudioServer.Instance?.PlayDungeonSynthTheme();
		}
	}
}
