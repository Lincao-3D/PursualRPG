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
                ClassSelect.ItemSelected += OnClassSelected;
            }

            // Assign initial default class
            _selectedClass = ClassFactoryMap.ClassFactory[CharacterClassEnum.Warrior];

            // Wire events
            if (CreateButton != null) CreateButton.Pressed += OnCreatePressed;
            if (RandomButton != null) RandomButton.Pressed += OnRandomPressed;
            if (MenuButton != null) MenuButton.Pressed += OnMenuPressed;
            if (NameInput != null) NameInput.TextChanged += text => _selectedName = text;
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

        private void OnCreatePressed()
        {
            if (NameInput != null && !string.IsNullOrWhiteSpace(NameInput.Text))
            {
                _selectedName = NameInput.Text.Trim();
            }

            var player = new Player(_selectedName, _selectedClass, _selectedRace, AttributeUtils.RandomAttribs());

            // Store player in global GameManager singleton reference
            GameManager.Instance.CurrentPlayer = player;

            GD.Print($"Player created: {player.Name} [{player.Race} {player.Clazz.Name}]");
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