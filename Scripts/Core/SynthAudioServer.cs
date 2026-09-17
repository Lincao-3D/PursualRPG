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
            _sfxPlayer.Play();
            _musicPlayer.Play();

            /*
            sugested generator:
            Instance = this;
            _player = new AudioStreamPlayer();
            var generator = new AudioStreamGenerator();
            generator.MixRate = 44100;
            generator.BufferLength = 0.5f;
            _player.Stream = generator;
            
            AddChild(_player);
            _player.Play(); // CRITICAL: Start the stream!

            and If playing a drone is intended during the menu, you can add a method public void SetDroneActive(bool active) that adjusts the synth's envelope/volume, and call SynthAudioServer.Instance.SetDroneActive(true); inside MainMenuScene.cs _Ready().
            */
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
            // Diagnostic (c before call): Check player state
            // GD.Print($"[Diagnostics] PlayDungeonSynthTheme called. _musicPlayer.Playing: {_musicPlayer.Playing}, Stream assigned: {_musicPlayer.Stream != null}");

            if (_musicPlayer.Playing && _musicPlayer.Stream != null) return;
            _dungeonSynthCache ??= ProceduralAudio.GenerateDungeonSynthTheme();
            _musicPlayer.Stream = _dungeonSynthCache;
            
            // Diagnostic (b): Verify Play() is reached
            // GD.Print("[Diagnostics] Reached _musicPlayer.Play() execution point.");
            _musicPlayer.Play();

            // Diagnostic (c after call): Check if audio stream successfully registered as playing
            // GD.Print($"[Diagnostics] Post-Play state -> _musicPlayer.Playing: {_musicPlayer.Playing}, Stream != null: {_musicPlayer.Stream != null}");
        }
        public void StopMusic()
        {
            if (_musicPlayer != null)
            {
                _musicPlayer.Stop();
                _musicPlayer.Stream = null;
            }
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