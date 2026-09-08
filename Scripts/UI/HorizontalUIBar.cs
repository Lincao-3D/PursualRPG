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
        private Tween _borderPulseTween;

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
            _borderPulseTween = TweenManager.PulseModulate(this, Colors.Gold, Colors.White, 0.8f);

            RunCinematicSequence(screenW, y);
        }

        private async void RunCinematicSequence(float screenW, float y)
        {
            var tweenIn = CreateTween();
            tweenIn.TweenProperty(this, "position", new Vector2(0, y), 0.4f).SetEase(Tween.EaseType.Out);
            await ToSignal(tweenIn, Tween.SignalName.Finished);

            await ToSignal(GetTree().CreateTimer(HoldDurationMs / 1000f), SceneTreeTimer.SignalName.Timeout);

            var tweenOut = CreateTween();
            tweenOut.TweenProperty(this, "position", new Vector2(-screenW, y), 0.4f).SetEase(Tween.EaseType.In);
            await ToSignal(tweenOut, Tween.SignalName.Finished);

            _borderPulseTween?.Kill();
            IsDone = true;
            QueueFree();
        }
    }
}