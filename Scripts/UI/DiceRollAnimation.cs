using Godot;
using System.Collections.Generic;
using PursualRPG.Scripts.Core;

namespace PursualRPG.Scripts.UI
{
    public partial class DiceRollAnimation : Control
    {
        [Export] public float DurationMs { get; set; } = 1500f;
        private double _startTime;
        private double _spawnTime;
        private bool _isRolling = true;
        private bool _isVisible = true;

        public Label ResultLabel { get; private set; }
        public TextureRect DiceTextureRect { get; private set; }

        private readonly List<(Texture2D texture, int delay)> _frames = new();
        private int _currentFrameIdx = 0;
        private double _lastFrameUpdate;

        private readonly (string filename, int delay)[] _frameConfig = new[]
        {
            ("sptD10a.jpg", 132),
            ("sptD10b.jpg", 132),
            ("sptD10c.jpg", 66),
            ("sptD10d.jpg", 132),
            ("sptD10e.jpg", 66),
            ("sptD10f.jpg", 132),
        };

        public override void _Ready()
        {
            _startTime = Time.GetTicksMsec();
            _spawnTime = _startTime;
            _lastFrameUpdate = _startTime;

            SetAnchorsPreset(LayoutPreset.Center);

            if (DiceTextureRect == null)
            {
                DiceTextureRect = new TextureRect
                {
                    CustomMinimumSize = new Vector2(80, 80),
                    ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize
                };
                AddChild(DiceTextureRect);
            }

            if (ResultLabel == null)
            {
                ResultLabel = new Label();
                FontService.ApplyFont(ResultLabel, FontType.SecondaryButton, 28);
                ResultLabel.Position = new Vector2(0, 90);
                AddChild(ResultLabel);
            }

            LoadFrames();
        }

        private void LoadFrames()
        {
            string basePath = "res://Assets/Images/dice";
            foreach (var (filename, delay) in _frameConfig)
            {
                string path = $"{basePath}/{filename}";
                var tex = GD.Load<Texture2D>(path);
                if (tex != null)
                {
                    _frames.Add((tex, delay));
                }
                else
                {
                    var fallback = new PlaceholderTexture2D { Size = new Vector2I(80, 80) };
                    _frames.Add((fallback, delay));
                }
            }
        }

        public void SetResult(string resultString)
        {
            if (ResultLabel != null)
            {
                ResultLabel.Text = $"Roll: {resultString}";
                ResultLabel.Visible = true;
            }
        }

        public void HideAfter3Seconds()
        {
            _spawnTime = Time.GetTicksMsec();
            _isVisible = true;
            Visible = true;
        }

        public override void _Process(double delta)
        {
            if (!_isVisible || _frames.Count == 0) return;

            double now = Time.GetTicksMsec();
            if (now - _spawnTime >= 3000)
            {
                _isVisible = false;
                Visible = false;
                QueueFree();
                return;
            }

            if (_isRolling)
            {
                if (now - _startTime >= DurationMs)
                {
                    _isRolling = false;
                    if (DiceTextureRect != null && _frames.Count > 0)
                        DiceTextureRect.Texture = _frames[0].texture;
                    return;
                }

                var currentConfig = _frames[_currentFrameIdx];
                if (now - _lastFrameUpdate >= currentConfig.delay)
                {
                    _currentFrameIdx = (_currentFrameIdx + 1) % _frames.Count;
                    _lastFrameUpdate = now;
                }

                if (DiceTextureRect != null)
                {
                    DiceTextureRect.Texture = _frames[_currentFrameIdx].texture;
                }
            }
        }
    }
}