using Godot;
using System.Collections.Generic;

namespace PursualRPG.Scripts.AI
{
    public static class AIToolRegistry
    {
        public static Dictionary<string, Godot.Collections.Dictionary> GetToolSchemas()
        {
            return new Dictionary<string, Godot.Collections.Dictionary>
            {
                {
                    "initialize_combat", new Godot.Collections.Dictionary
                    {
                        { "description", "Initialize an encounter combat sequence with enemies." },
                        { "parameters", new Godot.Collections.Dictionary { { "type", "object" } } }
                    }
                },
                {
                    "reward_player", new Godot.Collections.Dictionary
                    {
                        { "description", "Reward the player with positive gold and XP values." },
                        { "parameters", new Godot.Collections.Dictionary { { "type", "object" } } }
                    }
                }
            };
        }
    }
}