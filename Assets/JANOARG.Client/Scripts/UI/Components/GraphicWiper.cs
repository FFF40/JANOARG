using System;
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
    public class GraphicWiper : MaskableGraphic
    {

        [SerializeField]
        private float m_Angle;

        [SerializeField][Range(0,1)]
        private float m_FillAmount = 1;


        public float Angle
        {
            get { return m_Angle; }
            set { m_Angle = value; SetVerticesDirty(); }
        }

        public float FillAmount
        {
            get { return m_FillAmount; }
            set { m_FillAmount = value; SetVerticesDirty(); }
        }



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



        private static Shader s_shader;
        private static Material s_material;

        private void EnsureMaterial()
        {
            if (s_shader == null)
                s_shader = Shader.Find("UI/Wiper");

            if (s_shader == null)
            {
                return;
            }

            if (s_material == null)
                s_material = new Material(s_shader) { hideFlags = HideFlags.HideAndDontSave };

            if (material != s_material)
                material = s_material;
        }



        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            Rect rect = rectTransform.rect;
            float xScale = rect.width / rect.height;

            const float SQRT_2 = 1.4142135623730951f;

            float trueAngle = (m_Angle * Mathf.Deg2Rad) % (Mathf.PI * 2);
            if (trueAngle < 0) trueAngle += Mathf.PI * 2;
            trueAngle = Mathf.Atan(Mathf.Tan(trueAngle) / xScale) + Mathf.PI * Mathf.Ceil((trueAngle - Mathf.PI / 2) / Mathf.PI);

            float fillThreshold = Mathf.Sin(trueAngle % (.5f * Mathf.PI) + .25f * Mathf.PI) * SQRT_2;
            fillThreshold *= m_FillAmount * 2 - 1;

            Vector4 shaderParams = new(trueAngle, fillThreshold);

            vh.AddVert(new Vector3(rect.xMin, rect.yMin), color, new Vector2(-1, -1), shaderParams, Vector3.zero, Vector4.zero);
            vh.AddVert(new Vector3(rect.xMin, rect.yMax), color, new Vector2(-1,  1), shaderParams, Vector3.zero, Vector4.zero);
            vh.AddVert(new Vector3(rect.xMax, rect.yMax), color, new Vector2( 1,  1), shaderParams, Vector3.zero, Vector4.zero);
            vh.AddVert(new Vector3(rect.xMax, rect.yMin), color, new Vector2( 1, -1), shaderParams, Vector3.zero, Vector4.zero);

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            m_FillAmount   = Mathf.Clamp01(m_FillAmount);
            EnsureMaterial();
            SetVerticesDirty();
        }
#endif
    }
}
