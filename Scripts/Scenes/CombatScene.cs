using Godot;
using System;
using System.Collections.Generic;
using PursualRPG.Scripts.Domain;

namespace PursualRPG.Scripts.Scenes
{
    public partial class CombatScene : Control
    {
        private Combat _combat;
        private Entity _currentTarget;

        // UI Node references
        private TextureProgressBar _lifeBar;
        private Label _logLabel;
        private Button _attackButton;
        private Button _skillButton;
        private Button _itemButton;
        private Button _fleeButton;

        public void Initialize(Combat combat)
        {
            _combat = combat;
            if (_combat.Enemies.Count > 0)
            {
                _currentTarget = _combat.Enemies[0];
            }
        }

        public override void _Ready()
        {
            _lifeBar = GetNode<TextureProgressBar>("UI/LifeBar");
            _logLabel = GetNode<Label>("UI/LogLabel");
            _attackButton = GetNode<Button>("UI/ActionButtons/AttackButton");
            _skillButton = GetNode<Button>("UI/ActionButtons/SkillButton");
            _itemButton = GetNode<Button>("UI/ActionButtons/ItemButton");
            _fleeButton = GetNode<Button>("UI/ActionButtons/FleeButton");

            // Connect UI Action Signals
            _attackButton.Pressed += OnAttackPressed;
            _skillButton.Pressed += OnSkillPressed;
            _itemButton.Pressed += OnItemPressed;
            _fleeButton.Pressed += OnFleePressed;

            if (_combat != null)
            {
                _fleeButton.Visible = _combat.Fleeable;
                _fleeButton.Disabled = !_combat.Fleeable;
            }
        }

        public override void _Process(double delta)
        {
            if (_combat == null) return;

            // Step combat turn queue / updates
            // _combat.Update(); // Custom update depending on your C# implementation loop

            UpdateUIState();
        }

        private void UpdateUIState()
        {
            // Enable or disable action buttons based on whether it is the player's turn
            bool isPlayerTurn = _combat.IsPlayerTurn;
            _attackButton.Disabled = !isPlayerTurn;
            _skillButton.Disabled = !isPlayerTurn;
            _itemButton.Disabled = !isPlayerTurn;

            if (_combat.Log.Count > 0)
            {
                _logLabel.Text = _combat.Log[^1];
            }
        }

        private void OnAttackPressed()
        {
            if (!_combat.IsPlayerTurn || _currentTarget == null) return;

            // Execute attack routing using Entity / Player extension methods
            // Example:
            // var (passed, result, damage) = player.AttackTarget(_currentTarget);
            // if (passed) { _currentTarget.ApplyDamage(player, damage, DamageType.Bludgeoning, _combat); }
            
            _combat.IsPlayerTurn = false;
        }

        private void OnSkillPressed()
        {
            if (!_combat.IsPlayerTurn) return;
            // Open skill selector panel / dropdown menu
            GD.Print("Skill menu toggled.");
        }

        private void OnItemPressed()
        {
            if (!_combat.IsPlayerTurn) return;
            // Open inventory usable items list
            GD.Print("Item inventory toggled.");
        }

        private void OnFleePressed()
        {
            if (!_combat.Fleeable) return;
            // Trigger escape routine
            GD.Print("Player attempts to flee.");
        }

        public void SetTarget(Entity target)
        {
            _currentTarget = target;
        }
    }
}