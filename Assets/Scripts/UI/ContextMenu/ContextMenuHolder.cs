using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ContextMenuHolder : MonoBehaviour
{
    public static ContextMenuHolder main;

    [Header("Samples")]
    public ContextMenu ContextMenuSample;
    public ContextMenuItem ContextMenuItemSample;
    public GameObject SeparatorSample;
    [Space]
    public List<Sprite> IconPallete = new();

    [Header("Data")]
    public List<ContextMenu> ContextMenus = new();

    public void Awake() {
        main = this;
    }

    public void Update()
    {
        if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)) && ContextMenus.Count > 0 && ContextMenus[0].isOpen) 
        {
            for (int a = ContextMenus.Count - 1; a >= 0; a--) if (ContextMenus[a].isOpen && ContextMenus[a].justState == true)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint((RectTransform)ContextMenus[a].transform, Input.mousePosition, null))
                {
                    break;
                }
                else 
                {
                    ContextMenus[a].Close();
                }
            }
        }

        foreach (ContextMenu menu in ContextMenus) menu.justState = menu.isOpen;
    }

    public Sprite GetIcon(string name) => IconPallete.Find(x => x.name == name);

    public ContextMenu Open(ContextMenu menu, ContextMenuList items, RectTransform target, ContextMenuDirection direction = ContextMenuDirection.Down, Vector2 offset = new Vector2())
    {
        if (!menu) 
        {
            menu = Instantiate(ContextMenuSample, transform);
            ContextMenus.Add(menu);
        }
        if (menu.isOpen)
        {
            menu.Close();
        }
        menu.Open(items, target, direction, offset);
        return menu;
    }

    public void OpenRoot(ContextMenuList items, RectTransform target, ContextMenuDirection direction = ContextMenuDirection.Down, Vector2 offset = new Vector2())
        => Open(ContextMenus.Count > 0 ? ContextMenus[0] : null, items, target, direction, offset);

    public void Close(ContextMenu menu)
    {
        if (!menu) 
        {
            menu = Instantiate(ContextMenuSample, transform);
            ContextMenus.Add(menu);
        }
        menu.Close();
    }

    public void CloseRoot()
        => Close(ContextMenus.Count > 0 ? ContextMenus[0] : null);
}

public enum ContextMenuDirection
{
    Left,
    Right,
    Up,
    Down,
    Cursor,
}
