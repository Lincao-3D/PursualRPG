using Godot;
using PursualRPG.Scripts.Core;

namespace PursualRPG.Scripts.Scenes
{
    public partial class OptionsScene : Control
    {
        private HSlider _volumeSlider;
        private CheckBox _muteToggle;
        private CheckBox _physicalDiceToggle;
        private Button _worldBuilderButton;
        private Button _backButton;

        public override void _Ready()
        {
            _volumeSlider = GetNode<HSlider>("VBoxContainer/VolumeSlider");
            _muteToggle = GetNode<CheckBox>("VBoxContainer/MuteToggle");
            _physicalDiceToggle = GetNode<CheckBox>("VBoxContainer/PhysicalDiceToggle");
            _worldBuilderButton = GetNode<Button>("VBoxContainer/WorldBuilderButton");
            _backButton = GetNode<Button>("VBoxContainer/BackButton");

            _muteToggle.Text = Tr("BTN_MUTE");
            _physicalDiceToggle.Text = Tr("BTN_USE_PHYSICAL_DICE");
            _worldBuilderButton.Text = Tr("BTN_WORLD_BUILDER");
            _backButton.Text = Tr("BTN_BACK");

            FontService.ApplyFont(_muteToggle, FontType.DefaultMenu);
            FontService.ApplyFont(_physicalDiceToggle, FontType.DefaultMenu);
            _worldBuilderButton.BindAudioAndFont(FontType.SecondaryButton);
            _backButton.BindAudioAndFont(FontType.SecondaryButton);

            _physicalDiceToggle.ButtonPressed = GameManager.Instance.UsePhysicalDice;

            _volumeSlider.ValueChanged += (v) => SynthAudioServer.Instance.SetMasterVolume((float)v);
            _muteToggle.Toggled += (t) => SynthAudioServer.Instance.SetMuted(t);
            _physicalDiceToggle.Toggled += (e) => GameManager.Instance.UsePhysicalDice = e;

            _worldBuilderButton.Pressed += () => GameManager.Instance.ChangeScene("res://Scenes/ScenarioAssistantScene.tscn");
            _backButton.Pressed += () => GameManager.Instance.ChangeScene("res://Scenes/MainMenuScene.tscn");
        }
    }
}