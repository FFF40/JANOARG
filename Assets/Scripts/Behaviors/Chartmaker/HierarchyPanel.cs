using System;
using System.Collections;
using System.Collections.Generic;
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
    public RectTransform PanelHolder;

    public Sprite[] Icons;

    public HierarchyItemHolder HolderSample;
    public RectTransform HolderParent;
    List<HierarchyItem> Items = new();
    List<HierarchyItemHolder> Holders = new();

    Dictionary<string, HierarchyItem> GroupItems = new();

    public void Awake()
    {
        main = this;
    }

    public void Start()
    {
    }

    public void InitHierarchy() 
    {
        Items.Clear();
        foreach (var item in Holders) Destroy(item.gameObject);
        GroupItems.Clear();

        if (Chartmaker.main.CurrentChart != null) 
        {
            Chart chart = Chartmaker.main.CurrentChart;
            Items.Add(new HierarchyItem {
                Name = "Camera",
                Type = HierarchyItemType.Camera,
                Target = chart.Camera
            });

            HierarchyItem paletteItem;
            Items.Add(paletteItem = new () {
                Name = "Palette",
                Type = HierarchyItemType.Palette,
                Target = chart.Pallete
            });
            
            int index = 0;
            foreach (var style in chart.Pallete.LaneStyles)
            {
                HierarchyItem item = new () {
                    Name = "ID " + index,
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
                    Name = "ID " + index,
                    Type = HierarchyItemType.HitStyle,
                    Target = style,
                };
                paletteItem.Children.Add(item);
                index++;
            }

            HierarchyItem worldItem;
            Items.Add(worldItem = new () {
                Name = "World",
                Type = HierarchyItemType.World,
            });

            // Add lane groups
            foreach (var group in chart.Groups)
            {
                HierarchyItem item = new () {
                    Name = group.Name,
                    Type = HierarchyItemType.LaneGroup,
                    Target = group,
                };
                GroupItems[group.Name] = item;
            }
            foreach (var group in GroupItems)
            {
                LaneGroup data = (LaneGroup)group.Value.Target;
                if (!string.IsNullOrEmpty(data.Group) && GroupItems.ContainsKey(data.Group)) GroupItems[data.Group].Children.Add(group.Value);
                else worldItem.Children.Add(group.Value);
            }
            
            // Add lanes
            foreach (var lane in chart.Lanes)
            {
                HierarchyItem item = new () {
                    Name = "Lane <alpha=#77>" + lane.LaneSteps[0].Offset + "~" + lane.LaneSteps[^1].Offset,
                    Type = HierarchyItemType.Lane,
                    Target = lane,
                };

                if (!string.IsNullOrEmpty(lane.Group) && GroupItems.ContainsKey(lane.Group)) GroupItems[lane.Group].Children.Add(item);
                else worldItem.Children.Add(item);
            }
        }

        UpdateHolders();
    }

    public void UpdateHolders () 
    {
        int count = 0;
        void AddHolder(HierarchyItem item, int indent) 
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

        foreach (var item in Items) AddHolder(item, 0);

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
                holder.Target.Target != null
                && holder.Target.Target == InspectorPanel.main.CurrentHierarchyObject
            );
        }
    }

    public void Select(HierarchyItem item) 
    {
        if (item.Target != null) InspectorPanel.main.SetObject(item.Target);
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

public class HierarchyItem
{
    public string Name;
    public HierarchyItemType Type;
    public object Target;
    public List<HierarchyItem> Children = new();
    
    public bool Expanded;
}

public enum HierarchyItemType 
{
    Camera = 0,
    Palette = 1,
    LaneStyle = 2,
    HitStyle = 3,
    World = 4,
    LaneGroup = 5,
    Lane = 6,
}