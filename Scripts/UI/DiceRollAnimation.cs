using Godot;

namespace PursualRPG.Scripts.UI
{
    public partial class DiceRollAnimation : Control
    {
        [Export] public float DurationMs { get; set; } = 1500f;
        private double _startTime;
        private double _spawnTime;
        private bool _isRolling = true;
        private bool _isVisible = true;
        private string _resultText = "";

        [Export] public Label ResultLabel;

        public override void _Ready()
        {
            _startTime = Time.GetTicksMsec();
            _spawnTime = _startTime;
        }

        public void SetResult(string resultString)
        {
            _resultText = $"Roll result: {resultString}";
            if (ResultLabel != null) ResultLabel.Text = _resultText;
        }

        public void HideAfter3Seconds()
        {
            _spawnTime = Time.GetTicksMsec();
            _isVisible = true;
        }

        public override void _Process(double delta)
        {
            double now = Time.GetTicksMsec();
            if (now - _spawnTime >= 3000)
            {
                _isVisible = false;
                Visible = false;
                QueueFree();
                return;
            }

            if (_isRolling && now - _startTime >= DurationMs)
            {
                _isRolling = false;
            }
        }
    }
}