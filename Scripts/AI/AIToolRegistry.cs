using Godot;
using System;
using System.Linq;
using PursualRPG.Scripts.Domain;

namespace PursualRPG.Scripts.AI
{
    public static class AIToolRegistry
    {
        public static string BuildDynamicSystemPrompt(Player player, Scenario baseScenario)
        {
            var prompt = new System.Text.StringBuilder();
            prompt.AppendLine(baseScenario.SystemPrompt);
            prompt.AppendLine("\n--- GAME STATE & AVAILABLE ASSETS ---");
            
            // 1. Expose Player State (C.3)
            if (player != null)
            {
                prompt.AppendLine($"[PLAYER CAPABILITIES]");
                prompt.AppendLine($"- Class: {player.Clazz.Name}, Race: {player.Race}");
                prompt.AppendLine($"- HP: {player.Health}/{player.MaxHealth}, Mana: {player.Mana}/{player.MaxMana}");
                prompt.AppendLine($"- Skills: {string.Join(", ", player.SelectedSkills.Select(s => s.Name))}");
                prompt.AppendLine($"- Expertises: {string.Join(", ", player.SelectedExpertises)}");
                
                if (player.NarrationPlans.HasPlans)
                {
                    prompt.AppendLine($"[CURRENT NARRATION PLANS]");
                    prompt.AppendLine($"- Short-Term: {player.NarrationPlans.ShortTerm}");
                    prompt.AppendLine($"- Medium-Term: {player.NarrationPlans.MediumTerm}");
                    prompt.AppendLine($"- Long-Term: {player.NarrationPlans.LongTerm}");
                }
            }

            // 2. Expose Monsters & Images (Elaboration & C.1)
            var monsters = string.Join(", ", Enum.GetNames(typeof(EnemyEnum)));
            prompt.AppendLine($"\n[AVAILABLE MONSTERS]");
            prompt.AppendLine($"Enemies you can spawn: {monsters}");
            prompt.AppendLine("To show a creature's image in your narration, use exactly this BBCode format on a new line:");
            prompt.AppendLine("`[img]res://Assets/Images/entities/CREATURE_NAME.png[/img]` (Use lowercase names: bandit, skeleton, zombie, villager).");

            // 3. Define Tools (C.2 / C.3)
            prompt.AppendLine($"\n[JSON TOOLS]");
            prompt.AppendLine("You may append a `toolCommands` array to your JSON response to trigger mechanics.");
            prompt.AppendLine("1. {\"name\": \"initialize_combat\", \"arguments\": {\"enemies\": [\"Bandit\", \"Zombie\"]}} -> Starts combat with specific monsters.");
            prompt.AppendLine("2. {\"name\": \"reward_player\", \"arguments\": {\"gold\": 10, \"xp\": 50}} -> Grants rewards.");
            prompt.AppendLine("3. {\"name\": \"give_item\", \"arguments\": {\"item_id\": 1, \"quantity\": 1}} -> Gives item. IDs: 1=Healing Potion, 2=Mana Elixir, 3=Fire Bomb, 4=Ancient Coin.");
            prompt.AppendLine("4. {\"name\": \"update_plans\", \"arguments\": {\"short\": \"...\", \"medium\": \"...\", \"long\": \"...\"}} -> Updates your hidden narration plans.");

            return prompt.ToString();
        }
    }
}