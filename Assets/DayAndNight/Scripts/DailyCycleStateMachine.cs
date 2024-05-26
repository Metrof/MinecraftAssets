using System.Collections.Generic;
using UnityEngine;

namespace DayAndNight
{
    public class DailyCycleStateMachine : MonoBehaviour
    {
        private float _timeOfDay;

        [SerializeField] private float _cycleDuration = 10f;
        [SerializeField][Range(-1000, 0)] private float _skyObjectDistance = -200;

        [Header("Day Settings")]
        [SerializeField][Range(0, 1)] private float _sunIntensity = 1;
        [SerializeField] private Color _sunColor = Color.white;

        [Header("Night Settings")]
        [SerializeField][Range(0, 1)] private float _moonIntensity = 1;
        [SerializeField] private Color _moonColor = new Color(0.3f, 0.88f, 1f);

        [Space]

        [SerializeField] private AnimationCurve _skyboxCurve;

        [SerializeField] private Sprite _sunSprite;
        [SerializeField] private Sprite _moonSprite;
        [SerializeField] private SpriteRenderer _skyObjectRenderer;

        [SerializeField] private Light _lightSource;

        private SkyboxBlender _blender;
        private DayCycleState _currentState;
        private int _curveDir = 1;

        private Dictionary<string, DayCycleState> _states = new();

        public const string DayCycleName = "Day";
        public const string NightCycleName = "Night";

        public float SunIntensity => _sunIntensity;
        public Color SunColor => _sunColor;
        public float MoonIntensity => _moonIntensity;
        public Color MoonColor => _moonColor;

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

            Transform skyobjecttransform = _lightSource.GetComponentInChildren<SpriteRenderer>().gameObject.transform;
            skyobjecttransform.localPosition = new Vector3(0, 0, _skyObjectDistance);
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

            _blender.ChangeBlendValue(_skyboxCurve.Evaluate(_timeOfDay));

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
}
