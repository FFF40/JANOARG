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
        TextHolder.font = InitFontFamily(Original.TextFont);

        Text = Original.DisplayText;
    }

    TMP_FontAsset InitFontFamily(FontFamily font)
    {

        TMP_FontAsset rt = font switch
        {
            FontFamily.RobotoMono => Resources.Load<TMP_FontAsset>("Fonts/RobotoMono-Regular SDF"),
            FontFamily.Roboto => Resources.Load<TMP_FontAsset>("Fonts/Roboto-Regular SDF"),
            FontFamily.Garvette => Resources.Load<TMP_FontAsset>("Fonts/Garvette SDF"),
            FontFamily.Michroma => Resources.Load<TMP_FontAsset>("Fonts/Michroma-Regular SDF"),
            _ => Resources.Load<TMP_FontAsset>("Fonts/RobotoMono SDF")
        };

        if (rt == null)
        Debug.LogWarning($"Font asset for {font} not found in Resources.");
        
        return rt;
    }

    public void UpdateSelf(float time, float beat)
    {
        if (Current != null) Current.Advance(beat);
        else Current = (Text)Original.Get(beat);

        transform.localPosition = Current.Position;
        transform.localEulerAngles = Current.Rotation;

        Text = Original.GetUpdateText(time, beat, Original.DisplayText);
        TextHolder.text = Text;
        TextHolder.fontSize = Current.TextSize;
        TextHolder.color = Current.TextColor;
    }

}
