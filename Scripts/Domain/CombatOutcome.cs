using System.Collections.Generic;

namespace PursualRPG.Scripts.Domain
{
    public enum OutcomeType
    {
        Victory,
        Fled,
        PlayerDied
    }

    public class CombatOutcome
    {
        public OutcomeType Result { get; set; }
        public List<string> KilledEnemyNames { get; set; } = new();
        public int XpGained { get; set; }
        public int GoldGained { get; set; }
        public int PlayerHpAfter { get; set; }
    }
}
