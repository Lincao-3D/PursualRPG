// Scripts/Core/GameManager.cs
using Godot;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using PursualRPG.Scripts.Domain;

namespace PursualRPG.Scripts.Core
{
	public partial class GameManager : Node
	{
		public static GameManager Instance { get; private set; }

		public Player CurrentPlayer { get; set; }

		public bool UsePhysicalDice { get; set; }

		public override void _Ready()
		{
			Instance = this;
			ProcessMode = ProcessModeEnum.Always;
		}

		// 1. Point to a visible 'Saves' folder in the project root
        public string GetSaveDirectory()
        {
            string path = ProjectSettings.GlobalizePath("res://Saves");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }

		// Search for all .json files in the Godot user directory
		public string[] GetSaveFiles()
        {
            return Directory.GetFiles(GetSaveDirectory(), "*.json")
                            .Select(Path.GetFileNameWithoutExtension)
                            .ToArray();
        }

        public bool SaveExists() => GetSaveFiles().Length > 0;

        public void SaveGame(string saveName = "autosave")
        {
            if (string.IsNullOrWhiteSpace(saveName)) saveName = "autosave";
            
            var json = JsonConvert.SerializeObject(CurrentPlayer, Formatting.Indented);
            string filePath = Path.Combine(GetSaveDirectory(), $"{saveName}.json");
            File.WriteAllText(filePath, json);
        }

        public void LoadGame(string saveName = "autosave")
        {
            string filePath = Path.Combine(GetSaveDirectory(), $"{saveName}.json");
            if (!File.Exists(filePath)) return;

            var json = File.ReadAllText(filePath);
            CurrentPlayer = JsonConvert.DeserializeObject<Player>(json);

            // REHYDRATION: Re-link the ignored delegates from the Factory so skills work in combat
            if (CurrentPlayer?.SelectedSkills != null)
            {
                for (int i = 0; i < CurrentPlayer.SelectedSkills.Count; i++)
                {
                    var skillEnum = CurrentPlayer.SelectedSkills[i].Enum;
                    if (SkillFactoryRegistry.SkillFactory.TryGetValue(skillEnum, out var factorySkill))
                    {
                        CurrentPlayer.SelectedSkills[i] = factorySkill;
                    }
                }
            }
        }


		public void ChangeScene(string scenePath)
		{
			var uiLayer = GetTree().Root.GetNodeOrNull<CanvasLayer>("Main/UILayer");
			if (uiLayer == null)
			{
				GD.PrintErr("Fatal Error: 'Main/UILayer' CanvasLayer not found in scene tree!");
				return;
			}

			// Enable ASCII background only for Main Menu & Splash scenes
			var bgLayer = GetTree().Root.GetNodeOrNull<CanvasLayer>("Main/BackgroundLayer");
			if (bgLayer != null)
			{
				bool isMenuOrSplash = scenePath.EndsWith("MainMenuScene.tscn") || scenePath.EndsWith("SplashScene.tscn");
				bgLayer.Visible = isMenuOrSplash;
			}

			// Stop music during gameplay scenes, play during menus
			if (scenePath.EndsWith("CharacterCreatorScene.tscn") || scenePath.EndsWith("ChatScene.tscn") || scenePath.EndsWith("CombatScene.tscn"))
			{
				SynthAudioServer.Instance?.StopMusic();
			}
			else if (scenePath.EndsWith("MainMenuScene.tscn") || scenePath.EndsWith("OptionsScene.tscn"))
			{
				SynthAudioServer.Instance?.PlayDungeonSynthTheme();
			}

			foreach (var child in uiLayer.GetChildren())
				child.QueueFree();

			var packedScene = GD.Load<PackedScene>(scenePath);
			if (packedScene == null)
			{
				GD.PrintErr($"Unable to load scene: {scenePath}");
				return;
			}

			var screen = packedScene.Instantiate<Control>();
			uiLayer.AddChild(screen);
		}
	}
}
