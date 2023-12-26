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

    public static void PushCursor(CursorType type)
    {
        Cursors.Push(LoadCursor(IntPtr.Zero, type));
    }

    public static void PopCursor()
    {
        DestroyCursor(Cursors.Pop());
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
}
