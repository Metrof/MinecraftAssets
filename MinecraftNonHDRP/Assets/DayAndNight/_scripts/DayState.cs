using DG.Tweening;
using UnityEngine;

public class DayState : DayCycleState
{
    public DayState(DailyCycleStateMachine stateMachine) : base(stateMachine) { }
    public override void Enter()
    {
        EventBase.Invoke(new DayEvent());

        _stateMachine.SkyObjectRenderer.sprite = _stateMachine.SunSprite;

        _stateMachine.LightSource.color = Color.white;
        _stateMachine.LightSource.transform.localRotation = Quaternion.identity;

        _rotationTween.Kill();
        _rotationTween = _stateMachine.LightSource.transform.DOLocalRotate(new Vector3(180, 0, 0), _stateMachine.CycleDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() => _stateMachine.ChangeState(_stateMachine.NightCycleName));
    }
    public override void Exit()
    {
        _rotationTween.Kill();
    }
}

public class DayEvent { }
