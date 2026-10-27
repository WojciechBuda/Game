using System.Collections.Generic;
using UnityEngine;

namespace JellyDrop.Audio
{
    public enum Sfx
    {
        Move, Rotate, Lock, HardDrop, Hold,
        Clear1, Clear2, Clear3, Quad,
        Combo, Coin, Golden, Rainbow, Jackpot,
        LevelUp, GameOver, Win,
        Button, PowerUp, Bomb, Zap,
        WheelTick, Purchase, Error, Pop
    }

    /// <summary>Generates all sound effects once at startup.</summary>
    public static class SfxBank
    {
        public static Dictionary<Sfx, AudioClip> Build()
        {
            var clips = new Dictionary<Sfx, AudioClip>();
            foreach (Sfx s in System.Enum.GetValues(typeof(Sfx))) clips[s] = Synth.ToClip("sfx_" + s, Make(s));
            return clips;
        }

        private static float N(int midi) => Synth.MidiToFreq(midi);

        private static float[] Make(Sfx sfx)
        {
            float[] b;
            switch (sfx)
            {
                case Sfx.Move:
                    b = Buf(0.06f);
                    Synth.Note(b, 0, 0.025f, N(84), Wave.Pulse25, 0.25f, decay: 0.02f, sustain: 0.2f, release: 0.02f);
                    break;
                case Sfx.Rotate:
                    b = Buf(0.1f);
                    Synth.Note(b, 0, 0.05f, N(79), Wave.Square, 0.2f, decay: 0.03f, sustain: 0.3f, release: 0.03f, slideTo: N(88));
                    break;
                case Sfx.Lock:
                    b = Buf(0.15f);
                    Synth.Note(b, 0, 0.06f, N(48), Wave.Triangle, 0.6f, decay: 0.05f, sustain: 0.2f, release: 0.06f, slideTo: N(40));
                    break;
                case Sfx.HardDrop:
                    b = Buf(0.3f);
                    Synth.Note(b, 0, 0.1f, 1200f, Wave.Noise, 0.25f, attack: 0.01f, decay: 0.08f, sustain: 0.1f, release: 0.05f);
                    Synth.Kick(b, 0.06f, 0.9f);
                    break;
                case Sfx.Hold:
                    b = Buf(0.2f);
                    Synth.Note(b, 0, 0.05f, N(76), Wave.Triangle, 0.3f, release: 0.03f);
                    Synth.Note(b, 0.06f, 0.05f, N(71), Wave.Triangle, 0.3f, release: 0.05f);
                    break;
                case Sfx.Clear1: b = Arp(new[] { 72, 76 }, 0.06f, Wave.Square, 0.2f); break;
                case Sfx.Clear2: b = Arp(new[] { 72, 76, 79 }, 0.06f, Wave.Square, 0.2f); break;
                case Sfx.Clear3: b = Arp(new[] { 72, 76, 79, 84 }, 0.055f, Wave.Square, 0.2f); break;
                case Sfx.Quad:
                    b = Buf(1.0f);
                    int[] fan = { 72, 76, 79, 84, 88, 91, 96 };
                    for (int i = 0; i < fan.Length; i++)
                        Synth.Note(b, i * 0.05f, 0.08f, N(fan[i]), Wave.Square, 0.16f, release: 0.06f);
                    foreach (var n in new[] { 84, 88, 91 })
                        Synth.Note(b, 0.38f, 0.4f, N(n), Wave.Pulse25, 0.12f, decay: 0.2f, sustain: 0.6f, release: 0.2f, vibrato: 0.01f);
                    Synth.Kick(b, 0.38f, 0.6f);
                    break;
                case Sfx.Combo:
                    b = Arp(new[] { 79, 83, 86, 91 }, 0.04f, Wave.Pulse25, 0.16f);
                    break;
                case Sfx.Coin:
                    b = Buf(0.35f);
                    Synth.Note(b, 0, 0.06f, N(83), Wave.Square, 0.18f, release: 0.02f);
                    Synth.Note(b, 0.07f, 0.2f, N(88), Wave.Square, 0.18f, decay: 0.15f, sustain: 0.3f, release: 0.08f);
                    break;
                case Sfx.Golden:
                    b = Buf(0.9f);
                    for (int i = 0; i < 10; i++)
                        Synth.Note(b, i * 0.045f, 0.12f, N(84 + (i % 5) * 3), Wave.Triangle, 0.18f, release: 0.15f);
                    Synth.Note(b, 0.45f, 0.3f, N(96), Wave.Sine, 0.25f, decay: 0.2f, sustain: 0.4f, release: 0.2f, vibrato: 0.02f);
                    break;
                case Sfx.Rainbow:
                    b = Buf(1.0f);
                    Synth.Note(b, 0, 0.6f, N(60), Wave.Triangle, 0.25f, attack: 0.02f, sustain: 0.8f, release: 0.3f, slideTo: N(96));
                    Synth.Note(b, 0.1f, 0.5f, N(67), Wave.Pulse25, 0.1f, sustain: 0.8f, release: 0.3f, slideTo: N(103));
                    break;
                case Sfx.Jackpot:
                    b = Buf(2.2f);
                    int[] jp = { 72, 76, 79, 84, 79, 84, 88, 91, 88, 91, 96, 100 };
                    for (int i = 0; i < jp.Length; i++)
                        Synth.Note(b, i * 0.07f, 0.1f, N(jp[i]), Wave.Square, 0.15f, release: 0.05f);
                    foreach (var n in new[] { 84, 88, 91, 96 })
                        Synth.Note(b, 0.9f, 0.9f, N(n), Wave.Pulse25, 0.1f, decay: 0.3f, sustain: 0.7f, release: 0.4f, vibrato: 0.015f);
                    for (int i = 0; i < 4; i++) Synth.Kick(b, 0.9f + i * 0.22f, 0.5f);
                    break;
                case Sfx.LevelUp:
                    b = Arp(new[] { 67, 72, 76, 79, 84 }, 0.07f, Wave.Pulse25, 0.18f, tail: 0.3f);
                    break;
                case Sfx.GameOver:
                    b = Buf(1.4f);
                    int[] go = { 72, 67, 64, 60 };
                    for (int i = 0; i < go.Length; i++)
                        Synth.Note(b, i * 0.22f, 0.2f, N(go[i]), Wave.Triangle, 0.3f, release: 0.1f, vibrato: 0.01f);
                    Synth.Note(b, 0.88f, 0.4f, N(55), Wave.Triangle, 0.3f, release: 0.2f, slideTo: N(48));
                    break;
                case Sfx.Win:
                    b = Arp(new[] { 72, 76, 79, 84, 79, 84, 88 }, 0.09f, Wave.Square, 0.18f, tail: 0.5f);
                    break;
                case Sfx.Button:
                    b = Buf(0.1f);
                    Synth.Note(b, 0, 0.03f, N(74), Wave.Sine, 0.4f, decay: 0.02f, sustain: 0.3f, release: 0.03f, slideTo: N(86));
                    break;
                case Sfx.Pop:
                    b = Buf(0.12f);
                    Synth.Note(b, 0, 0.04f, N(66), Wave.Sine, 0.5f, decay: 0.03f, sustain: 0.2f, release: 0.04f, slideTo: N(90));
                    break;
                case Sfx.PowerUp:
                    b = Buf(0.6f);
                    Synth.Note(b, 0, 0.35f, N(60), Wave.Square, 0.15f, sustain: 0.8f, release: 0.15f, slideTo: N(84), vibrato: 0.03f);
                    Synth.Note(b, 0.05f, 0.3f, N(67), Wave.Triangle, 0.2f, sustain: 0.8f, release: 0.15f, slideTo: N(91));
                    break;
                case Sfx.Bomb:
                    b = Buf(0.8f);
                    Synth.Note(b, 0, 0.4f, 800f, Wave.Noise, 0.5f, attack: 0.005f, decay: 0.3f, sustain: 0.3f, release: 0.3f);
                    Synth.Kick(b, 0, 1f);
                    Synth.Note(b, 0, 0.3f, N(45), Wave.Triangle, 0.5f, release: 0.2f, slideTo: N(28));
                    break;
                case Sfx.Zap:
                    b = Buf(0.4f);
                    Synth.Note(b, 0, 0.25f, N(96), Wave.Saw, 0.14f, sustain: 0.7f, release: 0.1f, slideTo: N(48), vibrato: 0.08f);
                    break;
                case Sfx.WheelTick:
                    b = Buf(0.04f);
                    Synth.Note(b, 0, 0.01f, N(96), Wave.Square, 0.15f, decay: 0.01f, sustain: 0.1f, release: 0.01f);
                    break;
                case Sfx.Purchase:
                    b = Arp(new[] { 76, 79, 84, 88 }, 0.06f, Wave.Triangle, 0.25f, tail: 0.25f);
                    Synth.Note(b, 0.24f, 0.2f, N(100), Wave.Sine, 0.2f, release: 0.2f);
                    break;
                default: // Error
                    b = Buf(0.25f);
                    Synth.Note(b, 0, 0.08f, N(55), Wave.Square, 0.18f, release: 0.03f);
                    Synth.Note(b, 0.1f, 0.1f, N(50), Wave.Square, 0.18f, release: 0.05f);
                    break;
            }
            Synth.Master(b, 1.2f);
            return b;
        }

        private static float[] Buf(float seconds) => new float[Synth.Samples(seconds)];

        private static float[] Arp(int[] notes, float step, Wave wave, float volume, float tail = 0.15f)
        {
            var b = Buf(notes.Length * step + tail + 0.1f);
            for (int i = 0; i < notes.Length; i++)
            {
                bool last = i == notes.Length - 1;
                Synth.Note(b, i * step, last ? step + tail : step, N(notes[i]), wave, volume,
                    decay: 0.04f, sustain: 0.6f, release: last ? tail : 0.03f);
            }
            return b;
        }
    }
}
