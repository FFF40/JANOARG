using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode][RequireComponent(typeof(CanvasRenderer))]
public class SliderGradient : MaskableGraphic
{
    public Color color2 = Color.black;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        Vector2 center = rectTransform.rect.center;
        Vector2 radius = new Vector2(rectTransform.rect.width / 2, rectTransform.rect.height / 2) * Mathf.Sqrt(2);

        vh.Clear();

        UIVertex vert = UIVertex.simpleVert;
        vert.color = color;

        for (int a = 0; a <= 3; a++) 
        {
            float angle = (a + .5f) / 4 * Mathf.PI * 2;
            
            vert.color = a % 4 < 2 ? color2 : color;
            vert.position = new Vector2(Mathf.Sin(angle) * radius.x, Mathf.Cos(angle) * radius.y) + center;
            vh.AddVert(vert);
        }
        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(0, 2, 3);
    }
}
