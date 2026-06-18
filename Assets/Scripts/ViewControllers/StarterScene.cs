using Cysharp.Threading.Tasks;
using DG.Tweening;
using GCNBC.Constants;
using GCNBC.Controllers;
using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

using Random = UnityEngine.Random;

namespace GCNBC.ViewControllers
{
    public class StarterScene : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Slider _progressBar;
        [SerializeField] private TextMeshProUGUI _progressText;   // TMP ise TMP_Text + using TMPro

        [Header("Loading")]
        [Tooltip("How long the bar takes to fill (seconds).")]
        [SerializeField] private float _fillDuration = 3f;
        [SerializeField] private Ease _ease = Ease.InOutSine;
        [SerializeField] private string _gameSceneName = "GameScene";
        
        private SceneTransitionController _sceneController;

        [Inject]
        private void Construct(SceneTransitionController sceneController)
        {
            _sceneController = sceneController;
        }
        
        private void Start()
        {
            //RunLoadingAsync(this.GetCancellationTokenOnDestroy()).Forget();
            HandleDummyLoading();
        }

        private async UniTaskVoid RunLoadingAsync(CancellationToken ct)
        {
            // Start loading the game scene in the background (don't activate yet).
            var op = SceneManager.LoadSceneAsync(_gameSceneName);
            op.allowSceneActivation = false;

            // Animate the slider 0 -> 1 with DOTween easing, updating the text as it goes.
            _progressBar.value = 0f;
            Tween fillTween = _progressBar
                .DOValue(1f, _fillDuration)
                .SetEase(_ease)
                .OnUpdate(() => UpdateText(_progressBar.value));

            // Wait for BOTH the bar animation AND the scene to be ready (0.9 = loaded, awaiting activation).
            await UniTask.WaitUntil(
                () => !fillTween.IsActive() || fillTween.IsComplete(),
                cancellationToken: ct);

            // Make sure the scene finished loading (usually already done by now).
            await UniTask.WaitUntil(() => op.progress >= 0.9f, cancellationToken: ct);

            UpdateText(1f);
            await UniTask.Delay(TimeSpan.FromSeconds(0.3f), cancellationToken: ct);

            // Activate the loaded scene.
            op.allowSceneActivation = true;
        }

        private void UpdateText(float value01)
        {
            if (_progressText != null)
                _progressText.text = $"%{Mathf.RoundToInt(value01 * 100)}";
        }

        private void HandleDummyLoading()
        {
            Sequence startSequence = DOTween.Sequence();

            float random1 = Random.Range(0.7f, 0.75f);
            float random2 = Random.Range(0.85f, 0.9f);
            Slider slider = _progressBar;

            startSequence.Append(slider.DOValue(random1, 3f).SetEase(Ease.Linear));
            startSequence.AppendInterval(0.1f);
            startSequence.Append(slider.DOValue(random2, 1.5f).SetEase(Ease.Linear));
            startSequence.AppendInterval(0.1f);
            startSequence.Append(slider.DOValue(1, 0.5f).SetEase(Ease.Linear));
            startSequence.OnUpdate(() =>
            {
                _progressText.text = "Loading...%" + (slider.value * 100).ToString("F0");
            });
            startSequence.SetEase(Ease.Linear);
            startSequence.OnComplete(() =>
            {
                _sceneController.LoadSceneAsync(SceneNames.MainMenu, 0.5f, this.GetCancellationTokenOnDestroy()).Forget();
            });
        }
    }

}
