using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;

public class ProfilePanel : MonoBehaviour
{
    public Camera ScreenshotCamera;
    [Space]
    public TMP_Text PlayerName;
    public TMP_Text PlayerTitle;
    public TMP_Text LevelContent;
    public TMP_Text LevelProgress;
    public TMP_Text AbilityRatingContent;
    public TMP_Text AllFlawlessCount;
    public TMP_Text FullStreakCount;
    public TMP_Text ClearedCount;
    public TMP_Text UnlockedCount;

    public void Awake()
    {
        Storage Storage = Common.main.Storage;

        PlayerName.text = Storage.Get("INFO_Name", "JANOARG");
    }
    

    public bool IsAnimating { get; private set; }

    public Texture2D Screenshot(int width, int height) 
    {
        RenderTexture rTex = new (width, height, 16, RenderTextureFormat.ARGB32);
        rTex.Create();

        ScreenshotCamera.targetTexture = rTex;
        ScreenshotCamera.Render();
        
        Texture2D tex2D = new (width, height, TextureFormat.ARGB32, false);
        RenderTexture.active = rTex;
        tex2D.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex2D.Apply();

        ScreenshotCamera.targetTexture = null;
        rTex.Release();

        return tex2D;
    }

    public void ScreenshotRatingBreakdown() 
    {
        if (!IsAnimating) StartCoroutine(ScreenshotRatingBreakdownAnim());
    }

    public IEnumerator ScreenshotRatingBreakdownAnim() 
    {
        IsAnimating = true;
        Texture2D image = Screenshot(3072, 1280);
        yield return Share(image);
        IsAnimating = false;
    }

    public IEnumerator Share(Texture2D image) 
    {
        var task = File.WriteAllBytesAsync(Application.persistentDataPath + "/screenshot.png", ImageConversion.EncodeToPNG(image));
        yield return new WaitUntil(() => task.IsCompleted);
    }
}
