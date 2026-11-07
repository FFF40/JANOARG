using UnityEngine;
using UnityEngine.Rendering;

namespace JANOARG.Client.Behaviors.SongSelect.Map
{
    /// <summary>
    ///     URP replacement for the Built-in light halo. Add this to a light (or any object)
    ///     and tweak Tint/Size/Intensity in the inspector. The halo is an additive,
    ///     camera-facing billboard and is not saved as a child object in the scene.
    /// </summary>
    [ExecuteAlways]
    public class LightHalo : MonoBehaviour
    {
        public Color Tint = Color.white;
        [Min(0.01f)] public float Size = 1f;
        [Min(0f)]    public float Intensity = 1f;

        [Tooltip("Keep the hot white center. Off makes the halo a soft glow.")]
        public bool BrightCore = false;

        [Range(0f, 1f)]
        [Tooltip("Size of the bright core. Only used when Bright Core is enabled.")]
        public float CoreSize = 0.25f;

        static Material s_Material;
        static Mesh     s_Quad;

        MeshRenderer _renderer;

        void OnEnable()
        {
            EnsureHalo();
        }

        void OnValidate()
        {
            if (isActiveAndEnabled)
                EnsureHalo();
        }

        void OnDisable()
        {
            Cleanup();
        }

        void OnDestroy()
        {
            Cleanup();
        }

        public void Refresh()
        {
            EnsureHalo();
        }

        void EnsureHalo()
        {
            if (s_Material == null)
                s_Material = Resources.Load<Material>("Materials/Map/Light Halo");

            if (s_Quad == null)
                s_Quad = Resources.GetBuiltinResource<Mesh>("Quad.fbx");

            if (s_Material == null || s_Quad == null)
                return;

            if (_renderer == null)
            {
                Transform existing = transform.Find("Light Halo");
                if (existing != null)
                    _renderer = existing.GetComponent<MeshRenderer>();

                if (_renderer == null)
                {
                    GameObject halo = new("Light Halo")
                    {
                        hideFlags = HideFlags.HideAndDontSave,
                        layer     = gameObject.layer,
                    };

                    halo.transform.SetParent(transform, false);

                    MeshFilter filter = halo.AddComponent<MeshFilter>();
                    filter.sharedMesh = s_Quad;

                    _renderer = halo.AddComponent<MeshRenderer>();
                    _renderer.sharedMaterial             = s_Material;
                    _renderer.shadowCastingMode          = ShadowCastingMode.Off;
                    _renderer.receiveShadows             = false;
                    _renderer.lightProbeUsage            = LightProbeUsage.Off;
                    _renderer.reflectionProbeUsage       = ReflectionProbeUsage.Off;
                    _renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                    _renderer.localBounds                = new Bounds(Vector3.zero, Vector3.one * 100f);
                }
            }

            MaterialPropertyBlock props = new();
            props.SetColor("_Color", Tint * Mathf.Max(0f, Intensity));
            props.SetFloat("_Size", Mathf.Max(0.01f, Size));
            props.SetFloat("_NoCore", BrightCore ? 0f : 1f);
            props.SetFloat("_CoreSize", Mathf.Clamp01(CoreSize));
            _renderer.SetPropertyBlock(props);
        }

        void Cleanup()
        {
            _renderer = null;

            Transform child = transform.Find("Light Halo");
            if (child == null)
                return;

            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
    }
}
