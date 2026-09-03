using Godot;
using PursualRPG.Scripts.Domain;

namespace PursualRPG.Scripts.AI
{
    public static class RPGTools
    {
        public static void RewardPlayer(Player player, int gold, int xp)
        {
            if (gold > 0) player.Gold += gold;
            if (xp > 0) player.Xp += xp;
            GD.Print($"[RPGTools] Player rewarded: +{gold} Gold, +{xp} XP.");
        }

        public static void DamagePlayer(Player player, int damage, DamageType damageType)
        {
            player.ApplyDamage(null, damage, damageType);
            GD.Print($"[RPGTools] Player took {damage} damage of type {damageType}.");
        }
    }
}