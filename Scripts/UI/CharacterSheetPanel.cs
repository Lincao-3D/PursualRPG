using Godot;
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
            if (NameLabel != null) NameLabel.Text = _player.Name;
            if (StatsLabel != null) StatsLabel.Text = $"HP: {_player.Health}/{_player.MaxHealth}\nMana: {_player.Mana}/{_player.MaxMana}\nGold: {_player.Gold}";
        }
    }
}