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
    ///   1. No CPU tessellation — zero per-frame vertex allocation.
    ///   2. Perfectly smooth circles at any resolution.
    ///   3. Polygons are pixel-perfect (no staircase artefacts).
    ///   4. All parameters are driven via a MaterialPropertyBlock-style
    ///     Material instance so instances don't share state.
    ///
    /// Polygon mode: set <see cref="Sides"/> to 3 or higher.
    ///   Sides = 0  → smooth circle (default)
    ///   Sides = 3  → triangle
    ///   Sides = 4  → square / diamond
    ///   Sides = 6  → hexagon
    ///   And so forth...
    ///
    /// Fill / ring / rotation work identically to <see cref="GraphicCircle"/>.
    /// </summary>
    /// <remarks>
    /// This gives the advantage of performance, with the tradeoff being unable to apply custom material onto it
    /// </remarks>
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

        private static readonly int sr_PropFillAmount   = Shader.PropertyToID("_FillAmount");
        private static readonly int sr_PropInsideRadius = Shader.PropertyToID("_InsideRadius");
        private static readonly int sr_PropSides        = Shader.PropertyToID("_Sides");
        private static readonly int sr_PropRotation     = Shader.PropertyToID("_Rotation");

        private static Shader s_circleShader;
        private Material _SharedMat;   // owns the Material (we create it)

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
            if (s_circleShader == null)
                s_circleShader = Shader.Find("UI/Circle");

            if (s_circleShader == null)
            {
                Debug.LogError("[GraphicCircleGPU] Could not find shader 'UI/Circle'. " +
                               "Make sure UICircle.shader is in a Resources/Shaders folder.");
                return;
            }

            if (_SharedMat == null)
            {
                _SharedMat = new Material(s_circleShader) { hideFlags = HideFlags.HideAndDontSave };
                material = _SharedMat;
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
            if (_SharedMat == null) EnsureMaterial();
            if (_SharedMat == null) return;

            _SharedMat.SetFloat(sr_PropFillAmount,   FillAmount);
            _SharedMat.SetFloat(sr_PropInsideRadius, InsideRadius);
            _SharedMat.SetFloat(sr_PropSides,        Sides);
            _SharedMat.SetFloat(sr_PropRotation,     Rotation);
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
            if (_SharedMat != null)
            {
#if UNITY_EDITOR
                DestroyImmediate(_SharedMat);
#else
                Destroy(_SharedMat);
#endif
                _SharedMat = null;
            }
        }
    }
}
