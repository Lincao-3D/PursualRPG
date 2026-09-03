// Scripts/Core/GameManager.cs
using Godot;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using PursualRPG.Scripts.Domain;

namespace PursualRPG.Scripts.Core
{
    public partial class GameManager : Node
    {
        public static GameManager Instance { get; private set; }
        public PlayerData CurrentPlayer { get; set; }
        public bool UsePhysicalDice { get; set; } = false;

        private readonly string SavePath = "user://current_save.json";

        public override void _Ready()
        {
            Instance = this;
            ProcessMode = ProcessModeEnum.Always;
        }

        public bool SaveExists() => File.Exists(ProjectSettings.GlobalizePath(SavePath));

        public void SaveGame()
        {
            var json = JsonConvert.SerializeObject(CurrentPlayer, Formatting.Indented);
            File.WriteAllText(ProjectSettings.GlobalizePath(SavePath), json);
        }

        public void LoadGame()
        {
            if (SaveExists())
            {
                var json = File.ReadAllText(ProjectSettings.GlobalizePath(SavePath));
                CurrentPlayer = JsonConvert.DeserializeObject<PlayerData>(json);
            }
        }

        public void ChangeScene(string scenePath) => GetTree().ChangeSceneToFile(scenePath);
    }

    public class PlayerData
    {
        public string Name { get; set; }
        public string Race { get; set; }
        public string Class { get; set; }
        public int Gold { get; set; } = 50;
        public int Xp { get; set; } = 0;
        public Dictionary<string, int> Attributes { get; set; } = new();
    }
}