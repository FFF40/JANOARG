using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

public class JAEditorManual : EditorWindow
{
    [MenuItem("JANOARG/Editor's Manual", false, 200)]
    public static void Open()
    {
        JAEditorManual wnd = GetWindow<JAEditorManual>();
        wnd.titleContent = new GUIContent("Editor's Manual");
        wnd.minSize = new Vector2(720, 400);
    }

    public List<ManualEntry> ContentTable;

    public string CurrentPagePath = null;
    public System.Action CurrentPageFunc;
    public Vector2[] CurrentPageScrolls;

    Font MonospaceFont;

    public bool isBusy = false;
    List<IEnumerator> Coroutines = new();

    void StartCoroutine(IEnumerator routine)
    {
        Coroutines.Add(routine);
    }

    public IEnumerator LoadPage(string path)
    {
        isBusy = true;
        CurrentPagePath = path;

        Task<string[]> task = File.ReadAllLinesAsync(Application.dataPath + "/../Manual/" + path);
        while (!task.IsCompleted)
        {
            yield return null;
        }
        if (task.Exception != null)
        {
            Debug.LogException(task.Exception);
            isBusy = false;
            yield break;
        }

        GUIStyle contentStyle = new GUIStyle("label");
        contentStyle.padding = new RectOffset(25, 25, 2, 2);
        contentStyle.richText = true;
        contentStyle.wordWrap = true;
        contentStyle.fontSize = 13;
        
        GUIStyle titleStyle = new GUIStyle(contentStyle);
        titleStyle.padding = new RectOffset(25, 25, 12, 2);
        titleStyle.fontSize = 28;
        titleStyle.fontStyle = FontStyle.Bold;
        
        GUIStyle titleStyle2 = new GUIStyle(titleStyle);
        titleStyle2.fontSize = 20;

        GUIStyle titleStyle3 = new GUIStyle(titleStyle);
        titleStyle3.fontSize = 16;
        
        GUIStyle linkStyle = new GUIStyle(contentStyle);
        linkStyle.normal.textColor = new GUIStyle("linkLabel").normal.textColor;
        
        GUIStyle preBGStyle = new GUIStyle("textArea");
        preBGStyle.margin = new RectOffset(25, 25, 4, 4);
        
        GUIStyle preStyle = new GUIStyle("label");
        preStyle.font = MonospaceFont;
        preStyle.fontSize = 12;
        
        CurrentPageFunc = () => {};
        string mode = "", text = "";
        int scrollCount = 0;

        void AddText(string t)
        {
            text += t;
        }
        void EndText()
        {
            if (mode == "pre") { EndPre(); return; }
            string t = text.Trim();
            if (string.IsNullOrEmpty(t)) return;

            GUIStyle style = preStyle;
            if (mode == "") style = contentStyle;

            CurrentPageFunc += () => { GUILayout.Label(t, style); };
            text = "";
        }
        void EndPre()
        {
            string t = text.Substring(1);
            if (string.IsNullOrEmpty(t)) return;

            int lines = t.Split("\n").Length;
            int scroll = scrollCount;
            
            GUIStyle style = preStyle;
            
            CurrentPageFunc += () => { 
                CurrentPageScrolls[scroll] = EditorGUILayout.BeginScrollView(CurrentPageScrolls[scroll], true, false, 
                    GUI.skin.horizontalScrollbar, GUIStyle.none, preBGStyle, 
                    GUILayout.MinHeight(style.lineHeight * lines + 22));
                GUILayout.Label(t, style); 
                EditorGUILayout.EndScrollView();
            };
            text = "";

            scrollCount++;
        }

        foreach (string line in task.Result) 
        {
            if (line.StartsWith("```"))
            {
                EndText();
                string lastMode = mode;
                mode = line.Substring(3).Trim();
                if (lastMode == "" && mode == "") mode = "pre";
            }
            else if (mode == "")
            {
                if (string.IsNullOrWhiteSpace(line))     EndText();

                // Headers
                else if (line.StartsWith("###"))         { EndText(); CurrentPageFunc += () => { GUILayout.Label(line.Substring(3).Trim(), titleStyle3); }; }
                else if (line.StartsWith("##"))          { EndText(); CurrentPageFunc += () => { GUILayout.Label(line.Substring(2).Trim(), titleStyle2); }; }
                else if (line.StartsWith("#"))           { EndText(); CurrentPageFunc += () => { GUILayout.Label(line.Substring(1).Trim(), titleStyle); }; }
                
                // Links
                else if (line.StartsWith("=>"))
                { 
                    EndText(); 
                    string link = line.Substring(2).Trim();
                    string label = "⇒ <i>" + link.Substring(link.IndexOf(" ") + 1).Trim() + "</i>";
                    link = link.Remove(link.IndexOf(" "));
                    CurrentPageFunc += () => { 
                        if (GUILayout.Button(label, linkStyle)) 
                        { 
                            if (link.Contains("://")) Application.OpenURL(link);
                            else if (!isBusy) StartCoroutine(LoadPage(link)); 
                        }
                    }; 
                }

                // Insertions
                else if (line.StartsWith("$"))
                { 
                    EndText(); 
                    string ins = line.Substring(1).Trim();
                    if (ins == "SUBITEMS")
                    {
                        int ind = ContentTable.FindIndex(x => x.Path == path);
                        for (int i = ind + 1; i < ContentTable.Count; i++)
                        {
                            if (ContentTable[i].Depth <= ContentTable[ind].Depth) break;
                            else if (ContentTable[i].Depth == ContentTable[ind].Depth + 1)
                            {
                                string label = "⇒ <i>" + ContentTable[i].Name + "</i>";
                                int j = i;
                                CurrentPageFunc += () => { 
                                    if (GUILayout.Button(label, linkStyle)) 
                                    { 
                                        if (!isBusy) StartCoroutine(LoadPage(ContentTable[j].Path)); 
                                    }
                                }; 
                            }
                        }
                    }
                }

                // Plain text
                else AddText(" " + line);
            }
            else 
            {
                AddText("\n" + line);
            }
        }
        EndText();

        isBusy = false;
        contentScroll = Vector2.zero;
        CurrentPageScrolls = new Vector2[scrollCount];
    }

    public IEnumerator LoadIndex()
    {
        isBusy = true;

        Task<string[]> task = File.ReadAllLinesAsync(Application.dataPath + "/../Manual/index.jmi");
        while (!task.IsCompleted)
        {
            yield return null;
        }
        if (task.Exception != null)
        {
            Debug.LogException(task.Exception);
            isBusy = false;
            yield break;
        }
        
        ContentTable = new List<ManualEntry>();
        List<int> indents = new List<int>() { 0 };
        foreach (string line in task.Result) 
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            Match match = Regex.Match(line, @"(\s*)([^\s]+?)\s+(.*)");

            int indent = match.Groups[1].Value.Length;
            int depth = indents.IndexOf(indent);
            if (depth < 0) 
            {
                depth = indents.Count;
                indents.Add(indent);
                if (ContentTable.Count > 0) ContentTable[ContentTable.Count - 1].Collapsible = true;
            }
            else if (depth < indents.Count - 1)
            {
                indents.RemoveRange(depth + 1, indents.Count - depth - 1);
            }

            ContentTable.Add(new ManualEntry {
                Path = match.Groups[2].Value,
                Name = match.Groups[3].Value,
                Depth = depth
            });

        }

        isBusy = false;
        StartCoroutine(LoadPage(CurrentPagePath ?? "Index.jmf"));
    }

    Vector2 indexScroll = Vector2.zero, contentScroll = Vector2.zero;

    float width, height;

    public void OnGUI()
    {
        if (MonospaceFont == null) 
        {
            MonospaceFont = Resources.Load<Font>("Fonts/RobotoMono-Regular");
        }
        if (!isBusy && ContentTable == null) 
        {
            StartCoroutine(LoadIndex());
        }

        if (Event.current.type == EventType.Layout && Coroutines.Count > 0) 
        {
            Coroutines.RemoveAll(c => !c.MoveNext());
            Repaint();
        }

        width = position.width;
        height = position.height;

        GUIStyle tab = new GUIStyle("toolbarButton");
        tab.padding = new RectOffset(10, 10, 4, 4);
        tab.wordWrap = true;
        tab.fixedHeight = 0;
        tab.alignment = TextAnchor.MiddleLeft;
        
        GUIStyle tabSel = new GUIStyle(tab);
        tabSel.fontStyle = FontStyle.Bold;
        
        GUIStyle tabCol = new GUIStyle("miniButtonLeft");
        tabCol.padding = new RectOffset(0, 0, 0, 0);

        GUIStyle bg = new GUIStyle("toolbarButton");
        bg.fixedHeight = 0;

        GUILayout.BeginHorizontal();

        GUI.Label(new Rect(0, 0, 200, Screen.height), "", bg);
        indexScroll = EditorGUILayout.BeginScrollView(indexScroll, GUILayout.MinWidth(200), GUILayout.MaxWidth(200));
        int? CollapsedDepth = null;
        int LastDepth = 0;
        Rect rect = new Rect();
        if (ContentTable != null) 
        {
            foreach (ManualEntry entry in ContentTable)
            {
                if (CollapsedDepth == null || entry.Depth <= CollapsedDepth)
                {
                    if (LastDepth != entry.Depth)
                    {
                        GUILayout.Space(1);
                        rect = GUILayoutUtility.GetLastRect();
                        rect.x = 9 * Mathf.Min(LastDepth, entry.Depth) + 18;
                        rect.width = 200 - rect.x;
                        EditorGUI.DrawRect(rect, new Color(0, 0, 0, EditorGUIUtility.isProSkin ? .5f : .2f));
                    }

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(9 * entry.Depth + 2);

                    if (entry.Collapsible)
                    {
                        if (GUILayout.Button(EditorGUIUtility.IconContent(entry.Collapsed ? "forward" : "dropdown"), tabCol, GUILayout.MinWidth(17), GUILayout.MaxWidth(17)))
                        {
                            entry.Collapsed = !entry.Collapsed;
                        }
                    }
                    else
                    {
                        GUILayout.Space(17);
                    }

                    if (GUILayout.Toggle(entry.Path == CurrentPagePath, entry.Name, entry.Path == CurrentPagePath ? tabSel : tab) && entry.Path != CurrentPagePath && !isBusy)
                    {
                        StartCoroutine(LoadPage(entry.Path));
                    }
                    
                    if (LastDepth < entry.Depth)
                    {
                        rect.x += 1;
                        rect.y += 1;
                        EditorGUI.DrawRect(rect, new Color(0, 0, 0, EditorGUIUtility.isProSkin ? .2f : .1f));
                    }
                    LastDepth = entry.Depth;

                    GUILayout.EndHorizontal();
                    CollapsedDepth = entry.Collapsed ? entry.Depth : null;
                }
            }
            {
                GUILayout.Space(1);
                rect = GUILayoutUtility.GetLastRect();
                rect.x = 9 * LastDepth + 18;
                rect.width = 200 - rect.x;
                EditorGUI.DrawRect(rect, new Color(0, 0, 0, EditorGUIUtility.isProSkin ? .5f : .2f));
                rect.x += 1;
                rect.y += 1;
                EditorGUI.DrawRect(rect, new Color(0, 0, 0, EditorGUIUtility.isProSkin ? .2f : .1f));
            }
        }
        EditorGUILayout.EndScrollView();

        contentScroll = EditorGUILayout.BeginScrollView(contentScroll);
        if (CurrentPageFunc != null) CurrentPageFunc();
        GUILayout.Space(10);
        EditorGUILayout.EndScrollView();
        
        GUILayout.EndHorizontal();

        if (isBusy) EditorGUI.ProgressBar(new Rect(199, height - 19, width - 198, 20), 1, "Loading...");
    }
}

public class ManualEntry 
{
    public string Path;
    public string Name;
    public int Depth;

    public bool Collapsible = false;
    public bool Collapsed = true;
}
