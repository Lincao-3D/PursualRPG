using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PursualRPG.Scripts.Domain;
using PursualRPG.Scripts.Core;
using PursualRPG.Scripts.UI;

namespace PursualRPG.Scripts.Scenes
{
    public partial class CombatScene : Control
    {
        private Combat _combat;
        private Entity _currentTarget;
        private bool _isProcessingTurn = false;

        private TextureProgressBar _lifeBar;
        private Label _logLabel;
        private Button _attackButton;
        private Button _skillButton;
        private Button _itemButton;
        private Button _fleeButton;
        private Control _skillPanel;
        private VBoxContainer _skillList;

        public override void _Ready()
        {
            _lifeBar = GetNodeOrNull<TextureProgressBar>("UI/LifeBar");
            _logLabel = GetNode<Label>("UI/LogLabel");
            _attackButton = GetNode<Button>("UI/ActionButtons/AttackButton");
            _skillButton = GetNode<Button>("UI/ActionButtons/SkillButton");
            _itemButton = GetNode<Button>("UI/ActionButtons/ItemButton");
            _fleeButton = GetNode<Button>("UI/ActionButtons/FleeButton");
            _skillPanel = GetNodeOrNull<Control>("UI/SkillPanel");
            _skillList = GetNodeOrNull<VBoxContainer>("UI/SkillPanel/ScrollContainer/VBoxContainer");

            _attackButton.Text = Tr("BTN_ATTACK");
            _skillButton.Text = Tr("BTN_SKILL");
            _itemButton.Text = Tr("BTN_ITEM");
            _fleeButton.Text = Tr("BTN_FLEE");

            FontService.ApplyFont(_logLabel, FontType.ChatReading);
            _attackButton.BindAudioAndFont(FontType.SecondaryButton);
            _skillButton.BindAudioAndFont(FontType.SecondaryButton);
            _itemButton.BindAudioAndFont(FontType.SecondaryButton);
            _fleeButton.BindAudioAndFont(FontType.SecondaryButton);

            _attackButton.Pressed += () => _ = OnAttackPressedAsync();
            _skillButton.Pressed += OnSkillMenuToggled;
            _fleeButton.Pressed += () => _ = OnFleePressedAsync();

            if (_skillPanel != null) _skillPanel.Visible = false;
        }

        public async void Initialize(Combat combat)
        {
            _combat = combat;
            _combat.PlayerRef = GameManager.Instance.CurrentPlayer;
            _currentTarget = _combat.Enemies.FirstOrDefault();
            _fleeButton.Disabled = !_combat.Fleeable;

            PopulateSkills();
            await RollInitiativeAsync();
            ProcessTurnQueue();
        }

        private async Task RollInitiativeAsync()
        {
            _combat.LogMessage("Rolling initiative...");
            _combat.TurnOrder.Clear();

            var participants = new List<(Entity entity, int roll)>();

            int playerRoll = await GetD20RollAsync("Roll D20 for Initiative:");
            int playerTotal = playerRoll + ((_combat.PlayerRef.Attributes.GetValueOrDefault(CharacterAttrib.Dexterity, 10) - 10) / 2);
            participants.Add((_combat.PlayerRef, playerTotal));
            _combat.LogMessage($"Player initiative: {playerTotal}");

            foreach (var enemy in _combat.Enemies)
            {
                int eRoll = GD.RandRange(1, 20) + ((enemy.Attributes.GetValueOrDefault(CharacterAttrib.Dexterity, 10) - 10) / 2);
                participants.Add((enemy, eRoll));
            }

            _combat.TurnOrder = participants.OrderByDescending(p => p.roll).Select(p => p.entity).ToList();
            _combat.AdvanceTurn();
        }

        private async void ProcessTurnQueue()
        {
            if (_combat == null || _combat.IsFinished)
            {
                EndCombat();
                return;
            }

            UpdateUIState();

            if (!_combat.IsPlayerTurn && !_isProcessingTurn)
            {
                _isProcessingTurn = true;
                await ProcessEnemyTurnAsync();

                _isProcessingTurn = false;
                if (!_combat.IsFinished)
                {
                    _combat.AdvanceTurn();
                    ProcessTurnQueue();
                }
                else EndCombat();
            }
        }

        private async Task ProcessEnemyTurnAsync()
        {
            var enemy = _combat.CurrentEntity;
            _combat.LogMessage($"{enemy.Name}'s turn.");
            await ToSignal(GetTree().CreateTimer(1.0f), SceneTreeTimer.SignalName.Timeout);

            var target = _combat.PlayerRef;
            int roll = GD.RandRange(1, 20);
            var (passed, result, damage) = enemy.AttackTarget(target, rawD20: roll);

            if (passed)
            {
                _combat.LogMessage($"{enemy.Name} hits for {damage} damage!");
                target.ApplyDamage(enemy, damage, DamageType.Bludgeoning, _combat);
            }
            else
            {
                _combat.LogMessage($"{enemy.Name} misses.");
            }
            await ToSignal(GetTree().CreateTimer(1.0f), SceneTreeTimer.SignalName.Timeout);
        }

        private async Task<int> GetD20RollAsync(string promptText)
        {
            if (GameManager.Instance.UsePhysicalDice)
            {
                var tcs = new TaskCompletionSource<int>();
                ModalService.Instance.ShowPrompt(promptText, input => {
                    if (int.TryParse(input, out int result)) tcs.SetResult(result);
                    else tcs.SetResult(10);
                });
                return await tcs.Task;
            }
            else
            {
                int roll = GD.RandRange(1, 20);
                var animPrefab = GD.Load<PackedScene>("res://Scenes/DiceRollAnimation.tscn");
                if (animPrefab != null)
                {
                    var anim = animPrefab.Instantiate<DiceRollAnimation>();
                    AddChild(anim);
                    anim.SetResult(roll.ToString());
                    anim.HideAfter3Seconds();
                }
                await ToSignal(GetTree().CreateTimer(1.5f), SceneTreeTimer.SignalName.Timeout);
                return roll;
            }
        }

        private void PopulateSkills()
        {
            if (_skillList == null) return;
            foreach (Node child in _skillList.GetChildren()) child.QueueFree();

            var classSkills = SkillFactoryRegistry.SkillFactory.Values.Where(s => s.Classes.Contains(_combat.PlayerRef.Clazz.Name == "Warrior" ? CharacterClassEnum.Warrior : CharacterClassEnum.Paladin));

            foreach (var skill in SkillFactoryRegistry.SkillFactory.Values)
            {
                var btn = new Button { Text = Tr($"SKILL_{skill.Enum.ToString().ToUpper()}") };
                btn.BindAudioAndFont(FontType.SecondaryButton);
                btn.Pressed += () => _ = OnSkillExecuteAsync(skill);
                _skillList.AddChild(btn);
            }
        }

        private void OnSkillMenuToggled()
        {
            if (_skillPanel != null) _skillPanel.Visible = !_skillPanel.Visible;
        }

        private async Task OnAttackPressedAsync()
        {
            if (!_combat.IsPlayerTurn || _currentTarget == null || _isProcessingTurn) return;
            _isProcessingTurn = true;
            SetButtonsEnabled(false);

            int roll = await GetD20RollAsync("Roll D20 for Attack:");
            var (passed, result, damage) = _combat.PlayerRef.AttackTarget(_currentTarget, rawD20: roll);

            if (passed)
            {
                _combat.LogMessage($"Player hits {_currentTarget.Name} for {damage} damage!");
                _currentTarget.ApplyDamage(_combat.PlayerRef, damage, _combat.PlayerRef.GetDamageType(), _combat);
            }
            else _combat.LogMessage("Player misses.");

            CompletePlayerAction();
        }

        private async Task OnSkillExecuteAsync(Skill skill)
        {
            if (!_combat.IsPlayerTurn || _currentTarget == null || _isProcessingTurn) return;
            _isProcessingTurn = true;
            SetButtonsEnabled(false);
            if (_skillPanel != null) _skillPanel.Visible = false;

            _combat.LogMessage($"Player uses {skill.Name}!");
            int? roll = (skill.IsTargeted || skill.Enum == SkillEnum.MagicMissile) ? await GetD20RollAsync($"Roll D20 for {skill.Name}:") : null;

            skill.Execute(_combat.PlayerRef, _currentTarget, _combat, rawD20: roll);

            CompletePlayerAction();
        }

        private async Task OnFleePressedAsync()
        {
            if (!_combat.IsPlayerTurn || !_combat.Fleeable || _isProcessingTurn) return;
            _isProcessingTurn = true;
            SetButtonsEnabled(false);

            int roll = await GetD20RollAsync("Roll D20 to Flee (Dex test):");
            int dexMod = (_combat.PlayerRef.Attributes.GetValueOrDefault(CharacterAttrib.Dexterity, 10) - 10) / 2;

            if (roll + dexMod >= 12)
            {
                _combat.LogMessage("Flee successful!");
                _combat.Result.PlayerFlee = true;
                EndCombat();
            }
            else
            {
                _combat.LogMessage("Failed to flee!");
                CompletePlayerAction();
            }
        }

        private void CompletePlayerAction()
        {
            _isProcessingTurn = false;
            if (!_combat.IsFinished)
            {
                _combat.AdvanceTurn();
                ProcessTurnQueue();
            }
            else EndCombat();
        }

        private void SetButtonsEnabled(bool enabled)
        {
            _attackButton.Disabled = !enabled;
            _skillButton.Disabled = !enabled;
            _itemButton.Disabled = !enabled;
            _fleeButton.Disabled = !enabled || !_combat.Fleeable;
        }

        private void UpdateUIState()
        {
            SetButtonsEnabled(_combat.IsPlayerTurn && !_isProcessingTurn);
            if (_combat.Log.Count > 0) _logLabel.Text = _combat.Log.Last();
            if (_lifeBar != null)
            {
                _lifeBar.MaxValue = _combat.PlayerRef.MaxHealth;
                _lifeBar.Value = _combat.PlayerRef.Health;
            }
        }

        private void EndCombat()
        {
            string outcomeLog = _combat.Result.PlayerFlee ? Tr("MSG_COMBAT_ESCAPED") : (_combat.PlayerRef.Dead ? Tr("MSG_COMBAT_DEFEAT") : Tr("MSG_COMBAT_VICTORY"));
            _combat.LogMessage(outcomeLog);
            GameManager.Instance.ChangeScene("res://Scenes/ChatScene.tscn");
        }
    }
}