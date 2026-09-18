using System;

namespace PursualRPG.Scripts.Domain
{
    public enum EffectEnum { Aiming, Reckless, Stunned, DivineSmith }

    public class Effect
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Duration { get; set; } = 1;
        public bool Positive { get; set; }
        public bool Stackable { get; set; } = true;
        public bool SkipTurn { get; set; } = false;

        [Newtonsoft.Json.JsonIgnore]
        public Action<object>? OnApply { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public Action<object>? OnUnapply { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public Func<object, object, int, int>? OnAttack { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public Func<object, bool, int, int>? OnAttacked { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public Func<object, int, int>? OnDamaged { get; set; }
    }
}