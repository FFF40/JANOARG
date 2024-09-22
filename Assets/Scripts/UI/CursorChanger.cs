using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Runtime.InteropServices;
using System;

public class CursorChanger : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IEndDragHandler, IPointerUpHandler, IPointerExitHandler
{
    public CursorType CursorType;

    bool IsPointerInside;
    bool IsPointerHolding;
    bool IsShowingCursor;

    public void OnPointerEnter(PointerEventData eventData)
    {
        IsPointerInside = true;
        if (!IsShowingCursor) PushCursor(CursorType);
        IsShowingCursor = true;
        BorderlessWindow.UpdateCursor();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        IsPointerHolding = true;
        if (!IsPointerInside) OnPointerEnter(eventData);
        else BorderlessWindow.UpdateCursor();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        IsPointerHolding = false;
        if (!IsPointerInside && IsShowingCursor)
        {
            PopCursor();
            IsShowingCursor = false;
        }
        BorderlessWindow.UpdateCursor();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!IsPointerInside) OnPointerUp(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsPointerInside = false;
        if (!IsPointerHolding && IsShowingCursor)
        {
            PopCursor();
            IsShowingCursor = false;
        }
        BorderlessWindow.UpdateCursor();
    }

    public void OnDisable()
    {
        if (IsShowingCursor) PopCursor();
        IsShowingCursor = false;
        BorderlessWindow.UpdateCursor();
    }

    public static Stack<IntPtr> Cursors = new();
    public static Stack<CursorType> CursorTypes = new();

    public static void PushCursor(CursorType type)
    {
        if (Platform.IsWin32APIApplicable())
            Cursors.Push(!Chartmaker.Preferences.CustomCursors && type > 0 ? LoadCursor(IntPtr.Zero, type) : IntPtr.Zero);
        else
            Cursors.Push(IntPtr.Zero);
        CursorTypes.Push(type);
    }

    public static void PopCursor()
    {
        if (Platform.IsWin32APIApplicable())
            DestroyCursor(Cursors.Pop());
        else
            Cursors.Pop();

        CursorTypes.Pop();
    }

    [DllImport("user32.dll")]
    static extern IntPtr LoadCursor(IntPtr hInstance, CursorType lpCursorName);
    [DllImport("user32.dll")]
    static extern bool DestroyCursor(IntPtr hCursor);
}

public enum CursorType
{
    Arrow = 32512,
    Pointer = 32649,
    Busy = 32514,
    Text = 32513,
    SizeHorizontal = 32644,
    SizeVertical = 32645,
    Blocked = 32648,

    Grab = -1,
    Grabbing = -2,
}
