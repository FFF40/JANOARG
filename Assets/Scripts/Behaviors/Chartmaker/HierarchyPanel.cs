using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HierarchyPanel : MonoBehaviour
{
    public static HierarchyPanel main;

    [Space]
    public bool IsCollapsed;
    public HierarchyMode CurrentMode;
    public RectTransform PanelHolder;
    [Space]
    public Sprite[] Icons;
    [Space]
    public HierarchyItemHolder HolderSample;
    public RectTransform HolderParent;
    List<HierarchyItem> Items = new();
    List<HierarchyItemHolder> Holders = new();
    [Space]
    public TMP_InputField SearchField;
    public Button SearchButton;
    public Image SearchIcon;
    public Image SearchClearIcon;

    Dictionary<string, HierarchyItem> GroupItems = new();

    public void Awake()
    {
        main = this;
    }

    public void Start()
    {
    }

    public void SetMode(HierarchyMode mode)
    {
        CurrentMode = mode;
        InspectorPanel.main.SetObject(null);
        InformationBar.main.UpdateButtonActivity();
        PlayerView.main.UpdateObjects();
        InitHierarchy();
    }

    public void InitHierarchy() 
    {
        Items.Clear();
        foreach (var item in Holders) Destroy(item.gameObject);
        Holders.Clear();
        GroupItems.Clear();

        if (CurrentMode == HierarchyMode.PlayableSong)
        {
            PlayableSong song = Chartmaker.main.CurrentSong;
            HierarchyItem songItem;
            Items.Add(songItem = new () {
                Name = "Playable Song",
                Type = HierarchyItemType.PlayableSong,
                Target = song,
                Expanded = true,
            });
            
            HierarchyItem coverItem;
            songItem.Children.Add(coverItem = new HierarchyItem {
                Name = "Cover",
                Type = HierarchyItemType.Cover,
                Target = song.Cover,
            });
        }
        else if (CurrentMode == HierarchyMode.Chart)
        {
            if (Chartmaker.main.CurrentChart != null) 
            {
                Chart chart = Chartmaker.main.CurrentChart;
                HierarchyItem chartItem;
                Items.Add(chartItem = new () {
                    Name = "Chart",
                    Type = HierarchyItemType.Chart,
                    Target = chart,
                    Expanded = true,
                });

                chartItem.Children.Add(new HierarchyItem {
                    Name = "Camera",
                    Type = HierarchyItemType.Camera,
                    Target = chart.Camera
                });

                HierarchyItem paletteItem;
                chartItem.Children.Add(paletteItem = new () {
                    Name = "Palette",
                    Type = HierarchyItemType.Palette,
                    Target = chart.Pallete
                });

                HierarchyItem worldItem;
                chartItem.Children.Add(worldItem = new () {
                    Name = "World",
                    Type = HierarchyItemType.World,
                });
            }
        }
        UpdateHierarchy();
    }

    public void UpdateHierarchy()
    {
        
        if (CurrentMode == HierarchyMode.PlayableSong)
        {
            PlayableSong song = Chartmaker.main.CurrentSong;

            var coverItem = Items[0].Children[0];
            coverItem.Children.Clear();
        
            foreach (var layer in song.Cover.Layers)
            {
                HierarchyItem item = new () {
                    Name = layer.Target,
                    Type = HierarchyItemType.CoverLayer,
                    Target = layer,
                };

                coverItem.Children.Add(item);
            }
        }
        else if (CurrentMode == HierarchyMode.Chart)
        {
            Chart chart = Chartmaker.main.CurrentChart;

            if (chart != null)
            {
                var paletteItem = Items[0].Children[1];
                paletteItem.Children.Clear();
                    
                int index = 0;
                foreach (var style in chart.Pallete.LaneStyles)
                {
                    HierarchyItem item = new () {
                        Name = string.IsNullOrWhiteSpace(style.Name) ? ("Lane Style " + index) : style.Name,
                        Type = HierarchyItemType.LaneStyle,
                        Target = style,
                    };
                    paletteItem.Children.Add(item);
                    index++;
                }
                index = 0;
                foreach (var style in chart.Pallete.HitStyles)
                {
                    HierarchyItem item = new () {
                        Name = string.IsNullOrWhiteSpace(style.Name) ? ("Hit Style " + index) : style.Name,
                        Type = HierarchyItemType.HitStyle,
                        Target = style,
                    };
                    paletteItem.Children.Add(item);
                    index++;
                }

                HierarchyItem worldItem = Items[0].Children[2];
                worldItem.Children.Clear();

                // Add lane groups
                Dictionary<string, HierarchyItem> newGroupItems = new ();
                foreach (var group in chart.Groups)
                {
                    HierarchyItem item = new () {
                        Name = group.Name,
                        Type = HierarchyItemType.LaneGroup,
                        Target = group,
                        Expanded = GroupItems.ContainsKey(group.Name) ? GroupItems[group.Name].Expanded : false,
                    };
                    newGroupItems[group.Name] = item;
                }
                GroupItems = newGroupItems;
                var keys = GroupItems.Keys.ToList();
                int keyindex = 0;
                foreach (var key in keys)
                {
                    LaneGroup data = (LaneGroup)GroupItems[key].Target;
                    if (!string.IsNullOrEmpty(data.Group) && GroupItems.ContainsKey(data.Group)) GroupItems[data.Group].Children.Add(GroupItems[key]);
                    else worldItem.Children.Add(GroupItems[key]);
                    keyindex++;
                }
                // Add lanes
                foreach (var lane in chart.Lanes)
                {
                    HierarchyItem item = new () {
                        Name = string.IsNullOrWhiteSpace(lane.Name) ? "Lane" : lane.Name,
                        Subname = lane.LaneSteps[0].Offset + "~" + lane.LaneSteps[^1].Offset,
                        Type = HierarchyItemType.Lane,
                        Target = lane,
                    };

                    if (!string.IsNullOrEmpty(lane.Group) && GroupItems.ContainsKey(lane.Group)) GroupItems[lane.Group].Children.Add(item);
                    else worldItem.Children.Add(item);
                }
            }
        }
        UpdateHolders();
    }

    public void UpdateHolders () 
    {
        int count = 0;

        if (string.IsNullOrWhiteSpace(SearchField.text)) 
        {
            void AddHolder(HierarchyItem item, int indent) 
            {
                
                if (
                    HierarchyFiltersPanel.main.GetVisibility(item.Type, HierarchyContext.Hierarchy)
                )
                    {
                    HierarchyItemHolder holder;
                    if (count >= Holders.Count) 
                    {
                        holder = Instantiate(HolderSample, HolderParent);
                        Holders.Add(holder);
                    }
                    else 
                    {
                        holder = Holders[count];
                    }
                    count++;

                    holder.SetItem(item, indent);
                    holder.Icon.sprite = Icons[(int)item.Type];
                    holder.ExpandButton.gameObject.SetActive(item.Children.Count > 0);

                    if (item.Expanded) foreach (var child in item.Children) AddHolder(child, indent + 1);
                }
            }

            foreach (var item in Items) AddHolder(item, 0);
        }
        else 
        {
            void AddHolder(HierarchyItem item) 
            {
                if (
                    item.Name.ContainsInsensitive(SearchField.text) 
                    && HierarchyFiltersPanel.main.GetVisibility(item.Type, HierarchyContext.SearchResult)
                )
                {
                    HierarchyItemHolder holder;
                    if (count >= Holders.Count) 
                    {
                        holder = Instantiate(HolderSample, HolderParent);
                        Holders.Add(holder);
                    }
                    else 
                    {
                        holder = Holders[count];
                    }
                    count++;

                    holder.SetItem(item, 0);
                    holder.Icon.sprite = Icons[(int)item.Type];
                    holder.ExpandButton.gameObject.SetActive(false);
                }

                foreach (var child in item.Children) AddHolder(child);
            }

            foreach (var item in Items) AddHolder(item);
        }

        while (count < Holders.Count) 
        {
            Destroy(Holders[count].gameObject);
            Holders.RemoveAt(count);
        }

        UpdateHolderSelection();
    }

    public void UpdateHolderSelection()
    {
        foreach (var holder in Holders)
        {
            holder.SelectedBackground.SetActive(
                holder.Target.Target != null && (
                    InspectorPanel.main.CurrentHierarchyObject is IList list && list.Contains(holder.Target.Target)
                    || holder.Target.Target == InspectorPanel.main.CurrentHierarchyObject
                )
            );
        }
    }

    public void Select(HierarchyItem item) 
    {
        if (item.Target != null) InspectorPanel.main.SetObject(item.Target);
    }

    public void RightClickSelect(HierarchyItem item, HierarchyItemHolder holder) 
    {
        if (item.Target != null) 
        {
            static string KeyOf(string id) => KeyboardHandler.main.Keybindings[id].Keybind.ToString();

            InspectorPanel.main.SetObject(item.Target);
            ContextMenuHolder.main.OpenRoot(new ContextMenuList(
                new ContextMenuListAction("Cut", Chartmaker.main.Cut, KeyOf("ED:Cut"), 
                    icon: "Cut", _enabled: Chartmaker.main.CanCopy()),
                new ContextMenuListAction("Copy", Chartmaker.main.Copy, KeyOf("ED:Copy"), 
                    icon: "Copy", _enabled: Chartmaker.main.CanCopy()),
                new ContextMenuListAction("Paste <i>" + (Chartmaker.main.CanPaste() ? Chartmaker.GetItemName(Chartmaker.main.ClipboardItem) : ""), Chartmaker.main.Paste, KeyOf("ED:Paste"), 
                    icon: "Paste", _enabled: Chartmaker.main.CanPaste()),
                new ContextMenuListSeparator(),
                new ContextMenuListAction("Rename", () => Rename(holder), KeyOf("ED:Rename"),
                    _enabled: Chartmaker.main.CanRename()),
                new ContextMenuListAction("Delete", () => KeyboardHandler.main.Keybindings["ED:Delete"].Invoke(), KeyOf("ED:Delete"), 
                    _enabled: Chartmaker.main.CanCopy())
            ), (RectTransform)holder.transform, ContextMenuDirection.Cursor);
        }
    }

    public void RenameCurrent() 
    {
        HierarchyItemHolder holder = Holders.Find(x => x.SelectedBackground.activeSelf);
        if (holder) holder.Rename();
    }

    public void Rename(HierarchyItemHolder holder) 
    {
        holder.Rename();
    }

    public void OnSearchFieldUpdate() 
    {
        UpdateHolders();
        bool active = !string.IsNullOrEmpty(SearchField.text);
        SearchButton.interactable = active;
        SearchIcon.gameObject.SetActive(!active);
        SearchClearIcon.gameObject.SetActive(active);
    }

    public void ClearSearch() 
    {
        SearchField.text = "";
    }

    public void ToggleExpand(HierarchyItem item) 
    {
        item.Expanded = !item.Expanded;
        UpdateHolders();
    }
    
    public void OnResizerDrag()
    {
        ResizeHierarchy(Input.mousePosition.x, false);
    }
    public void OnResizerEndDrag()
    {
        ResizeHierarchy(Input.mousePosition.x);
    }
    
    public void ResizeHierarchy(float width, bool snap = true)
    {
        if (snap) width = width < 142 ? 42 : 242;
        else width = Mathf.Clamp(Mathf.Floor(width), 42, 242);

        PanelHolder.anchoredPosition = new(width - 200, PanelHolder.anchoredPosition.y);
        Chartmaker.main.PlayerViewHolder.anchoredPosition = new(
            width, 
            Chartmaker.main.PlayerViewHolder.anchoredPosition.y
        );
        Chartmaker.main.PlayerViewHolder.sizeDelta = new(
            Chartmaker.main.InspectorHolder.anchoredPosition.x - Chartmaker.main.InspectorHolder.sizeDelta.x - width, 
            Chartmaker.main.PlayerViewHolder.sizeDelta.y
        );

        PlayerView.main.Update();
        PlayerView.main.UpdateObjects();

        if (snap) 
        {
            IsCollapsed = width < 141;
        }
    }
    
    public void Collapse()
    {
        ResizeHierarchy(0, true);
    }
    
    public void Restore()
    {
        ResizeHierarchy(240, true);
    }
}

public enum HierarchyMode 
{
    PlayableSong,
    Chart,
}

public class HierarchyItem
{
    public string Name;
    public string Subname;
    public HierarchyItemType Type;
    public object Target;
    public List<HierarchyItem> Children = new();
    
    public bool Expanded;
}

public enum HierarchyContext 
{
    Hierarchy,
    SearchResult
}

public enum HierarchyItemType 
{
    Chart = 0,
    Camera = 1,
    Palette = 2,
    LaneStyle = 3,
    HitStyle = 4,
    World = 5,
    LaneGroup = 6,
    Lane = 7,
    PlayableSong = 8,
    Cover = 9,
    CoverLayer = 10,
}