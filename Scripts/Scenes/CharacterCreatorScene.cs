using Godot;
using System;
using System.Collections.Generic;
using PursualRPG.Scripts.Core;
using PursualRPG.Scripts.Domain;

namespace PursualRPG.Scripts.Scenes
{
    public partial class CharacterCreatorScene : Control
    {
        private string _selectedName = "Hero";
        private CharacterRace _selectedRace = CharacterRace.Human;
        private CharacterClass _selectedClass;
        private List<Skill> _selectedSkills = new();
        private List<CharacterExpertise> _selectedExpertises = new();
        private Dictionary<CharacterAttrib, int> _selectedAttributes = new();

        [Export] public Label TitleLabel;
        [Export] public Label NameLabel;
        [Export] public LineEdit NameInput;
        [Export] public Label RaceLabel;
        [Export] public OptionButton RaceSelect;
        [Export] public Label ClassLabel;
        [Export] public OptionButton ClassSelect;
        [Export] public Button CreateButton;
        [Export] public Button RandomButton;
        [Export] public Button MenuButton;
        [Export] public OptionButton SkillSelect;
        [Export] public ItemList ExpertiseList;

        public override void _Ready()
        {
            // Fallback node resolution if exports are unassigned in the inspector
            if (TitleLabel == null) TitleLabel = GetNodeOrNull<Label>("VBoxContainer/Title");
            if (NameLabel == null) NameLabel = GetNodeOrNull<Label>("VBoxContainer/NameLabel");
            if (NameInput == null) NameInput = GetNodeOrNull<LineEdit>("VBoxContainer/NameInput");
            if (RaceLabel == null) RaceLabel = GetNodeOrNull<Label>("VBoxContainer/RaceLabel");
            if (RaceSelect == null) RaceSelect = GetNodeOrNull<OptionButton>("VBoxContainer/RaceSelect");
            if (ClassLabel == null) ClassLabel = GetNodeOrNull<Label>("VBoxContainer/ClassLabel");
            if (ClassSelect == null) ClassSelect = GetNodeOrNull<OptionButton>("VBoxContainer/ClassSelect");
            if (CreateButton == null) CreateButton = GetNodeOrNull<Button>("VBoxContainer/CreateButton");
            if (RandomButton == null) RandomButton = GetNodeOrNull<Button>("VBoxContainer/RandomButton");
            if (MenuButton == null) MenuButton = GetNodeOrNull<Button>("VBoxContainer/MenuButton");
            if (ExpertiseList == null) ExpertiseList = GetNodeOrNull<ItemList>("VBoxContainer/ExpertiseList");
            if (SkillSelect == null) SkillSelect = GetNodeOrNull<OptionButton>("VBoxContainer/SkillSelect");

            // Apply Localized Texts via strings.csv keys
            if (TitleLabel != null) TitleLabel.Text = Tr("TITLE_CHARACTER_CREATION");
            if (NameLabel != null) NameLabel.Text = Tr("LBL_CHARACTER_NAME");
            if (RaceLabel != null) RaceLabel.Text = Tr("LBL_CHARACTER_RACE");
            if (ClassLabel != null) ClassLabel.Text = Tr("LBL_CHARACTER_CLASS");
            if (CreateButton != null) CreateButton.Text = Tr("BTN_CREATE_CHARACTER");
            if (RandomButton != null) RandomButton.Text = Tr("BTN_RANDOM_CHARACTER");
            if (MenuButton != null) MenuButton.Text = Tr("BTN_BACK_TO_MENU");

            if (NameInput != null) NameInput.Text = _selectedName;

            // Populate RaceSelect dynamically from enum and localize keys
            if (RaceSelect != null)
            {
                RaceSelect.Clear();
                foreach (CharacterRace race in Enum.GetValues(typeof(CharacterRace)))
                {
                    RaceSelect.AddItem(Tr($"RACE_{race.ToString().ToUpper()}"));
                }
                RaceSelect.ItemSelected += OnRaceSelected;
            }

            // Populate ClassSelect dynamically from enum and localize keys
            if (ClassSelect != null)
            {
                ClassSelect.Clear();
                foreach (CharacterClassEnum classEnum in Enum.GetValues(typeof(CharacterClassEnum)))
                {
                    ClassSelect.AddItem(Tr($"CLASS_{classEnum.ToString().ToUpper()}"));
                }
            }

            // Assign initial default class
            _selectedClass = ClassFactoryMap.ClassFactory[CharacterClassEnum.Warrior];

            // Populate Skills and Expertises setup
            PopulateSkillsAndExpertises();

            // Populate initial skills for the default class (Warrior is index 0)
            OnClassSelectedAndPopulateSkills(0);

            // Wire remaining events
            if (CreateButton != null) CreateButton.Pressed += OnCreatePressed;
            if (RandomButton != null) RandomButton.Pressed += OnRandomPressed;
            if (MenuButton != null) MenuButton.Pressed += OnMenuPressed;
            if (NameInput != null) NameInput.TextChanged += text => _selectedName = text;
        }

        private void PopulateSkillsAndExpertises()
        {
            // Populate expertises selection (require choosing 4 like the python version)
            if (ExpertiseList != null)
            {
                ExpertiseList.SelectMode = ItemList.SelectModeEnum.Multi;
                ExpertiseList.Clear();
                foreach (CharacterExpertise exp in Enum.GetValues(typeof(CharacterExpertise)))
                {
                    ExpertiseList.AddItem(exp.ToString());
                }
                ExpertiseList.MultiSelected += OnExpertisesSelected;
            }

            if (ClassSelect != null)
            {
                ClassSelect.ItemSelected += (idx) => OnClassSelectedAndPopulateSkills(idx);
            }
        }

        private void OnRaceSelected(long index)
        {
            var races = (CharacterRace[])Enum.GetValues(typeof(CharacterRace));
            if (index >= 0 && index < races.Length)
            {
                _selectedRace = races[index];
            }
        }

        private void OnClassSelected(long index)
        {
            var classEnums = (CharacterClassEnum[])Enum.GetValues(typeof(CharacterClassEnum));
            if (index >= 0 && index < classEnums.Length)
            {
                _selectedClass = ClassFactoryMap.ClassFactory[classEnums[index]];
            }
        }

        private void OnClassSelectedAndPopulateSkills(long index)
        {
            OnClassSelected(index);
            _selectedSkills.Clear();

            // Populate available level 1 skills for the chosen class
            foreach (var kvp in SkillFactoryRegistry.SkillFactory)
            {
                if (kvp.Value.Classes.Contains(_selectedClass.Name == "Warrior" ? CharacterClassEnum.Warrior : CharacterClassEnum.Paladin) && kvp.Value.MinLevel == 1)
                {
                    _selectedSkills.Add(kvp.Value);
                }
            }
        }

        private void OnExpertisesSelected(long index, bool selected)
        {
            _selectedExpertises.Clear();
            var selectedIndices = ExpertiseList.GetSelectedItems();
            var allExpertises = (CharacterExpertise[])Enum.GetValues(typeof(CharacterExpertise));

            foreach (var idx in selectedIndices)
            {
                if (idx < allExpertises.Length)
                {
                    _selectedExpertises.Add(allExpertises[idx]);
                }
            }
        }

        private void OnCreatePressed()
        {
            if (NameInput != null && !string.IsNullOrWhiteSpace(NameInput.Text))
            {
                _selectedName = NameInput.Text.Trim();
            }

            // Pass skills and expertises directly to the Player constructor (or assign them to your player model properties if handled post-creation)
            var player = new Player(_selectedName, _selectedClass, _selectedRace, AttributeUtils.RandomAttribs());
            
            // Store player in global GameManager singleton reference
            GameManager.Instance.CurrentPlayer = player;

            GD.Print($"Player created: {player.Name} [{player.Race} {player.Clazz.Name}] with {_selectedSkills.Count} skills and {_selectedExpertises.Count} expertises.");
            GameManager.Instance.ChangeScene("res://Scenes/ChatScene.tscn");
        }

        private void OnRandomPressed()
        {
            _selectedName = "RandomHero";
            if (NameInput != null) NameInput.Text = _selectedName;
            OnCreatePressed();
        }

        private void OnMenuPressed()
        {
            GameManager.Instance.ChangeScene("res://Scenes/MainMenuScene.tscn");
        }
    }
}
/* Key Changes Made:
Removed Duplicate Event Binding: Replaced the original standalone ClassSelect.ItemSelected += OnClassSelected; with PopulateSkillsAndExpertises(), which hooks up the combined callback safely.

Node Fallbacks: Added safe fallback resolution for ExpertiseList and SkillSelect inside _Ready().

Initial State Populating: Called OnClassSelectedAndPopulateSkills(0) right after setup to ensure default skills are populated for the initial Warrior class when the scene loads. */