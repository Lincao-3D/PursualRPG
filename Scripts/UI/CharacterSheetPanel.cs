using Godot;
using System.Collections.Generic;
using System.Linq;
using PursualRPG.Scripts.Domain;

namespace PursualRPG.Scripts.UI
{
	public partial class CharacterSheetPanel : Control
	{
		private Player _player;
		private bool _isOpen = false;
		private float _targetX;
		private float _currentX;
		private float _animationSpeed = 28f;
		private float _panelWidth = 410f;

		[Export] public Label NameLabel;
		[Export] public Label StatsLabel;
		[Export] public Label AttributesLabel;
		[Export] public Label SkillsLabel;
		[Export] public Label InventoryLabel;

		public override void _Ready()
		{
			// FORÇA o painel invisível a ignorar cliques fora dos textos
			MouseFilter = MouseFilterEnum.Ignore;

			if (NameLabel == null) NameLabel = GetNodeOrNull<Label>("NameLabel");
			if (StatsLabel == null) StatsLabel = GetNodeOrNull<Label>("StatsLabel");
			if (AttributesLabel == null) AttributesLabel = GetNodeOrNull<Label>("AttributesLabel");
			if (SkillsLabel == null) SkillsLabel = GetNodeOrNull<Label>("SkillsLabel");
			
			if (InventoryLabel == null)
			{
				InventoryLabel = GetNodeOrNull<Label>("InventoryLabel");
				if (InventoryLabel == null)
				{
					InventoryLabel = new Label { Position = new Vector2(20, 510), CustomMinimumSize = new Vector2(360, 120) };
					AddChild(InventoryLabel);
				}
			}

			// Garante que todos os textos filhos parem o mouse se você quiser clicar neles,
			// mas o painel pai em si não vai bloquear o resto da tela.
			foreach (Node child in GetChildren())
			{
				if (child is Control control)
				{
					control.MouseFilter = MouseFilterEnum.Pass;
				}
			}
		}

		public void Initialize(Player player)
		{
			_player = player;
			
			// Força o tamanho do painel a bater com o esperado pelo script
			CustomMinimumSize = new Vector2(_panelWidth, Size.Y);
			
			float viewportWidth = GetViewportRect().Size.X;
			_currentX = viewportWidth;
			_targetX = _currentX;
			
			// Mantém o Y original do seu design (60) em vez de zerar e quebrar o layout
			Position = new Vector2(_currentX, Position.Y); 
			_isOpen = false;
			
			UpdatePanelContent();
		}

		public void Toggle()
		{
			_isOpen = !_isOpen;
			float viewportWidth = GetViewportRect().Size.X;
			_targetX = _isOpen ? viewportWidth - _panelWidth : viewportWidth;

			// Se estiver fechado, o painel inteiro e seus filhos se tornam totalmente transparentes ao mouse
			if (!_isOpen)
			{
				MouseFilter = MouseFilterEnum.Ignore;
				foreach (Node child in GetChildren())
				{
					if (child is Control control) control.MouseFilter = MouseFilterEnum.Ignore;
				}
			}
			else
			{
				// Se estiver aberto, permite interações apenas onde houver conteúdo
				MouseFilter = MouseFilterEnum.Ignore; 
				foreach (Node child in GetChildren())
				{
					if (child is Control control) control.MouseFilter = MouseFilterEnum.Pass;
				}
			}
		}

		public override void _Process(double delta)
		{
			if (!Mathf.IsEqualApprox(_currentX, _targetX))
			{
				_currentX = Mathf.MoveToward(_currentX, _targetX, _animationSpeed);
				Position = new Vector2(_currentX, Position.Y);
			}
		}

		private void UpdatePanelContent()
		{
			if (_player == null) return;

			int level = 1 + (_player.Xp / 100);

			if (NameLabel != null)
				NameLabel.Text = $"{_player.Name}\n[{_player.Race} {_player.Clazz.Name}] - Lvl {level}";

			if (StatsLabel != null)
			{
				StatsLabel.Text =
					$"HP: {_player.Health}/{_player.MaxHealth} | Mana: {_player.Mana}/{_player.MaxMana}\n" +
					$"XP: {_player.Xp} | Gold: {_player.Gold}";
			}

			if (AttributesLabel != null)
			{
				var attributes = _player.Attributes;
				AttributesLabel.Text =
					$"STR: {attributes.GetValueOrDefault(CharacterAttrib.Strength)} | " +
					$"DEX: {attributes.GetValueOrDefault(CharacterAttrib.Dexterity)} | " +
					$"CON: {attributes.GetValueOrDefault(CharacterAttrib.Constitution)}\n" +
					$"INT: {attributes.GetValueOrDefault(CharacterAttrib.Intelligence)} | " +
					$"WIS: {attributes.GetValueOrDefault(CharacterAttrib.Wisdom)} | " +
					$"CHA: {attributes.GetValueOrDefault(CharacterAttrib.Charisma)}";
			}

			if (SkillsLabel != null)
			{
				string skillsText = "Skills: " + string.Join(", ", _player.SelectedSkills.Select(s => s.Name)) + "\n";
				string expertisesText = "Expertises: " + string.Join(", ", _player.SelectedExpertises.Select(e => e.ToString()));
				SkillsLabel.Text = $"{skillsText}\n{expertisesText}";
			}

			if (InventoryLabel != null)
			{
				if (_player.Inventory == null || _player.Inventory.Count == 0)
				{
					InventoryLabel.Text = "Inventory: Empty";
				}
				else
				{
					var itemsText = _player.Inventory
						.Where(kv => kv.Value > 0)
						.Select(kv => $"{ItemFactoryRegistry.GetItem(kv.Key).Name} x{kv.Value}");
					InventoryLabel.Text = "Inventory:\n" + string.Join("\n", itemsText);
				}
			}
		}
	}
}
