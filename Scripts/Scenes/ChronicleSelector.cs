using Godot;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using PursualRPG.Scripts.Core;
using PursualRPG.Scripts.UI;
using PursualRPG.Scripts.AI;

namespace PursualRPG.Scripts.Scenes
{
    public partial class OptionsScene : Control
    {
        private VBoxContainer _optionsColumn;
        private CheckBox _physicalDiceToggle;
        private CheckBox _localSystemDiceToggle;
        private Button _manageChroniclesButton;
        private Button _backButton;

        public override void _Ready()
        {
            _optionsColumn = GetNode<VBoxContainer>("ScrollContainer/CenterContainer/MarginContainer/VBoxContainer");

            _physicalDiceToggle = GetNodeOrNull<CheckBox>("ScrollContainer/CenterContainer/MarginContainer/VBoxContainer/PhysicalDiceToggle");
            if (_physicalDiceToggle != null)
            {
                _physicalDiceToggle.ButtonPressed = GameManager.Instance.UsePhysicalDice;
                _physicalDiceToggle.Toggled += (e) => 
                {
                    GameManager.Instance.UsePhysicalDice = e;
                    SaveSettingsToggle("use_physical_dice", e);
                };
            }

            _localSystemDiceToggle = new CheckBox { Text = "Use Local System Dice (Advanced)" };
            _optionsColumn.AddChild(_localSystemDiceToggle);
            _optionsColumn.MoveChild(_localSystemDiceToggle, _physicalDiceToggle != null ? _physicalDiceToggle.GetIndex() + 1 : 4);
            _localSystemDiceToggle.ButtonPressed = GameManager.Instance.UseLocalSystemDice;
            _localSystemDiceToggle.Toggled += (e) => 
            {
                GameManager.Instance.UseLocalSystemDice = e;
                SaveSettingsToggle("use_local_system_dice", e);
            };

            // Inject the dynamic Chronicle Manager Button
            _manageChroniclesButton = new Button();
            _optionsColumn.AddChild(_manageChroniclesButton);
            _optionsColumn.MoveChild(_manageChroniclesButton, 0); // Put it at the top
            RefreshChronicleButtonText();
            _manageChroniclesButton.Pressed += OnManageChroniclesPressed;
            Ui.EnforceTouchTarget(_manageChroniclesButton, new Vector2(0, Ui.MinimumTouchTarget));

            _backButton = GetNodeOrNull<Button>("ScrollContainer/CenterContainer/MarginContainer/VBoxContainer/BackButton");
            if (_backButton != null)
            {
                _backButton.Pressed += () => { QueueFree(); };
            }
        }

        private void RefreshChronicleButtonText()
        {
            string currentId = ChronicleRegistry.Current?.Meta?.Id ?? "Unknown";
            _manageChroniclesButton.Text = $"Active Chronicle: {currentId}\n(Tap to Change)";
        }

        private void OnManageChroniclesPressed()
        {
            List<string> options = new List<string> { "Default (res://Assets/Chronicles/chronicle.v1.json)" };
            
            // Scan user://chronicles/ for custom packs
            string userDir = ProjectSettings.GlobalizePath("user://chronicles");
            if (Directory.Exists(userDir))
            {
                var files = Directory.GetFiles(userDir, "*.json");
                foreach (var file in files)
                {
                    options.Add(Path.GetFileName(file));
                }
            }

            ModalService.Instance.ShowSelection("Select Active Chronicle:", options.ToArray(), selection =>
            {
                string newPath;
                if (selection.StartsWith("Default"))
                {
                    newPath = "res://Assets/Chronicles/chronicle.v1.json";
                }
                else
                {
                    newPath = $"user://chronicles/{selection}";
                }

                GameManager.Instance.SetActiveChronicle(newPath);
                RefreshChronicleButtonText();
                
                // Show brief success toast
                ModalService.Instance.ShowPrompt($"Chronicle Switched to:\n{ChronicleRegistry.Current.Meta.Id}", _ => {});
            });
        }

        private void SaveSettingsToggle(string key, bool value)
        {
            var config = new ConfigFile();
            string path = "user://settings.cfg";
            config.Load(path);
            config.SetValue("Game", key, value);
            config.Save(path);
        }
    }
}