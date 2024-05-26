using DG.Tweening;
using UnityEngine;

namespace DayAndNight
{
    public class NightState : DayCycleState
    {
        public NightState(DailyCycleStateMachine stateMachine) : base(stateMachine) { }
        public override void Enter()
        {
            EventBase.Invoke(new NightEvent());

            _stateMachine.SkyObjectRenderer.sprite = _stateMachine.MoonSprite;

            _stateMachine.LightSource.color = _stateMachine.MoonColor;
            _stateMachine.LightSource.intensity = _stateMachine.MoonIntensity;
            _stateMachine.LightSource.transform.localRotation = Quaternion.Euler(-10, 0, 0);

            _rotationTween.Kill();
            _rotationTween = _stateMachine.LightSource.transform.DOLocalRotate(new Vector3(210, 0, 0), _stateMachine.CycleDuration, RotateMode.LocalAxisAdd)
                .SetEase(Ease.Linear)
                .OnComplete(() => _stateMachine.ChangeState(DailyCycleStateMachine.DayCycleName));
        }
        public override void Exit()
        {
            _rotationTween.Kill();
        }
    }
    public class NightEvent { }
}