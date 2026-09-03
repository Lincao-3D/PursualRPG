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
            "Você é o MESTRE DE RPG de uma mesa de RPG...",
            "A chuva cai fina e constante..."
        );
    }
}