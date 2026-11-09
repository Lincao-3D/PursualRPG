using Godot;

namespace PursualRPG.Scripts.Core
{
    public partial class BackgroundService : VideoStreamPlayer
    {
        [Export] public string DesktopVideoPath = "res://Assets/Videos/desktop_splash.ogv";
        [Export] public string MobileVideoPath = "res://Assets/Videos/mobile_splash.ogv";

        public override void _Ready()
        {
            ProcessMode = ProcessModeEnum.Always;

            bool isMobile = OS.HasFeature("mobile")
                || OS.HasFeature("android")
                || OS.HasFeature("ios");
            string videoPath = isMobile ? MobileVideoPath : DesktopVideoPath;
            bool isPortrait = isMobile;

            if (!ResourceLoader.Exists(videoPath))
            {
                string fallbackPath = isMobile ? DesktopVideoPath : MobileVideoPath;
                if (!ResourceLoader.Exists(fallbackPath))
                {
                    GD.PushError($"Background video not found: '{videoPath}' or '{fallbackPath}'.");
                    return;
                }

                videoPath = fallbackPath;
                isPortrait = fallbackPath == MobileVideoPath;
            }

            var videoStream = GD.Load<VideoStream>(videoPath);
            if (videoStream == null)
            {
                GD.PushError($"Unable to load background video: '{videoPath}'.");
                return;
            }

            Stream = videoStream;
            Finished += OnVideoFinished;
            ConfigureAspectContainer(isPortrait);
            Play();
        }

        private void ConfigureAspectContainer(bool isPortrait)
        {
            for (Node ancestor = GetParent(); ancestor != null; ancestor = ancestor.GetParent())
            {
                if (ancestor is AspectRatioContainer aspectContainer)
                {
                    aspectContainer.Ratio = isPortrait ? (9f / 16f) : (16f / 9f);
                    aspectContainer.StretchMode = AspectRatioContainer.StretchModeEnum.Cover;
                    return;
                }
            }

            GD.PushWarning("Background video has no parent AspectRatioContainer.");
        }

        private void OnVideoFinished()
        {
            Play();
        }

        public void SetActive(bool active)
        {
            if (Stream == null)
                return;

            Visible = active;
            if (active)
            {
                if (!IsPlaying())
                    Play();
            }
            else
            {
                Stop();
            }
        }
    }
}
