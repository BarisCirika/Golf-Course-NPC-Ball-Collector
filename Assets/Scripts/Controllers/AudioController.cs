// AudioController.cs
using Cysharp.Threading.Tasks;
using GCNBC.Services;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace GCNBC.Controllers
{
    // Plays music (single looping source) and SFX (pooled one-shot sources).
    // Lives in ProjectContext, persists across scenes.
    public class AudioController : MonoBehaviour, IInitializableService
    {
        [Header("Mixer Groups")]
        [SerializeField] private AudioMixerGroup _musicGroup;
        [SerializeField] private AudioMixerGroup _sfxGroup;
        [SerializeField] private AudioMixerGroup _uiGroup;

        [Header("Music")]
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioClip _musicAudioClip;
        [SerializeField] private AudioClip _loseMusic;

        [Header("SFX")]
        [SerializeField] private AudioClip _ballCollectedClip;
        [SerializeField] private AudioClip _ballPickedUp;
        [SerializeField] private AudioClip _npcDiedClip;
        [SerializeField] private AudioClip _npcSpawned;

        [Header("UI")]
        [SerializeField] private AudioClip _clickClip;
        [SerializeField] private AudioClip _transitionClip;

        [Header("Footsteps")]
        [SerializeField] private AudioClip[] _footstepClips;
        [SerializeField] private Vector2 _footstepVolumeRange = new Vector2(0.4f, 0.6f);
        [SerializeField] private Vector2 _footstepPitchRange = new Vector2(0.9f, 1.1f);

        [Header("SFX Pool")]
        [Tooltip("Prefab/template AudioSource for SFX (or auto-created if null).")]
        [SerializeField] private int _sfxPoolSize = 10;


        // Pool of reusable AudioSources for one-shot SFX.
        private readonly Queue<AudioSource> _sfxPool = new();
        private readonly List<AudioSource> _activeSfx = new();


        public void Initialize()
        {
            if (_musicSource != null && _musicGroup != null)
                _musicSource.outputAudioMixerGroup = _musicGroup;

            BuildSfxPool();
            Debug.Log("[AudioController] Initialized.");
        }

        private void BuildSfxPool()
        {
            for (int i = 0; i < _sfxPoolSize; i++)
            {
                var go = new GameObject($"SFX_Source_{i}");
                go.transform.SetParent(transform);
                var source = go.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.outputAudioMixerGroup = _sfxGroup;
                _sfxPool.Enqueue(source);
            }
        }

        public void PlayUiClick() => PlaySfx(_clickClip, 1, _uiGroup);

        public void PlayUITransition() => PlaySfx(_transitionClip, 1, _uiGroup);

        public void PlayBallCollected() => PlaySfx(_ballCollectedClip, 1f, _sfxGroup);
        public void PlayBallPickedUp() => PlaySfx(_ballPickedUp, 1f, _sfxGroup);
        public void PlayNpcDied()
        {
            PlaySfx(_npcDiedClip, 1f, _sfxGroup);
            PlayLose(0.7f);   // Example: lower music volume when NPC dies.
        }

        public void PlayNpcSpawned() => PlaySfx(_npcSpawned, 1f, _sfxGroup);

        public void PlayMusic(float volume = 1f)
        {
            if (_musicSource.isPlaying && _musicSource.clip == _musicAudioClip)
            {
                _musicSource.volume = volume;   // sadece volume güncelle, istersen
                return;
            }
            _musicSource.loop = true;
            _musicSource.clip = _musicAudioClip;
            _musicSource.volume = volume;
            _musicSource.Play();
        }

        public void PlayLose(float volume = 1f)
        {
            if (_musicSource.isPlaying && _musicSource.clip == _loseMusic)
            {
                _musicSource.volume = volume;   // sadece volume güncelle, istersen
                return;
            }
            _musicSource.loop = false;
            _musicSource.clip = _loseMusic;
            _musicSource.volume = volume;
            _musicSource.Play();
        }

        public void StopMusic()
        {
            if (_musicSource != null) _musicSource.Stop();
        }

        public void PlaySfx(AudioClip clip, float volume = 1f, AudioMixerGroup group = null)
        {
            if (clip == null) return;

            AudioSource source = GetPooledSource();
            if (source == null) return;

            source.clip = clip;
            source.volume = volume;
            source.Play();
            source.outputAudioMixerGroup = group;

            _activeSfx.Add(source);
            // Return to pool when finished.
            ReturnWhenDone(source, clip.length).Forget();
        }

        // Optional: positional SFX (3D sound at a world position).
        public void PlaySfxAt(AudioClip clip, Vector3 position, float volume = 1f)
        {
            if (clip == null) return;

            AudioSource source = GetPooledSource();
            if (source == null) return;

            source.transform.position = position;
            source.spatialBlend = 1f;   // 3D
            source.clip = clip;
            source.volume = volume;
            source.pitch = 1f;
            source.Play();

            _activeSfx.Add(source);
            ReturnWhenDone(source, clip.length).Forget();
        }

        public void PlayFootstep(Vector3 position)
        {
            if (_footstepClips == null || _footstepClips.Length == 0) return;

            AudioClip clip = _footstepClips[Random.Range(0, _footstepClips.Length)];
            float volume = Random.Range(_footstepVolumeRange.x, _footstepVolumeRange.y);
            float pitch = Random.Range(_footstepPitchRange.x, _footstepPitchRange.y);

            PlaySfxAt(clip, position, volume, pitch);
        }

        private AudioSource GetPooledSource()
        {
            if (_sfxPool.Count > 0)
                return _sfxPool.Dequeue();

            // Pool empty: optionally grow, or reuse the oldest active one.
            if (_activeSfx.Count > 0)
            {
                var oldest = _activeSfx[0];
                _activeSfx.RemoveAt(0);
                oldest.Stop();
                return oldest;
            }
            return null;
        }

        public void PlaySfxAt(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
        {
            if (clip == null) return;
            AudioSource source = GetPooledSource();
            if (source == null) return;

            source.transform.position = position;
            source.spatialBlend = 1f;
            source.clip = clip;
            source.volume = volume;
            source.pitch = pitch;
            source.Play();

            _activeSfx.Add(source);
            ReturnWhenDone(source, clip.length / Mathf.Max(pitch, 0.01f)).Forget();
        }

        private async UniTaskVoid ReturnWhenDone(AudioSource source, float delay)
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(delay),
                cancellationToken: this.GetCancellationTokenOnDestroy());

            source.spatialBlend = 0f;
            source.outputAudioMixerGroup = _sfxGroup;
            source.clip = null;
            _activeSfx.Remove(source);
            _sfxPool.Enqueue(source);
        }
    }
}