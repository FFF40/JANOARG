using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class NavBarButton : Button
{
    public override void OnPointerClick(PointerEventData eventData)
    {
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left) 
        {
            onClick.Invoke();
        }
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        if (ContextMenuHolder.main.ContextMenus.Count > 0 
            && ContextMenuHolder.main.ContextMenus[0].isOpen
            && ContextMenuHolder.main.ContextMenus[0].currentTarget.parent == transform.parent)
        {
            ContextMenuHolder.main.ContextMenus[0].justState = null;
            onClick.Invoke();
        }
    }

    bool isMenuActive; 

    public void SetMenuActive(bool active) 
    {
        isMenuActive = active;
        DoStateTransition(currentSelectionState, instant: false);
    }

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        if (isMenuActive) state = SelectionState.Highlighted;
        base.DoStateTransition(state, instant);
    }
}
