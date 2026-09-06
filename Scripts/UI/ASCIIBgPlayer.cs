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

            // Locate the sibling RichTextLabel in the SubViewport
            _displayLabel = GetNodeOrNull<RichTextLabel>("../ASCIIDisplay");
            
            // Apply centralized typography to ensure terminal-like ASCII mapping
            if (_displayLabel != null)
            {
                FontService.ApplyFont(_displayLabel, FontType.ChatReading);
            }
        }

        public override void _Process(double delta)
        {
            // Push the current frame to the UI continuously, decoupled from GameManager screen changes
            if (_displayLabel != null && _frameContents.Count > 0)
            {
                _displayLabel.Text = GetCurrentFrameText();
            }
        }

        public bool LoadFramesFromAssets()
        {
            string targetDir = ProjectSettings.GlobalizePath("res://Assets/ASCIItxtFrames");
            if (!Directory.Exists(targetDir))
            {
                GD.PrintErr($"[ASCII Player] Warning: Frame directory not found at {targetDir}");
                return false;
            }

            var frameFiles = Directory.GetFiles(targetDir, "frame_*.txt").OrderBy(f => f).ToList();
            if (frameFiles.Count == 0)
            {
                GD.PrintErr("[ASCII Player] Warning: No frame files found.");
                return false;
            }

            _frameContents.Clear();
            foreach (var file in frameFiles)
            {
                _frameContents.Add(File.ReadAllText(file));
            }

            GD.Print($"[ASCII Player] Successfully loaded {_frameContents.Count} frames at {_effectiveFps:F2} FPS.");
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

            return _frameContents[_currentFrameIdx];
        }
    }
}