using System.Collections.Generic;
using UnityEngine;

public class DailyCycleStateMachine : MonoBehaviour
{
    private float _timeOfDay;

    [SerializeField] private float _cycleDuration = 10f;

    [SerializeField] private AnimationCurve _skyboxCurve;

    [SerializeField] private Sprite _sunSprite;
    [SerializeField] private Sprite _moonSprite;
    [SerializeField] private SpriteRenderer _skyObjectRenderer;

    [SerializeField] private Light _lightSource;

    private SkyboxBlender _blender;
    private DayCycleState _currentState;
    private int _curveDir = 1;

    private Dictionary<string, DayCycleState> _states = new();

    public string DayCycleName { get { return "Day"; } }
    public string NightCycleName { get { return "Night"; } }

    public Sprite SunSprite => _sunSprite;
    public Sprite MoonSprite => _moonSprite;
    public SpriteRenderer SkyObjectRenderer => _skyObjectRenderer;
    public float CycleDuration => _cycleDuration;
    public Light LightSource => _lightSource;

    private void Awake()
    {
        _blender = GetComponent<SkyboxBlender>();
        _states.Add(DayCycleName, new DayState(this));
        _states.Add(NightCycleName, new NightState(this));
    }
    private void OnDisable()
    {
        _currentState?.Exit();
    }
    private void Start()
    {
        ChangeState(DayCycleName);
        RenderSettings.sun = _lightSource;
        DynamicGI.UpdateEnvironment();
    }
    private void Update()
    {
        _timeOfDay += Time.deltaTime / _cycleDuration * _curveDir;

        _blender.blend = _skyboxCurve.Evaluate(_timeOfDay);

        if (Mathf.Abs(_timeOfDay) >= 1)
        {
            _timeOfDay = 0;
            _curveDir *= -1;
        }
    }
    public void ChangeState(string stateName)
    {
        _currentState?.Exit();
        _currentState = _states[stateName];
        _currentState.Enter();
    }
}
