using Godot;
using System.Collections.Generic;
using System.Linq;
using PursualRPG.Scripts.Domain;

namespace PursualRPG.Scripts.UI
{
    public partial class CharacterSheetPanel : Control
    {
        private Player _player;
        private bool _isOpen = false;
        private float _targetX;
        private float _currentX;
        private float _animationSpeed = 28f;
        private float _panelWidth = 410f;

        [Export] public Label NameLabel;
        [Export] public Label StatsLabel;
        [Export] public Label AttributesLabel;
        [Export] public Label SkillsLabel;
        [Export] public Label InventoryLabel;

        public override void _Ready()
        {
            if (NameLabel == null) NameLabel = GetNodeOrNull<Label>("NameLabel");
            if (StatsLabel == null) StatsLabel = GetNodeOrNull<Label>("StatsLabel");
            if (AttributesLabel == null) AttributesLabel = GetNodeOrNull<Label>("AttributesLabel");
            if (SkillsLabel == null) SkillsLabel = GetNodeOrNull<Label>("SkillsLabel");
            
            if (InventoryLabel == null)
            {
                InventoryLabel = GetNodeOrNull<Label>("InventoryLabel");
                if (InventoryLabel == null)
                {
                    InventoryLabel = new Label { Position = new Vector2(20, 510), CustomMinimumSize = new Vector2(360, 120) };
                    AddChild(InventoryLabel);
                }
            }
        }

        public void Initialize(Player player)
        {
            _player = player;
            _currentX = GetViewportRect().Size.X;
            _targetX = _currentX;
            Position = new Vector2(_currentX, 0);
            UpdatePanelContent();
        }

        public void Toggle()
        {
            _isOpen = !_isOpen;
            float viewportWidth = GetViewportRect().Size.X;
            _targetX = _isOpen ? viewportWidth - _panelWidth : viewportWidth;
        }

        public override void _Process(double delta)
        {
            if (!Mathf.IsEqualApprox(_currentX, _targetX))
            {
                _currentX = Mathf.MoveToward(_currentX, _targetX, _animationSpeed);
                Position = new Vector2(_currentX, Position.Y);
            }
        }

        private void UpdatePanelContent()
        {
            if (_player == null) return;

            int level = 1 + (_player.Xp / 100);

            if (NameLabel != null)
                NameLabel.Text = $"{_player.Name}\n[{_player.Race} {_player.Clazz.Name}] - Lvl {level}";

            if (StatsLabel != null)
            {
                StatsLabel.Text =
                    $"HP: {_player.Health}/{_player.MaxHealth} | Mana: {_player.Mana}/{_player.MaxMana}\n" +
                    $"XP: {_player.Xp} | Gold: {_player.Gold}";
            }

            if (AttributesLabel != null)
            {
                var attributes = _player.Attributes;
                AttributesLabel.Text =
                    $"STR: {attributes.GetValueOrDefault(CharacterAttrib.Strength)} | " +
                    $"DEX: {attributes.GetValueOrDefault(CharacterAttrib.Dexterity)} | " +
                    $"CON: {attributes.GetValueOrDefault(CharacterAttrib.Constitution)}\n" +
                    $"INT: {attributes.GetValueOrDefault(CharacterAttrib.Intelligence)} | " +
                    $"WIS: {attributes.GetValueOrDefault(CharacterAttrib.Wisdom)} | " +
                    $"CHA: {attributes.GetValueOrDefault(CharacterAttrib.Charisma)}";
            }

            if (SkillsLabel != null)
            {
                string skillsText = "Skills: " + string.Join(", ", _player.SelectedSkills.Select(s => s.Name)) + "\n";
                string expertisesText = "Expertises: " + string.Join(", ", _player.SelectedExpertises.Select(e => e.ToString()));
                SkillsLabel.Text = $"{skillsText}\n{expertisesText}";
            }

            if (InventoryLabel != null)
            {
                if (_player.Inventory == null || _player.Inventory.Count == 0)
                {
                    InventoryLabel.Text = "Inventory: Empty";
                }
                else
                {
                    var itemsText = _player.Inventory
                        .Where(kv => kv.Value > 0)
                        .Select(kv => $"{ItemFactoryRegistry.GetItem(kv.Key).Name} x{kv.Value}");
                    InventoryLabel.Text = "Inventory:\n" + string.Join("\n", itemsText);
                }
            }
        }
    }
}