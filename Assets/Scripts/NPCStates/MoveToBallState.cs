
using GCNBC.Components;
using GCNBC.Enums.Components;
using GCNBC.Signals;
using UnityEngine;

namespace GCNBC.NPCStates
{
    public class MoveToBallState : BaseState<NpcComponent, NpcState>
    {
        protected override NpcState SetStateType() => NpcState.MoveToBall;

        public override void Enter()
        {
            var target = EvaluateState._target;
            if (target == null)
            {
                _playerController.ChangeState(NpcState.Evaluate);
                return;
            }

            var agent = _playerController.Agent;

            // Guard: only touch the agent if it's actually on the NavMesh.
            if (!agent.isOnNavMesh)
                return;   // skip this frame; Tick will retry or controller re-evaluates

            agent.isStopped = false;
            agent.SetDestination(target.transform.position);
            _playerController.Animation.SetSpeed(_playerController.Speed);
        }

        public override void Tick()
        {
            var target = EvaluateState._target;
            if (target == null)
            {
                _playerController.ChangeState(NpcState.Evaluate);
                return;
            }

            var agent = _playerController.Agent;

            // Guard: agent must be on the NavMesh before querying it.
            if (!agent.isOnNavMesh)
                return;

            if (!agent.pathPending && agent.remainingDistance <= _playerController.ArriveDistance)
            {
                _playerController.CarriedBall = target;

                // Attach the ball to the NPC's carry point so it travels with the NPC.
                AttachBallToNpc(target);
                _playerController.SignalBus.Fire(new BallPickedUpSignal(target.Level));
                EvaluateState._target = null;
                _playerController.ChangeState(NpcState.ReturnToCart);
            }
        }

        // MoveToBallState içine yardımcı metod
        private void AttachBallToNpc(BallComponent ball)
        {
            var carryPoint = _playerController.CarryPoint;

            var rb = ball.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            var col = ball.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            ball.transform.SetParent(carryPoint != null ? carryPoint : _playerController.transform);
            ball.transform.localPosition = Vector3.zero;
        }

        public override void Exit() { }
    }
}