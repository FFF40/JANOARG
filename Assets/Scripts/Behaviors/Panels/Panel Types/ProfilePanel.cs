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

        Debug.Log(Storage.Get("INFO:Name", "JANOARG"));
        PlayerName.text = Storage.Get("INFO:Name", "JANOARG");
        PlayerTitle.text = Storage.Get("INFO:Title", "Perfectly Generic Player");

        // TODO: Leveling Stuff
        LevelContent.text = Storage.Get("INFO:Level", 1).ToString();
        
        int LevelProgressGained = Storage.Get("INFO:LevelProgressNumerator", 1);
        int LevelProgressLimit = Storage.Get("INFO:LevelProgressDenominator", 100);

        LevelProgress.text = LevelProgressGained + " / " + LevelProgressLimit;

        // TODO: AR Calucation (Check ProfileBar)
        AbilityRatingContent.text = (Storage.Get("INFO:AbilityRating", 0.00)).ToString("f2"); //getting some errors for some reason

        // TODO: Remember last filter option 
        // TODO: Dropdown menu for filter
        // -> Simple, Normal, Complex, Overdrive, Special, All

        AllFlawlessCount.text = Storage.Get("INFO:AllFlawlessCount", "0");
        FullStreakCount.text = Storage.Get("INFO:FullStreakCount", "0");
        ClearedCount.text = Storage.Get("INFO:ClearedCount", "0");
        UnlockedCount.text = Storage.Get("INFO:UnlockedCount", "0");

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
