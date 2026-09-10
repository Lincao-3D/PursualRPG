using Godot;
using System.Collections.Generic;

namespace PursualRPG.Scripts.Core
{
    public enum FontType
    {
        DefaultMenu,
        SecondaryButton,
        ChatReading
    }

    public static class FontService
    {
        private static readonly Dictionary<FontType, Font> _fontCache = new();
        private static bool _isInitialized = false;

        private static void Initialize()
        {
            if (_isInitialized) return;

            _fontCache[FontType.DefaultMenu] = GD.Load<Font>("res://Assets/Fonts/font.ttf");
            _fontCache[FontType.SecondaryButton] = GD.Load<Font>("res://Assets/Fonts/font2.ttf");
            _fontCache[FontType.ChatReading] = GD.Load<Font>("res://Assets/Fonts/font1.otf");

            _isInitialized = true;
        }

        public static void ApplyFont(Node node, FontType type, int sizeOverride = 0)
        {
            Initialize();
            if (!_fontCache.TryGetValue(type, out var font) || font == null) return;

            if (node is Control control)
            {
                control.AddThemeFontOverride("font", font);
                control.AddThemeFontOverride("normal_font", font);
                control.AddThemeFontOverride("bold_font", font);

                if (sizeOverride > 0)
                {
                    control.AddThemeFontSizeOverride("font_size", sizeOverride);
                    control.AddThemeFontSizeOverride("normal_font_size", sizeOverride);
                    control.AddThemeFontSizeOverride("bold_font_size", sizeOverride);
                }
            }
            else if (node is Window window)
            {
                window.AddThemeFontOverride("font", font);
                if (sizeOverride > 0)
                {
                    window.AddThemeFontSizeOverride("font_size", sizeOverride);
                }
            }
        }
    }

    public static class UIExtensions
    {
        public static void BindAudioAndFont(this Button btn, FontType fontType = FontType.SecondaryButton, int size = 0)
        {
            FontService.ApplyFont(btn, fontType, size);
            btn.MouseEntered += () => SynthAudioServer.Instance.PlayButtonHover();
            btn.Pressed += () => SynthAudioServer.Instance.PlayButtonClick();
        }
    }
}