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
    public RectTransform ModalHolder;
    public RectTransform LoaderHolder;
    public GameObject MenuButton;
    public RectTransform SongDetails;
    [Space]
    public TooltipTarget ResizeTooltip;
    public RectTransform ResizeIcon1;
    public RectTransform ResizeIcon2;
    [Header("Window")]
    public Vector2Int defaultWindowSize;
    public Vector2Int borderSize;
    public Vector2Int windowMargin;
    [Header("Window")]
    public List<CursorType> CursorMapping;
    public List<Texture2D> Cursors;
    public List<Vector2> CursorPivots;
    public Dictionary<CursorType, int> CursorMap;

    Vector2 mousePos = Vector2.zero;
    public bool maximized { get; private set; }
    bool isFullScreen;

    bool framed;

    // TODO: Do something with this variable or remove it
    float clickTime = float.NegativeInfinity;

    public void Awake()
    {
        main = this;
        CursorMap = new();
        for (int a = 0; a < CursorMapping.Count; a++)
        {
            CursorMap.Add(CursorMapping[a], a);
        }
    }

    public void Start()
    {
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
            maximized = BorderlessWindow.IsMaximized;
            ResizeTooltip.Text = maximized ? "Restore" : "Maximize";
            ResizeIcon1.sizeDelta = ResizeIcon2.sizeDelta = maximized ? new(8, 8) : new(10, 10);
        }
        if (framed != BorderlessWindow.IsFramed) 
        {
            framed = BorderlessWindow.IsFramed;
            ContentHolder.sizeDelta = ModalHolder.sizeDelta = LoaderHolder.sizeDelta = NavBar.anchoredPosition = Vector2.up * (framed ? 0 : -28);
            MenuButton.SetActive(framed);
            SongDetails.anchoredPosition = Vector2.right * (framed ? 32 : 4);
        }
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

        ResizeTooltip.Text = maximized ? "Restore" : "Maximize";
        ResizeIcon1.sizeDelta = ResizeIcon2.sizeDelta = maximized ? new(8, 8) : new(10, 10);
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

    public void OnPointerEnter(PointerEventData data)
    {
        BorderlessWindow.IsInTitleBar = true;
    }

    public void OnPointerExit(PointerEventData data)
    {
        BorderlessWindow.IsInTitleBar = false;
    }
}
