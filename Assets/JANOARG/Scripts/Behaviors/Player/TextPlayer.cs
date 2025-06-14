using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;
using TMPro;

public class TextPlayer : MonoBehaviour
{
    public Text Original;
    public Text Current;

    public string Text;
    public TextMeshPro TextHolder;

    //public float CurrentPosition;

    public void Init()
    {
        TextHolder.text = Original.DisplayText;
        Text = Original.DisplayText;
        //TextHolder.color = Original.TextColor;
    }

    public void UpdateSelf(float time, float beat)
    {
        if (Current != null) Current.Advance(beat);
        else Current = (Text)Original.Get(beat);

        transform.localPosition = Current.Position;
        transform.localEulerAngles = Current.Rotation;
      
        Text = Original.GetUpdateText(time,Original.DisplayText);
        //Text = Original.GetUpdateColor(time, Original.Col);
        TextHolder.text = Text;
        TextHolder.fontSize = Current.TextSize;
        //Debug.Log(Current.TextColor);
        TextHolder.color = Current.TextColor;


    }

}
