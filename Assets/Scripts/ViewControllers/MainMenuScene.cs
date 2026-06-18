using Cysharp.Threading.Tasks;
using DG.Tweening;
using GCNBC.Constants;
using GCNBC.Controllers;
using UnityEngine;
using Zenject;

namespace GCNBC.ViewControllers
{
    public class MainMenuScene : MonoBehaviour
    {
        [Header("Start Button Pulse")]
        [SerializeField] private Transform _startButton;
        [Tooltip("How big it grows at the peak of the pulse (1.1 = 10% bigger).")]
        [SerializeField] private float _pulseScale = 1.1f;
        [Tooltip("Duration of one grow (or shrink) phase, in seconds.")]
        [SerializeField] private float _pulseDuration = 0.8f;

        private SceneTransitionController _sceneController;
        private AudioController _audioController;
        private Tween _pulseTween;

        [Inject]
        private void Construct(SceneTransitionController sceneController, AudioController audioController)
        {
            _sceneController = sceneController;
            _audioController = audioController;
        }

        private void Start()
        {
            _audioController.PlayMusic();
            StartPulse();
        }

        private void StartPulse()
        {
            // Grow to _pulseScale and back, looping forever with smooth easing.
            _pulseTween = _startButton
                .DOScale(_pulseScale, _pulseDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)   // -1 = infinite, Yoyo = grow then shrink
                .SetLink(gameObject);           // auto-kill when this object is destroyed
        }

        public void OnStartButtonClicked()
        {
            _audioController.PlayUiClick();
            _pulseTween?.Kill();   // stop the pulse when leaving the menu
            _sceneController.LoadSceneAsync(SceneNames.Gameplay, 0.5f, this.GetCancellationTokenOnDestroy()).Forget();
        }

        public void OnQuitButtonClicked()
        {
            _audioController.PlayUiClick();
            Application.Quit();
        }
    }
}