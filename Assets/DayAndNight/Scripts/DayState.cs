using DG.Tweening;
using UnityEngine;

namespace DayAndNight
{
    public class DayState : DayCycleState
    {
        public DayState(DailyCycleStateMachine stateMachine) : base(stateMachine) { }
        public override void Enter()
        {
            EventBase.Invoke(new DayEvent());

            _stateMachine.SkyObjectRenderer.sprite = _stateMachine.SunSprite;

            _stateMachine.LightSource.color = _stateMachine.SunColor;
            _stateMachine.LightSource.intensity = _stateMachine.SunIntensity;
            _stateMachine.LightSource.transform.localRotation = Quaternion.Euler(-10, 0, 0);

            _rotationTween.Kill();
            _rotationTween = _stateMachine.LightSource.transform.DOLocalRotate(new Vector3(210, 0, 0), _stateMachine.CycleDuration, RotateMode.LocalAxisAdd)
                .SetEase(Ease.Linear)
                .OnComplete(() => _stateMachine.ChangeState(DailyCycleStateMachine.NightCycleName));
        }
        public override void Exit()
        {
            _rotationTween.Kill();
        }
    }

    public class DayEvent { }
}