using System.Collections.Generic;
using System.Linq;

namespace PursualRPG.Scripts.Domain
{
    public class CombatResult
    {
        public bool Victory { get; set; }
        public bool PlayerFlee { get; set; }
        public int Kills { get; set; }
    }

    public class Combat
    {
        public Player PlayerRef { get; set; }
        public List<Entity> Enemies { get; set; }
        public bool Fleeable { get; set; }

        public List<Entity> TurnOrder { get; set; } = new();
        public int TurnIndex { get; set; } = -1;

        public Entity CurrentEntity => (TurnIndex >= 0 && TurnIndex < TurnOrder.Count) ? TurnOrder[TurnIndex] : null;
        public bool IsPlayerTurn => CurrentEntity == PlayerRef;
        public bool IsFinished => PlayerRef.Dead || Enemies.All(e => e.Dead) || Result.PlayerFlee;

        public CombatResult Result { get; set; } = new();
        public List<string> Log { get; set; } = new();

        public Combat(Player player, List<Entity> enemies, bool fleeable)
        {
            PlayerRef = player;
            Enemies = enemies;
            Fleeable = fleeable;
        }

        public void LogMessage(string msg)
        {
            Log.Add(msg);
            Godot.GD.Print($"[Combat] {msg}");
        }

        public void AdvanceTurn()
        {
            if (IsFinished) return;
            do
            {
                TurnIndex = (TurnIndex + 1) % TurnOrder.Count;
            } while (TurnOrder[TurnIndex].Dead && !IsFinished);
        }
    }
}