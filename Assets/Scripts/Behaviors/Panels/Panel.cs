using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Panel : MonoBehaviour
{
    public static List<Panel> Panels = new();

    public RectTransform Holder;
    public CanvasGroup HolderGroup;
    [Space]
    public string SceneName;
    [Space]
    public bool IsAnimating;

    public void Intro() 
    {
        if (!IsAnimating) 
        {
            StartCoroutine(IntroAnim());
        }
    }

    public void OnEnable()
    {
        Panels.Add(this);
        HolderGroup.alpha = 0;
        HolderGroup.blocksRaycasts = false;
    }
    public void OnDisable()
    {
        Panels.Remove(this);
    }

    public IEnumerator IntroAnim() 
    {
        IsAnimating = true;

        HolderGroup.blocksRaycasts = true;

        yield return Ease.Animate(.2f, a => {
            SetPanelVisibility(Ease.Get(a, EaseFunction.Cubic, EaseMode.Out));
        });

        IsAnimating = false;
    }

    public void Close() 
    {
        if (!IsAnimating) 
        {
            StartCoroutine(CloseAnim());
        }
    }

    public IEnumerator CloseAnim() 
    {
        IsAnimating = true;
        
        HolderGroup.blocksRaycasts = false;

        if (Panels.Count <= 1) AudioManager.main.SetSceneLayerLowPassCutoff(5000, 1f);

        yield return Ease.Animate(.2f, a => {
            SetPanelVisibility(1 - Ease.Get(a, EaseFunction.Cubic, EaseMode.Out));
        });

        Common.main.StartCoroutine(UnloadAnim());

        IsAnimating = false;
    }

    public IEnumerator UnloadAnim() 
    {
        yield return SceneManager.UnloadSceneAsync(SceneName);
        if (Panels.Count <= 1) QuickMenu.main.HideFromPanel();
        else Panels[^2].Intro();
    }

    public void SetPanelVisibility(float a)
    {
        HolderGroup.alpha = a * a;
        Holder.anchoredPosition = new (-10 * (1 - a), Holder.anchoredPosition.y);
    }
}
