
using System.Collections;
using System.Data;
using UnityEngine;
using UnityEngine.UI;

public class PlaylistMapItemUI : MapItemUI<PlaylistMapItem>
{
    public RawImage CoverImage;

    public override void SetParent(PlaylistMapItem parent)
    {
        base.SetParent(parent);
        UpdateStatus();
        LoadCoverImage();
    }


    public void UpdateStatus()
    {
        if (!Parent.IsRevealed)
        {
            if (CoverImage.texture) SongSelectCoverManager.main.UnregisterUse(CoverImage);
            gameObject.SetActive(false);
            return;
        }

        if (!CoverImage.texture && CoverLoadRoutine == null) LoadCoverImage();
        gameObject.SetActive(true);
    }



    public Coroutine CoverLoadRoutine = null;
    public void LoadCoverImage()
    {
        if (!Parent.IsRevealed) return;
        if (CoverLoadRoutine != null) StopCoroutine(CoverLoadRoutine);
        CoverLoadRoutine = StartCoroutine(LoadCoverImageRoutine());
    }
    private IEnumerator LoadCoverImageRoutine()
    {
        SongSelectCoverManager.main.UnregisterUse(CoverImage);
        yield return SongSelectCoverManager.main.RegisterUse(CoverImage, Parent.TargetID);
    }

    public void OnClick()
    {
        // TODO show playlist info when clicked
    }
}