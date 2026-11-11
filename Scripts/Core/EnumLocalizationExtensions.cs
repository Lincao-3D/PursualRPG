using System;
using PursualRPG.Scripts.Domain;

namespace PursualRPG.Scripts.Core
{
    /// <summary>
    /// Presentation-edge localization extensions for domain enums.
    /// Keeps domain logic and models in English while localizing solely at the display boundary.
    /// </summary>
    public static class EnumLocalizationExtensions
    {
        public static string ToDisplayName(this CharacterAttrib attrib)
        {
            return L.T($"enum.attribute.{attrib.ToString().ToLowerInvariant()}");
        }

        public static string ToDisplayName(this CharacterRace race)
        {
            return L.T($"enum.race.{race.ToString().ToLowerInvariant()}");
        }

        public static string ToDisplayName(this CharacterClassEnum cEnum)
        {
            return L.T($"enum.class.{cEnum.ToString().ToLowerInvariant()}");
        }

        public static string ToDisplayName(this CharacterExpertise expertise)
        {
            return L.T($"enum.expertise.{expertise.ToString().ToLowerInvariant()}");
        }

        public static string ToDisplayName(this OutcomeType outcome)
        {
            return outcome switch
            {
                OutcomeType.Victory => L.T("msg.combat.victory"),
                OutcomeType.PlayerDied => L.T("msg.combat.defeat"),
                OutcomeType.Fled => L.T("msg.combat.escaped"),
                _ => outcome.ToString()
            };
        }
    }
}
