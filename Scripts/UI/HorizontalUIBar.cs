using Godot;
using PursualRPG.Scripts.Core;

namespace PursualRPG.Scripts.UI
{
    public partial class HorizontalUIBar : Control
    {
        [Export] public float HoldDurationMs { get; set; } = 2000f;
        public bool IsDone { get; private set; } = false;

        private Label _textLabel;
        private ColorRect _topBorder;
        private ColorRect _bottomBorder;
        private Tween _topBorderPulseTween;
        private Tween _bottomBorderPulseTween;

        public void Initialize(string text, float holdDurationMs = 2000f)
        {
            HoldDurationMs = holdDurationMs;
            float screenW = GetViewportRect().Size.X;
            float height = 60f;
            float y = (GetViewportRect().Size.Y / 2f) - (height / 2f);

            CustomMinimumSize = new Vector2(screenW, height);
            Position = new Vector2(screenW, y);

            var bg = new ColorRect
            {
                Color = new Color(0, 0, 0, 0.85f),
                CustomMinimumSize = new Vector2(screenW, height)
            };
            AddChild(bg);

            _topBorder = new ColorRect { Color = Colors.Gold, CustomMinimumSize = new Vector2(screenW, 4) };
            AddChild(_topBorder);

            _bottomBorder = new ColorRect { Color = Colors.Gold, CustomMinimumSize = new Vector2(screenW, 4), Position = new Vector2(0, height - 4) };
            AddChild(_bottomBorder);

            _textLabel = new Label
            {
                Text = text,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            FontService.ApplyFont(_textLabel, FontType.SecondaryButton, 28);
            _textLabel.SetAnchorsPreset(LayoutPreset.FullRect);
            AddChild(_textLabel);

            SynthAudioServer.Instance?.PlayRetroWoosh();

            // P1.7: Isolate PulseModulate to _topBorder and _bottomBorder instead of the entire node (this)
            _topBorderPulseTween = TweenManager.PulseModulate(_topBorder, Colors.Gold, Colors.White, 0.8f);
            _bottomBorderPulseTween = TweenManager.PulseModulate(_bottomBorder, Colors.Gold, Colors.White, 0.8f);

            RunCinematicSequence(screenW, y);
        }

        private async void RunCinematicSequence(float screenW, float y)
        {
            Vector2 hiddenPos = new Vector2(screenW, y);
            Vector2 shownPos = new Vector2(0, y);
            float holdSeconds = HoldDurationMs / 1000f;

            // P1.6: Replace hand-rolled tween logic with TweenManager.SlideInHoldOut
            await TweenManager.SlideInHoldOut(this, hiddenPos, shownPos, holdSeconds, () =>
            {
                _topBorderPulseTween?.Kill();
                _bottomBorderPulseTween?.Kill();
                IsDone = true;
                QueueFree();
            });
        }
    }
}