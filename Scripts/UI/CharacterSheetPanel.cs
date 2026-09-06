using Godot;
using System.Collections.Generic;
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

        public override void _Ready()
        {
            if (NameLabel == null) NameLabel = GetNodeOrNull<Label>("NameLabel");
            if (StatsLabel == null) StatsLabel = GetNodeOrNull<Label>("StatsLabel");
            if (AttributesLabel == null) AttributesLabel = GetNodeOrNull<Label>("AttributesLabel");
            if (SkillsLabel == null) SkillsLabel = GetNodeOrNull<Label>("SkillsLabel");
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
            if (_player == null)
                return;

            if (NameLabel != null)
                NameLabel.Text = _player.Name;

            if (StatsLabel != null)
            {
                StatsLabel.Text =
                    $"HP: {_player.Health}/{_player.MaxHealth}\n" +
                    $"Mana: {_player.Mana}/{_player.MaxMana}\n" +
                    $"Gold: {_player.Gold}";
            }

            if (AttributesLabel != null)
            {
                var attributes = _player.Attributes;
                AttributesLabel.Text =
                    $"STR: {attributes.GetValueOrDefault(CharacterAttrib.Strength)}\n" +
                    $"DEX: {attributes.GetValueOrDefault(CharacterAttrib.Dexterity)}\n" +
                    $"CON: {attributes.GetValueOrDefault(CharacterAttrib.Constitution)}\n" +
                    $"INT: {attributes.GetValueOrDefault(CharacterAttrib.Intelligence)}\n" +
                    $"WIS: {attributes.GetValueOrDefault(CharacterAttrib.Wisdom)}\n" +
                    $"CHA: {attributes.GetValueOrDefault(CharacterAttrib.Charisma)}";
            }
        }
    }
}