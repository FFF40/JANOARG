using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class QuickMenu : MonoBehaviour
{
    public static QuickMenu main;

    public CanvasGroup Background;
    public Graphic MainBackground;

    [Space]
    public RectTransform LeftPanel;
    public CanvasGroup LeftPanelGroup;

    public bool IsAnimating;

    public void Awake() 
    {
        main = this;
    }

    public void ShowLeft() 
    {
        if (!IsAnimating) 
        {
            gameObject.SetActive(true);
            StartCoroutine(ShowLeftAnim());
        }
    }

    public IEnumerator ShowLeftAnim() 
    {
        IsAnimating = true;

        MainBackground.color = Common.main.MainCamera.backgroundColor * new Color(1, 1, 1, 0) + new Color(0, 0, 0, 0.75f);

        yield return Ease.Animate(.2f, a => {
            ProfileBar.main.SetVisibilty(1 - Ease.Get(a, EaseFunction.Cubic, EaseMode.Out));
            Background.alpha = a;
        });

        LeftPanel.gameObject.SetActive(true);

        yield return Ease.Animate(.2f, a => {
            SetLeftPanelVisibility(Ease.Get(a, EaseFunction.Cubic, EaseMode.Out));
        });

        IsAnimating = false;
    }

    public void HideLeft() 
    {
        if (!IsAnimating) 
        {
            StartCoroutine(HideLeftAnim());
        }
    }

    public IEnumerator HideLeftAnim() 
    {
        IsAnimating = true;

        yield return Ease.Animate(.2f, a => {
            SetLeftPanelVisibility(1 - Ease.Get(a, EaseFunction.Cubic, EaseMode.Out));
        });

        LeftPanel.gameObject.SetActive(false);

        yield return Ease.Animate(.2f, a => {
            ProfileBar.main.SetVisibilty(Ease.Get(a, EaseFunction.Cubic, EaseMode.Out));
            Background.alpha = 1 - a;
        });

        IsAnimating = false;
        gameObject.SetActive(false);
    }

    public void HideFromPanel() 
    {
        if (!IsAnimating) 
        {
            StartCoroutine(HideFromPanelAnim());
        }
    }

    public IEnumerator HideFromPanelAnim() 
    {
        IsAnimating = true;

        yield return Ease.Animate(.2f, a => {
            ProfileBar.main.SetVisibilty(Ease.Get(a, EaseFunction.Cubic, EaseMode.Out));
            Background.alpha = 1 - a;
        });

        IsAnimating = false;
        gameObject.SetActive(false);
    }

    public void SetLeftPanelVisibility(float a)
    {
        LeftPanelGroup.alpha = a * a;
        LeftPanel.anchoredPosition = new (-10 * (1 - a), LeftPanel.anchoredPosition.y);
    }

    public void ShowPanel(string sceneName) 
    {
        if (!IsAnimating) 
        {
            StartCoroutine(ShowPanelAnim(sceneName));
        }
    }

    public IEnumerator ShowPanelAnim(string sceneName) 
    {
        IsAnimating = true;

        yield return Ease.Animate(.2f, a => {
            SetLeftPanelVisibility(1 - Ease.Get(a, EaseFunction.Cubic, EaseMode.Out));
        });

        LeftPanel.gameObject.SetActive(false);

        AsyncOperation sceneReq = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        float time = 0;

        while (time < .2f || (!sceneReq.isDone && time < 1))
        {
            yield return null;
            time += Time.deltaTime;
        }
        
        if (!sceneReq.isDone)
        {
            LoadingBar.main.Show();
            yield return new WaitWhile(() => LoadingBar.main.IsAnimating || !sceneReq.isDone);
            LoadingBar.main.Hide();
            yield return new WaitWhile(() => LoadingBar.main.IsAnimating);
        }

        if (Panel.Panels.Count > 0)
        {
            Panel panel = Panel.Panels[^1];
            panel.SceneName = sceneName;
            panel.Intro();
        }

        IsAnimating = false;
    }
}