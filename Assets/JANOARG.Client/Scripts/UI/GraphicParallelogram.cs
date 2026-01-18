using UnityEngine;
using UnityEngine.UI;

namespace JANOARG.Client.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(CanvasRenderer))]
    public class GraphicParallelogram : MaskableGraphic
    {
        [Space] [Range(-90, 90)] public float Slant = 15;

        public bool ExpandStart;
        public bool ExpandEnd;

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            Vector2 min = rectTransform.rect.min;
            Vector2 max = rectTransform.rect.max;

            float tan = Mathf.Tan(Slant * Mathf.Deg2Rad);
            float offset = tan * (max.y - min.y);

            vertexHelper.Clear();

            UIVertex vert = UIVertex.simpleVert;
            vert.color = color;

            vert.position = new Vector3(
                min.x + (ExpandStart ? Mathf.Min(-offset, 0) : Mathf.Max(-offset, 0)),
                min.y);

            vertexHelper.AddVert(vert);

            vert.position = new Vector3(
                min.x + (ExpandStart ? Mathf.Min(offset, 0) : Mathf.Max(offset, 0)),
                max.y);

            vertexHelper.AddVert(vert);

            vert.position = new Vector3(
                max.x + (ExpandEnd ? Mathf.Max(-offset, 0) : Mathf.Min(-offset, 0)),
                min.y);

            vertexHelper.AddVert(vert);

            vert.position = new Vector3(
                max.x + (ExpandEnd ? Mathf.Max(offset, 0) : Mathf.Min(offset, 0)),
                max.y);

            vertexHelper.AddVert(vert);

            vertexHelper.AddTriangle(0, 2, 3);
            vertexHelper.AddTriangle(0, 3, 1);
        }
    }
}
