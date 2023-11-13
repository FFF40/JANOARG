using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class GraphicThemeable : Themeable<Graphic>
{
    public string ID;

    public override void SetColors()
    {
        if (Themer.main.Keys.TryGetValue(ID, out Color color)) Target.color = color;
    }
}
