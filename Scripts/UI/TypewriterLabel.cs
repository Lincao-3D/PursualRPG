using Godot;

namespace PursualRPG.Scripts.UI
{
    public partial class TypewriterLabel : RichTextLabel
    {
        [Export] public float SpeedMs { get; set; } = 25f;
        private string _fullText = "";
        private int _charIndex = 0;
        private double _lastUpdate = 0;
        public bool IsComplete => _charIndex >= _fullText.Length;

        public void StartTyping(string text)
        {
            _fullText = text;
            _charIndex = 0;
            Text = "";
        }

        public string RevealAll()
        {
            _charIndex = _fullText.Length;
            Text = _fullText;
            return _fullText;
        }

        public override void _Process(double delta)
        {
            if (IsComplete) return;

            double now = Time.GetTicksMsec();
            if (now - _lastUpdate >= SpeedMs)
            {
                if (_charIndex < _fullText.Length)
                {
                    Text += _fullText[_charIndex];
                    _charIndex++;
                }
                _lastUpdate = now;
            }
        }
    }
}