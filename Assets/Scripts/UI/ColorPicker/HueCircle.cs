using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteInEditMode][RequireComponent(typeof(CanvasRenderer))]
public class HueCircle : MaskableGraphic, IPointerDownHandler, IDragHandler
{
    public int Resolution = 90;
    [Range(0, 1)]
    public float InsideRadius = 0;

    public ColorPicker Picker;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        Vector2 center = rectTransform.rect.center;
        Vector2 radius = new Vector2(rectTransform.rect.width / 2, rectTransform.rect.height / 2);

        vh.Clear();

        UIVertex vert = UIVertex.simpleVert;
        vert.color = color;

        if (Resolution > 1000) Resolution = 1000;

        for (int a = 0; a <= Resolution; a++) 
        {
            float angle = (float)a / Resolution * Mathf.PI * 2;
            
            vert.color = Color.HSVToRGB(angle / Mathf.PI / 2, 1, 1);
            vert.position = new Vector2(Mathf.Cos(angle) * radius.x * InsideRadius, Mathf.Sin(angle) * radius.y * InsideRadius) + center;
            vh.AddVert(vert);
            vert.position = new Vector2(Mathf.Cos(angle) * radius.x, Mathf.Sin(angle) * radius.y) + center;
            vh.AddVert(vert);

            if (a > 0) 
            {
                vh.AddTriangle(a * 2 - 1, a * 2 + 1, a * 2);
                vh.AddTriangle(a * 2 - 1, a * 2, a * 2 - 2);
            }
        }
    }


    public void OnDrag(PointerEventData eventData)
    {
        OnPointerDown(eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        Rect worldRect = new Rect(corners[0], corners[2] - corners[0]);
        Vector2 offset = (Vector2)eventData.position - worldRect.center;
        Picker.CurrentHSV.x = (Mathf.Atan2(offset.y, offset.x) / Mathf.PI / 2 + 1) % 1;
        Picker.UpdateRGB();
        Picker.UpdateHex();
        Picker.UpdateUI();
    }
}
