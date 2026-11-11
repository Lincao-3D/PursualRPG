using Godot;
using System;

namespace PursualRPG.Scripts.Core
{
    /// <summary>
    /// Engine/UI localization helper wrapping Godot's TranslationServer.
    /// Provides strict separation for Engine/UI strings.
    /// In debug builds, missing keys return "⟦key⟧" to easily spot untranslated keys.
    /// </summary>
    public static class L
    {
        public static string T(string key, params object[] args)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;

            string translated = TranslationServer.Translate(key);

            // In debug mode, if translation is missing (Godot returns the key verbatim), show missing brackets
            if (OS.IsDebugBuild())
            {
                if (string.IsNullOrEmpty(translated) || translated == key)
                {
                    translated = $"⟦{key}⟧";
                }
            }
            else
            {
                if (string.IsNullOrEmpty(translated))
                {
                    translated = key;
                }
            }

            if (args != null && args.Length > 0)
            {
                try
                {
                    return string.Format(translated, args);
                }
                catch (FormatException ex)
                {
                    GD.PrintErr($"[L.T] FormatException for key '{key}' with text '{translated}': {ex.Message}");
                    return translated;
                }
            }

            return translated;
        }

        public static void SetLocale(string locale)
        {
            if (!string.IsNullOrWhiteSpace(locale))
            {
                TranslationServer.SetLocale(locale);
            }
        }

        public static string GetLocale()
        {
            return TranslationServer.GetLocale();
        }
    }
}
