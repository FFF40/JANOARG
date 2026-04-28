using UnityEngine;
using UnityEngine.UI;

namespace JANOARG.Client.UI
{
    /// <summary>
    /// GPU-rendered circle / ring / regular-polygon graphic for Unity UI.
    ///
    /// Unlike <see cref="GraphicCircle"/>, this component renders the shape
    /// entirely in a fragment shader on a single quad (4 vertices, 2 triangles).
    /// This means:
    ///   • No CPU tessellation — zero per-frame vertex allocation.
    ///   • Perfectly smooth circles at any resolution.
    ///   • Polygons are pixel-perfect (no staircase artefacts).
    ///   • All parameters are driven via a MaterialPropertyBlock-style
    ///     Material instance so instances don't share state.
    ///
    /// Polygon mode: set <see cref="Sides"/> to 3 or higher.
    ///   Sides = 3  → triangle
    ///   Sides = 4  → square / diamond
    ///   Sides = 6  → hexagon
    ///   Sides = 0  → smooth circle (default)
    ///
    /// Fill / ring / rotation work identically to <see cref="GraphicCircle"/>.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(CanvasRenderer))]
    public class GraphicCircleGPU : MaskableGraphic
    {
        // ------------------------------------------------------------------ //
        // Serialised fields
        // ------------------------------------------------------------------ //

        [Tooltip("Number of polygon sides. 0 = smooth circle.")]
        [SerializeField] private int Sides = 0;

        [Tooltip("Fraction of the shape to fill, clockwise from top.")]
        [SerializeField][Range(0, 1)] private float FillAmount = 1f;

        [Tooltip("Inner hole radius as a fraction of the outer radius.")]
        [SerializeField][Range(0, 1)] private float InsideRadius = 0f;

        [Tooltip("Rotation offset in degrees (0 = top / 12-o'clock).")]
        [SerializeField][Range(0, 360)] private float Rotation = 0f;

        // ------------------------------------------------------------------ //
        // Public API (mirrors GraphicCircle for easy drop-in replacement)
        // ------------------------------------------------------------------ //

        public int sides
        {
            get => Sides;
            set { Sides = Mathf.Max(0, value); UpdateMaterialProperties(); }
        }

        public float fillAmount
        {
            get => FillAmount;
            set { FillAmount = Mathf.Clamp01(value); UpdateMaterialProperties(); }
        }

        public float insideRadius
        {
            get => InsideRadius;
            set { InsideRadius = Mathf.Clamp01(value); UpdateMaterialProperties(); }
        }

        public float rotation
        {
            get => Rotation;
            set { Rotation = value % 360f; UpdateMaterialProperties(); }
        }

        // ------------------------------------------------------------------ //
        // Private state
        // ------------------------------------------------------------------ //

        private static readonly int PropFillAmount   = Shader.PropertyToID("_FillAmount");
        private static readonly int PropInsideRadius = Shader.PropertyToID("_InsideRadius");
        private static readonly int PropSides        = Shader.PropertyToID("_Sides");
        private static readonly int PropRotation     = Shader.PropertyToID("_Rotation");

        private static Shader _circleShader;
        private Material _sharedMat;   // owns the Material (we create it)

        // ------------------------------------------------------------------ //
        // Initialisation
        // ------------------------------------------------------------------ //

        protected override void Awake()
        {
            base.Awake();
            EnsureMaterial();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            EnsureMaterial();
            UpdateMaterialProperties();
        }

        private void EnsureMaterial()
        {
            if (_circleShader == null)
                _circleShader = Shader.Find("UI/Circle");

            if (_circleShader == null)
            {
                Debug.LogError("[GraphicCircleGPU] Could not find shader 'UI/Circle'. " +
                               "Make sure UICircle.shader is in a Resources/Shaders folder.");
                return;
            }

            if (_sharedMat == null)
            {
                _sharedMat = new Material(_circleShader) { hideFlags = HideFlags.HideAndDontSave };
                material = _sharedMat;
            }
        }

        // ------------------------------------------------------------------ //
        // Mesh — just a single quad; the shader does all the work
        // ------------------------------------------------------------------ //

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            var rect   = rectTransform.rect;
            var color32 = color;

            // Four corners, UVs in [0,1]
            vh.AddVert(new Vector3(rect.xMin, rect.yMin), color32, new Vector2(0, 0));
            vh.AddVert(new Vector3(rect.xMin, rect.yMax), color32, new Vector2(0, 1));
            vh.AddVert(new Vector3(rect.xMax, rect.yMax), color32, new Vector2(1, 1));
            vh.AddVert(new Vector3(rect.xMax, rect.yMin), color32, new Vector2(1, 0));

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);

            // Sync shader params whenever the mesh is rebuilt (covers initial
            // population as well as inspector-driven dirty-marks).
            UpdateMaterialProperties();
        }

        // ------------------------------------------------------------------ //
        // Shader parameter sync
        // ------------------------------------------------------------------ //

        private void UpdateMaterialProperties()
        {
            if (_sharedMat == null) EnsureMaterial();
            if (_sharedMat == null) return;

            _sharedMat.SetFloat(PropFillAmount,   FillAmount);
            _sharedMat.SetFloat(PropInsideRadius, InsideRadius);
            _sharedMat.SetFloat(PropSides,        Sides);
            _sharedMat.SetFloat(PropRotation,     Rotation);
        }

        // ------------------------------------------------------------------ //
        // Editor support
        // ------------------------------------------------------------------ //

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            Sides        = Mathf.Max(0, Sides);
            FillAmount   = Mathf.Clamp01(FillAmount);
            InsideRadius = Mathf.Clamp01(InsideRadius);
            Rotation     = Rotation % 360f;
            EnsureMaterial();
            UpdateMaterialProperties();
            SetVerticesDirty();
        }
#endif

        // ------------------------------------------------------------------ //
        // Cleanup
        // ------------------------------------------------------------------ //

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_sharedMat != null)
            {
#if UNITY_EDITOR
                DestroyImmediate(_sharedMat);
#else
                Destroy(_sharedMat);
#endif
                _sharedMat = null;
            }
        }
    }
}
