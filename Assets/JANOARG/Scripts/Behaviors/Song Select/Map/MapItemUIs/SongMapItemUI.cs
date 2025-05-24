
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SongMapItemUI : MapItemUI<SongMapItem>
{
    public RawImage CoverImage;

    public override void SetParent(SongMapItem parent)
    {
        base.SetParent(parent);
        LoadCoverImage();
    }
    
    public Coroutine CoverLoadRoutine = null;
    public void LoadCoverImage()
    {
        if (CoverLoadRoutine != null) StopCoroutine(CoverLoadRoutine);
        CoverLoadRoutine = StartCoroutine(LoadCoverImageRoutine());
    }
    private IEnumerator LoadCoverImageRoutine()
    {
        SongSelectCoverManager.main.UnregisterUse(CoverImage);
        yield return SongSelectCoverManager.main.RegisterUse(CoverImage, Parent.TargetID);
    }
}