using Godot;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PursualRPG.Scripts.Core;

namespace PursualRPG.Scripts.UI
{
    public partial class ASCIIBgPlayer : Node
    {
        [Export] public float BaseFps { get; set; } = 9.0f;
        [Export] public float Slowdown { get; set; } = 2.0f;

        private float _effectiveFps;
        private float _frameDelay;
        private List<string> _frameContents = new();
        private int _currentFrameIdx = 0;
        private double _lastFrameTime = 0.0;
        
        private RichTextLabel _displayLabel;

        public override void _Ready()
        {
            _effectiveFps = BaseFps / Mathf.Max(1.0f, Slowdown);
            _frameDelay = 1.0f / _effectiveFps;
            LoadFramesFromAssets();

            _displayLabel = GetNodeOrNull<RichTextLabel>("../ASCIIDisplay");
            if (_displayLabel != null)
            {
                FontService.ApplyFont(_displayLabel, FontType.ChatReading);
                AdjustFontSizeToViewport();
            }

            // Subscribe to the root window size changed event in Godot 4
            GetTree().Root.SizeChanged += OnWindowResized;
        }

        public override void _ExitTree()
        {
            // Unsubscribe to avoid memory leaks when changing scenes
            if (GetTree()?.Root != null)
            {
                GetTree().Root.SizeChanged -= OnWindowResized;
            }
        }

        private void OnWindowResized()
        {
            var viewport = GetViewport();
            if (viewport != null)
            {
                var subViewport = GetNodeOrNull<SubViewport>("..");
                if (subViewport != null)
                {
                    subViewport.Size = (Vector2I)viewport.GetVisibleRect().Size;
                }
            }
            AdjustFontSizeToViewport();
        }

        private void AdjustFontSizeToViewport()
        {
        if (_displayLabel == null) return;
        var viewportSize = GetViewport().GetVisibleRect().Size;
        
        // Drastically reduce divisor to shrink the text to fit 100+ column ASCII art
        int adaptiveSize = Mathf.Clamp((int)(viewportSize.X / 120.0f), 6, 12);
        
        _displayLabel.AddThemeFontSizeOverride("normal_font_size", adaptiveSize);
        
        // Force tighter line heights to prevent vertical overflow
        _displayLabel.AddThemeConstantOverride("line_separation", -3);
        _displayLabel.BbcodeEnabled = true;
        }

        public override void _Process(double delta)
        {
            if (_displayLabel != null && _frameContents.Count > 0)
            {
                _displayLabel.Text = GetCurrentFrameText();
            }
        }

        public bool LoadFramesFromAssets()
        {
            string targetDir = ProjectSettings.GlobalizePath("res://Assets/ASCIItxtFrames");
            if (!Directory.Exists(targetDir)) return false;

            var frameFiles = Directory.GetFiles(targetDir, "frame_*.txt").OrderBy(f => f).ToList();
            if (frameFiles.Count == 0) return false;

            _frameContents.Clear();
            foreach (var file in frameFiles)
            {
                _frameContents.Add(File.ReadAllText(file));
            }
            return _frameContents.Count > 0;
        }

        public string GetCurrentFrameText()
        {
        if (_frameContents.Count == 0) return string.Empty;
        double now = Time.GetTicksMsec() / 1000.0;
        if (now - _lastFrameTime >= _frameDelay)
        {
            _currentFrameIdx = (_currentFrameIdx + 1) % _frameContents.Count;
            _lastFrameTime = now;
        }
        // Wrap in BBCode center tags to fix the top-left origin issue
        return $"[center]{_frameContents[_currentFrameIdx]}[/center]";
        }
    }
}