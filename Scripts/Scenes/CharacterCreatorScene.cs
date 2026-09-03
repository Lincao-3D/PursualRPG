using Godot;
using System.Collections.Generic;
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

        [Export] public LineEdit NameInput;
        [Export] public Button CreateButton;
        [Export] public Button RandomButton;
        [Export] public Button MenuButton;

        public override void _Ready()
        {
            if (NameInput != null) NameInput.Text = _selectedName;
            if (CreateButton != null) CreateButton.Pressed += OnCreatePressed;
            if (RandomButton != null) RandomButton.Pressed += OnRandomPressed;
            if (MenuButton != null) MenuButton.Pressed += OnMenuPressed;

            _selectedClass = ClassFactoryMap.ClassFactory[CharacterClassEnum.Warrior];
        }

        private void OnCreatePressed()
        {
            var player = new Player(_selectedName, _selectedClass, _selectedRace, AttributeUtils.RandomAttribs());
            // Store player in global GameManager singleton reference here
            GD.Print($"Player created: {player.Name} [{player.Race} {player.Clazz.Name}]");
            GetTree().ChangeSceneToFile("res://Scenes/ChatScene.tscn");
        }

        private void OnRandomPressed()
        {
            _selectedName = "RandomHero";
            if (NameInput != null) NameInput.Text = _selectedName;
            OnCreatePressed();
        }

        private void OnMenuPressed()
        {
            GetTree().ChangeSceneToFile("res://Scenes/MainMenuScene.tscn");
        }
    }
}

// intended code with .csv strings
// Scripts/Scenes/CharacterCreatorScene.cs
/* using Godot;
using PursualRPG.Scripts.Core;
using PursualRPG.Scripts.Domain; // Fixes "CharacterClassEnum does not exist"

namespace PursualRPG.Scripts.Scenes
{
    public partial class CharacterCreatorScene : Control
    {
        private OptionButton _classSelect;
        
        public override void _Ready()
        {
            _classSelect = GetNode<OptionButton>("VBoxContainer/ClassSelect");

            // Populating UI from the Domain Enums directly and wiring strings.csv translations
            foreach (CharacterClassEnum cls in System.Enum.GetValues(typeof(CharacterClassEnum)))
            {
                // Ensure strings.csv has keys matching CLASS_WARRIOR, CLASS_PALADIN, etc.
                _classSelect.AddItem(Tr($"CLASS_{cls.ToString().ToUpper()}"));
            }
        }
    }
} */