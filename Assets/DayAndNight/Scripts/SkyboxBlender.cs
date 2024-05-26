using UnityEngine;
using UnityEngine.Rendering;

namespace DayAndNight
{
    [ExecuteInEditMode]
    public class SkyboxBlender : MonoBehaviour
    {
        private enum BlendMode { Linear, Smoothstep, Maximum, Add, Substract, Multiply }
        private enum ProbeResolution { _16, _32, _64, _128, _256, _512, _1024, _2048 }

        [Header("Input Skyboxes")]
        [SerializeField] private Material _nightSkyBox;
        [SerializeField] private Material _daySkyBox;

        [Header("Blended Skybox")]
        [SerializeField] private Material _blendedSkybox;
        [SerializeField][Range(0, 8)] private float _exposure = 0.5f;
        [SerializeField][Range(0, 360)] private float _rotation = 0;
        [SerializeField] private Color _tint = Color.white;
        [SerializeField][Range(0, 1)] private float _invertColors = 0;
        [SerializeField] private BlendMode _blendMode = BlendMode.Linear;
        [SerializeField][Range(0, 1)] private float _blend = 0;

        [SerializeField] private bool _bindOnStart = true;
        [SerializeField] private bool _updateReflectionsOnStart = true;
        [SerializeField] private bool _updateReflectionsEveryFrame = true;

        [SerializeField] private ProbeResolution _reflectionResolution = ProbeResolution._128;

        private ReflectionProbe _probeComponent = null;
        private GameObject _probeGameObject = null;
        private Cubemap _blendedCubemap = null;
        private int _renderId = -1;

        #region MonoBehaviour Functions

        private void Start()
        {

            if (_bindOnStart)
                BindTextures();

            UpdateBlendedMaterialParameters();

            if (_updateReflectionsOnStart)
                UpdateReflections();
        }

        private void Update()
        {
            if (_updateReflectionsEveryFrame)
                UpdateReflections();
        }

        #endregion

        private int GetProbeResolution(ProbeResolution probeResolution)
        {
            return probeResolution switch
            {
                ProbeResolution._16 => 16,
                ProbeResolution._32 => 32,
                ProbeResolution._64 => 64,
                ProbeResolution._128 => 128,
                ProbeResolution._256 => 256,
                ProbeResolution._512 => 512,
                ProbeResolution._1024 => 1024,
                ProbeResolution._2048 => 2048,
                _ => 128,
            };
        }

        private void CreateReflectionProbe()
        {
            //Search for the reflection probe object
            _probeGameObject = GameObject.Find("Skybox Blender Reflection Probe");

            if (!_probeGameObject)
            {
                //Create the gameobject if its not here
                _probeGameObject = new GameObject("Skybox Blender Reflection Probe");
                _probeGameObject.transform.parent = gameObject.transform;
                // Use a location such that the new Reflection Probe will not interfere with other Reflection Probes in the scene.
                _probeGameObject.transform.position = new Vector3(0, -1000, 0);
            }

            _probeComponent = _probeGameObject.GetComponent<ReflectionProbe>();

            if (_probeComponent)
            {
                DestroyImmediate(_probeComponent);
            }

            // Create a Reflection Probe that only contains the Skybox. The Update function controls the Reflection Probe refresh.
            _probeComponent = _probeGameObject.AddComponent<ReflectionProbe>() as ReflectionProbe;

        }

        public void UpdateReflectionProbe()
        {
            CreateReflectionProbe();

            _probeComponent.resolution = GetProbeResolution(_reflectionResolution);
            _probeComponent.size = new Vector3(1, 1, 1);
            _probeComponent.cullingMask = 0;
            _probeComponent.clearFlags = ReflectionProbeClearFlags.Skybox;
            _probeComponent.mode = ReflectionProbeMode.Realtime;
            _probeComponent.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
            _probeComponent.timeSlicingMode = ReflectionProbeTimeSlicingMode.NoTimeSlicing;

            // A cubemap is used as a default specular reflection.
            _blendedCubemap = new Cubemap(_probeComponent.resolution, _probeComponent.hdr ? TextureFormat.RGBAHalf : TextureFormat.RGBA32, true);

            //Set the render reflection mode to Custom
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
            RenderSettings.customReflectionTexture = _blendedCubemap;
        }

        public void UpdateLighting()
        {
            DynamicGI.UpdateEnvironment();
        }

        public void UpdateReflections()
        {
            if (!_probeGameObject || !_probeComponent)
                UpdateReflectionProbe();

            // The Update function refreshes the Reflection Probe and copies the result to the default specular reflection Cubemap.

            // The texture associated with the real-time Reflection Probe is a render target and RenderSettings.customReflection is a Cubemap. We have to check the support if copying from render targets to Textures is supported.
            if ((SystemInfo.copyTextureSupport & CopyTextureSupport.RTToTexture) != 0)
            {
                // Wait until previous RenderProbe is finished before we refresh the Reflection Probe again.
                // renderId is a token used to figure out when the refresh of a Reflection Probe is finished. The refresh of a Reflection Probe can take mutiple frames when time-slicing is used.
                if (_renderId == -1 || _probeComponent.IsFinishedRendering(_renderId))
                {
                    if (_probeComponent.IsFinishedRendering(_renderId))
                    {
                        // After the previous RenderProbe is finished, we copy the probe's texture to the cubemap and set it as a custom reflection in RenderSettings.
                        if (_probeComponent.texture != null)
                        {
                            if (_probeComponent.texture.width == _blendedCubemap.width && _probeComponent.texture.height == _blendedCubemap.height)
                            {
                                Graphics.CopyTexture(_probeComponent.texture, _blendedCubemap as Texture);
                            }
                        }

                        RenderSettings.customReflectionTexture = _blendedCubemap;
                    }

                    _renderId = _probeComponent.RenderProbe();
                }
            }
        }

        private int GetBlendModeIndex(BlendMode blendMode)
        {
            return blendMode switch
            {
                BlendMode.Linear => 0,
                BlendMode.Smoothstep => 5,
                BlendMode.Maximum => 1,
                BlendMode.Add => 2,
                BlendMode.Substract => 3,
                BlendMode.Multiply => 4,
                _ => 0,
            };
        }

        public void BindTextures()
        {
            _blendedSkybox.SetTexture("_FrontTex_1", _nightSkyBox.GetTexture("_FrontTex"));
            _blendedSkybox.SetTexture("_BackTex_1", _nightSkyBox.GetTexture("_BackTex"));
            _blendedSkybox.SetTexture("_LeftTex_1", _nightSkyBox.GetTexture("_LeftTex"));
            _blendedSkybox.SetTexture("_RightTex_1", _nightSkyBox.GetTexture("_RightTex"));
            _blendedSkybox.SetTexture("_UpTex_1", _nightSkyBox.GetTexture("_UpTex"));
            _blendedSkybox.SetTexture("_DownTex_1", _nightSkyBox.GetTexture("_DownTex"));

            _blendedSkybox.SetTexture("_FrontTex_2", _daySkyBox.GetTexture("_FrontTex"));
            _blendedSkybox.SetTexture("_BackTex_2", _daySkyBox.GetTexture("_BackTex"));
            _blendedSkybox.SetTexture("_LeftTex_2", _daySkyBox.GetTexture("_LeftTex"));
            _blendedSkybox.SetTexture("_RightTex_2", _daySkyBox.GetTexture("_RightTex"));
            _blendedSkybox.SetTexture("_UpTex_2", _daySkyBox.GetTexture("_UpTex"));
            _blendedSkybox.SetTexture("_DownTex_2", _daySkyBox.GetTexture("_DownTex"));
        }

        void UpdateBlendedMaterialParameters()
        {
            _blendedSkybox.SetColor("_Tint", _tint);
            _blendedSkybox.SetFloat("_Exposure", _exposure);
            _blendedSkybox.SetFloat("_Rotation", _rotation);
            _blendedSkybox.SetFloat("_Blend", _blend);
            _blendedSkybox.SetInt("_BlendMode", GetBlendModeIndex(_blendMode));
            _blendedSkybox.SetFloat("_InvertColors", _invertColors);

            UpdateLighting();
        }
        public void ChangeBlendValue(float blend)
        {
            _blend = blend;
            UpdateBlendedMaterialParameters();
        }
    }
}