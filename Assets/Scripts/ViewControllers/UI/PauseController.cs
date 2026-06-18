// PauseController.cs — pause + game over birleşik
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GCNBC.Constants;
using GCNBC.Controllers;
using GCNBC.Signals;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Zenject;

namespace GCNBC.ViewControllers.UI
{
    public class PauseController : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private CanvasGroup _panelGroup;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private float _fadeDuration = 0.25f;

        [Header("Titles")]
        [SerializeField] private string _pausedTitle = "Paused";
        [SerializeField] private string _gameOverTitle = "You Died";

        private SignalBus _signalBus;
        private SceneTransitionController _transition;
        private AudioController _audio;

        private bool _isOpen;
        private bool _isGameOver;   // once dead, ESC can't resume

        [Inject]
        private void Construct(SignalBus signalBus, SceneTransitionController transition, AudioController audio)
        {
            _signalBus = signalBus;
            _transition = transition;
            _audio = audio;
        }

        private void Start() => SetPanelInstant(false);

        private void OnEnable() => _signalBus.Subscribe<NpcDiedSignal>(OnNpcDied);
        private void OnDisable() => _signalBus.Unsubscribe<NpcDiedSignal>(OnNpcDied);

        private void Update()
        {
            // ESC toggles pause — but NOT after game over (death is final).
            if (_isGameOver) return;

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                TogglePause();
        }

        // --- Triggers ---

        private void TogglePause()
        {
            if (_isOpen) Resume();
            else OpenPanel(_pausedTitle, isGameOver: false);
        }

        private void OnNpcDied(NpcDiedSignal s)
        {
            // Death opens the same panel but as game over (no resume).
            OpenPanel(_gameOverTitle, isGameOver: true);
        }

        // --- Panel control ---

        private void OpenPanel(string title, bool isGameOver)
        {
            _isOpen = true;
            _isGameOver = isGameOver;
            Time.timeScale = 0f;

            if (_titleText != null) _titleText.text = title;

            _panelGroup.interactable = true;
            _panelGroup.blocksRaycasts = true;
            _panelGroup.DOFade(1f, _fadeDuration).SetUpdate(true).SetLink(gameObject);

            _panelGroup.transform.localScale = Vector3.one * 0.9f;
            _panelGroup.transform.DOScale(1f, _fadeDuration)
                       .SetEase(Ease.OutBack).SetUpdate(true).SetLink(gameObject);
        }

        private void Resume()
        {
            _isOpen = false;
            Time.timeScale = 1f;

            _panelGroup.interactable = false;
            _panelGroup.blocksRaycasts = false;
            _panelGroup.DOFade(0f, _fadeDuration).SetUpdate(true).SetLink(gameObject);
        }

        private void SetPanelInstant(bool visible)
        {
            _panelGroup.alpha = visible ? 1f : 0f;
            _panelGroup.interactable = visible;
            _panelGroup.blocksRaycasts = visible;
        }

        // --- Buttons ---

        public void OnReplayClicked()
        {
            _audio.PlayUiClick();
            Time.timeScale = 1f;
            string current = SceneManager.GetActiveScene().name;
            _transition.LoadSceneAsync(current, 0f, this.GetCancellationTokenOnDestroy()).Forget();
        }

        public void OnCloseClicked()
        {
            _audio.PlayUiClick();
            Resume();
        }

        public void OnReturnMainMenuClicked()
        {
            _audio.PlayUiClick();
            Time.timeScale = 1f;
            _transition.LoadSceneAsync(SceneNames.MainMenu, 0f, this.GetCancellationTokenOnDestroy()).Forget();
        }
    }
}