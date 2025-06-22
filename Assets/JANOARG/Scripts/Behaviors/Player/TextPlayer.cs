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

    public void Init()
    {
        TextHolder.text     = Original.DisplayText;
        TextHolder.fontSize = Original.TextSize;
        TextHolder.color    = Original.TextColor;
        TextHolder.font     = InitFontFamily(Original.TextFont);

        transform.localPosition = Original.Position;
        transform.localEulerAngles = Original.Rotation;

        Text = Original.DisplayText;
        Current.TextSteps = Original.TextSteps;
    }

    public void UpdateSelf(float time, float beat)
    {
        if (Current != null) Current.Advance(beat);
        else Current = (Text)Original.Get(beat);
        
        transform.localPosition = Current.Position;
        transform.localEulerAngles = Current.Rotation;

        Text = Current.GetUpdateText(time, beat, Current.DisplayText);
        TextHolder.text = Text;
        TextHolder.fontSize = Current.TextSize;
        TextHolder.color = Current.TextColor;
    }

    TMP_FontAsset InitFontFamily(FontFamily font)
    {
        //Add a case statement if you added a FontFamily item in FontFamily enum 
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
}
