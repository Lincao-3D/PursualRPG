// Scripts/Core/GameManager.cs
using Godot;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using PursualRPG.Scripts.Domain;

namespace PursualRPG.Scripts.Core
{
	public partial class GameManager : Node
	{
		public static GameManager Instance { get; private set; }

		public Player CurrentPlayer { get; set; }

		public bool UsePhysicalDice { get; set; }

		private readonly string SavePath = "user://current_save.json";

		public override void _Ready()
		{
			Instance = this;
			ProcessMode = ProcessModeEnum.Always;
		}

		public bool SaveExists() =>
			File.Exists(ProjectSettings.GlobalizePath(SavePath));

		public void SaveGame()
		{
			var json = JsonConvert.SerializeObject(CurrentPlayer, Formatting.Indented);
			File.WriteAllText(ProjectSettings.GlobalizePath(SavePath), json);
		}

		public void LoadGame()
		{
			if (!SaveExists())
				return;

			var json = File.ReadAllText(ProjectSettings.GlobalizePath(SavePath));
			CurrentPlayer = JsonConvert.DeserializeObject<Player>(json);
		}

		public void ChangeScene(string scenePath)
		{
			var uiLayer = GetTree().Root.GetNode<CanvasLayer>("Main/UILayer");

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
