using Godot;
using PursualRPG.Scripts.Core;
using PursualRPG.Scripts.UI;

namespace PursualRPG.Scripts.Scenes
{
    public partial class SplashScene : Control
    {
        [Export] public float Duration { get; set; } = 8.0f;

        private double _elapsedTime;
        // private ASCIIBgPlayer _asciiPlayer;
        // private RichTextLabel _asciiDisplay;

        public override void _Ready()
        {
            // _asciiDisplay = GetNode<RichTextLabel>("ASCIIDisplay");
            // _asciiPlayer = GetNode<ASCIIBgPlayer>("ASCIIBgPlayer");
        }

        // public override void _Process(double delta)
        // {
        //     _elapsedTime += delta;

        //     if (_elapsedTime >= Duration || Input.IsAnythingPressed())
        //         TransitionToMainMenu();

        //     if (_asciiPlayer != null && _asciiDisplay != null)
        //         _asciiDisplay.Text = _asciiPlayer.GetCurrentFrameText();
        // }

        private void TransitionToMainMenu()
        {
            GameManager.Instance.ChangeScene(
                "res://Scenes/MainMenuScene.tscn");
        }
    }
}