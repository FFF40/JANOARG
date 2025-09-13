using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace JANOARG.Client.UI
{
    [ExecuteAlways] [RequireComponent(typeof(CanvasRenderer))]
    public class GraphicCircle : MaskableGraphic
    {
        [SerializeField]               private int   Resolution  = 90;
        [SerializeField] [Range(0, 1)] private float FillAmount   = 1f;
        [SerializeField] [Range(0, 1)] private float InsideRadius = 0f;
        
        // Cache frequently used values
        private float _LastFillAmount = -1f;
        private int _LastResolution = -1;
        private float _LastInsideRadius = -1f;
        private Vector2 _LastRectSize = Vector2.zero;
        
        public int resolution 
        { 
            get => Resolution; 
            set 
            { 
                Resolution = Mathf.Clamp(value, 3, 1000);
                SetVerticesDirty();
            } 
        }

        public float fillAmount 
        { 
            get => FillAmount; 
            set 
            { 
                FillAmount = Mathf.Clamp01(value);
                SetVerticesDirty();
            } 
        }

        public float insideRadius 
        { 
            get => InsideRadius; 
            set 
            { 
                InsideRadius = Mathf.Clamp01(value);
                SetVerticesDirty();
            } 
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            var rect = rectTransform.rect;
            var center = rect.center;
            var radius = new Vector2(rect.width * 0.5f, rect.height * 0.5f);
            
            // Early exit if no change
            if (HasNoChanges(rect.size))
                return;
                
            UpdateCachedValues(rect.size);

            vh.Clear();

            // Clamp resolution and calculate actual segments needed
            var clampedResolution = Mathf.Clamp(Resolution, 3, 1000);
            var actualSegments = Mathf.CeilToInt(clampedResolution * FillAmount);
            
            if (actualSegments <= 0) 
                return;

            var vert = UIVertex.simpleVert;
            vert.color = color;

            if (InsideRadius <= 0.001f) // Use epsilon for float comparison
            {
                PopulateFilledCircle(vh, center, radius, actualSegments, vert);
            }
            else
            {
                PopulateRingCircle(vh, center, radius, actualSegments, vert);
            }
        }

        private bool HasNoChanges(Vector2 currentRectSize)
        {
            return Mathf.Approximately(FillAmount, _LastFillAmount) &&
                   Resolution == _LastResolution &&
                   Mathf.Approximately(InsideRadius, _LastInsideRadius) &&
                   Vector2.Distance(currentRectSize, _LastRectSize) < 0.01f;
        }

        private void UpdateCachedValues(Vector2 currentRectSize)
        {
            _LastFillAmount = FillAmount;
            _LastResolution = Resolution;
            _LastInsideRadius = InsideRadius;
            _LastRectSize = currentRectSize;
        }

        private void PopulateFilledCircle(VertexHelper vertexHelper, Vector2 center, Vector2 radius, int segments, UIVertex vert)
        {
            // Center vertex
            vert.position = center;
            vertexHelper.AddVert(vert);

            var angleStep = (Mathf.PI * 2f) / Resolution;
            
            for (var i = 0; i <= segments; i++)
            {
                var angle = i * angleStep;
                var sin = Mathf.Sin(angle);
                var cos = Mathf.Cos(angle);
                
                vert.position = new Vector2(sin * radius.x, cos * radius.y) + center;
                vertexHelper.AddVert(vert);

                if (i > 0)
                {
                    vertexHelper.AddTriangle(0, i, i + 1);
                }
            }
        }

        private void PopulateRingCircle(VertexHelper vertexHelper, Vector2 center, Vector2 radius, int segments, UIVertex vert)
        {
            var innerRadius = new Vector2(radius.x * InsideRadius, radius.y * InsideRadius);
            var angleStep = (Mathf.PI * 2f) / Resolution;
            
            for (var i = 0; i <= segments; i++)
            {
                var angle = i * angleStep;
                var sin = Mathf.Sin(angle);
                var cos = Mathf.Cos(angle);
                
                // Inner vertex
                vert.position = new Vector2(sin * innerRadius.x, cos * innerRadius.y) + center;
                vertexHelper.AddVert(vert);
                
                // Outer vertex
                vert.position = new Vector2(sin * radius.x, cos * radius.y) + center;
                vertexHelper.AddVert(vert);

                if (i > 0)
                {
                    var prevInner = (i - 1) * 2;
                    var prevOuter = prevInner + 1;
                    var currInner = i * 2;
                    var currOuter = currInner + 1;
                    
                    // Two triangles to form a quad
                    vertexHelper.AddTriangle(prevOuter, currOuter, currInner);
                    vertexHelper.AddTriangle(prevOuter, currInner, prevInner);
                }
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            Resolution = Mathf.Clamp(Resolution, 3, 1000);
            FillAmount = Mathf.Clamp01(FillAmount);
            InsideRadius = Mathf.Clamp01(InsideRadius);
        }
#endif
    }
}