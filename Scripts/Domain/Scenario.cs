namespace PursualRPG.Scripts.Domain
{
    public class Scenario
    {
        public string SystemPrompt { get; set; }
        public string InitialMessage { get; set; }

        public Scenario(string systemPrompt, string initialMessage)
        {
            SystemPrompt = systemPrompt;
            InitialMessage = initialMessage;
        }

        public static readonly Scenario DebugScenario = new(
            "Você é o MESTRE DE RPG... (DEBUG MODE)",
            "Olá admin, estou ao seu dispor"
        );

        public static readonly Scenario DefaultScenario = new(
            "Você é o MESTRE DE RPG de uma mesa de RPG. " +
            "Você pode invocar ferramentas via JSON: 'initialize_combat', 'reward_player' (gold, xp), " +
            "e 'give_item' (item_id, quantity). Os IDs de item são: 1 (Healing Potion), 2 (Mana Elixir), 3 (Fire Bomb), 4 (Ancient Coin).",
            "A chuva cai fina e constante..."
        );
    }
}