using System.Collections.Generic;

namespace PursualRPG.Scripts.Domain
{
    public enum EnemyEnum { Skeleton, Bandit, Zombie, AngryVillager }

    public class MonsterFactory
    {
        public static Dictionary<EnemyEnum, Entity> GetFactories() => new()
        {
            { EnemyEnum.Skeleton, new Entity("Skeleton", 10, 0, 10, 10, new() { { CharacterAttrib.Strength, 8 }, { CharacterAttrib.Dexterity, 12 } }) },
            { EnemyEnum.Bandit, new Entity("Bandit", 6, 5, 12, 15, new() { { CharacterAttrib.Dexterity, 14 } }) },
            { EnemyEnum.Zombie, new Entity("Zombie", 15, 0, 8, 10, new() { { CharacterAttrib.Constitution, 14 } }, EntityCategory.Undead) },
            { EnemyEnum.AngryVillager, new Entity("Angry Villager", 6, 0, 10, 5, new()) }
        };
    }
}