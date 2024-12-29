using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WindowHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static WindowHandler main;

    [Header("Objects")]
    public GameObject WindowControls;
    public RectTransform NavBar;
    public RectTransform ContentHolder;
    public RectTransform ContextMenuHolder;
    public RectTransform ModalHolder;
    public RectTransform LoaderHolder;
    public GameObject MenuButton;
    public RectTransform SongDetails;
    [Space]
    public TooltipTarget ResizeTooltip;
    public GameObject ResizeIconMaximize;
    public GameObject ResizeIconRestore;
    public RectTransform TopBorder;
    [Space]
    public GameObject InactiveBackground;
    public CanvasGroup LeftGroup;
    public CanvasGroup CenterGroup;
    public CanvasGroup RightGroup;
    [Header("Window")]
    public Vector2Int defaultWindowSize;
    public Vector2Int borderSize;
    public Vector2Int windowMargin;
    [Header("Window")]
    public List<CursorDefinition> CursorDefinitions;
    public Dictionary<CursorType, CursorDefinition> CursorMap;

    CursorDefinition activeCustomCursor;
    int currentCursorFrame;
    float currentCursorFrameTime;

    Vector2 mousePos = Vector2.zero;
    public bool maximized { get; private set; }
    public bool active { get; private set; }
    bool isFullScreen;

    bool framed;

    // TODO: Do something with this variable or remove it
    float clickTime = float.NegativeInfinity;

    public void Awake()
    {
        main = this;
        CursorMap = new();
        for (int a = 0; a < CursorDefinitions.Count; a++)
        {
            CursorMap.Add(CursorDefinitions[a].CursorType, CursorDefinitions[a]);
        }
    }

    public void Quit()
    {
        #if !UNITY_EDITOR && UNITY_STANDALONE_WIN 
            BorderlessWindow.UnhookWindowProc();
        #endif
    }

    public void Update() 
    {
        if (Screen.fullScreen != isFullScreen) 
        {
            isFullScreen = Screen.fullScreen;
            WindowControls.SetActive(!isFullScreen);
            if (!isFullScreen) BorderlessWindow.InitializeWindow();
        }
        

        if (maximized != BorderlessWindow.IsMaximized) 
        {
            OnSizeChange();
        }
        if (active != BorderlessWindow.IsActive) 
        {
            OnActiveChange();
        }
        if (framed != BorderlessWindow.IsFramed) 
        {
            framed = BorderlessWindow.IsFramed;
            OnFrameChanged();
        }

        if (activeCustomCursor && activeCustomCursor.Frames.Count > 1) 
        {
            currentCursorFrameTime += Time.unscaledDeltaTime;
            int lastFrame = currentCursorFrame;
            int a = 1000;
            while (currentCursorFrameTime >= activeCustomCursor.Frames[currentCursorFrame].Duration && a > 0)
            {
                currentCursorFrameTime -= activeCustomCursor.Frames[currentCursorFrame].Duration;
                currentCursorFrame = (currentCursorFrame + 1) % activeCustomCursor.Frames.Count;
                a--;
            }
            if (lastFrame != currentCursorFrame)
                Cursor.SetCursor(activeCustomCursor.Frames[currentCursorFrame].Texture, activeCustomCursor.Pivot, CursorMode.Auto);
        }
    }

    public void OnFrameChanged()
    {
        bool isNavbar = !framed || Chartmaker.Preferences.ForceNavigationBar;
        ContentHolder.sizeDelta = ContextMenuHolder.sizeDelta = ModalHolder.sizeDelta 
            = LoaderHolder.sizeDelta = NavBar.anchoredPosition 
            = Vector2.up * (isNavbar ? -28 : 0);
        MenuButton.SetActive(!isNavbar);

        TopBorder.gameObject.SetActive(!framed);
        CenterGroup.gameObject.SetActive(!framed);
        RightGroup.gameObject.SetActive(!framed);

        SongDetails.anchoredPosition = Vector2.right * (isNavbar ? 4 : 32);
    }

    public void ResetWindowSize()
    {
        BorderlessWindow.ResizeWindow(defaultWindowSize.x, defaultWindowSize.y);
    }

    public void CloseWindow()
    {
        EventSystem.current.SetSelectedGameObject(null);
        Application.Quit();
    }

    public void MinimizeWindow()
    {
        EventSystem.current.SetSelectedGameObject(null);
        BorderlessWindow.MinimizeWindow();
    }

    public void ResizeWindow()
    {
        EventSystem.current.SetSelectedGameObject(null);

        maximized = !maximized;

        if (maximized) BorderlessWindow.MaximizeWindow();
        else BorderlessWindow.RestoreWindow();
        
        var rect = BorderlessWindow.GetWindowRect();
        if (!maximized && rect.yMin < 0) BorderlessWindow.MoveWindowDelta(Vector2.up * rect.yMin);

        OnSizeChange();
    }

    public void FinalizeDrag() 
    {
        if (!maximized) 
        {
            var rect = BorderlessWindow.GetWindowRect();
            if (rect.yMin - Input.mousePosition.y + Screen.height < 1 && !maximized) ResizeWindow();
            else if (rect.yMin < 0) BorderlessWindow.MoveWindowDelta(Vector2.up * rect.yMin);
        }
    }

    public void OnSizeChange() 
    {
        maximized = BorderlessWindow.IsMaximized;
        ResizeTooltip.Text = maximized ? "Restore" : "Maximize";
        ResizeIconMaximize.SetActive(!maximized);
        ResizeIconRestore.SetActive(maximized);
        TopBorder.gameObject.SetActive(!maximized);
    }
    public void OnActiveChange() 
    {
        active = BorderlessWindow.IsActive;
        InactiveBackground.SetActive(!active);
        LeftGroup.alpha = CenterGroup.alpha = RightGroup.alpha = active ? 1 : 0.5f;
    }

    public void OnPointerEnter(PointerEventData data)
    {
        if (!framed) BorderlessWindow.CurrentWindowZone = WindowZone.TitleBar;
    }

    public void OnPointerExit(PointerEventData data)
    {
        BorderlessWindow.CurrentWindowZone = WindowZone.Client;
    }

    public void SetCustomCursor(CursorDefinition cursor) 
    {
        activeCustomCursor = cursor;
        currentCursorFrame = 0;
        Cursor.SetCursor(cursor.Frames[0].Texture, cursor.Pivot, CursorMode.Auto);
    }
}
