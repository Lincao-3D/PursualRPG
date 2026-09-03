// Scripts/Scenes/OptionsScene.cs
using Godot;
using PursualRPG.Scripts.Core; // Fixes namespace issue

namespace PursualRPG.Scripts.Scenes
{
    public partial class OptionsScene : Control
    {
        private HSlider _volumeSlider;
        private CheckBox _muteToggle;
        private Button _backButton;

        public override void _Ready()
        {
            _volumeSlider = GetNode<HSlider>("VBoxContainer/VolumeSlider");
            _muteToggle = GetNode<CheckBox>("VBoxContainer/MuteToggle");
            _backButton = GetNode<Button>("VBoxContainer/BackButton");

            // Localize static UI elements
            _muteToggle.Text = Tr("BTN_MUTE");
            _backButton.Text = Tr("BTN_BACK");

            _volumeSlider.ValueChanged += OnVolumeChanged;
            _muteToggle.Toggled += OnMutedToggled;
            _backButton.Pressed += OnBackPressed;
            
            _backButton.MouseEntered += () => SynthAudioServer.Instance.PlayButtonHover();
        }

        private void OnVolumeChanged(double value)
        {
            SynthAudioServer.Instance.SetMasterVolume((float)value);
        }

        private void OnMutedToggled(bool toggledOn)
        {
            SynthAudioServer.Instance.SetMuted(toggledOn);
        }

        private void OnBackPressed()
        {
            SynthAudioServer.Instance.PlayButtonClick();
            GameManager.Instance.ChangeScene("res://Scenes/MainMenuScene.tscn");
        }
    }
}