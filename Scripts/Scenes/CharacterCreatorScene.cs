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
        [Export] public GridContainer SkillGrid;
        [Export] public GridContainer ExpertiseGrid;
        [Export] public Button MenuButton;

        private List<CheckBox> _skillCheckboxes = new();
        private List<CheckBox> _expertiseCheckboxes = new();

        private void SyncNodeReferences()
        {
            // Fallback node retrieval using existing public properties
            ChecklistLabel ??= GetNodeOrNull<RichTextLabel>("%ChecklistLabel") ?? GetNodeOrNull<RichTextLabel>("ScrollContainer/MarginContainer/VBoxContainer/ChecklistLabel") ?? GetNodeOrNull<RichTextLabel>("ScrollContainer/VBoxContainer/ChecklistLabel");
            ExpertiseGrid ??= GetNodeOrNull<GridContainer>("%ExpertiseGrid") ?? GetNodeOrNull<GridContainer>("ScrollContainer/MarginContainer/VBoxContainer/ExpertiseGrid") ?? GetNodeOrNull<GridContainer>("ScrollContainer/VBoxContainer/ExpertiseGrid");
            SkillGrid ??= GetNodeOrNull<GridContainer>("%SkillGrid") ?? GetNodeOrNull<GridContainer>("ScrollContainer/MarginContainer/VBoxContainer/SkillGrid") ?? GetNodeOrNull<GridContainer>("ScrollContainer/VBoxContainer/SkillGrid");
            CreateButton ??= GetNodeOrNull<Button>("%CreateButton") ?? GetNodeOrNull<Button>("ActionFooter/CreateButton") ?? GetNodeOrNull<Button>("ScrollContainer/MarginContainer/VBoxContainer/CreateButton") ?? GetNodeOrNull<Button>("ScrollContainer/VBoxContainer/CreateButton");
        }
        public override async void _Ready()
        {
            base._Ready();
            SyncNodeReferences();
            NameInput = GetNodeOrNull<LineEdit>("ScrollContainer/MarginContainer/VBoxContainer/NameInput") ?? GetNodeOrNull<LineEdit>("ScrollContainer/VBoxContainer/NameInput");
            RaceSelect = GetNodeOrNull<OptionButton>("ScrollContainer/MarginContainer/VBoxContainer/RaceSelect") ?? GetNodeOrNull<OptionButton>("ScrollContainer/VBoxContainer/RaceSelect");
            ClassSelect = GetNodeOrNull<OptionButton>("ScrollContainer/MarginContainer/VBoxContainer/ClassSelect") ?? GetNodeOrNull<OptionButton>("ScrollContainer/VBoxContainer/ClassSelect");
            AttributeAssignmentContainer = GetNodeOrNull<VBoxContainer>("ScrollContainer/MarginContainer/VBoxContainer/AttributeAssignmentContainer") ?? GetNodeOrNull<VBoxContainer>("ScrollContainer/VBoxContainer/AttributeAssignmentContainer");
            ChecklistLabel = GetNodeOrNull<RichTextLabel>("ScrollContainer/MarginContainer/VBoxContainer/ChecklistLabel") ?? GetNodeOrNull<RichTextLabel>("ScrollContainer/VBoxContainer/ChecklistLabel");
            ErrorLabel = GetNodeOrNull<Label>("ScrollContainer/MarginContainer/VBoxContainer/ErrorLabel") ?? GetNodeOrNull<Label>("ScrollContainer/VBoxContainer/ErrorLabel");
            RerollButton = GetNodeOrNull<Button>("ScrollContainer/MarginContainer/VBoxContainer/RerollButton") ?? GetNodeOrNull<Button>("ScrollContainer/VBoxContainer/RerollButton");
            SkillGrid = GetNodeOrNull<GridContainer>("ScrollContainer/MarginContainer/VBoxContainer/SkillGrid") ?? GetNodeOrNull<GridContainer>("ScrollContainer/VBoxContainer/SkillGrid");
            ExpertiseGrid = GetNodeOrNull<GridContainer>("ScrollContainer/MarginContainer/VBoxContainer/ExpertiseGrid") ?? GetNodeOrNull<GridContainer>("ScrollContainer/VBoxContainer/ExpertiseGrid");

            // CharacterCreatorScene.cs (Button Wiring)
            CreateButton?.BindAudioAndFont(FontType.SecondaryButton);
            RandomButton?.BindAudioAndFont(FontType.SecondaryButton);
            RerollButton?.BindAudioAndFont(FontType.SecondaryButton);
            MenuButton?.BindAudioAndFont(FontType.SecondaryButton);

            if (ErrorLabel != null)
            {
                ErrorLabel.AddThemeColorOverride("font_color", Colors.Red);
                ErrorLabel.Text = "";
            }

            SetupSelectors();
            ApplyFontsToAllUIControls(this);
            await RunCinematicIntroAsync();

            InitializeRolls();
            UpdateChecklist();

            if (CreateButton != null) CreateButton.Pressed += OnCreatePressed;
            if (RandomButton != null) RandomButton.Pressed += () => _ = OnRandomPressedAsync();
            if (RerollButton != null) RerollButton.Pressed += OnRerollPressed;
            if (NameInput != null) NameInput.TextChanged += text => _selectedName = text;

            if (MenuButton != null)
            {
                MenuButton.Pressed += () => GameManager.Instance.ChangeScene("res://Scenes/MainMenuScene.tscn");
            }
        }
        
        private void ApplyFontsToAllUIControls(Control parent)
        {
            foreach (Node child in parent.GetChildren())
            {
                if (child is Label label)
                {
                    FontService.ApplyFont(label, FontType.DefaultMenu);
                }
                else if (child is OptionButton optionButton)
                {
                    FontService.ApplyFont(optionButton, FontType.DefaultMenu);
                    var popup = optionButton.GetPopup();
                    if (popup != null)
                    {
                        FontService.ApplyFont(popup, FontType.DefaultMenu);
                    }
                }
                else if (child is ItemList itemList)
                {
                    FontService.ApplyFont(itemList, FontType.DefaultMenu);
                }
                else if (child is Button button)
                {
                    FontService.ApplyFont(button, FontType.SecondaryButton);
                }

                // Recursively traverse container nodes
                if (child is Control childControl && child.GetChildCount() > 0)
                {
                    ApplyFontsToAllUIControls(childControl);
                }
            }
        }
        private async Task RunCinematicIntroAsync()
        {
            await ShowBarAsync("O jogo vai começar!", 1500f);
            await ShowBarAsync("Rolando dados...", 1500f);
            
            if (GameManager.Instance.UsePhysicalDice)
            {
                // Physical dice mode: Prompt stat-by-stat and map directly
                await PromptPhysicalDiceAsync();
            }
            else
            {
                // Play visual dice animation while generating stats
                var diceAnimScene = GD.Load<PackedScene>("res://Scenes/DiceRollAnimation.tscn");
                if (diceAnimScene != null)
                {
                    var diceNode = diceAnimScene.Instantiate<DiceRollAnimation>();
                    AddChild(diceNode);
                    diceNode.SetResult("4D6");
                    diceNode.HideAfter3Seconds();
                    await ToSignal(GetTree().CreateTimer(3.0f), SceneTreeTimer.SignalName.Timeout);
                }

                // Random mode: Roll pool and let player assign via UI
                _rolledValues = AttributeUtils.RollFourD6DropLowestSet();
                InitializeRolls();
                await ShowBarAsync($"Resultados: {string.Join(", ", _rolledValues)}", 2500f);
            }
            
            UpdateChecklist();
        }

        private async Task<List<int>> PromptPhysicalDiceAsync()
        {
            var scores = new List<int>();
            _rollAssignments.Clear();
            
            string[] statNames = { "Strength", "Constitution", "Dexterity", "Intelligence", "Wisdom", "Charisma" };
            CharacterAttrib[] attributes = {
                CharacterAttrib.Strength, 
                CharacterAttrib.Constitution, 
                CharacterAttrib.Dexterity, 
                CharacterAttrib.Intelligence, 
                CharacterAttrib.Wisdom, 
                CharacterAttrib.Charisma 
            };

            if (AttributeAssignmentContainer != null)
            {
                foreach (Node child in AttributeAssignmentContainer.GetChildren())
                    child.QueueFree();
                
                var infoLabel = new Label { Text = "Atributos inseridos via Dados Físicos (Ficha Direta)." };
                AttributeAssignmentContainer.AddChild(infoLabel);
            }

            for (int i = 0; i < statNames.Length; i++)
            {
                string stat = statNames[i];
                CharacterAttrib attr = attributes[i];

                var tcs = new TaskCompletionSource<string>();
                ModalService.Instance.ShowPrompt($"Enter physical dice score for {stat} (3-18):", input => {
                    tcs.SetResult(input);
                });
                string input = await tcs.Task;
                
                int parsedScore = 12;
                if (int.TryParse(input, out int val) && val >= 3 && val <= 18)
                {
                    parsedScore = val;
                }

                scores.Add(parsedScore);
                _rollAssignments[parsedScore] = attr; 
            }

            await ShowBarAsync("Atributos Físicos Definidos!", 2000f);
            return scores; // Return the list so it can be assigned to _rolledValues
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
            PopulateExpertises();
        }

        private void PopulateSkills()
        {
            if (SkillGrid == null) return;
            foreach (Node child in SkillGrid.GetChildren()) child.QueueFree();
            _skillCheckboxes.Clear();

            var classEnum = (CharacterClassEnum)Enum.Parse(typeof(CharacterClassEnum), _selectedClass.Name);
            var classSkills = SkillFactoryRegistry.SkillFactory.Values
                .Where(s => s.Classes.Contains(classEnum)).ToList();

            foreach (var skill in classSkills)
            {
                var cb = new CheckBox { Text = skill.Name };
                cb.AddThemeFontSizeOverride("font_size", 14);
                cb.Toggled += (state) => UpdateChecklist();
                SkillGrid.AddChild(cb);
                _skillCheckboxes.Add(cb);
            }
        }

        private void PopulateExpertises()
        {
            if (ExpertiseGrid == null) return;
            foreach (Node child in ExpertiseGrid.GetChildren()) child.QueueFree();
            _expertiseCheckboxes.Clear();

            foreach (CharacterExpertise exp in Enum.GetValues(typeof(CharacterExpertise)))
            {
                var cb = new CheckBox { Text = exp.ToString() };
                cb.AddThemeFontSizeOverride("font_size", 14);
                cb.Toggled += (state) => UpdateChecklist();
                ExpertiseGrid.AddChild(cb);
                _expertiseCheckboxes.Add(cb);
            }
        }

        // CharacterCreatorScene.cs (InitializeRolls)
        private void InitializeRolls()
        {
        _rollAssignments.Clear();
            if (AttributeAssignmentContainer == null) return;

            foreach (Node child in AttributeAssignmentContainer.GetChildren())
                child.QueueFree();

            foreach (var roll in _rolledValues)
            {
                var hbox = new HBoxContainer();
                hbox.AddChild(new Label { Text = $"Dice [{roll}]: ", CustomMinimumSize = new Vector2(80, 0) });

                var buttonGroup = new ButtonGroup();
                foreach (CharacterAttrib attr in Enum.GetValues(typeof(CharacterAttrib)))
                {
                    var radio = new CheckBox { Text = attr.ToString(), ButtonGroup = buttonGroup };
                    radio.Pressed += () => { 
                        _rollAssignments[roll] = attr; 
                        UpdateChecklist(); 
                    };
                    hbox.AddChild(radio);
                }
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
            var allExpertises = (CharacterExpertise[])Enum.GetValues(typeof(CharacterExpertise));
            for (int i = 0; i < _expertiseCheckboxes.Count; i++)
            {
                if (_expertiseCheckboxes[i].ButtonPressed && i < allExpertises.Length)
                {
                    _selectedExpertises.Add(allExpertises[i]);
                }
            }
        }

        private void UpdateSkillsSelection()
        {
            _selectedSkills.Clear();
            var classEnum = (CharacterClassEnum)Enum.Parse(typeof(CharacterClassEnum), _selectedClass.Name);
            var availableSkills = SkillFactoryRegistry.SkillFactory.Values
                .Where(s => s.Classes.Contains(classEnum)).ToList();

            for (int i = 0; i < _skillCheckboxes.Count; i++)
            {
                if (_skillCheckboxes[i].ButtonPressed && i < availableSkills.Count)
                {
                    _selectedSkills.Add(availableSkills[i]);
                }
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
                if (assignedCounts.GetValueOrDefault(attr, 0) != 1)
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
            GameManager.Instance.ChangeScene("res://Scenes/ChatScene.tscn");
        }
        private readonly string[] _randomNames = { "Albatroz", "Gimli", "Legendo", "Lyren", "Elgronnd", "Taurinis", "Kedren", "Vinx" };
        private async Task OnRandomPressedAsync()
        {
            var rng = new Random();

            // 1. Pick a random name from the array
            if (_randomNames.Length > 0)
            {
                _selectedName = _randomNames[rng.Next(_randomNames.Length)];
            }
            else
            {
                _selectedName = "RandomHero";
            }
            if (NameInput != null) NameInput.Text = _selectedName;

            // 2. Randomize Race and Class OptionButtons
            if (RaceSelect != null && RaceSelect.ItemCount > 0)
            {
                int randomRaceIdx = rng.Next(RaceSelect.ItemCount);
                RaceSelect.Selected = randomRaceIdx;
                _selectedRace = (CharacterRace)randomRaceIdx;
            }

            if (ClassSelect != null && ClassSelect.ItemCount > 0)
            {
                int randomClassIdx = rng.Next(ClassSelect.ItemCount);
                ClassSelect.Selected = randomClassIdx;
                _selectedClass = ClassFactoryMap.ClassFactory[(CharacterClassEnum)randomClassIdx];
                PopulateSkills(); // Repopulate skill checkboxes based on the newly selected class
            }

            // 3. Roll or prompt for attributes (supports physical dice mode safely)
            _rolledValues = GameManager.Instance.UsePhysicalDice ? await PromptPhysicalDiceAsync() : AttributeUtils.RollFourD6DropLowestSet();

            _rollAssignments.Clear();
            int i = 0;
            foreach (CharacterAttrib attr in Enum.GetValues(typeof(CharacterAttrib)))
            {
                if (i < _rolledValues.Count)
                {
                    _rollAssignments[_rolledValues[i]] = attr;
                    i++;
                }
            }
            InitializeRolls();

            // 4. Randomize starting skills and 4 expertises using Checkbox collections
            if (_skillCheckboxes != null && _skillCheckboxes.Count > 0)
            {
                // Reset all skill checkboxes
                foreach (var cb in _skillCheckboxes)
                {
                    cb.ButtonPressed = false;
                }
                
                // Select one random skill checkbox
                int randomSkillIdx = rng.Next(_skillCheckboxes.Count);
                _skillCheckboxes[randomSkillIdx].ButtonPressed = true;
            }

            if (_expertiseCheckboxes != null && _expertiseCheckboxes.Count >= 4)
            {
                // Reset all expertise checkboxes
                foreach (var cb in _expertiseCheckboxes)
                {
                    cb.ButtonPressed = false;
                }

                // Pick 4 unique random checkboxes and check them
                var randomExpertiseBoxes = _expertiseCheckboxes
                    .OrderBy(_ => rng.Next())
                    .Take(4);

                foreach (var cb in randomExpertiseBoxes)
                {
                    cb.ButtonPressed = true;
                }
            }

            // 5. Execute all necessary UI synchronizations and proceed to character creation
            UpdateSkillsSelection();
            UpdateExpertises();
            UpdateChecklist();
            OnCreatePressed();
        }
    }
}