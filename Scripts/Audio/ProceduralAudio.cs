using Godot;
using System;

namespace PursualRPG.Scripts.Audio
{
    public static class ProceduralAudio
    {
        private const int SampleRate = 44100;

        public static AudioStreamWav GenerateRetroWoosh()
        {
            float duration = 0.3f;
            int nSamples = (int)(SampleRate * duration);
            byte[] data = new byte[nSamples * 4];

            for (int i = 0; i < nSamples; i++)
            {
                double t = (double)i / SampleRate;
                double freq = 800.0 - ((800.0 - 200.0) * ((double)i / nSamples));
                double wave = 0.5 * Math.Sin(2 * Math.PI * freq * t);

                short sample = (short)Math.Clamp(wave * 32767, -32768, 32767);
                PackStereoSample(data, i * 4, sample);
            }

            return CreateStream(data, loop: false);
        }

        public static AudioStreamWav GenerateFemaleOhhStab()
        {
            double duration = 0.85;
            int nSamples = (int)(SampleRate * duration);
            byte[] data = new byte[nSamples * 4];
            double noteFreq = 523.25; // Soprano C5

            for (int i = 0; i < nSamples; i++)
            {
                double t = (double)i / SampleRate;
                double scoop = Math.Exp(-t * 50) * -0.04;
                double vibrato = 0.012 * Math.Sin(2 * Math.PI * 6.0 * t);
                double freq = noteFreq * (1.0 + scoop + vibrato);
                double phase = freq * t;
                
                double voice = (1.00 * Math.Sin(2 * Math.PI * phase) +
                                0.80 * Math.Sin(2 * Math.PI * phase * 2) +
                                0.25 * Math.Sin(2 * Math.PI * phase * 3) +
                                0.45 * Math.Sin(2 * Math.PI * phase * 4) +
                                0.15 * Math.Sin(2 * Math.PI * phase * 5)) / 2.65;

                double breath = 1.0 + 0.10 * Math.Sin(2 * Math.PI * 4.5 * t);
                voice *= breath;

                double env = 1.0;
                if (t < 0.015) env = Math.Sqrt(t / 0.015);
                else if (t < 0.315) env = 1.0 - 0.25 * ((t - 0.015) / 0.30);
                else env = 0.75 * (1.0 - ((t - 0.315) / 0.535));

                short sample = (short)Math.Clamp(voice * env * 32767 * 0.6, -32768, 32767);
                PackStereoSample(data, i * 4, sample);
            }

            return CreateStream(data, loop: false);
        }

        public static AudioStreamWav GenerateDungeonSynthTheme()
        {
            double bpm = 72.0;
            double beatDuration = 60.0 / bpm;
            double passDuration = beatDuration * 8; // 8 beats per pass
            double totalDuration = passDuration * 4; // 4 passes total
            int nSamples = (int)(SampleRate * totalDuration);
            byte[] data = new byte[nSamples * 4];

            double[] frequencies = { 220.0, 261.63, 329.63, 392.0 }; // A minor arpeggio
            int choirSamples = (int)(SampleRate * 0.85);
            int choirOffset = (int)(SampleRate * passDuration * 3.0); // End of 3rd pass

            for (int i = 0; i < nSamples; i++)
            {
                double t = (double)i / SampleRate;
                
                // Base Synth Arpeggio
                int noteIndex = ((int)(t / (beatDuration / 2.0))) % frequencies.Length;
                double freq = frequencies[noteIndex];
                double wave = 0.3 * Math.Sin(2 * Math.PI * freq * t) + 0.15 * (Math.Sin(2 * Math.PI * freq * 2 * t) > 0 ? 1 : -1);

                // Add Choir Vocal Stab specifically at the end of the 3rd pass
                if (i >= choirOffset && i < choirOffset + choirSamples)
                {
                    double choirT = (double)(i - choirOffset) / SampleRate;
                    double choirFreq = 523.25; // Soprano C5
                    double choirWave = 0.4 * Math.Sin(2 * Math.PI * choirFreq * choirT);
                    wave += choirWave;
                }

                short sample = (short)Math.Clamp(wave * 32767 * 0.5, -32768, 32767);
                PackStereoSample(data, i * 4, sample);
            }

            return CreateStream(data, loop: true);
        }

        private static void PackStereoSample(byte[] data, int offset, short sample)
        {
            byte[] bytes = BitConverter.GetBytes(sample);
            data[offset] = bytes[0];
            data[offset + 1] = bytes[1];
            data[offset + 2] = bytes[0];
            data[offset + 3] = bytes[1];
        }

        private static AudioStreamWav CreateStream(byte[] data, bool loop = false)
        {
            return new AudioStreamWav
            {
                Format = AudioStreamWav.FormatEnum.Format16Bits,
                MixRate = SampleRate,
                Stereo = true,
                Data = data,
                LoopMode = loop ? AudioStreamWav.LoopModeEnum.Forward : AudioStreamWav.LoopModeEnum.Disabled
            };
        }
    }
}