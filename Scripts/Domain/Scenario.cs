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

        public static readonly Scenario DebugScenario = new("Você é o Core de Suporte e Debug do Godot .NET RPG (GRASS RPG modificado). Você está em MODO DEBUG fora da narrativa: NÃO interpreta personagens, NÃO narra e NÃO mantém imersão. Papel: inspecionar o estado interno (nós do Godot, C#, inventário customizado), explicar mecânicas/código, forçar alterações e auxiliar no balanceamento. REGRAS ABSOLUTAS: Seja técnico, direto e objetivo. Sem dramatização ou eventos narrativos. Obedeça sempre ao admin.", 
        "Olá admin. Sistema de Debug Godot .NET ativo e aguardando comandos.");

        public static readonly Scenario DefaultScenario = new(
            "Você é o MESTRE DE RPG de uma mesa de RPG. " +
            "Você pode invocar ferramentas via JSON: 'initialize_combat', 'reward_player' (gold, xp), " +
            "e 'give_item' (item_id, quantity). Os IDs de item são: 1 (Healing Potion), 2 (Mana Elixir), 3 (Fire Bomb), 4 (Ancient Coin).",
            "A chuva cai fina e constante num ermo pedregoso e alaranjado. A viagem foi longa e agora, após lugáres mais inóspitos, alguns viajantes de volta lhe indicaram que, mais duas horas caminho à frente, você deveria ficar atento, pois não estaria sozinho. Arbustos longínquos indicavam o mais próximo de uma mata rasteira, de um lado. De outro, plantas desérticas espalhavam-se e, nas pequenas colinas distantes, muitas pedras, grutas naturais e formações rochosas se espalhavam. Adiante, a trilha da estrada ficava mais arenosa e menos pedregosa, no caminho que dava no Pedregoso Forte. O que você faz?"
        );
    }
}