// SceneAudioBinder.cs
using GCNBC.Controllers;
using GCNBC.Signals;
using System;
using Zenject;

namespace GCNBC.Services
{
    // Bridge between scene-scoped signals and the persistent (project-scoped) AudioController.
    // Lives in the SceneContext: subscribes to this scene's signals and routes them to audio.
    // Cleans up on scene teardown via IDisposable.
    public class SceneAudioBinder : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;      // scene's SignalBus (SceneContext)
        private readonly AudioController _audio;    // persistent audio (ProjectContext, parent)

        public SceneAudioBinder(SignalBus signalBus, AudioController audio)
        {
            _signalBus = signalBus;
            _audio = audio;
        }

        // Subscribe when the scene starts.
        public void Initialize()
        {
            _signalBus.Subscribe<BallCollectedSignal>(OnBallCollected);
            _signalBus.Subscribe<NpcDiedSignal>(OnNpcDied);
            _signalBus.Subscribe<NpcSpawnedSignal>(OnNpcSpawned);
            _signalBus.Subscribe<BallPickedUpSignal>(OnGolfBallPickedUp);
        }

        // Unsubscribe when the scene is torn down (audio persists, subscriptions don't).
        public void Dispose()
        {
            _signalBus.Unsubscribe<BallCollectedSignal>(OnBallCollected);
            _signalBus.Unsubscribe<NpcDiedSignal>(OnNpcDied);
            _signalBus.Unsubscribe<NpcSpawnedSignal>(OnNpcSpawned);
        }

        // Map each signal to an audio call. Audio doesn't know about signals;
        // this binder owns the "which signal -> which sound" mapping.
        private void OnBallCollected(BallCollectedSignal s) => _audio.PlayBallCollected();
        private void OnNpcDied(NpcDiedSignal s) => _audio.PlayNpcDied();
        private void OnNpcSpawned(NpcSpawnedSignal s) => _audio.PlayNpcSpawned();

        private void OnGolfBallPickedUp(BallPickedUpSignal s) => _audio.PlayBallPickedUp();
    }
}