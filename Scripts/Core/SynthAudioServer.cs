using Godot;
using PursualRPG.Scripts.Audio;

namespace PursualRPG.Scripts.Core
{
    public partial class SynthAudioServer : Node
    {
        public static SynthAudioServer Instance { get; private set; }
        public float MasterVolume { get; private set; } = 1.0f;
        public bool IsMuted { get; private set; } = false;

        private AudioStreamPlayer _sfxPlayer;
        private AudioStreamPlayer _musicPlayer;

        private AudioStreamWav _retroWooshCache;
        private AudioStreamWav _femaleOhhCache;
        private AudioStreamWav _dungeonSynthCache;

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

        public void PlayRetroWoosh()
        {
            _retroWooshCache ??= ProceduralAudio.GenerateRetroWoosh();
            _sfxPlayer.Stream = _retroWooshCache;
            _sfxPlayer.Play();
        }

        public void PlayFemaleOhhStab()
        {
            _femaleOhhCache ??= ProceduralAudio.GenerateFemaleOhhStab();
            _sfxPlayer.Stream = _femaleOhhCache;
            _sfxPlayer.Play();
        }

        public void PlayDungeonSynthTheme()
        {
            if (_musicPlayer.Playing && _musicPlayer.Stream != null) return;
            _dungeonSynthCache ??= ProceduralAudio.GenerateDungeonSynthTheme();
            _musicPlayer.Stream = _dungeonSynthCache;
            _musicPlayer.Play();
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