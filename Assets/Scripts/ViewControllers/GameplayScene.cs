// GameplayUIController.cs — HUD: score, time, health
using DG.Tweening;
using GCNBC.Controllers;
using GCNBC.Services;
using GCNBC.Signals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GCNBC.ViewControllers
{
    public class GameplayUIController : MonoBehaviour
    {
        [Header("Score & Time")]
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _timeText;

        [Header("Health")]
        [SerializeField] private Slider _healthBar;
        [SerializeField] private TMP_Text _healthText;   // optional: for color shift

        private IScoreService _score;
        private SignalBus _signalBus;
        private AudioController _audio;

        private float _elapsed;
        private bool _running = true;

        [Inject]
        private void Construct(IScoreService score, AudioController audio, SignalBus signalBus)
        {
            _score = score;
            _signalBus = signalBus;
            _audio = audio;
        }

        private void OnEnable()
        {
            _signalBus.Subscribe<ScoreChangedSignal>(OnScoreChanged);
            _signalBus.Subscribe<NpcHealthChangedSignal>(OnHealthChanged);
        }

        private void OnDisable()
        {
            _signalBus.Unsubscribe<ScoreChangedSignal>(OnScoreChanged);
            _signalBus.Unsubscribe<NpcHealthChangedSignal>(OnHealthChanged);
        }

        private void Start()
        {
            UpdateScore(_score.Current);
            UpdateTime(0f);
            _audio.PlayMusic();
            if (_healthBar != null) _healthBar.value = 1f;
        }

        private void Update()
        {
            if (!_running) return;

            // Count up. Scaled time -> freezes automatically when paused (timeScale = 0).
            _elapsed += Time.deltaTime;
            UpdateTime(_elapsed);
        }

        // --- Score ---
        private void OnScoreChanged(ScoreChangedSignal s) => UpdateScore(s.NewScore);

        private void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"Score: {score}";
        }

        // --- Time (mm:ss) ---
        private void UpdateTime(float seconds)
        {
            int min = Mathf.FloorToInt(seconds / 60f);
            int sec = Mathf.FloorToInt(seconds % 60f);
            if (_timeText != null) _timeText.text = $"{min:00}:{sec:00}";
        }

        // --- Health ---
        private void OnHealthChanged(NpcHealthChangedSignal s)
        {
            float ratio = s.Max > 0f ? s.Current / s.Max : 0f;

            if (_healthBar != null)
                _healthBar.DOValue(ratio, 0.2f).SetEase(Ease.OutSine).SetLink(gameObject);

            _healthText.text = s.Current.ToString("0") + " / " + s.Max.ToString("0");
        }

        // Call this to stop the timer (e.g. on game over).
        public void StopTimer() => _running = false;

        // Useful for game-over screen: how long the run lasted.
        public float ElapsedTime => _elapsed;
    }
}