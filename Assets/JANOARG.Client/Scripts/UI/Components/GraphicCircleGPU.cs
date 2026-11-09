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
    ///   4. All instances share one Material — per-instance parameters are
    ///      packed into vertex uv1 instead of material uniforms, so the UI
    ///      batcher can merge multiple circles into a single draw call.
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
    /// Requires the owning Canvas to have "Additional Shader Channels" include
    /// TexCoord1, otherwise the per-instance uv1 data won't reach the shader.
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
            set { Sides = Mathf.Max(0, value); SetVerticesDirty(); }
        }

        public float fillAmount
        {
            get => FillAmount;
            set { FillAmount = Mathf.Clamp01(value); SetVerticesDirty(); }
        }

        public float insideRadius
        {
            get => InsideRadius;
            set { InsideRadius = Mathf.Clamp01(value); SetVerticesDirty(); }
        }

        public float rotation
        {
            get => Rotation;
            set { Rotation = value % 360f; SetVerticesDirty(); }
        }

        // ------------------------------------------------------------------ //
        // Private state
        // ------------------------------------------------------------------ //

        private static Shader   s_circleShader;
        private static Material s_SharedMat; // one Material shared by every instance — enables UI batching

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

            if (s_SharedMat == null)
                s_SharedMat = new Material(s_circleShader) { hideFlags = HideFlags.HideAndDontSave };

            if (material != s_SharedMat)
                material = s_SharedMat;
        }

        // ------------------------------------------------------------------ //
        // Mesh — just a single quad; the shader does all the work
        // ------------------------------------------------------------------ //

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            var rect   = rectTransform.rect;
            var color32 = color;

            // Per-instance circle params packed into uv1 (x=FillAmount, y=InsideRadius,
            // z=Sides, w=Rotation) — identical on all 4 verts, read by the shader instead
            // of a material uniform, so every GraphicCircleGPU can share one Material.
            Vector4 circleParams = new(FillAmount, InsideRadius, Sides, Rotation);

            // Four corners, UVs in [0,1]
            vh.AddVert(new Vector3(rect.xMin, rect.yMin), color32, new Vector2(0, 0), circleParams, Vector3.zero, Vector4.zero);
            vh.AddVert(new Vector3(rect.xMin, rect.yMax), color32, new Vector2(0, 1), circleParams, Vector3.zero, Vector4.zero);
            vh.AddVert(new Vector3(rect.xMax, rect.yMax), color32, new Vector2(1, 1), circleParams, Vector3.zero, Vector4.zero);
            vh.AddVert(new Vector3(rect.xMax, rect.yMin), color32, new Vector2(1, 0), circleParams, Vector3.zero, Vector4.zero);

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
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
            SetVerticesDirty();
        }
#endif
    }
}
