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

    public List<float> Positions = new();
    public List<float> Times = new();
    public float CurrentPosition;


    public void Init()
    {
        var met = PlayerScreen.TargetSong.Timing;
        foreach (TextStep step in Current.TextSteps)
        {
            Times.Add(met.ToSeconds(step.Offset));
        }
        Debug.Log("Display Text = " + Original.DisplayText);
        Debug.Log("Display Text = " + Current.DisplayText);
        Debug.Log("Text Holder = " + TextHolder.text);
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
