// SceneTransitionController.cs
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GCNBC.Controllers
{
    // Centralized async scene loading. Lives in ProjectContext (persists across scenes).
    // Other systems call LoadSceneAsync to transition; progress is reported via callback.
    public class SceneTransitionController
    {
        // Optional: fire progress (0..1) so a loading bar can react. Decoupled from UI.
        public event Action<float> OnProgress;

        private bool _isLoading;

        // Loads a scene asynchronously. Reports progress, optionally waits for a minimum
        // time, and activates the scene when ready.
        public async UniTask LoadSceneAsync(string sceneName, float minDuration, CancellationToken ct)
        {
            if (_isLoading)
            {
                Debug.LogWarning("[SceneTransition] Already loading, ignoring request.");
                return;
            }
            _isLoading = true;

            try
            {
                var op = SceneManager.LoadSceneAsync(sceneName);
                op.allowSceneActivation = false;

                float elapsed = 0f;

                // Wait until both: scene loaded (0.9) AND min duration elapsed.
                while (op.progress < 0.9f || elapsed < minDuration)
                {
                    elapsed += Time.deltaTime;

                    // Combine real load progress (0..0.9 -> 0..1) with time progress.
                    float loadProgress = Mathf.Clamp01(op.progress / 0.9f);
                    float timeProgress = minDuration > 0f ? Mathf.Clamp01(elapsed / minDuration) : 1f;
                    float progress = Mathf.Min(loadProgress, timeProgress);

                    OnProgress?.Invoke(progress);
                    await UniTask.Yield(ct);
                }

                OnProgress?.Invoke(1f);
                await UniTask.Delay(TimeSpan.FromSeconds(0.2f), cancellationToken: ct);

                op.allowSceneActivation = true;
            }
            finally
            {
                _isLoading = false;
            }
        }
    }
}