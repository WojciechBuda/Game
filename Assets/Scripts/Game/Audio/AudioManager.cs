using System.Collections.Generic;
using JellyDrop.Core.Economy;
using UnityEngine;

namespace JellyDrop.Audio
{
    /// <summary>Plays music and pooled sound effects. Music speeds up a little with the game level.</summary>
    public sealed class AudioManager : MonoBehaviour
    {
        private const int SfxVoices = 6;

        private AudioSource _music;
        private readonly List<AudioSource> _voices = new List<AudioSource>();
        private int _nextVoice;
        private Dictionary<Sfx, AudioClip> _sfx;
        private readonly Dictionary<string, AudioClip> _musicCache = new Dictionary<string, AudioClip>();
        private string _currentMusicKey;
        private float _targetPitch = 1f;
        private float _musicVolume = 0.55f;
        private float _duck = 1f;

        public bool MusicOn { get; private set; } = true;
        public bool SfxOn { get; private set; } = true;

        private void Awake()
        {
            _music = gameObject.AddComponent<AudioSource>();
            _music.loop = true;
            _music.playOnAwake = false;
            _music.volume = _musicVolume;
            for (int i = 0; i < SfxVoices; i++)
            {
                var v = gameObject.AddComponent<AudioSource>();
                v.playOnAwake = false;
                _voices.Add(v);
            }
            _sfx = SfxBank.Build();
        }

        public void ApplySettings(bool musicOn, bool sfxOn)
        {
            MusicOn = musicOn;
            SfxOn = sfxOn;
            if (!musicOn) _music.Pause();
            else if (_music.clip != null && !_music.isPlaying) _music.Play();
        }

        public void PlayThemeMusic(ShopItem theme)
        {
            if (theme == null) return;
            string key = theme.Id;
            if (_currentMusicKey == key && _music.isPlaying) return;
            if (!_musicCache.TryGetValue(key, out var clip))
            {
                int seed = 17;
                foreach (char ch in key) seed = seed * 31 + ch; // stable across runtimes, unlike GetHashCode
                clip = MusicComposer.Compose(theme.Music, theme.MusicBpm, seed);
                _musicCache[key] = clip;
            }
            _currentMusicKey = key;
            _music.clip = clip;
            _music.time = 0f;
            if (MusicOn) _music.Play();
        }

        /// <summary>Music gets faster and a bit higher with the level (1.0 .. 1.25).</summary>
        public void SetIntensity(int level) => _targetPitch = 1f + Mathf.Min(0.25f, (level - 1) * 0.02f);

        public void ResetIntensity() => _targetPitch = 1f;

        /// <summary>Temporarily lowers the music (e.g. during the jackpot jingle or pause menu).</summary>
        public void Duck(float amount) => _duck = Mathf.Clamp01(amount);

        public void Play(Sfx sfx, float volume = 1f, float pitchVariance = 0.04f)
        {
            if (!SfxOn || _sfx == null || !_sfx.TryGetValue(sfx, out var clip)) return;
            var v = _voices[_nextVoice];
            _nextVoice = (_nextVoice + 1) % _voices.Count;
            v.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
            v.volume = volume;
            v.clip = clip;
            v.Play();
        }

        public void PlayPitched(Sfx sfx, float pitch, float volume = 1f)
        {
            if (!SfxOn || _sfx == null || !_sfx.TryGetValue(sfx, out var clip)) return;
            var v = _voices[_nextVoice];
            _nextVoice = (_nextVoice + 1) % _voices.Count;
            v.pitch = pitch;
            v.volume = volume;
            v.clip = clip;
            v.Play();
        }

        private void Update()
        {
            _music.pitch = Mathf.MoveTowards(_music.pitch, _targetPitch, Time.unscaledDeltaTime * 0.1f);
            _music.volume = Mathf.MoveTowards(_music.volume, _musicVolume * _duck, Time.unscaledDeltaTime * 1.5f);
        }
    }
}
