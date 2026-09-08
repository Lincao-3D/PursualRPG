using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PursualRPG.Scripts.Core;
using PursualRPG.Scripts.Domain;
using PursualRPG.Scripts.UI;

namespace PursualRPG.Scripts.Scenes
{
    public partial class CharacterCreatorScene : Control
    {
        private string _selectedName = "Hero";
        private CharacterRace _selectedRace = CharacterRace.Human;
        private CharacterClass _selectedClass;
        private List<Skill> _selectedSkills = new();
        private List<CharacterExpertise> _selectedExpertises = new();
        
        private List<int> _rolledValues = new();
        private Dictionary<int, CharacterAttrib> _rollAssignments = new();
        private int _rerollsRemaining = 3;

        [Export] public LineEdit NameInput;
        [Export] public OptionButton RaceSelect;
        [Export] public OptionButton ClassSelect;
        [Export] public VBoxContainer AttributeAssignmentContainer;
        [Export] public RichTextLabel ChecklistLabel;
        [Export] public Label ErrorLabel;
        [Export] public Button RerollButton;
        [Export] public Button CreateButton;
        [Export] public Button RandomButton;
        [Export] public ItemList SkillList;
        [Export] public ItemList ExpertiseList;

        public override async void _Ready()
        {
            NameInput = GetNodeOrNull<LineEdit>("VBoxContainer/NameInput");
            RaceSelect = GetNodeOrNull<OptionButton>("VBoxContainer/RaceSelect");
            ClassSelect = GetNodeOrNull<OptionButton>("VBoxContainer/ClassSelect");
            AttributeAssignmentContainer = GetNodeOrNull<VBoxContainer>("VBoxContainer/AttributeAssignmentContainer");
            ChecklistLabel = GetNodeOrNull<RichTextLabel>("VBoxContainer/ChecklistLabel");
            ErrorLabel = GetNodeOrNull<Label>("VBoxContainer/ErrorLabel");
            RerollButton = GetNodeOrNull<Button>("VBoxContainer/RerollButton");
            CreateButton = GetNodeOrNull<Button>("VBoxContainer/CreateButton");
            RandomButton = GetNodeOrNull<Button>("VBoxContainer/RandomButton");
            SkillList = GetNodeOrNull<ItemList>("VBoxContainer/SkillList");
            ExpertiseList = GetNodeOrNull<ItemList>("VBoxContainer/ExpertiseList");

            if (ErrorLabel != null)
            {
                ErrorLabel.AddThemeColorOverride("font_color", Colors.Red);
                ErrorLabel.Text = "";
            }

            SetupSelectors();
            
            await RunCinematicIntroAsync();

            InitializeRolls();
            UpdateChecklist();

            if (CreateButton != null) CreateButton.Pressed += OnCreatePressed;
            if (RandomButton != null) RandomButton.Pressed += OnRandomPressed;
            if (RerollButton != null) RerollButton.Pressed += OnRerollPressed;
            if (NameInput != null) NameInput.TextChanged += text => _selectedName = text;
        }

        private async Task RunCinematicIntroAsync()
        {
            await ShowBarAsync("O jogo vai começar!", 1500f);
            await ShowBarAsync("Rolando dados...", 1500f);
            _rolledValues = GameManager.Instance.UsePhysicalDice ? CollectPhysicalDiceModal() : AttributeUtils.RollFourD6DropLowestSet();
            await ShowBarAsync($"Resultados: {string.Join(", ", _rolledValues)}", 2500f);
        }

        private async Task ShowBarAsync(string text, float durationMs)
        {
            var bar = new HorizontalUIBar();
            AddChild(bar);
            bar.Initialize(text, durationMs);
            while (!bar.IsDone)
            {
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            }
        }

        private List<int> CollectPhysicalDiceModal()
        {
            return AttributeUtils.RollFourD6DropLowestSet();
        }

        private void SetupSelectors()
        {
            if (RaceSelect != null)
            {
                RaceSelect.Clear();
                foreach (CharacterRace race in Enum.GetValues(typeof(CharacterRace)))
                    RaceSelect.AddItem(race.ToString());
                RaceSelect.ItemSelected += idx => _selectedRace = (CharacterRace)idx;
            }

            if (ClassSelect != null)
            {
                ClassSelect.Clear();
                foreach (CharacterClassEnum cEnum in Enum.GetValues(typeof(CharacterClassEnum)))
                    ClassSelect.AddItem(cEnum.ToString());
                ClassSelect.ItemSelected += idx => {
                    _selectedClass = ClassFactoryMap.ClassFactory[(CharacterClassEnum)idx];
                    PopulateSkills();
                };
            }
            _selectedClass = ClassFactoryMap.ClassFactory[CharacterClassEnum.Warrior];
            PopulateSkills();

            if (ExpertiseList != null)
            {
                ExpertiseList.SelectMode = ItemList.SelectModeEnum.Multi;
                ExpertiseList.Clear();
                foreach (CharacterExpertise exp in Enum.GetValues(typeof(CharacterExpertise)))
                    ExpertiseList.AddItem(exp.ToString());
                ExpertiseList.MultiSelected += (idx, selected) => UpdateExpertises();
            }
        }

        private void PopulateSkills()
        {
            if (SkillList == null) return;
            SkillList.SelectMode = ItemList.SelectModeEnum.Multi;
            SkillList.Clear();
            foreach (var kvp in SkillFactoryRegistry.SkillFactory)
            {
                if (kvp.Value.Classes.Contains((CharacterClassEnum)Enum.Parse(typeof(CharacterClassEnum), _selectedClass.Name)))
                {
                    SkillList.AddItem(kvp.Value.Name);
                }
            }
        }

        private void InitializeRolls()
        {
            _rollAssignments.Clear();
            if (AttributeAssignmentContainer == null) return;

            foreach (Node child in AttributeAssignmentContainer.GetChildren())
                child.QueueFree();

            foreach (var roll in _rolledValues)
            {
                var hbox = new HBoxContainer();
                var label = new Label { Text = $"Roll [{roll}]: " };
                var option = new OptionButton();
                option.AddItem("-- Select Attribute --");
                foreach (CharacterAttrib attr in Enum.GetValues(typeof(CharacterAttrib)))
                    option.AddItem(attr.ToString());

                option.ItemSelected += idx => {
                    if (idx == 0) _rollAssignments.Remove(roll);
                    else _rollAssignments[roll] = (CharacterAttrib)(idx - 1);
                    UpdateChecklist();
                };

                hbox.AddChild(label);
                hbox.AddChild(option);
                AttributeAssignmentContainer.AddChild(hbox);
            }
        }

        private void UpdateChecklist()
        {
            if (ChecklistLabel == null) return;

            var assignedCounts = new Dictionary<CharacterAttrib, int>();
            foreach (var attr in _rollAssignments.Values)
                assignedCounts[attr] = assignedCounts.GetValueOrDefault(attr, 0) + 1;

            string text = "[b]Attribute Checklist:[/b]\n";
            foreach (CharacterAttrib attr in Enum.GetValues(typeof(CharacterAttrib)))
            {
                int count = assignedCounts.GetValueOrDefault(attr, 0);
                if (count == 1)
                {
                    int val = _rollAssignments.First(x => x.Value == attr).Key;
                    text += $"[color=green][X] {attr}: {val}[/color]\n";
                }
                else if (count > 1)
                {
                    text += $"[color=red][!] {attr}: Conflict (x{count})[/color]\n";
                }
                else
                {
                    text += $"[color=gray][ ] {attr}: --[/color]\n";
                }
            }
            ChecklistLabel.Text = text;
        }

        private void UpdateExpertises()
        {
            _selectedExpertises.Clear();
            foreach (int idx in ExpertiseList.GetSelectedItems())
            {
                _selectedExpertises.Add((CharacterExpertise)idx);
            }
        }

        private void UpdateSkillsSelection()
        {
            _selectedSkills.Clear();
            if (SkillList == null) return;
            var classEnum = (CharacterClassEnum)Enum.Parse(typeof(CharacterClassEnum), _selectedClass.Name);
            var availableSkills = SkillFactoryRegistry.SkillFactory.Values
                .Where(s => s.Classes.Contains(classEnum)).ToList();

            foreach (int idx in SkillList.GetSelectedItems())
            {
                if (idx < availableSkills.Count)
                    _selectedSkills.Add(availableSkills[idx]);
            }
        }

        private void OnRerollPressed()
        {
            if (_rerollsRemaining <= 0) return;
            _rerollsRemaining--;
            if (RerollButton != null) RerollButton.Text = $"Reroll ({_rerollsRemaining})";
            if (_rerollsRemaining == 0 && RerollButton != null) RerollButton.Disabled = true;

            _rolledValues = AttributeUtils.RollFourD6DropLowestSet();
            InitializeRolls();
            UpdateChecklist();
        }

        private bool ValidateCharacter(out string error)
        {
            UpdateSkillsSelection();
            UpdateExpertises();

            if (string.IsNullOrWhiteSpace(_selectedName))
            {
                error = "Character name cannot be empty.";
                return false;
            }

            var assignedCounts = new Dictionary<CharacterAttrib, int>();
            foreach (var attr in _rollAssignments.Values)
                assignedCounts[attr] = assignedCounts.GetValueOrDefault(attr, 0) + 1;

            foreach (CharacterAttrib attr in Enum.GetValues(typeof(CharacterAttrib)))
            {
                int count = assignedCounts.GetValueOrDefault(attr, 0);
                if (count != 1)
                {
                    error = $"All 6 attributes must be assigned exactly once. Conflict/Missing on {attr}.";
                    return false;
                }
            }

            if (_selectedExpertises.Count != 4)
            {
                error = "You must select exactly 4 expertises.";
                return false;
            }

            if (_selectedSkills.Count == 0)
            {
                error = "You must select at least one starting skill.";
                return false;
            }

            error = "";
            return true;
        }

        private void OnCreatePressed()
        {
            if (!ValidateCharacter(out string error))
            {
                if (ErrorLabel != null) ErrorLabel.Text = error;
                return;
            }

            var finalAttributes = new Dictionary<CharacterAttrib, int>();
            foreach (var kvp in _rollAssignments)
                finalAttributes[kvp.Value] = kvp.Key;

            var player = new Player(_selectedName, _selectedClass, _selectedRace, finalAttributes, _selectedSkills, _selectedExpertises);
            GameManager.Instance.CurrentPlayer = player;

            GD.Print($"Character successfully created: {player.Name} [{player.Race} {player.Clazz.Name}]");
            GameManager.Instance.ChangeScene("res://Scenes/ChatScene.tscn");
        }

        private void OnRandomPressed()
        {
            _selectedName = "RandomHero";
            if (NameInput != null) NameInput.Text = _selectedName;
            _rolledValues = AttributeUtils.RollFourD6DropLowestSet();
            
            _rollAssignments.Clear();
            int i = 0;
            foreach (CharacterAttrib attr in Enum.GetValues(typeof(CharacterAttrib)))
            {
                _rollAssignments[_rolledValues[i]] = attr;
                i++;
            }
            UpdateChecklist();
            OnCreatePressed();
        }
    }
}
/* Key Changes Made:
Removed Duplicate Event Binding: Replaced the original standalone ClassSelect.ItemSelected += OnClassSelected; with PopulateSkillsAndExpertises(), which hooks up the combined callback safely.

Node Fallbacks: Added safe fallback resolution for ExpertiseList and SkillSelect inside _Ready().

Initial State Populating: Called OnClassSelectedAndPopulateSkills(0) right after setup to ensure default skills are populated for the initial Warrior class when the scene loads. */