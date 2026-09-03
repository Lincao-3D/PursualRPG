using System;

namespace PursualRPG.Scripts.Domain
{
    public enum EffectEnum { Aiming, Reckless, Stunned, DivineSmith }

    public class Effect
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; } = 1;
        public bool Positive { get; set; }
        public bool Stackable { get; set; } = true;
        public bool SkipTurn { get; set; } = false;

        public Action<object> OnApply { get; set; }
        public Action<object> OnUnapply { get; set; }
        public Func<object, object, int, int> OnAttack { get; set; }
        public Func<object, bool, int, int> OnAttacked { get; set; }
        public Func<object, int, int> OnDamaged { get; set; }
    }
}