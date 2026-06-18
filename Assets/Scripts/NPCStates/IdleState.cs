// IdleState.cs
using GCNBC.Components;
using GCNBC.Enums.Components;

namespace GCNBC.NPCStates
{
    public class IdleState : BaseState<NpcComponent, NpcState>
    {
        protected override NpcState SetStateType() => NpcState.Idle;

        public override void Enter() 
        { 
            _playerController.Animation.SetSpeed(0f);
        }
        public override void Tick()
        {
            _playerController.ChangeState(NpcState.Evaluate);
        }
        public override void Exit() { }
    }
}