using System;
using UnityEngine;

namespace JellyDrop.Audio
{
    public enum Wave { Square, Pulse25, Triangle, Sine, Saw, Noise }

    /// <summary>
    /// Small chiptune synthesiser. Writes notes into float buffers that become AudioClips,
    /// so the game ships with music and sound effects without any audio files.
    /// </summary>
    public static class Synth
    {
        public const int SampleRate = 22050;

        public static float MidiToFreq(float note) => 440f * Mathf.Pow(2f, (note - 69f) / 12f);

        public static int Samples(float seconds) => Mathf.Max(1, (int)(seconds * SampleRate));

        public static float Osc(Wave wave, float phase, ref uint noiseState)
        {
            phase -= Mathf.Floor(phase);
            switch (wave)
            {
                case Wave.Square: return phase < 0.5f ? 1f : -1f;
                case Wave.Pulse25: return phase < 0.25f ? 1f : -1f;
                case Wave.Triangle: return 1f - 4f * Mathf.Abs(phase - 0.5f);
                case Wave.Sine: return Mathf.Sin(phase * 2f * Mathf.PI);
                case Wave.Saw: return 2f * phase - 1f;
                case Wave.Noise:
                    noiseState ^= noiseState << 13;
                    noiseState ^= noiseState >> 17;
                    noiseState ^= noiseState << 5;
                    return (noiseState & 0xFFFF) / 32767.5f - 1f;
                default: return 0f;
            }
        }

        /// <summary>
        /// Adds a note to <paramref name="buffer"/>. When <paramref name="wrap"/> is true, a tail that runs past
        /// the end continues at the start, which keeps looping music seamless.
        /// </summary>
        public static void Note(float[] buffer, float startSeconds, float duration, float freq, Wave wave, float volume,
            float attack = 0.005f, float decay = 0.08f, float sustain = 0.6f, float release = 0.08f,
            float vibrato = 0f, float slideTo = 0f, bool wrap = false)
        {
            int start = (int)(startSeconds * SampleRate);
            int noteLen = Samples(duration);
            int total = noteLen + Samples(release);
            float phase = 0f;
            uint noise = 2463534242u ^ (uint)(start * 7919);
            float a = Mathf.Max(1, Samples(attack));
            float d = Mathf.Max(1, Samples(decay));
            float r = Mathf.Max(1, Samples(release));

            for (int i = 0; i < total; i++)
            {
                int idx = start + i;
                if (idx >= buffer.Length)
                {
                    if (!wrap) break;
                    idx %= buffer.Length;
                }
                if (idx < 0) continue;

                float env;
                if (i < a) env = i / a;
                else if (i < a + d) env = 1f - (1f - sustain) * ((i - a) / d);
                else if (i < noteLen) env = sustain;
                else env = sustain * (1f - (i - noteLen) / r);
                if (env <= 0f) continue;

                float t = (float)i / total;
                float f = slideTo > 0f ? Mathf.Lerp(freq, slideTo, t) : freq;
                if (vibrato > 0f && i > a + d) f *= 1f + vibrato * Mathf.Sin(i * 2f * Mathf.PI * 5.5f / SampleRate);
                phase += f / SampleRate;

                buffer[idx] += Osc(wave, phase, ref noise) * env * volume;
            }
        }

        public static void Kick(float[] buffer, float startSeconds, float volume, bool wrap = false)
        {
            int start = (int)(startSeconds * SampleRate);
            int len = Samples(0.22f);
            float phase = 0f;
            for (int i = 0; i < len; i++)
            {
                int idx = start + i;
                if (idx >= buffer.Length) { if (!wrap) break; idx %= buffer.Length; }
                float t = (float)i / SampleRate;
                float f = 45f + 110f * Mathf.Exp(-t * 30f);
                phase += f / SampleRate;
                buffer[idx] += Mathf.Sin(phase * 2f * Mathf.PI) * Mathf.Exp(-t * 14f) * volume;
            }
        }

        public static void Snare(float[] buffer, float startSeconds, float volume, bool wrap = false)
        {
            int start = (int)(startSeconds * SampleRate);
            int len = Samples(0.18f);
            uint noise = 22222u + (uint)start;
            float phase = 0f;
            for (int i = 0; i < len; i++)
            {
                int idx = start + i;
                if (idx >= buffer.Length) { if (!wrap) break; idx %= buffer.Length; }
                float t = (float)i / SampleRate;
                phase += 190f / SampleRate;
                float tone = Mathf.Sin(phase * 2f * Mathf.PI) * Mathf.Exp(-t * 30f) * 0.5f;
                float n = Osc(Wave.Noise, 0f, ref noise) * Mathf.Exp(-t * 22f);
                buffer[idx] += (tone + n * 0.7f) * volume;
            }
        }

        public static void HiHat(float[] buffer, float startSeconds, float volume, bool open = false, bool wrap = false)
        {
            int start = (int)(startSeconds * SampleRate);
            int len = Samples(open ? 0.15f : 0.05f);
            uint noise = 777u + (uint)start * 3u;
            float prev = 0f;
            for (int i = 0; i < len; i++)
            {
                int idx = start + i;
                if (idx >= buffer.Length) { if (!wrap) break; idx %= buffer.Length; }
                float t = (float)i / SampleRate;
                float n = Osc(Wave.Noise, 0f, ref noise);
                float hp = n - prev; // crude high-pass
                prev = n;
                buffer[idx] += hp * Mathf.Exp(-t * (open ? 25f : 80f)) * volume * 0.5f;
            }
        }

        // ------------------------------------------------------------------ modern sounds

        /// <summary>
        /// Warm "supersaw" pad: several slightly detuned saws through a soft low-pass filter.
        /// <paramref name="cutoff"/> is 0..1 (how bright the filter is).
        /// </summary>
        public static void Pad(float[] buffer, float startSeconds, float duration, float freq, float volume,
            float cutoff = 0.12f, float attack = 0.35f, float release = 0.6f, int voices = 5, float detune = 0.012f, bool wrap = true)
        {
            int start = (int)(startSeconds * SampleRate);
            int noteLen = Samples(duration);
            int total = noteLen + Samples(release);
            float a = Mathf.Max(1, Samples(attack)), r = Mathf.Max(1, Samples(release));
            var phases = new float[voices];
            for (int v = 0; v < voices; v++) phases[v] = v * 0.37f;
            float lp = 0f, lp2 = 0f;
            for (int i = 0; i < total; i++)
            {
                int idx = start + i;
                if (idx >= buffer.Length) { if (!wrap) break; idx %= buffer.Length; }
                float env = i < a ? i / a : i < noteLen ? 1f : 1f - (i - noteLen) / r;
                if (env <= 0f) continue;
                float sum = 0f;
                for (int v = 0; v < voices; v++)
                {
                    float spread = voices == 1 ? 0f : (v / (float)(voices - 1) - 0.5f) * 2f;
                    phases[v] += freq * (1f + spread * detune) / SampleRate;
                    float ph = phases[v] - Mathf.Floor(phases[v]);
                    sum += 2f * ph - 1f;
                }
                sum /= voices;
                // Two one-pole low-pass stages = gentle 12 dB slope.
                lp += (sum - lp) * cutoff;
                lp2 += (lp - lp2) * cutoff;
                buffer[idx] += lp2 * env * volume;
            }
        }

        /// <summary>Electric-piano style key: a sine body with a bell-like tine that fades quickly.</summary>
        public static void Keys(float[] buffer, float startSeconds, float duration, float freq, float volume, bool wrap = true)
        {
            int start = (int)(startSeconds * SampleRate);
            int total = Samples(duration + 0.4f);
            float p1 = 0f, p2 = 0f;
            for (int i = 0; i < total; i++)
            {
                int idx = start + i;
                if (idx >= buffer.Length) { if (!wrap) break; idx %= buffer.Length; }
                float t = (float)i / SampleRate;
                float env = Mathf.Min(1f, t / 0.004f) * Mathf.Exp(-t * (t < duration ? 2.2f : 9f));
                p1 += freq / SampleRate;
                p2 += freq * 3.01f / SampleRate;
                float body = Mathf.Sin(p1 * 2f * Mathf.PI);
                float tine = Mathf.Sin(p2 * 2f * Mathf.PI) * Mathf.Exp(-t * 14f) * 0.35f;
                buffer[idx] += (body + tine) * env * volume;
            }
        }

        /// <summary>Round sub bass (sine with a touch of saturation).</summary>
        public static void Sub(float[] buffer, float startSeconds, float duration, float freq, float volume, bool wrap = true)
        {
            int start = (int)(startSeconds * SampleRate);
            int total = Samples(duration + 0.05f);
            float phase = 0f;
            for (int i = 0; i < total; i++)
            {
                int idx = start + i;
                if (idx >= buffer.Length) { if (!wrap) break; idx %= buffer.Length; }
                float t = (float)i / SampleRate;
                float env = Mathf.Min(1f, t / 0.01f) * (t < duration ? 1f : Mathf.Max(0f, 1f - (t - duration) / 0.05f));
                phase += freq / SampleRate;
                float x = Mathf.Sin(phase * 2f * Mathf.PI);
                buffer[idx] += (float)Math.Tanh(x * 1.6f) * env * volume;
            }
        }

        /// <summary>Hand clap: three quick noise bursts and a short tail.</summary>
        public static void Clap(float[] buffer, float startSeconds, float volume, bool wrap = true)
        {
            int start = (int)(startSeconds * SampleRate);
            int len = Samples(0.22f);
            uint noise = 99173u + (uint)start;
            float prev = 0f;
            for (int i = 0; i < len; i++)
            {
                int idx = start + i;
                if (idx >= buffer.Length) { if (!wrap) break; idx %= buffer.Length; }
                float t = (float)i / SampleRate;
                float burst = t < 0.03f ? Mathf.Exp(-(t % 0.01f) * 300f) : Mathf.Exp(-(t - 0.03f) * 18f);
                float n = Osc(Wave.Noise, 0f, ref noise);
                float bp = n - prev; prev = n * 0.6f;
                buffer[idx] += bp * burst * volume;
            }
        }

        /// <summary>Quiet vinyl hiss and random crackles across the whole buffer (lo-fi texture).</summary>
        public static void Vinyl(float[] buffer, float volume, int seed)
        {
            uint noise = 12345u + (uint)seed;
            var rng = new System.Random(seed);
            float lp = 0f;
            for (int i = 0; i < buffer.Length; i++)
            {
                float n = Osc(Wave.Noise, 0f, ref noise);
                lp += (n - lp) * 0.05f;
                buffer[i] += lp * volume * 0.5f;
                if (rng.NextDouble() < 0.00025) buffer[i] += (float)(rng.NextDouble() * 2 - 1) * volume * 6f;
            }
        }

        /// <summary>Feedback echo (wraps around so loops stay seamless).</summary>
        public static void Echo(float[] buffer, float delaySeconds, float feedback, float mix)
        {
            int d = Samples(delaySeconds);
            if (d <= 0 || d >= buffer.Length) return;
            var wet = new float[buffer.Length];
            for (int pass = 0; pass < 2; pass++)
                for (int i = 0; i < buffer.Length; i++)
                {
                    int j = i - d;
                    if (j < 0) j += buffer.Length;
                    wet[i] = buffer[j] * mix + wet[j] * feedback;
                }
            for (int i = 0; i < buffer.Length; i++) buffer[i] += wet[i];
        }

        /// <summary>Gentle low-pass over a whole buffer (0..1, lower = darker).</summary>
        public static void LowPass(float[] buffer, float amount)
        {
            float lp = buffer.Length > 0 ? buffer[buffer.Length - 1] : 0f;
            for (int i = 0; i < buffer.Length; i++)
            {
                lp += (buffer[i] - lp) * amount;
                buffer[i] = lp;
            }
        }

        /// <summary>Soft-clips and scales the buffer so it never distorts harshly.</summary>
        public static void Master(float[] buffer, float gain)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                float x = buffer[i] * gain;
                buffer[i] = (float)Math.Tanh(x);
            }
        }

        public static void FadeEdges(float[] buffer, float fadeSeconds)
        {
            int n = Mathf.Min(buffer.Length / 2, Samples(fadeSeconds));
            for (int i = 0; i < n; i++)
            {
                float g = (float)i / n;
                buffer[i] *= g;
                buffer[buffer.Length - 1 - i] *= g;
            }
        }

        public static AudioClip ToClip(string name, float[] data)
        {
            var clip = AudioClip.Create(name, data.Length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
