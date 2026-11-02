using System.Collections.Generic;
using JellyDrop.Core.Economy;
using UnityEngine;

namespace JellyDrop.Audio
{
    /// <summary>
    /// Modern-sounding loops for the newer themes, still generated in code:
    /// <list type="bullet">
    /// <item><b>Chill</b>: lo-fi beats. Jazzy 7th/9th electric-piano chords, a round sub bass, a swung boom-bap
    /// groove, a sparse echoing melody and a little vinyl hiss.</item>
    /// <item><b>Future</b>: future-house / synth-pop. Wide detuned-saw chords that "pump" with the kick,
    /// four-on-the-floor drums with claps and off-beat hats, a bouncy sub bass and a plucky lead with echo.</item>
    /// </list>
    /// </summary>
    public static class ModernComposer
    {
        private static readonly int[] Major = { 0, 2, 4, 5, 7, 9, 11 };
        private static readonly int[] Minor = { 0, 2, 3, 5, 7, 8, 10 };

        public static bool Handles(MusicStyle style) => style == MusicStyle.Chill || style == MusicStyle.Future;

        public static float[] Compose(MusicStyle style, int bpm, int seed)
        {
            return style == MusicStyle.Chill ? Chill(Mathf.Clamp(bpm, 70, 100), seed) : Future(Mathf.Clamp(bpm, 110, 128), seed);
        }

        // ------------------------------------------------------------------ lo-fi

        private static float[] Chill(int bpm, int seed)
        {
            var rng = new System.Random(seed * 17 + 3);
            const int bars = 16;
            float beat = 60f / bpm, bar = beat * 4f, six = beat / 4f;
            float swing = six * 0.28f;
            int n = Synth.Samples(bar * bars);
            var keys = new float[n];
            var bass = new float[n];
            var drums = new float[n];
            var lead = new float[n];

            int root = 65; // F major: ii - V - I - vi with 7ths and 9ths
            int[] prog = { 1, 4, 0, 5 };
            var motif = Motif(rng, 5);

            for (int b = 0; b < bars; b++)
            {
                float t0 = b * bar;
                int deg = prog[b % prog.Length];
                var chord = Voicing(root - 12, Major, deg, true);

                // Electric piano: on the one, and a lazy push on the "and" of two.
                foreach (var note in chord)
                {
                    Synth.Keys(keys, t0, beat * 1.4f, Synth.MidiToFreq(note), 0.05f);
                    Synth.Keys(keys, t0 + beat * 1.5f + swing, beat * 1.8f, Synth.MidiToFreq(note), 0.035f);
                }
                // Soft pad underneath
                foreach (var note in chord)
                    Synth.Pad(keys, t0, bar, Synth.MidiToFreq(note), 0.02f, cutoff: 0.04f, attack: 0.6f, release: 0.8f, voices: 3);

                // Sub bass
                int bnote = Scale(root - 24, Major, deg);
                Synth.Sub(bass, t0, beat * 1.3f, Synth.MidiToFreq(bnote), 0.3f);
                Synth.Sub(bass, t0 + beat * 2.5f + swing, beat * 0.9f, Synth.MidiToFreq(bnote), 0.24f);
                if (b % 2 == 1) Synth.Sub(bass, t0 + beat * 3.5f + swing, beat * 0.4f, Synth.MidiToFreq(bnote + 7), 0.2f);

                // Boom-bap groove
                Synth.Kick(drums, t0, 0.55f, true);
                Synth.Kick(drums, t0 + beat * 1.5f + swing, 0.32f, true);
                Synth.Kick(drums, t0 + beat * 2.75f, 0.38f, true);
                Synth.Snare(drums, t0 + beat, 0.2f, true);
                Synth.Snare(drums, t0 + beat * 3f, 0.2f, true);
                for (int s = 0; s < 8; s++)
                    Synth.HiHat(drums, t0 + s * beat * 0.5f + (s % 2 == 1 ? swing : 0f), s % 2 == 0 ? 0.12f : 0.07f, wrap: true);

                // Sparse melody in the second half, pentatonic, with echo added later
                if (b >= 4)
                {
                    foreach (var ev in motif)
                    {
                        if (ev.step / 16 != b % 2) continue;
                        int s = ev.step % 16;
                        int note = Pentatonic(root + 12, ev.tone);
                        Synth.Keys(lead, t0 + s * six + (s % 2 == 1 ? swing : 0f), ev.len * six, Synth.MidiToFreq(note), 0.07f);
                    }
                }
            }

            Synth.Echo(lead, beat * 0.75f, 0.35f, 0.4f);
            Synth.LowPass(drums, 0.55f);
            var mix = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Synth.SampleRate;
                float pump = Pump(t, beat, 0.65f);
                mix[i] = keys[i] * pump + bass[i] + drums[i] + lead[i] * 0.9f;
            }
            Synth.Vinyl(mix, 0.012f, seed);
            Synth.LowPass(mix, 0.72f);
            Synth.Master(mix, 1.5f);
            return mix;
        }

        // ------------------------------------------------------------------ future house / synth-pop

        private static float[] Future(int bpm, int seed)
        {
            var rng = new System.Random(seed * 29 + 11);
            const int bars = 16;
            float beat = 60f / bpm, bar = beat * 4f, six = beat / 4f;
            int n = Synth.Samples(bar * bars);
            var chords = new float[n];
            var bass = new float[n];
            var drums = new float[n];
            var lead = new float[n];

            int root = 57; // A minor: i - VI - III - VII
            int[] prog = { 0, 5, 2, 6 };
            var motifA = Motif(rng, 5);
            var motifB = Motif(rng, 5);

            for (int b = 0; b < bars; b++)
            {
                float t0 = b * bar;
                int deg = prog[b % prog.Length];
                var chord = Voicing(root, Minor, deg, false);
                bool drop = b >= 4;

                // Wide saw chords: held in the intro, rhythmic stabs in the drop
                if (!drop)
                {
                    foreach (var note in chord) Synth.Pad(chords, t0, bar * 0.95f, Synth.MidiToFreq(note), 0.05f, cutoff: 0.08f, attack: 0.4f);
                }
                else
                {
                    float[] hits = { 0f, 0.75f, 1.5f, 2.5f, 3.25f };
                    foreach (var h in hits)
                        foreach (var note in chord)
                            Synth.Pad(chords, t0 + h * beat, beat * 0.45f, Synth.MidiToFreq(note), 0.06f, cutoff: 0.16f, attack: 0.01f, release: 0.18f);
                }

                // Bass: root on the one, then bouncy off-beats
                int bnote = Scale(root - 24, Minor, deg);
                Synth.Sub(bass, t0, beat * 0.4f, Synth.MidiToFreq(bnote), 0.3f);
                for (int k = 0; k < 4; k++)
                    Synth.Sub(bass, t0 + k * beat + beat * 0.5f, beat * 0.35f, Synth.MidiToFreq(bnote + (k == 3 ? 12 : 0)), drop ? 0.28f : 0.16f);

                // Drums
                for (int k = 0; k < 4; k++)
                {
                    float t = t0 + k * beat;
                    if (drop || k == 0) Synth.Kick(drums, t, 0.6f, true);
                    if (drop && k % 2 == 1) Synth.Clap(drums, t, 0.32f);
                    Synth.HiHat(drums, t + beat * 0.5f, drop ? 0.2f : 0.1f, open: drop, wrap: true);
                    if (drop)
                        for (int s = 0; s < 4; s++) Synth.HiHat(drums, t + s * six, 0.05f, wrap: true);
                }
                if (b == 3) // build-up snare roll into the drop
                    for (int s = 0; s < 16; s++) Synth.Snare(drums, t0 + s * six, 0.04f + s * 0.012f, true);

                // Plucky lead
                if (b >= 2)
                {
                    var m = b >= 8 ? motifB : motifA;
                    foreach (var ev in m)
                    {
                        if (ev.step / 16 != b % 2) continue;
                        int s = ev.step % 16;
                        int note = Pentatonic(root + 12, ev.tone, minor: true);
                        Synth.Keys(lead, t0 + s * six, ev.len * six * 0.6f, Synth.MidiToFreq(note), 0.06f);
                        Synth.Note(lead, t0 + s * six, ev.len * six * 0.5f, Synth.MidiToFreq(note + 12), Wave.Saw, 0.012f,
                            attack: 0.003f, decay: 0.08f, sustain: 0.2f, release: 0.1f, wrap: true);
                    }
                }
            }

            Synth.Echo(lead, beat * 0.75f, 0.3f, 0.35f);
            var mix = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Synth.SampleRate;
                mix[i] = chords[i] * Pump(t, beat, 0.8f) + bass[i] * Pump(t, beat, 0.5f) + drums[i] + lead[i];
            }
            Synth.Master(mix, 1.4f);
            return mix;
        }

        // ------------------------------------------------------------------ helpers

        /// <summary>Side-chain style volume dip after every beat ("pumping").</summary>
        private static float Pump(float t, float beat, float depth)
        {
            float k = Mathf.Repeat(t, beat) / (beat * 0.5f);
            float g = Mathf.Clamp01(k);
            return 1f - depth + depth * g * g;
        }

        /// <summary>Chord voicing: root, third, fifth, seventh (and ninth for jazzy chords).</summary>
        private static List<int> Voicing(int root, int[] scale, int degree, bool jazzy)
        {
            var v = new List<int> { Scale(root, scale, degree), Scale(root, scale, degree + 2), Scale(root, scale, degree + 4), Scale(root, scale, degree + 6) };
            if (jazzy) v.Add(Scale(root, scale, degree + 8));
            return v;
        }

        private static int Scale(int root, int[] scale, int degree)
        {
            int octave = Mathf.FloorToInt(degree / 7f);
            int idx = ((degree % 7) + 7) % 7;
            return root + octave * 12 + scale[idx];
        }

        private static readonly int[] PentaMajor = { 0, 2, 4, 7, 9 };
        private static readonly int[] PentaMinor = { 0, 3, 5, 7, 10 };

        private static int Pentatonic(int root, int step, bool minor = false)
        {
            var p = minor ? PentaMinor : PentaMajor;
            int octave = step / p.Length;
            return root + octave * 12 + p[step % p.Length];
        }

        private struct Ev { public int step, len, tone; }

        /// <summary>Two-bar motif with plenty of space (modern melodies breathe).</summary>
        private static List<Ev> Motif(System.Random rng, int range)
        {
            var list = new List<Ev>();
            int step = 0;
            int[] lens = { 2, 2, 3, 4, 6 };
            int tone = rng.Next(range);
            while (step < 30)
            {
                int len = lens[rng.Next(lens.Length)];
                if (step + len > 32) len = 32 - step;
                if (rng.NextDouble() > 0.35)
                {
                    tone = Mathf.Clamp(tone + rng.Next(-2, 3), 0, range + 2);
                    list.Add(new Ev { step = step, len = len, tone = tone });
                }
                step += len;
            }
            return list;
        }
    }
}
