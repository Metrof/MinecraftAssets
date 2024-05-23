using DG.Tweening;
using UnityEngine;

public class NightState : DayCycleState
{
    public NightState(DailyCycleStateMachine stateMachine) : base(stateMachine) { }
    public override void Enter()
    {
        EventBase.Invoke(new NightEvent());

        _stateMachine.SkyObjectRenderer.sprite = _stateMachine.MoonSprite;

        _stateMachine.LightSource.color = new Color(0.3f, 0.88f, 1f);
        _stateMachine.LightSource.transform.localRotation = Quaternion.identity;

        _rotationTween.Kill();
        _rotationTween = _stateMachine.LightSource.transform.DOLocalRotate(new Vector3(180, 0, 0), _stateMachine.CycleDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() => _stateMachine.ChangeState(_stateMachine.DayCycleName));
    }
    public override void Exit()
    {
        _rotationTween.Kill();
    }
}
public class NightEvent { }
