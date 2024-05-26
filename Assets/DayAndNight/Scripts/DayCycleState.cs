using DG.Tweening;

namespace DayAndNight
{
    public abstract class DayCycleState
    {
        protected DailyCycleStateMachine _stateMachine;
        protected Tween _rotationTween;

        public DayCycleState(DailyCycleStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public abstract void Enter();
        public abstract void Exit();
    }
}