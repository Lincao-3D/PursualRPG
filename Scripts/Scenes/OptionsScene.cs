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
        private Button _returnButton;
        private Button _backButton;

        public override void _Ready()
        {
            _volumeSlider = GetNodeOrNull<HSlider>("ScrollContainer/MarginContainer/VBoxContainer/VolumeSlider") ?? GetNodeOrNull<HSlider>("ScrollContainer/VBoxContainer/VolumeSlider");
            _muteToggle = GetNodeOrNull<CheckBox>("ScrollContainer/MarginContainer/VBoxContainer/MuteToggle") ?? GetNodeOrNull<CheckBox>("ScrollContainer/VBoxContainer/MuteToggle");
            _physicalDiceToggle = GetNodeOrNull<CheckBox>("ScrollContainer/MarginContainer/VBoxContainer/PhysicalDiceToggle") ?? GetNodeOrNull<CheckBox>("ScrollContainer/VBoxContainer/PhysicalDiceToggle");
            _worldBuilderButton = GetNodeOrNull<Button>("ScrollContainer/MarginContainer/VBoxContainer/WorldBuilderButton") ?? GetNodeOrNull<Button>("ScrollContainer/VBoxContainer/WorldBuilderButton");
            _returnButton = GetNodeOrNull<Button>("ScrollContainer/MarginContainer/VBoxContainer/ReturnButton");
            _backButton = GetNodeOrNull<Button>("ScrollContainer/MarginContainer/VBoxContainer/BackButton") ?? GetNodeOrNull<Button>("ScrollContainer/VBoxContainer/BackButton");

            if (_muteToggle != null)
            {
                string txt = Tr("BTN_MUTE");
                _muteToggle.Text = (txt != "BTN_MUTE" && !string.IsNullOrEmpty(txt)) ? txt : "Mute Audio";
                FontService.ApplyFont(_muteToggle, FontType.DefaultMenu);
                _muteToggle.Toggled += (t) => SynthAudioServer.Instance.SetMuted(t);
            }

            if (_physicalDiceToggle != null)
            {
                string txt = Tr("BTN_USE_PHYSICAL_DICE");
                _physicalDiceToggle.Text = (txt != "BTN_USE_PHYSICAL_DICE" && !string.IsNullOrEmpty(txt)) ? txt : "Use Physical Dice";
                FontService.ApplyFont(_physicalDiceToggle, FontType.DefaultMenu);
                _physicalDiceToggle.ButtonPressed = GameManager.Instance.UsePhysicalDice;
                _physicalDiceToggle.Toggled += (e) => GameManager.Instance.UsePhysicalDice = e;
            }

            if (_volumeSlider != null)
            {
                _volumeSlider.SizeFlagsHorizontal = Control.SizeFlags.ShrinkBegin;
                _volumeSlider.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;
                _volumeSlider.CustomMinimumSize = new Vector2(300, 20);
                _volumeSlider.Value = SynthAudioServer.Instance.MasterVolume;
                _volumeSlider.ValueChanged += (val) => SynthAudioServer.Instance.SetMasterVolume((float)val);
            }

            if (_worldBuilderButton != null)
            {
                string txt = Tr("BTN_WORLD_BUILDER");
                _worldBuilderButton.Text = (txt != "BTN_WORLD_BUILDER" && !string.IsNullOrEmpty(txt)) ? txt : "World Builder Assistant";
                _worldBuilderButton.BindAudioAndFont(FontType.SecondaryButton);
                _worldBuilderButton.Pressed += () => GameManager.Instance.ChangeScene("res://Scenes/ScenarioAssistantScene.tscn");
            }

            // Back button checks if a scene is already loaded in the tree, if so it will return to that currently playing scene.
            bool hasActiveGame = GameManager.Instance.CurrentPlayer != null;
            if (_returnButton != null)
            {
                _returnButton.Visible = hasActiveGame;
                if (hasActiveGame)
                {
                    _returnButton.Text = "Return to Game";
                    _returnButton.BindAudioAndFont(FontType.SecondaryButton);
                    _returnButton.Pressed += () => GameManager.Instance.ChangeScene("res://Scenes/ChatScene.tscn");
                }
            }

            if (_backButton != null)
            {
                string txt = Tr("BTN_BACK");
                _backButton.Text = (txt != "BTN_BACK" && !string.IsNullOrEmpty(txt)) ? txt : "Back to Main Menu";
                _backButton.BindAudioAndFont(FontType.SecondaryButton);
                _backButton.Pressed += () => GameManager.Instance.ChangeScene("res://Scenes/MainMenuScene.tscn");
            }
        }
    }
}