// Scripts/Core/SynthAudioServer.cs
using Godot;

namespace PursualRPG.Scripts.Core
{
    public partial class SynthAudioServer : Node
    {
        public static SynthAudioServer Instance { get; private set; }
        public float MasterVolume { get; private set; } = 1.0f;
        public bool IsMuted { get; private set; } = false;

        private AudioStreamPlayer _sfxPlayer;
        private AudioStreamPlayer _musicPlayer;

        public override void _Ready()
        {
            Instance = this;
            ProcessMode = ProcessModeEnum.Always;

            _sfxPlayer = new AudioStreamPlayer { Bus = "Master" };
            AddChild(_sfxPlayer);

            _musicPlayer = new AudioStreamPlayer { Bus = "Master" };
            AddChild(_musicPlayer);
        }

        public void PlayButtonClick()
        {
            var stream = GD.Load<AudioStream>("res://Assets/Sfx/button_click.mp3");
            if (stream != null) { _sfxPlayer.Stream = stream; _sfxPlayer.Play(); }
        }

        public void PlayButtonHover()
        {
            var stream = GD.Load<AudioStream>("res://Assets/Sfx/button_hover.mp3");
            if (stream != null) { _sfxPlayer.Stream = stream; _sfxPlayer.Play(); }
        }

        public void SetMasterVolume(float volumeLinear)
        {
            MasterVolume = volumeLinear;
            int busIndex = AudioServer.GetBusIndex("Master");
            float db = volumeLinear <= 0f ? -80f : Mathf.LinearToDb(volumeLinear);
            AudioServer.SetBusVolumeDb(busIndex, db);
        }

        public void SetMuted(bool muted)
        {
            IsMuted = muted;
            int busIndex = AudioServer.GetBusIndex("Master");
            AudioServer.SetBusMute(busIndex, muted);
        }
    }
}