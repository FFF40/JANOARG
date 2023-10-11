using UnityEngine;
using UnityEngine.EventSystems;

public class WindowHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler
{
    public static WindowHandler main;

    [Header("Objects")]
    public GameObject WindowControls;
    [Space]
    public TooltipTarget ResizeTooltip;
    public RectTransform ResizeIcon1;
    public RectTransform ResizeIcon2;
    [Header("Window")]
    public Vector2Int defaultWindowSize;
    public Vector2Int borderSize;
    public Vector2Int windowMargin;

    Vector2 mousePos = Vector2.zero;
    bool maximized;
    bool isFullScreen;

    float clickTime = float.NegativeInfinity;

    public void Awake()
    {
        main = this;
    }

    public void Start()
    {
        #if !UNITY_EDITOR && UNITY_STANDALONE_WIN 
            BorderlessWindow.HookWindowProc();
        #endif
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

        if (maximized) BorderlessWindow.RestoreWindow();
        else BorderlessWindow.MaximizeWindow();

        maximized = !maximized;
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

    public void OnPointerDown(PointerEventData data)
    {
        if (Time.time - clickTime < .5f)
        {
            ResizeWindow();
            clickTime = float.NegativeInfinity;
            mousePos = Vector2.zero * float.NaN;
        }
        else 
        {
            clickTime = Time.time;
            mousePos = Input.mousePosition;
        }
    }

    public void OnDrag(PointerEventData data)
    {
        if (float.IsNaN(mousePos.x) || BorderlessWindow.framed) return;

        if (maximized) {
            ResizeWindow();
            var rect = BorderlessWindow.GetWindowRect();
            BorderlessWindow.MoveWindow(new Vector2(Mathf.Clamp(Input.mousePosition.x - rect.width / 2 + 7, 0, Screen.width), Screen.height * 2 - rect.height - Input.mousePosition.y - 28));
            mousePos = new Vector2(rect.width / 2 + 7, rect.height - 30);
        } else {
        }

        if (data.dragging)
        {
            BorderlessWindow.MoveWindowDelta((Vector2)Input.mousePosition - mousePos);
        }
    }

    public void OnEndDrag(PointerEventData data)
    {
        FinalizeDrag();
    }
}
