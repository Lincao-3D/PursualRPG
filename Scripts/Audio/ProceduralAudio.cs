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
            byte[] data = new byte[nSamples * 4]; // 16-bit Stereo

            for (int i = 0; i < nSamples; i++)
            {
                double t = (double)i / SampleRate;
                double freq = 800.0 - ((800.0 - 200.0) * ((double)i / nSamples));
                double wave = 0.5 * Math.Sin(2 * Math.PI * freq * t);

                short sample = (short)Math.Clamp(wave * 32767, -32768, 32767);
                PackStereoSample(data, i * 4, sample);
            }

            return CreateStream(data);
        }

        public static AudioStreamWav GenerateFemaleOhhStab()
        {
            double duration = 0.85;
            int nSamples = (int)(SampleRate * duration);
            byte[] data = new byte[nSamples * 4];
            double noteFreq = 523.25;

            for (int i = 0; i < nSamples; i++)
            {
                double t = (double)i / SampleRate;
                double scoop = Math.Exp(-t * 50) * -0.04;
                double vibrato = 0.012 * Math.Sin(2 * Math.PI * 6.0 * t);
                double freq = noteFreq * (1.0 + scoop + vibrato);
                
                double phase = freq * t; // Simplified phase
                
                // Formants
                double voice = (1.00 * Math.Sin(2 * Math.PI * phase) +
                                0.80 * Math.Sin(2 * Math.PI * phase * 2) +
                                0.25 * Math.Sin(2 * Math.PI * phase * 3) +
                                0.45 * Math.Sin(2 * Math.PI * phase * 4) +
                                0.15 * Math.Sin(2 * Math.PI * phase * 5)) / 2.65;

                double breath = 1.0 + 0.10 * Math.Sin(2 * Math.PI * 4.5 * t);
                voice *= breath;

                // Envelope
                double env = 1.0;
                if (t < 0.015) env = Math.Sqrt(t / 0.015);
                else if (t < 0.315) env = 1.0 - 0.25 * ((t - 0.015) / 0.30);
                else env = 0.75 * (1.0 - ((t - 0.315) / 0.535));

                short sample = (short)Math.Clamp(voice * env * 32767 * 0.6, -32768, 32767);
                PackStereoSample(data, i * 4, sample);
            }

            return CreateStream(data);
        }

        private static void PackStereoSample(byte[] data, int offset, short sample)
        {
            byte[] bytes = BitConverter.GetBytes(sample);
            data[offset] = bytes[0];     // L
            data[offset + 1] = bytes[1]; // L
            data[offset + 2] = bytes[0]; // R
            data[offset + 3] = bytes[1]; // R
        }

        private static AudioStreamWav CreateStream(byte[] data)
        {
            return new AudioStreamWav
            {
                Format = AudioStreamWav.FormatEnum.Format16Bits,
                MixRate = SampleRate,
                Stereo = true,
                Data = data
            };
        }
    }
}