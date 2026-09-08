using Godot;
using PursualRPG.Scripts.Audio;
using PursualRPG.Scripts.Core;

namespace PursualRPG.Scripts.UI
{
    public partial class HorizontalUIBar : Control
    {
        [Export] public float HoldDurationMs { get; set; } = 2000f;
        [Export] public float Speed { get; set; } = 40f;

        private string _text;
        private float _screenW;
        private float _height = 60f;
        private float _y;
        private float _x;
        private string _state = "ENTER";
        private double _holdStartTime;
        private int _flashTimer = 0;
        private bool _playedSound = false;
        public bool IsDone { get; private set; } = false;

        private Label _textLabel;
        private AudioStreamPlayer _audioPlayer;

        public HorizontalUIBar(float screenW, float screenH, string text, float holdDurationMs = 2000f)
        {
            _screenW = screenW;
            _text = text;
            HoldDurationMs = holdDurationMs;
            _height = 60f;
            _y = (screenH / 2f) - (_height / 2f);
            _x = screenW;
        }

        public override void _Ready()
        {
            CustomMinimumSize = new Vector2(_screenW, _height);
            Position = new Vector2(_x, _y);

            var bg = new ColorRect
            {
                Color = new Color(0, 0, 0, 0.78f),
                CustomMinimumSize = new Vector2(_screenW, _height)
            };
            AddChild(bg);

            _textLabel = new Label
            {
                Text = _text,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            FontService.ApplyFont(_textLabel, FontType.SecondaryButton, 28);
            _textLabel.SetAnchorsPreset(LayoutPreset.FullRect);
            AddChild(_textLabel);

            _audioPlayer = new AudioStreamPlayer();
            AddChild(_audioPlayer);
        }

        public override void _Process(double delta)
        {
            if (IsDone) return;

            if (_state == "ENTER")
            {
                if (!_playedSound)
                {
                    _audioPlayer.Stream = ProceduralAudio.GenerateRetroWoosh();
                    _audioPlayer.Play();
                    _playedSound = true;
                }

                _x -= Speed;
                if (_x <= 0)
                {
                    _x = 0;
                    _state = "HOLD";
                    _holdStartTime = Time.GetTicksMsec();
                }
            }
            else if (_state == "HOLD")
            {
                if (Time.GetTicksMsec() - _holdStartTime > HoldDurationMs)
                {
                    _state = "EXIT";
                }
            }
            else if (_state == "EXIT")
            {
                _x -= Speed;
                if (_x < -_screenW)
                {
                    IsDone = true;
                    QueueFree();
                }
            }

            Position = new Vector2(_x, _y);
            _flashTimer++;
        }
    }
}