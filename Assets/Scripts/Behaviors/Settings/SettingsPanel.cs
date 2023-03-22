using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    public static SettingsPanel main;
    public TabbedSidebar Sidebar;

    public Image FocusBackground;
    public Image FocusHolderBackground;
    public RectTransform FocusHolder;
    public RectTransform FocusPlaceholder;

    Setting FocusingSetting;
    Setting ScheduledSetting;
    bool isAnimating;


    void Awake()
    {
        main = this;
        CommonScene.Load();
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Sidebar.ShowAnimation());
    }
    
    public void Close()
    {
        Common.main.Storage.Save();

        StartCoroutine(CloseAnimation());
    }

    public IEnumerator CloseAnimation()
    {
        StartCoroutine(Sidebar.HideAnimation());
        yield return new WaitForSeconds(.3f);
        
        SceneManager.UnloadSceneAsync("Settings");
        PlaylistScroll.main?.ShowSelection();
    }


    public void FocusSetting(Setting setting)
    {
        ScheduledSetting = setting;
        if (isAnimating || FocusingSetting == setting) return;
        FocusingSetting = setting;
        StartCoroutine(FocusAnimation());
    }

    public IEnumerator FocusAnimation()
    {
        isAnimating = true;

        RectTransform rt = FocusingSetting.GetComponent<RectTransform>();
        Vector3 oldPos = rt.position;
        float oldWidth = rt.rect.width;

        FocusBackground.gameObject.SetActive(true);
        FocusHolder.gameObject.SetActive(true);
        FocusPlaceholder.gameObject.SetActive(true);
        FocusPlaceholder.SetParent(rt.parent);
        FocusPlaceholder.SetSiblingIndex(rt.GetSiblingIndex());
        rt.SetParent(FocusHolder);

        float kbHeight = GetKeyboardHeightRatio();
        FocusHolder.anchorMin = new Vector2(0, kbHeight);

        void Lerp(float value)
        {
            float ease = Ease.Get(value, EaseFunction.Cubic, EaseMode.InOut);

            rt.sizeDelta = new Vector2(Mathf.Lerp(oldWidth, FocusHolder.rect.width - 100, ease), rt.sizeDelta.y);
            rt.position = Vector3.Lerp(oldPos, rt.parent.position, ease);
            FocusHolderBackground.color = Color.black * ease;
            FocusBackground.color = Color.black * ease / 2;
            
            FocusHolderBackground.rectTransform.position = new Vector2(FocusHolderBackground.rectTransform.position.x, rt.position.y);
            FocusHolderBackground.rectTransform.sizeDelta = new Vector2(FocusHolderBackground.rectTransform.sizeDelta.x, 50 * ease);
            
            FocusingSetting.FocusLerp(ease);
            FocusingSetting.Label.rectTransform.anchoredPosition = Vector2.right * 20 * (1 - ease);
        }

        for (float a = 0; a < 1; a += Time.deltaTime / .4f)
        {
            Lerp(a);
            yield return null;
        }
        Lerp(1);
        
        isAnimating = false;

        if (FocusingSetting != ScheduledSetting) UnfocusSetting();
    }

    public void UnfocusSetting()
    {
        ScheduledSetting = null;
        if (isAnimating || !FocusingSetting) return;
        StartCoroutine(UnfocusAnimation());
    }

    public IEnumerator UnfocusAnimation()
    {
        isAnimating = true;

        RectTransform rt = FocusingSetting.GetComponent<RectTransform>();
        float oldWidth = rt.rect.width;

        void Lerp(float value)
        {
            float ease = Ease.Get(value, EaseFunction.Cubic, EaseMode.Out);

            rt.sizeDelta = new Vector2(Mathf.Lerp(oldWidth, FocusPlaceholder.rect.width, ease), rt.sizeDelta.y);
            rt.position = Vector3.Lerp(rt.parent.position, FocusPlaceholder.position, ease);
            FocusHolderBackground.color = Color.black * (1 - value);
            FocusBackground.color = Color.black * (1 - value) / 2;
            FocusingSetting.Label.rectTransform.anchoredPosition = Vector2.right * 20 * ease;
            
            FocusingSetting.FocusLerp(1 - ease);
            FocusHolderBackground.rectTransform.sizeDelta = new Vector2(FocusHolderBackground.rectTransform.sizeDelta.x, 50 * (1 - ease));
        }

        for (float a = 0; a < 1; a += Time.deltaTime / .3f)
        {
            Lerp(a);
            yield return null;
        }
        Lerp(1);

        rt.SetParent(FocusPlaceholder.parent);
        rt.SetSiblingIndex(FocusPlaceholder.GetSiblingIndex());
        FocusPlaceholder.gameObject.SetActive(false);
        FocusHolder.gameObject.SetActive(false);
        FocusBackground.gameObject.SetActive(false);
        
        FocusingSetting = null;
        
        isAnimating = false;

        if (ScheduledSetting != null) FocusSetting(ScheduledSetting);
    }
    
    private static float GetKeyboardHeightRatio(bool includeInput = false)
    {

#if UNITY_EDITOR
        return .4f;

#elif UNITY_ANDROID
        using (AndroidJavaClass unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject unityPlayer = unityClass.GetStatic<AndroidJavaObject>("currentActivity").Get<AndroidJavaObject>("mUnityPlayer");
            AndroidJavaObject view = unityPlayer.Call<AndroidJavaObject>("getView");
            AndroidJavaObject dialog = unityPlayer.Get<AndroidJavaObject>("mSoftInputDialog");
            if (view == null || dialog == null)
                return 0;
            var decorHeight = 0;
            if (includeInput)
            {
                AndroidJavaObject decorView = dialog.Call<AndroidJavaObject>("getWindow").Call<AndroidJavaObject>("getDecorView");
                if (decorView != null)
                    decorHeight = decorView.Call<int>("getHeight");
            }
            using (AndroidJavaObject rect = new AndroidJavaObject("android.graphics.Rect"))
            {
                view.Call("getWindowVisibleDisplayFrame", rect);
                return 1 - (rect.Call<int>("height") + decorHeight) / Screen.height;
            }
        }

#elif UNITY_IOS
        return TouchScreenKeyboard.area.height / Screen.height;

#endif
    }
}
