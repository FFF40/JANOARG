using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEditor;
using UnityEngine;

public class ThemeMultiEditor : EditorWindow
{
    [MenuItem("JANOARG/Theme Multi-Editor", priority = 100)]
    public static void ShowWindow() 
    {
        ThemeMultiEditor window = GetWindow<ThemeMultiEditor>();
        // window.minSize = new Vector2(800, 480);
        window.titleContent = new GUIContent(window.name = "Theme Multi-Editor");
        window.Show();
    }

    Vector2 scrollPos;
    List<Theme> Themes = new();
    List<ThemeKeyInfo> Keys = new();

    string keyToAdd;
    
    bool isKeyDirty, isKeyDragging;
    int keyDragIndex;
    float keyDragTarget;
    
    public void OnGUI()
    {
        
        float cellWidth = 80;
        float cellHeight = EditorGUIUtility.singleLineHeight;
        float labelWidth = 180;

        float cellGap = EditorGUIUtility.standardVerticalSpacing;
        float cellWidthGap = cellWidth + cellGap;
        float cellHeightGap = cellHeight + cellGap;
        float labelWidthGap = labelWidth + cellGap;

        RectOffset padding = GUI.skin.box.margin;
        Vector2 paddingLeft = new (padding.left, 0);
        Vector2 paddingTop = new (0, padding.top);

        Rect getCellRect(float x, float y)
        {
            return new (
                cellWidthGap * x,
                cellHeightGap * y,
                cellWidth,
                cellHeight
            );
        }


        // -------------------------------------------------- Dragging
        
        if (isKeyDragging) 
        {
            EditorGUIUtility.AddCursorRect(new (0, 0, Screen.width, Screen.height), MouseCursor.Pan);
            if (Event.current.type is EventType.MouseDrag or EventType.MouseUp)
            {
                keyDragTarget = (Event.current.mousePosition.y + scrollPos.y - cellHeight - padding.top - padding.bottom) / cellHeightGap - .5f;
                keyDragTarget = Mathf.Clamp(keyDragTarget, 0, Keys.Count - 1);
            }
            if (Event.current.type is EventType.MouseUp)
            {
                isKeyDragging = false;
                int target = Mathf.RoundToInt(keyDragTarget);
                if (keyDragIndex != target) 
                {
                    ThemeKeyInfo key = Keys[keyDragIndex];
                    Keys.RemoveAt(keyDragIndex);
                    Keys.Insert(target, key);
                    isKeyDirty = true;
                }
            }
            if (Event.current.type is EventType.MouseDrag or EventType.MouseMove or EventType.MouseUp)
            {
                Event.current.Use();
            }
        }



        // -------------------------------------------------- Utilities

        GUI.enabled = isKeyDirty;
        if (GUI.Button(new (padding.left, padding.top, 81, cellHeight), "Apply Keys", GUI.skin.FindStyle("buttonLeft")))
        {
            foreach (Theme theme in Themes) 
            {
                for (int a = 0; a < theme.Keys.Count; a++)
                {
                    ThemeKey key = theme.Keys[a];
                    ThemeKeyInfo info = Keys.Find(x => x.CurrentKey == key.Key);
                    if (info != null) key.Key = info.NewKey;
                }
                theme.Keys.Sort((x, y) => Keys.FindIndex(a => a.NewKey == x.Key).CompareTo(Keys.FindIndex(a => a.NewKey == y.Key)));
                EditorUtility.SetDirty(theme);
            }
            foreach (ThemeKeyInfo key in Keys) 
            {
                key.CurrentKey = key.NewKey;
            }
            isKeyDirty = false;
        }
        if (GUI.Button(new (padding.left + 81, padding.top, 81, cellHeight), "Revert Keys", GUI.skin.FindStyle("buttonRight")))
        {
            UpdateKeys();
        }
        GUI.enabled = true;
        

        // -------------------------------------------------- Themes

        Vector2 themeMove = paddingTop + Vector2.left * scrollPos.x;

        GUILayout.BeginArea(new (
            labelWidth + padding.left + padding.right,
            0,
            cellWidthGap * (Themes.Count + 1),
            cellHeightGap + padding.top + padding.bottom
        ));

        for (int i = 0; i < Themes.Count; i++)
        {
            Theme changedTheme = (Theme)EditorGUI.ObjectField(getCellRect(i, 0).Move(themeMove), Themes[i], typeof(Theme), true);
            if (changedTheme != Themes[i]) 
            {
                Themes[i] = changedTheme;
                UpdateKeys();
            }
        }

        Theme newTheme;
        if (newTheme = (Theme)EditorGUI.ObjectField(getCellRect(Themes.Count, 0).Move(themeMove), null, typeof(Theme), true))
        {
            Themes.Add(newTheme);
            Debug.Log("add new theme");
            UpdateKeys();
        }

        GUILayout.EndArea();

        GUILayout.Box("", 
            GUIStyle.none,
            GUILayout.Height(cellHeight + padding.top + padding.bottom)
        );


        // -------------------------------------------------- Keys

        Vector2 keyMove = paddingLeft + Vector2.down * scrollPos.y;

        GUILayout.BeginArea(new (
            0,
            cellHeight + padding.top + padding.bottom,
            labelWidthGap + 120 + padding.left * 2 + padding.right,
            cellHeightGap * (Keys.Count + 1)
        ));

        GUIStyle draggerStyle = GUI.skin.FindStyle("RL DragHandle");

        void drawKey(int i, float pos) 
        {
            Rect draggerRect = new (keyMove.x, cellHeightGap * pos + keyMove.y, cellHeight, cellHeight);
            Rect textFieldRect = new (cellGap + cellHeight + keyMove.x, cellHeightGap * pos + keyMove.y, labelWidth - cellHeight - cellGap, cellHeight);

            if (Keys[i].CurrentKey != Keys[i].NewKey)
            {
                GUI.Label(draggerRect.Move(new (-10, 0)), "", GUI.skin.FindStyle("CN EntryErrorIconSmall"));
            }

            GUI.Label(draggerRect.ShrinkBy(new RectOffset(3, 3, 7, 7)), "", draggerStyle);
            EditorGUIUtility.AddCursorRect(draggerRect, MouseCursor.Pan);

            if (Event.current.type == EventType.MouseDown && draggerRect.Contains(Event.current.mousePosition))
            {
                isKeyDragging = true;
                keyDragTarget = keyDragIndex = i;
                Event.current.Use();
            }

            string changedKey = EditorGUI.TextField(textFieldRect, Keys[i].NewKey);
            if (changedKey != Keys[i].NewKey) 
            {
                Keys[i].NewKey = changedKey;
                isKeyDirty = true;
            }
        }

        for (int i = 0; i < Keys.Count; i++)
        {
            if (isKeyDragging && keyDragIndex == i) continue;
            drawKey(i, i + GetDragOffset(i));
        }
        {
            Rect textFieldRect = new (cellGap + cellHeight + keyMove.x, cellHeightGap * Keys.Count + keyMove.y, labelWidth - cellHeight - cellGap, cellHeight);
            keyToAdd = EditorGUI.TextField(textFieldRect, keyToAdd);

            GUI.enabled = !string.IsNullOrWhiteSpace(keyToAdd);
            if (GUI.Button(new(textFieldRect.x + textFieldRect.width + padding.left, textFieldRect.y, 100, cellHeight), "Add Key")) 
            {
                Keys.Add(new(keyToAdd.Trim()));
                keyToAdd = "";
                isKeyDirty = true;
            } 
            GUI.enabled = true;
        }
        if (isKeyDragging) 
        {
            if (Event.current.type == EventType.Repaint) 
            {
                GUIStyle bgStyle = GUI.skin.button;
                GUI.Toggle(
                    new (
                        -5, keyDragTarget * cellHeightGap - padding.top - scrollPos.y, 
                        Screen.width + 10, padding.top + padding.bottom + cellHeight
                    ), 
                    true, "", bgStyle
                );
            }
            drawKey(keyDragIndex, keyDragTarget);
        }

        GUILayout.EndArea();

        GUILayout.BeginHorizontal();
        GUILayout.Box("", 
            GUIStyle.none,
            GUILayout.Width(labelWidth + padding.left + padding.right)
        );


        // -------------------------------------------------- Cells

        scrollPos = GUILayout.BeginScrollView(scrollPos);

        void drawCell(int themeIndex, int keyIndex, float pos) 
        {
            Theme theme = Themes[themeIndex];
            int index = theme.Keys.FindIndex(x => x.Key == Keys[keyIndex].CurrentKey);
            if (index >= 0)
            {
                Color changedColor = EditorGUI.ColorField(getCellRect(themeIndex, pos), theme.Keys[index].Value);
                if (changedColor != theme.Keys[index].Value) 
            {
                theme.Keys[index] = new (Keys[keyIndex].CurrentKey, changedColor);
                EditorUtility.SetDirty(theme);
            }
            }
            else if (GUI.Button(getCellRect(themeIndex, keyIndex), "Missing")) 
            {
                theme.Keys.Add(new (Keys[keyIndex].CurrentKey, Color.black));
                EditorUtility.SetDirty(theme);
            }
        }

        for (int i = 0; i < Keys.Count; i++)
        {
            if (isKeyDragging && keyDragIndex == i) continue;
            for (int j = 0; j < Themes.Count; j++)
            {
                drawCell(j, i, i + GetDragOffset(i));
            }
        }
        if (isKeyDragging) 
        {
            if (Event.current.type == EventType.Repaint) 
            {
                GUIStyle bgStyle = GUI.skin.button;
                GUI.Toggle(
                    new (
                        -5, keyDragTarget * cellHeightGap - padding.top, 
                        Screen.width + 10, padding.top + padding.bottom + cellHeight
                    ), 
                    true, "", bgStyle
                );
            }
            for (int j = 0; j < Themes.Count; j++)
            {
                drawCell(j, keyDragIndex, keyDragTarget);
            }
        }
        

        GUILayout.Box("", 
            GUIStyle.none,
            GUILayout.Width(cellWidthGap * (Themes.Count + 1) + padding.right),
            GUILayout.Height(cellHeightGap * (Keys.Count + 1) - cellGap + padding.bottom)
        );

        GUILayout.EndScrollView();
        GUILayout.EndHorizontal();
    }

    public int GetDragOffset(int i)
    {
        if (!isKeyDragging) return 0;
        int index = keyDragIndex;
        int target = Mathf.RoundToInt(keyDragTarget);
        return (target >= i && index < i) ? -1 : 
            (target <= i && index > i) ? 1 : 0;
    } 

    public void UpdateKeys()
    {
        Keys.Clear();
        HashSet<string> idSet = new();

        int maxLen = Themes.Max(x => x.Keys.Count);
        for (int i = 0; i < maxLen; i++) 
        {
            foreach (Theme theme in Themes)
            {
                if (theme.Keys.Count <= i) continue;
                ThemeKey key = theme.Keys[i];
                if (idSet.Contains(key.Key)) continue;
                Keys.Add(new(key.Key));
                idSet.Add(key.Key);
            }
        }

        isKeyDirty = false;
    }
}

[Serializable]
class ThemeKeyInfo 
{
    public string CurrentKey;
    public string NewKey;

    public ThemeKeyInfo(string key) 
    {
        CurrentKey = NewKey = key;
    }
    public ThemeKeyInfo(string currentKey, string newKey) 
    {
        CurrentKey = currentKey;
        NewKey = newKey;
    }
}

static class RectExtensions
{
    public static Rect Move(this Rect rect, Vector2 offset)
    {
        return new (rect.position + offset, rect.size);
    }
}
