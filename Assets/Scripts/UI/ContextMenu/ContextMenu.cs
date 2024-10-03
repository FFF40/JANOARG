using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ContextMenu : MonoBehaviour
{
    public RectTransform Holder;
    public LayoutGroup Layout;
    public ContextMenu ChildMenu;

    public List<ContextMenuItem> Items;
    public List<GameObject> Separators;

    public RectTransform currentTarget;

    public bool isOpen;
    public bool? justState;

    public void Open(ContextMenuList items, RectTransform target, ContextMenuDirection direction = ContextMenuDirection.Down, Vector2 offset = new Vector2())
    {
        if (isOpen && currentTarget == target) return;
        if (!isOpen && justState == true) return;

        if (ChildMenu) ChildMenu.Close();

        int itemCount = 0;
        int sepCount = 0;

        foreach (ContextMenuListItem item in items.Items) 
        {
            if (item is ContextMenuListSeparator)
            {
                if (sepCount >= Separators.Count)
                {
                    var sep = Instantiate(ContextMenuHolder.main.SeparatorSample, Holder);
                    Separators.Add(sep);
                }
                else 
                {
                    var sep = Separators[sepCount];
                    sep.SetActive(true);
                    sep.transform.SetAsLastSibling();
                }
                sepCount++;
            }
            else if (item is ContextMenuListAction action)
            {
                ContextMenuItem sep = null;
                if (itemCount >= Items.Count)
                {
                    sep = Instantiate(ContextMenuHolder.main.ContextMenuItemSample, Holder);
                    Items.Add(sep);
                }
                else 
                {
                    sep = Items[itemCount];
                    sep.gameObject.SetActive(true);
                    sep.Button.onHover.RemoveAllListeners();
                    sep.Button.onClick.RemoveAllListeners();
                    sep.transform.SetAsLastSibling();
                }
                sep.Group.alpha = (sep.Button.interactable = action.Enabled) ? 1 : .5f;
                sep.ContentLabel.text = action.Content;
                sep.ShortcutLabel.text = action.Shortcut;
                sep.SubmenuIndicator.SetActive(false);
                sep.CheckedIndicator.SetActive(action.Checked);
                sep.Icon.gameObject.SetActive(!string.IsNullOrEmpty(action.Icon));
                if (sep.Icon.gameObject.activeSelf) sep.Icon.sprite = ContextMenuHolder.main.GetIcon(action.Icon);
                sep.CheckedIndicator.SetActive(action.Checked);
                sep.Button.onClick.AddListener(action.Action);
                sep.Button.onClick.AddListener(ContextMenuHolder.main.CloseRoot);
                sep.Button.onHover.AddListener(() => ChildMenu?.Close());
                itemCount++;
            }
            else if (item is ContextMenuListSublist list)
            {
                ContextMenuItem sep = null;
                if (itemCount >= Items.Count)
                {
                    sep = Instantiate(ContextMenuHolder.main.ContextMenuItemSample, Holder);
                    Items.Add(sep);
                }
                else 
                {
                    sep = Items[itemCount];
                    sep.gameObject.SetActive(true);
                    sep.Button.onHover.RemoveAllListeners();
                    sep.Button.onClick.RemoveAllListeners();
                    sep.transform.SetAsLastSibling();
                }
                sep.Group.alpha = 1;
                sep.Button.interactable = true;
                sep.ContentLabel.text = list.Title;
                sep.ShortcutLabel.text = "";
                sep.SubmenuIndicator.SetActive(true);
                sep.CheckedIndicator.SetActive(false);
                sep.Icon.gameObject.SetActive(!string.IsNullOrEmpty(list.Icon));
                if (sep.Icon.gameObject.activeSelf) sep.Icon.sprite = ContextMenuHolder.main.GetIcon(list.Icon);
                void act()
                {
                    if (ChildMenu?.isOpen == false || ChildMenu?.currentTarget != sep.transform)
                    {
                        if (ChildMenu)
                        {
                            ChildMenu.Close();
                            ChildMenu.justState = false;
                        } 
                        ChildMenu = ContextMenuHolder.main.Open(
                            ChildMenu, list.Items, (RectTransform)sep.transform, ContextMenuDirection.Right, new Vector2(0, 3)
                        );
                    }
                }
                sep.Button.onHover.AddListener(act);
                sep.Button.onClick.AddListener(act);
                itemCount++;
            }
        }
        
        gameObject.SetActive(true);

        RectTransform rt = (RectTransform)transform;
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);

        rt.sizeDelta = new Vector2(Mathf.Ceil(Layout.preferredWidth), Mathf.Ceil(Layout.preferredHeight));
        
        Rect rect = GetWorldRect(target);

        bool hasBeenFunnied = false;
        funny:
        if (direction == ContextMenuDirection.Cursor) 
        {
            rt.anchoredPosition = new Vector2(
                Mathf.Round(Input.mousePosition.x),
                Mathf.Round(Input.mousePosition.y)
            ) + offset;
            if (rt.anchoredPosition.x + rt.sizeDelta.x > Screen.width) 
            {
                rt.anchoredPosition += Vector2.left * rt.rect.width;
                Debug.Log(rt.rect.width - rect.width);
            }
            if (rt.anchoredPosition.y - rt.sizeDelta.y < 0) 
            {
                rt.anchoredPosition += Vector2.up * rt.rect.height;
            }
        }
        else if (direction == ContextMenuDirection.Down) 
        {
            rt.anchoredPosition = new Vector2(
                Mathf.Round(rect.xMin),
                Mathf.Round(rect.yMin)
            ) + offset;
            if (rt.anchoredPosition.x + rt.sizeDelta.x > Screen.width) 
            {
                rt.anchoredPosition += Vector2.left * (rt.rect.width - rect.width);
                Debug.Log(rt.rect.width - rect.width);
            }
            if (rt.anchoredPosition.y - rt.sizeDelta.y < 0 && !hasBeenFunnied) 
            {
                direction = ContextMenuDirection.Up;
                hasBeenFunnied = true;
                goto funny;
            }
        }
        else if (direction == ContextMenuDirection.Up) 
        {
            rt.anchoredPosition = new Vector2(
                Mathf.Round(rect.xMin),
                Mathf.Round(rect.yMax + rt.sizeDelta.y)
            ) + offset;
            if (rt.anchoredPosition.x + rt.sizeDelta.x > Screen.width) 
            {
                rt.anchoredPosition += Vector2.left * (rt.rect.width - rect.width);
                Debug.Log(rt.rect.width - rect.width);
            }
            if (rt.anchoredPosition.y > Screen.height && !hasBeenFunnied) 
            {
                direction = ContextMenuDirection.Down;
                hasBeenFunnied = true;
                goto funny;
            }
        }
        else if (direction == ContextMenuDirection.Left) 
        {
            rt.anchoredPosition = new Vector2(
                Mathf.Round(rect.xMin - rt.rect.width),
                Mathf.Round(rect.yMax)
            ) + offset;
            if (rt.anchoredPosition.y - rt.sizeDelta.y < -Screen.height) 
            {
                rt.anchoredPosition += Vector2.up * (rt.rect.height - rect.height);
            }
        }
        else if (direction == ContextMenuDirection.Right) 
        {
            rt.anchoredPosition = new Vector2(
                Mathf.Round(rect.xMax),
                Mathf.Round(rect.yMax)
            ) + offset;
            if (rt.anchoredPosition.y - rt.sizeDelta.y < -Screen.height) 
            {
                rt.anchoredPosition += Vector2.up * (rt.rect.height - rect.height);
            }
        }

        rt.anchoredPosition = new (
            Mathf.Max(rt.anchoredPosition.x, 0),
            Mathf.Min(rt.anchoredPosition.y, Screen.height - (Chartmaker.Preferences.UseDefaultWindow ? 0 : 28))
        );

        isOpen = true;
        StopCoroutine(Intro());
        if (justState == false || currentTarget != target) StartCoroutine(Intro());

        currentTarget = target;

        SetParentMenuActive(true);
    }

    public void Close()
    {
        if (isOpen && justState == false) return;

        foreach (GameObject obj in Separators) obj.SetActive(false);
        foreach (ContextMenuItem item in Items) item.gameObject.SetActive(false);

        gameObject.SetActive(false);
        isOpen = false;
        SetParentMenuActive(false);
        
        ChildMenu?.Close();
        StopCoroutine(Intro());
    }

    public void SetParentMenuActive(bool active)
    {
        NavBarButton nbb;
        ContextMenuButton cmb;
        if (nbb = currentTarget.GetComponent<NavBarButton>()) nbb.SetMenuActive(active);
        else if (cmb = currentTarget.GetComponent<ContextMenuButton>()) cmb.SetMenuActive(active);
    }

    IEnumerator Intro()
    {
        RectTransform rt = (RectTransform)transform;
        rt.anchoredPosition -= new Vector2(-2, 2);
        yield return new WaitForSecondsRealtime(0.05f);
        rt.anchoredPosition += new Vector2(-2, 2);
    }

    static public Rect GetWorldRect (RectTransform rt) {
		Vector3[] corners = new Vector3[4];
		rt.GetWorldCorners(corners);
		return new Rect(corners[0], corners[2] - corners[0]);
	}
}
