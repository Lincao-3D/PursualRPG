using Godot;

namespace PursualRPG.Scripts.Core
{
    public enum FontRole
    {
        Title,
        Heading,
        Body,
        Caption
    }

    public static class Ui
    {
        public const float MinimumTouchTarget = 56f;

        public static Rect2 GetSafeArea(Control control)
        {
            Vector2 viewportSize = control.GetViewportRect().Size;
            Vector2I windowSize = DisplayServer.WindowGetSize();
            if (windowSize.X <= 0 || windowSize.Y <= 0)
                return new Rect2(Vector2.Zero, viewportSize);

            Rect2I safeArea = DisplayServer.GetDisplaySafeArea();
            if (safeArea.Size.X <= 0 || safeArea.Size.Y <= 0)
                return new Rect2(Vector2.Zero, viewportSize);

            Vector2I windowPosition = DisplayServer.WindowGetPosition();
            Vector2I safePosition = safeArea.Position - windowPosition;
            Vector2I safeEnd = safeArea.End - windowPosition;
            safePosition.X = Mathf.Clamp(safePosition.X, 0, windowSize.X);
            safePosition.Y = Mathf.Clamp(safePosition.Y, 0, windowSize.Y);
            safeEnd.X = Mathf.Clamp(safeEnd.X, safePosition.X, windowSize.X);
            safeEnd.Y = Mathf.Clamp(safeEnd.Y, safePosition.Y, windowSize.Y);

            Vector2 scale = viewportSize / new Vector2(windowSize.X, windowSize.Y);
            return new Rect2(
                new Vector2(safePosition.X, safePosition.Y) * scale,
                new Vector2(safeEnd.X - safePosition.X, safeEnd.Y - safePosition.Y) * scale);
        }

        public static void ApplySafeArea(Control control)
        {
            Rect2 safeArea = GetSafeArea(control);
            Vector2 viewportSize = control.GetViewportRect().Size;
            control.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            control.OffsetLeft = safeArea.Position.X;
            control.OffsetTop = safeArea.Position.Y;
            control.OffsetRight = safeArea.End.X - viewportSize.X;
            control.OffsetBottom = safeArea.End.Y - viewportSize.Y;
        }

        public static void ApplyVirtualKeyboardInset(Control control, int keyboardHeightPixels, float bottomInset = 0f)
        {
            Rect2 safeArea = GetSafeArea(control);
            Vector2 viewportSize = control.GetViewportRect().Size;
            Vector2I windowSize = DisplayServer.WindowGetSize();
            float scaleY = windowSize.Y > 0 ? viewportSize.Y / windowSize.Y : 1f;
            float keyboardHeight = keyboardHeightPixels * scaleY;

            control.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            control.OffsetLeft = safeArea.Position.X;
            control.OffsetTop = safeArea.Position.Y;
            control.OffsetRight = safeArea.End.X - viewportSize.X;
            control.OffsetBottom = safeArea.End.Y - viewportSize.Y - keyboardHeight - bottomInset;
        }

        public static void ApplyFontRole(Control control, FontRole role)
        {
            int size = role switch
            {
                FontRole.Title => 30,
                FontRole.Heading => 24,
                FontRole.Body => 18,
                FontRole.Caption => 14,
                _ => 18
            };
            control.AddThemeFontSizeOverride("font_size", size);
        }

        public static void EnforceTouchTarget(Control control, Vector2? minimumSize = null)
        {
            Vector2 minimum = minimumSize ?? new Vector2(MinimumTouchTarget, MinimumTouchTarget);
            control.CustomMinimumSize = new Vector2(
                Mathf.Max(control.CustomMinimumSize.X, minimum.X),
                Mathf.Max(control.CustomMinimumSize.Y, minimum.Y));
        }

        public static void EnforceTouchTargets(Node root)
        {
            foreach (Node child in root.GetChildren())
            {
                if (child is Button button)
                    EnforceTouchTarget(button);

                EnforceTouchTargets(child);
            }
        }
    }
}
