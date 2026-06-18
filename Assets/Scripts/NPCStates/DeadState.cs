// DeadState.cs
using GCNBC.Components;
using GCNBC.Enums.Components;
using GCNBC.Signals;
using Zenject;

namespace GCNBC.NPCStates
{
    public class DeadState : BaseState<NpcComponent, NpcState>
    {
        protected override NpcState SetStateType() => NpcState.Dead;

        public override void Enter()
        {
            if (_playerController.Agent.isOnNavMesh)
                _playerController.Agent.isStopped = true;

            _playerController.Animation.SetDead(true);
            _playerController.SignalBus.Fire(new NpcDiedSignal());
        }
        public override void Tick() { }
        public override void Exit() { }
    }
}