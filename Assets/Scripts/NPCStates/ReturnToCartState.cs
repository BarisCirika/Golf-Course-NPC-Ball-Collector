using GCNBC.Components;
using GCNBC.Enums.Components;
using GCNBC.Enums.Model;
using GCNBC.Signals;
using UnityEngine;

namespace GCNBC.NPCStates
{
    public class ReturnToCartState : BaseState<NpcComponent, NpcState>
    {
        protected override NpcState SetStateType() => NpcState.ReturnToCart;

        public override void Enter()
        {
            var agent = _playerController.Agent;
            if (!agent.isOnNavMesh) return;

            agent.isStopped = false;
            agent.SetDestination(_playerController.Cart.Position);
            _playerController.Animation.SetSpeed(_playerController.Speed);
            _playerController.Animation.SetCarrying(true);
        }

        public override void Tick()
        {
            var agent = _playerController.Agent;
            if (!agent.isOnNavMesh) return;   // guard

            if (!agent.pathPending && agent.remainingDistance <= _playerController.ArriveDistance)
            {
                DeliverBall();
                _playerController.ChangeState(NpcState.Evaluate);
            }
        }

        public override void Exit() { }

        private void DeliverBall()
        {
            var ball = _playerController.CarriedBall;
            if (ball == null) return;

            ball.transform.SetParent(null);

            var rb = ball.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = false;
            var col = ball.GetComponent<Collider>();
            if (col != null) col.enabled = true;

            _playerController.SignalBus.Fire(new BallCollectedSignal(ball.Level, ball.Points));

            _playerController.ScoreManager.Add(ball.Points);
            _playerController.BallProvider.Release(ball);
            _playerController.Cart.OnBallDelivered();
            _playerController.AddHealth(_playerController.HealthPerDelivery);
            _playerController.CarriedBall = null;
            _playerController.Animation.SetCarrying(false);
        }
    }
}