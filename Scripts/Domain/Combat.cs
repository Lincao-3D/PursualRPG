using System;
using System.Collections.Generic;

namespace PursualRPG.Scripts.Domain
{
    public class CombatResult
    {
        public bool Victory { get; set; }
        public bool PlayerFlee { get; set; }
        public int Kills { get; set; }
        public List<string> EnemiesFlee { get; set; } = new();
        public List<string> Enemies { get; set; } = new();
    }

    public class TimedAction
    {
        public float Delay { get; set; }
        public Action ActionCallback { get; set; }
        public string Text { get; set; }
        private DateTime? _startTime;

        public TimedAction(float delay, Action action, string text)
        {
            Delay = delay;
            ActionCallback = action;
            Text = text;
        }

        public void Start() => _startTime = DateTime.Now;
        public bool Ready() => _startTime.HasValue && (DateTime.Now - _startTime.Value).TotalSeconds >= Delay;
    }

    public class Combat
    {
        public object GameController { get; set; } // Reference to GameManager
        public List<Entity> Enemies { get; set; }
        public bool Fleeable { get; set; }
        public List<Entity> TurnOrder { get; set; }
        public int TurnIndex { get; set; } = 0;
        public bool FleePrep { get; set; } = false;
        public bool Running { get; set; } = true;
        public bool IsPlayerTurn { get; set; } = false;
        public CombatResult Result { get; set; }
        public List<string> Log { get; set; } = new();
        
        private List<TimedAction> _actionQueue = new();
        private TimedAction _currentAction;

        public Combat(object game, List<Entity> enemies, bool fleeable)
        {
            GameController = game;
            Enemies = enemies;
            Fleeable = fleeable;
            TurnOrder = InitInitiative();
        }

        public void PrintText(string text, float delay = 3.0f) => _actionQueue.Add(new TimedAction(delay, null, text));
        public void DelayedAction(Action action, string text = null, float delay = 3.0f) => _actionQueue.Add(new TimedAction(delay, action, text));

        private List<Entity> InitInitiative()
        {
            var participants = new List<Entity> { /* Player reference */ };
            participants.AddRange(Enemies);
            // Sort by initiative rolls descending
            return participants;
        }
    }
}