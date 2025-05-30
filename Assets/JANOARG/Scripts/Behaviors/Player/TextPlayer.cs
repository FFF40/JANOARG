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

    public float CurrentPosition;


    public void Init()
    {
        var met = PlayerScreen.TargetSong.Timing;
        TextHolder.text = Original.DisplayText;
    }


    public void UpdateSelf(float time, float beat)
    {
        if (Current != null) Current.Advance(beat);
        else Current = (Text)Original.Get(beat);

        transform.localPosition = Current.Position;
        transform.localEulerAngles = Current.Rotation;
    }

}
