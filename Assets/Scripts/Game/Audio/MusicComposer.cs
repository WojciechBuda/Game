using System.Collections.Generic;
using JellyDrop.Core.Economy;
using UnityEngine;

namespace JellyDrop.Audio
{
    /// <summary>
    /// Writes a happy 16-bar looping chiptune for each theme: chords, bass, arpeggios, a generated
    /// melody and drums. Different styles change key, scale, progression and groove.
    /// </summary>
    public static class MusicComposer
    {
        private static readonly int[] Major = { 0, 2, 4, 5, 7, 9, 11 };
        private static readonly int[] Minor = { 0, 2, 3, 5, 7, 8, 10 };

        private sealed class StyleDef
        {
            public int Root;
            public bool IsMinor;
            public int[] Progression; // scale degrees, 0-based
            public bool FourOnFloor;
            public bool SyncopatedBass;
            /// <summary>Driving straight 8th-note bass (synthwave).</summary>
            public bool EighthBass;
            public bool Pad;
            public Wave Lead;
            public Wave ArpWave;
            public float Swing;
        }

        private static StyleDef Style(MusicStyle style)
        {
            switch (style)
            {
                case MusicStyle.Mellow:
                    return new StyleDef { Root = 65, Progression = new[] { 0, 5, 3, 4 }, Lead = Wave.Triangle, ArpWave = Wave.Sine, Pad = true };
                case MusicStyle.Funky:
                    return new StyleDef { Root = 62, IsMinor = true, Progression = new[] { 0, 6, 5, 6 }, FourOnFloor = true, SyncopatedBass = true, Lead = Wave.Pulse25, ArpWave = Wave.Square, Swing = 0.12f };
                case MusicStyle.Space:
                    return new StyleDef { Root = 64, IsMinor = true, Progression = new[] { 0, 5, 2, 6 }, FourOnFloor = true, Pad = true, Lead = Wave.Saw, ArpWave = Wave.Triangle };
                case MusicStyle.Arcade:
                    return new StyleDef { Root = 57, IsMinor = true, Progression = new[] { 0, 5, 2, 6 }, FourOnFloor = true, EighthBass = true, Pad = true, Lead = Wave.Saw, ArpWave = Wave.Square };
                case MusicStyle.Dreamy:
                    return new StyleDef { Root = 67, Progression = new[] { 3, 4, 2, 5 }, Pad = true, Lead = Wave.Sine, ArpWave = Wave.Triangle, Swing = 0.08f };
                default: // Bubbly
                    return new StyleDef { Root = 60, Progression = new[] { 0, 4, 5, 3 }, FourOnFloor = true, Lead = Wave.Square, ArpWave = Wave.Pulse25 };
            }
        }

        public static AudioClip Compose(MusicStyle style, int bpm, int seed = 1)
        {
            var def = Style(style);
            var scale = def.IsMinor ? Minor : Major;
            var rng = new System.Random(seed * 31 + (int)style);

            const int bars = 16;
            float beat = 60f / Mathf.Max(60, bpm);
            float bar = beat * 4f;
            float sixteenth = beat / 4f;
            var buffer = new float[Synth.Samples(bar * bars)];

            // Two-bar melody motif, reused with variation (makes it catchy).
            var motif = MakeMotif(rng);
            var motifB = MakeMotif(rng);

            for (int b = 0; b < bars; b++)
            {
                float barStart = b * bar;
                int degree = def.Progression[b % def.Progression.Length];
                var chord = Chord(def.Root, scale, degree);
                bool sectionB = b >= 8;

                // --- Pad / chords
                if (def.Pad)
                {
                    foreach (var n in chord)
                        Synth.Note(buffer, barStart, bar * 0.95f, Synth.MidiToFreq(n - 12), Wave.Triangle, 0.035f,
                            attack: 0.25f, decay: 0.3f, sustain: 0.8f, release: 0.4f, wrap: true);
                }
                else
                {
                    // Bouncy stabs on the off-beats
                    for (int k = 0; k < 4; k++)
                    {
                        float t = barStart + k * beat + beat * 0.5f;
                        foreach (var n in chord)
                            Synth.Note(buffer, t, beat * 0.2f, Synth.MidiToFreq(n), Wave.Pulse25, 0.022f, decay: 0.05f, sustain: 0.3f, release: 0.05f, wrap: true);
                    }
                }

                // --- Bass
                int bassNote = chord[0] - 24;
                for (int s = 0; s < 16; s++)
                {
                    bool play = def.EighthBass
                        ? s % 2 == 0
                        : def.SyncopatedBass
                            ? (s == 0 || s == 3 || s == 6 || s == 8 || s == 10 || s == 14)
                            : (s % 4 == 0 || s % 4 == 2 && s != 2);
                    if (!play) continue;
                    int note = bassNote + (def.SyncopatedBass && (s == 6 || s == 14) ? 12 : 0) + (def.EighthBass && s % 4 == 2 ? 12 : 0);
                    if (!def.SyncopatedBass && s == 12) note += 7;
                    Synth.Note(buffer, barStart + s * sixteenth, sixteenth * 1.6f, Synth.MidiToFreq(note), Wave.Triangle, 0.22f,
                        decay: 0.06f, sustain: 0.7f, release: 0.04f, wrap: true);
                }

                // --- Arpeggio (louder in section B)
                float arpVol = sectionB ? 0.05f : 0.035f;
                for (int s = 0; s < 16; s++)
                {
                    if (!sectionB && s % 2 == 1) continue;
                    int n = chord[s % 3] + 12 + (s % 6 >= 3 ? 12 : 0);
                    float swing = (s % 2 == 1) ? def.Swing * sixteenth : 0f;
                    Synth.Note(buffer, barStart + s * sixteenth + swing, sixteenth * 0.6f, Synth.MidiToFreq(n), def.ArpWave, arpVol,
                        decay: 0.04f, sustain: 0.4f, release: 0.03f, wrap: true);
                }

                // --- Melody (rests in bars 0-1 to build up; section B uses second motif)
                if (b >= 2)
                {
                    var m = (b / 2) % 2 == 0 ? (sectionB ? motifB : motif) : (sectionB ? motif : motifB);
                    int half = b % 2;
                    foreach (var ev in m)
                    {
                        if (ev.Step / 16 != half) continue;
                        int stepInBar = ev.Step % 16;
                        int note = ScaleNote(def.Root + 12, scale, ChordToneToDegree(degree, ev.Tone));
                        float swing = (stepInBar % 2 == 1) ? def.Swing * sixteenth : 0f;
                        Synth.Note(buffer, barStart + stepInBar * sixteenth + swing, ev.Length * sixteenth * 0.9f,
                            Synth.MidiToFreq(note), def.Lead, def.Lead == Wave.Sine ? 0.13f : 0.07f,
                            attack: 0.01f, decay: 0.1f, sustain: 0.55f, release: 0.06f, vibrato: 0.006f, wrap: true);
                    }
                }

                // --- Drums
                for (int k = 0; k < 4; k++)
                {
                    float t = barStart + k * beat;
                    if (def.FourOnFloor || k % 2 == 0) Synth.Kick(buffer, t, 0.5f, wrap: true);
                    if (k % 2 == 1) Synth.Snare(buffer, t, 0.22f, wrap: true);
                    Synth.HiHat(buffer, t + beat * 0.5f + def.Swing * sixteenth, 0.18f, open: k == 3 && b % 4 == 3, wrap: true);
                    if (sectionB) Synth.HiHat(buffer, t, 0.1f, wrap: true);
                }
                // Little drum fill at the end of every 4 bars
                if (b % 4 == 3)
                {
                    for (int s = 12; s < 16; s++) Synth.Snare(buffer, barStart + s * sixteenth, 0.08f + 0.03f * (s - 12), wrap: true);
                }
            }

            Synth.Master(buffer, 1.6f);
            return Synth.ToClip($"music_{style}", buffer);
        }

        private struct MelodyEvent
        {
            public int Step;   // 0..31 sixteenths over two bars
            public int Length; // in sixteenths
            public int Tone;   // chord tone index (0 root, 1 third, 2 fifth, 3 octave, 4 = 2nd scale step for passing notes)
        }

        private static List<MelodyEvent> MakeMotif(System.Random rng)
        {
            var events = new List<MelodyEvent>();
            int step = 0;
            int[] lengths = { 2, 2, 2, 4, 1, 3, 2, 6 };
            while (step < 30)
            {
                int len = lengths[rng.Next(lengths.Length)];
                if (step + len > 32) len = 32 - step;
                bool rest = rng.NextDouble() < 0.18;
                if (!rest) events.Add(new MelodyEvent { Step = step, Length = len, Tone = rng.Next(5) });
                step += len;
            }
            return events;
        }

        private static int[] Chord(int root, int[] scale, int degree) => new[]
        {
            ScaleNote(root, scale, degree),
            ScaleNote(root, scale, degree + 2),
            ScaleNote(root, scale, degree + 4)
        };

        private static int ChordToneToDegree(int chordDegree, int tone)
        {
            switch (tone)
            {
                case 0: return chordDegree;
                case 1: return chordDegree + 2;
                case 2: return chordDegree + 4;
                case 3: return chordDegree + 7;
                default: return chordDegree + 1;
            }
        }

        private static int ScaleNote(int root, int[] scale, int degree)
        {
            int octave = Mathf.FloorToInt(degree / 7f);
            int idx = ((degree % 7) + 7) % 7;
            return root + octave * 12 + scale[idx];
        }
    }
}
