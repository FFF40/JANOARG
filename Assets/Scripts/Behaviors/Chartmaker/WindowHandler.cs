using UnityEngine;
using UnityEngine.EventSystems;

public class WindowHandler : MonoBehaviour, IDragHandler
{
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

    Vector2 deltaValue = Vector2.zero;
    bool maximized;
    bool isFullScreen;

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

    public void OnDrag(PointerEventData data)
    {
        if (BorderlessWindow.framed) return;

        if (maximized) {
            ResizeWindow();
            var rect = BorderlessWindow.GetWindowRect();
            BorderlessWindow.MoveWindow(new Vector2(0, Mathf.Clamp(Input.mousePosition.x - rect.width / 2, 0, Screen.width)));
        }

        deltaValue += data.delta;
        if (data.dragging)
        {
            BorderlessWindow.MoveWindowDelta(deltaValue);
        }
    }
}
