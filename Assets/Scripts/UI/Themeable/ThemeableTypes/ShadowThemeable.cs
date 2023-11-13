using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Shadow))]
public class ShadowThemeable : Themeable<Shadow>
{
    public string ID = "ShadowNormal";

    public override void SetColors()
    {
        if (Themer.main.Keys.TryGetValue(ID, out Color color)) Target.effectColor = color;
    }
}
