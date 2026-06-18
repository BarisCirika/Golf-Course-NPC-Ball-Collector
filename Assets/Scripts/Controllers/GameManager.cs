using GCNBC.Components;
using GCNBC.Signals;
using UnityEngine;
using Zenject;


namespace GCNBC.Services
{
    public class GameManager : MonoBehaviour
    {
        private IScoreService _scoreManager;
        private IBallProvider _spawner;
        private SignalBus _signalBus;

        [Inject]
        private void Construct(IScoreService scoreService, IBallProvider spawner, SignalBus signalBus)
        {
            _scoreManager = scoreService;
            _spawner = spawner;
            _signalBus = signalBus;
        }

        // Called when a ball is collected (by the NPC's collision logic).
        public void HandleBallCollected(BallComponent ball)
        {
            if (ball == null) return;

            _scoreManager.Add(ball.Points);
            _signalBus.Fire(new BallCollectedSignal(ball.Level, ball.Points));

            _spawner.Release(ball);   // spawner only returns the ball to its pool now
        }
    }
}

