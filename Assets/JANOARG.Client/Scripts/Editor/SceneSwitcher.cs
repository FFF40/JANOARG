using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class SceneSwitcher : EditorWindow
{
    class Entry { public string path, name, folder; }

    // Scenes whose asset path contains any of these get hidden. Edit freely.
    static readonly string[] Hidden = { "TextMesh Pro", "/Examples" };

    const string ScenePinKey  = "SceneSwitcher.Pins";
    const string FolderPinKey = "SceneSwitcher.PinnedFolders";

    List<Entry> scenes = new();
    HashSet<string> pinnedScenes = new();
    HashSet<string> pinnedFolders = new();
    Dictionary<string, bool> collapsed = new();
    string search = "";
    bool includePackages;
    Vector2 scroll;

    [MenuItem("Window/Scene Switcher %#o")]
    static void Open() => GetWindow<SceneSwitcher>("Scenes");

    void OnEnable()
    {
        wantsMouseMove = true;
        pinnedScenes  = Load(ScenePinKey);
        pinnedFolders = Load(FolderPinKey);
        Refresh();
    }

    void OnFocus() => Refresh();
    void OnProjectChange() => Refresh();

    static HashSet<string> Load(string key) =>
        new HashSet<string>(EditorPrefs.GetString(key, "").Split('|', StringSplitOptions.RemoveEmptyEntries));

    static void Save(string key, HashSet<string> set) =>
        EditorPrefs.SetString(key, string.Join("|", set));

    void Refresh()
    {
        scenes = AssetDatabase.FindAssets("t:Scene")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(p => includePackages || p.StartsWith("Assets/"))
            .Where(p => !Hidden.Any(h => p.Contains(h)))
            .Select(p => new Entry
            {
                path = p,
                name = Path.GetFileNameWithoutExtension(p),
                folder = Path.GetDirectoryName(p).Replace('\\', '/').Replace("Assets/", "")
            })
            .OrderBy(e => e.folder).ThenBy(e => e.name)
            .ToList();
        Repaint();
    }

    void OnGUI()
    {
        if (Event.current.type == EventType.MouseMove) Repaint();

        // Toolbar
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            search = GUILayout.TextField(search, EditorStyles.toolbarSearchField);
            EditorGUI.BeginChangeCheck();
            includePackages = GUILayout.Toggle(includePackages, "Pkgs", EditorStyles.toolbarButton, GUILayout.Width(45));
            if (EditorGUI.EndChangeCheck()) Refresh();
        }

        string current = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
        bool searching = !string.IsNullOrEmpty(search);

        var filtered = scenes.Where(e => !searching ||
            e.name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
            e.folder.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

        scroll = EditorGUILayout.BeginScrollView(scroll);

        // Pinned scenes
        var pinned = filtered.Where(e => pinnedScenes.Contains(e.path)).ToList();
        if (pinned.Count > 0)
        {
            EditorGUI.LabelField(EditorGUILayout.GetControlRect(false, 18), "★ Pinned", EditorStyles.miniBoldLabel);
            foreach (var e in pinned) Row(e, current);
            GUILayout.Space(6);
        }

        // Folders: pinned first, then alphabetical
        foreach (var g in filtered.GroupBy(e => e.folder)
                                  .OrderByDescending(g => pinnedFolders.Contains(g.Key))
                                  .ThenBy(g => g.Key))
        {
            bool folderPinned = pinnedFolders.Contains(g.Key);
            collapsed.TryGetValue(g.Key, out bool isCollapsed);

            var hr    = EditorGUILayout.GetControlRect(false, 18);
            var starR = new Rect(hr.xMax - 22, hr.y, 22, hr.height);
            var headR = new Rect(hr.x, hr.y, hr.width - 22, hr.height);

            if (GUI.Button(starR, new GUIContent(folderPinned ? "★" : "☆", "Pin folder"), EditorStyles.label))
            {
                if (folderPinned) pinnedFolders.Remove(g.Key); else pinnedFolders.Add(g.Key);
                Save(FolderPinKey, pinnedFolders);
                GUIUtility.ExitGUI();
            }

            if (!searching)
            {
                bool open = EditorGUI.Foldout(headR, !isCollapsed, g.Key, true, EditorStyles.foldoutHeader);
                collapsed[g.Key] = !open;
                if (!open) continue;
            }
            else EditorGUI.LabelField(headR, g.Key, EditorStyles.miniBoldLabel);

            foreach (var e in g) Row(e, current);
            GUILayout.Space(4);
        }

        EditorGUILayout.EndScrollView();
    }

    void Row(Entry e, string current)
    {
        var r = EditorGUILayout.GetControlRect(false, 22);
        bool isCurrent = e.path == current;
        bool hover = r.Contains(Event.current.mousePosition);

        if (Event.current.type == EventType.Repaint)
        {
            if (isCurrent) EditorGUI.DrawRect(r, new Color(0.17f, 0.36f, 0.53f, 1f));
            else if (hover) EditorGUI.DrawRect(r, new Color(1f, 1f, 1f, 0.08f));
        }

        var star = new Rect(r.x + 2, r.y, 20, r.height);
        var ping = new Rect(r.xMax - 24, r.y, 22, r.height);
        var add  = new Rect(r.xMax - 48, r.y, 22, r.height);
        var main = new Rect(star.xMax, r.y, add.x - star.xMax, r.height);

        // Name label
        var label = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleLeft };
        if (isCurrent) { label.fontStyle = FontStyle.Bold; label.normal.textColor = Color.white; }
        GUI.Label(main, new GUIContent(e.name, e.path), label);

        // Single click = ping in Project, double click = open
        var ev = Event.current;
        if (ev.type == EventType.MouseDown && ev.button == 0 && main.Contains(ev.mousePosition))
        {
            if (ev.clickCount == 2)
            {
                ev.Use();
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    EditorSceneManager.OpenScene(e.path, OpenSceneMode.Single);
                GUIUtility.ExitGUI();
            }
            else
            {
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(e.path));
            }
        }

        // Pin scene
        bool isPinned = pinnedScenes.Contains(e.path);
        if (GUI.Button(star, new GUIContent(isPinned ? "★" : "☆", "Pin scene"), EditorStyles.label))
        {
            if (isPinned) pinnedScenes.Remove(e.path); else pinnedScenes.Add(e.path);
            Save(ScenePinKey, pinnedScenes);
            GUIUtility.ExitGUI();
        }

        // Open additively
        if (GUI.Button(add, new GUIContent("+", "Open additively"), EditorStyles.label))
        {
            EditorSceneManager.OpenScene(e.path, OpenSceneMode.Additive);
            GUIUtility.ExitGUI();
        }

        // Show in Project
        if (GUI.Button(ping, new GUIContent("↗", "Show in Project"), EditorStyles.label))
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(e.path));
    }
}