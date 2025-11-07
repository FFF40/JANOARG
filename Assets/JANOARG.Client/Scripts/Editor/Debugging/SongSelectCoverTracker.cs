using Codice.CM.Client.Gui;
using JANOARG.Client.Behaviors.SongSelect;
using JANOARG.Client.Behaviors.SongSelect.List;
using JANOARG.Shared.Data.ChartInfo;
using UnityEditor;
using UnityEngine;

namespace JANOARG.Client.Editor.Debugging
{
    public class SongSelectTracker : EditorWindow
    {
        [MenuItem("JANOARG/Debug Tools/Song Select Tracker", priority = 20000)]
        public static void ShowWindow()
        {
            EditorWindow me = GetWindow<SongSelectTracker>();
            me.titleContent = new GUIContent("Song Select Tracker");
        }

        Vector2 _ScrollPosition = Vector2.zero;
        string _CurrentTab = "cover";

        public void OnGUI()
        {
            Rect rect = new(10, 10, position.width - 20, position.height - 20);

            // Tab display
            {
                GUIStyle leftButtonStyle = GUI.skin.GetStyle("buttonLeft");
                GUIStyle middleButtonStyle = GUI.skin.GetStyle("buttonMid");
                GUIStyle rightButtonStyle = GUI.skin.GetStyle("buttonRight");
                GUILayout.BeginHorizontal();
                if (GUILayout.Toggle(_CurrentTab == "cover", "Cover", leftButtonStyle))
                    _CurrentTab = "cover";
                if (GUILayout.Toggle(_CurrentTab == "list", "List View", rightButtonStyle))
                    _CurrentTab = "list";

                rect.yMin += GUILayoutUtility.GetLastRect().height;
                GUILayout.EndHorizontal();
            }


            if (_CurrentTab == "cover")
            {
                SongSelectCoverManager manager = SongSelectCoverManager.sMain;
                if (manager)
                {
                    GUIStyle labelStyle = GUI.skin.GetStyle("boldLabel");

                    GUILayout.Label("");
                    rect = GUILayoutUtility.GetLastRect();
                    labelStyle.fontStyle = FontStyle.Bold;
                    GUI.Label(new Rect(rect.x + rect.height + 5, rect.y, 200, rect.height), "Key", labelStyle);
                    GUI.Label(new Rect(rect.x + rect.height + 210, rect.y, 50, rect.height), "Uses", labelStyle);
                    GUI.Label(new Rect(rect.x + rect.height + 265, rect.y, 50, rect.height), "Status", labelStyle);

                    _ScrollPosition = GUILayout.BeginScrollView(_ScrollPosition);
                    labelStyle.fontStyle = FontStyle.Normal;
                    foreach (var cover in manager.CoverInfos)
                    {
                        GUILayout.Label("");
                        rect = GUILayoutUtility.GetLastRect();

                        if (cover.Value.Icon)
                        {
                            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.height, rect.height), cover.Value.Icon);
                        }
                        else if (cover.Value.Coroutine == null)
                        {
                            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.height, rect.height), cover.Value.BackgroundColor);
                        }

                        GUI.Label(new Rect(rect.x + rect.height + 5, rect.y, 200, rect.height), cover.Key, labelStyle);
                        GUI.Label(new Rect(rect.x + rect.height + 210, rect.y, 50, rect.height), cover.Value.Uses.ToString(), labelStyle);
                        GUI.Label(new Rect(rect.x + rect.height + 265, rect.y, 50, rect.height), cover.Value.Coroutine == null ? "Done" : "Loading", labelStyle);
                    }
                    GUILayout.EndScrollView();
                }
                else
                {
                    GUILayout.BeginArea(rect);
                    GUIStyle labelStyle = GUI.skin.GetStyle("boldLabel");
                    labelStyle.wordWrap = true;
                    labelStyle.fontStyle = FontStyle.Bold;
                    GUILayout.Label("Cover Manager is not active", labelStyle);
                    labelStyle.fontStyle = FontStyle.Normal;
                    GUILayout.Label("Open the game on Song Select screen to inspect the Cover Manager.", labelStyle);
                    GUILayout.EndArea();
                }
            }
            else if (_CurrentTab == "list")
            {
                SongSelectListView manager = SongSelectScreen.sMain != null ? SongSelectScreen.sMain.ListView : null;
                if (manager)
                {
                    GUIStyle labelStyle = GUI.skin.GetStyle("boldLabel");

                    GUILayout.Label("");
                    rect = GUILayoutUtility.GetLastRect();
                    labelStyle.fontStyle = FontStyle.Bold;
                    GUI.Label(new Rect(rect.x, rect.y, rect.height, rect.height), "#", labelStyle);
                    GUI.Label(new Rect(rect.x + rect.height + 5, rect.y, 200, rect.height), "Key", labelStyle);

                    _ScrollPosition = GUILayout.BeginScrollView(_ScrollPosition);
                    labelStyle.fontStyle = FontStyle.Normal;
                    int index = 0;
                    foreach (var cover in manager.SongItems)
                    {
                        GUILayout.Label("");
                        rect = GUILayoutUtility.GetLastRect();

                        GUI.Label(new Rect(rect.x, rect.y, rect.height, rect.height), (++index).ToString(), labelStyle);
                        GUI.Label(new Rect(rect.x + rect.height + 5, rect.y, 200, rect.height), cover.TargetSongCover, labelStyle);
                    }
                    GUILayout.EndScrollView();
                }
                else
                {
                    GUILayout.BeginArea(rect);
                    GUIStyle labelStyle = GUI.skin.GetStyle("boldLabel");
                    labelStyle.wordWrap = true;
                    labelStyle.fontStyle = FontStyle.Bold;
                    GUILayout.Label("List View is not active", labelStyle);
                    labelStyle.fontStyle = FontStyle.Normal;
                    GUILayout.Label("Open the game on Song Select screen to inspect the List View.", labelStyle);
                    GUILayout.EndArea();
                }
            }
        }

        public void Update()
        {
            this.Repaint();
        }
    }
}