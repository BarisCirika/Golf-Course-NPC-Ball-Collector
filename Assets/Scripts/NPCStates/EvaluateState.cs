using GCNBC.Components;
using GCNBC.Enums.Components;
using UnityEngine;

namespace GCNBC.NPCStates
{
    public class EvaluateState : BaseState<NpcComponent, NpcState>
    {
        protected override NpcState SetStateType() => NpcState.Evaluate;

        public override void Enter()
        {
            BallComponent best = ChooseBestBall();

            if (best == null)
            {
                // No balls available -> idle.
                _playerController.ChangeState(NpcState.Idle);
                return;
            }

            // Remember the target and go get it.
            _playerController.CarriedBall = null;
            _target = best;
            _playerController.ChangeState(NpcState.MoveToBall);
        }

        public override void Tick() { }
        public override void Exit() { }

        // Static handoff of the chosen target to MoveToBall (kept simple for the demo).
        public static BallComponent _target;

        // Utility scoring: balance value vs distance, penalised more when health is low.
        private BallComponent ChooseBestBall()
        {
            var balls = _playerController.BallProvider.ActiveBalls;
            if (balls == null || balls.Count == 0) return null;

            Vector3 npcPos = _playerController.transform.position;
            Vector3 cartPos = _playerController.Cart.Position;

            float health = _playerController.CurrentHealth;
            float drainPerSec = _playerController.HealthDrainPerSecond;   // controller'dan expose et
            float speed = _playerController.Agent.speed;                  // gerçek hız
            if (speed <= 0.01f) speed = 1f;                               // sıfıra bölme koruması

            BallComponent best = null;
            float bestScore = float.NegativeInfinity;

            foreach (var ball in balls)
            {
                if (ball == null) continue;

                Vector3 ballPos = ball.transform.position;

                // Total travel: NPC -> ball -> cart (full round trip to actually score).
                float toBall = Vector3.Distance(npcPos, ballPos);
                float ballToCart = Vector3.Distance(ballPos, cartPos);
                float totalDistance = toBall + ballToCart;

                // Convert distance to TIME (scale-independent) and then to health cost.
                float travelTime = totalDistance / speed;
                float healthCost = travelTime * drainPerSec;

                // Can the NPC survive this trip? If not, it's not viable.
                // Add a safety margin so it doesn't cut it too close.
                float safetyMargin = 1.2f;
                if (healthCost * safetyMargin >= health)
                    continue;   // would die before delivering -> skip this ball

                // Efficiency: points earned per second of effort. Higher = better.
                float efficiency = ball.Points / travelTime;

                // When health is low, prefer SAFER (closer) balls even if less efficient.
                // healthFactor 0..1; low health amplifies the preference for short trips.
                float healthFactor = health / _playerController.MaxHealth;
                float reachabilityBonus = (1f - healthFactor) * (1f / travelTime) * 10f;

                float score = efficiency + reachabilityBonus;

                if (score > bestScore)
                {
                    bestScore = score;
                    best = ball;
                }
            }

            return best;
        }
    }
}