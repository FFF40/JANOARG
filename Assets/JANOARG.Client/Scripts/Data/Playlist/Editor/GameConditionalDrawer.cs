using System;
using System.Collections.Generic;
using System.Linq;
using JANOARG.Client.Data.Playlist.Conditionals;
using JANOARG.Shared.Data.ChartInfo;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace JANOARG.Client.Data.Playlist.Editor
{
    [CustomPropertyDrawer(typeof(GameConditional))]
    public class GameConditionalDrawer : PropertyDrawer
    {
        static float LineHeight => EditorGUIUtility.singleLineHeight;
        static float LineSpacing => EditorGUIUtility.standardVerticalSpacing;
        static float LinesHeight(int lines) => lines * LineHeight + Mathf.Max(0, lines - 1) * LineSpacing;

        // Advances `area` past a line of `height` and returns the rect for that line.
        // Rect-based equivalent of EditorGUILayout's auto-flowing controls, needed
        // because mixing GUILayout calls inside a rect-based PropertyDrawer.OnGUI
        // desyncs the Layout/Repaint control count once this is drawn inside a
        // scrolling Inspector (throws "Getting control X's position in a group
        // with only Y controls").
        static Rect NextLine(ref Rect area, float height)
        {
            Rect r = new(area.x, area.y, area.width, height);
            area.y += height + LineSpacing;
            return r;
        }

        public static readonly Dictionary<string, IGameConditionalFactory> conditionList = new()
        {
            ["Has Score Entry"] = new GameConditionFactory<ScoreStoreGameConditional>
            {
                MakeItemFunc = () => new ScoreStoreGameConditional(),
                GetHeightFunc = (item, prop) =>
                {
                    int lines = 3; // SongID, Achievement, Difficulty
                    if (item.Achievement < 0) lines++;
                    if (item.Difficulty < 0) lines++;
                    return LinesHeight(lines);
                },
                DrawItemFunc = (item, prop, rect) =>
                {
                    EditorGUI.PropertyField(NextLine(ref rect, LineHeight), prop.FindPropertyRelative("SongID"));
                    EditorGUI.PropertyField(NextLine(ref rect, LineHeight), prop.FindPropertyRelative("Achievement"));
                    if (item.Achievement < 0)
                    {
                        EditorGUI.PropertyField(NextLine(ref rect, LineHeight), prop.FindPropertyRelative("AchievementThreshold"), new GUIContent(" "));
                    }
                    EditorGUI.PropertyField(NextLine(ref rect, LineHeight), prop.FindPropertyRelative("Difficulty"));
                    if (item.Difficulty < 0)
                    {
                        DrawDifficultyThresholdField(NextLine(ref rect, LineHeight), item, prop.FindPropertyRelative("DifficultyThreshold"));
                    }
                },
                GetNameFunc = (item) =>
                {
                    return (item.Achievement switch
                    {
                        ScoreStoreGameConditional.AchievementReq.Played         => "Play ",
                        ScoreStoreGameConditional.AchievementReq.Cleared        => "Clear ",
                        ScoreStoreGameConditional.AchievementReq.FullCombo      => "Full Combo ",
                        ScoreStoreGameConditional.AchievementReq.AllPerfect     => "All Perfect ",
                        ScoreStoreGameConditional.AchievementReq.ScoreThreshold => $"Reach {item.AchievementThreshold} points on ",
                        ScoreStoreGameConditional.AchievementReq.ComboThreshold => $"Reach {item.AchievementThreshold} combo on ",
                        _                                                       => "",
                    }) + "\"" + item.SongID + "\"" + (item.Difficulty switch
                    {
                        ScoreStoreGameConditional.DifficultyReq.Any => " on any difficulty",
                        _ => "",
                    });
                }
            },
            ["Has Flag"] = new GameConditionFactory<FlagStoreGameConditional>
            {
                MakeItemFunc = () => new FlagStoreGameConditional(),
                GetHeightFunc = (item, prop) => LinesHeight(1),
                DrawItemFunc = (item, prop, rect) =>
                {
                    EditorGUI.PropertyField(rect, prop.FindPropertyRelative("Flag"));
                },
                GetNameFunc = (item) =>
                {
                    return $"Has \"{item.Flag}\" flag in save";
                }
            }
        };

        static void DrawDifficultyThresholdField(Rect rect, ScoreStoreGameConditional item, SerializedProperty prop)
        {
            List<ExternalChartMeta> charts = GetSortedCharts(item.SongID);
            if (charts == null || charts.Count == 0)
            {
                EditorGUI.PropertyField(rect, prop, new GUIContent(" "));
                return;
            }

            ExternalChartMeta current = charts.FirstOrDefault(c => c.DifficultyIndex == prop.intValue);
            string label = current != null
                ? $"{current.DifficultyName} ({current.DifficultyIndex})"
                : $"Unknown ({prop.intValue})";

            Rect fieldRect = EditorGUI.PrefixLabel(rect, new GUIContent(" "));
            if (EditorGUI.DropdownButton(fieldRect, new GUIContent(label), FocusType.Keyboard))
            {
                var menu = new GenericMenu();
                foreach (ExternalChartMeta chart in charts)
                {
                    int index = chart.DifficultyIndex;
                    menu.AddItem(
                        new GUIContent($"{chart.DifficultyName} ({index})"),
                        index == prop.intValue,
                        () =>
                        {
                            prop.intValue = index;
                            prop.serializedObject.ApplyModifiedProperties();
                        });
                }
                menu.DropDown(fieldRect);
            }
        }

        static List<ExternalChartMeta> GetSortedCharts(string songId)
        {
            if (string.IsNullOrEmpty(songId)) return null;

            // Same path SongSelectScreen.InitPlaylist resolves at runtime - Resources'
            // own index, not a project-wide AssetDatabase search.
            ExternalPlayableSong song = Resources.Load<ExternalPlayableSong>($"Songs/{songId}/{songId}");
            if (song?.Data?.Charts == null) return null;

            return song.Data.Charts.OrderBy(c => c.DifficultyIndex).ToList();
        }

        private List<string> conditionNames => conditionList.Keys.ToList();

        object propertySetTarget = null;
        string propertySet = null;

        static GameConditional GetBoxedCondition(SerializedProperty property)
        {
            try { return (GameConditional)property.boxedValue; }
            catch { return null; }
        }

        static string GetConditionKey(GameConditional condition)
        {
            try { return conditionList.FirstOrDefault(x => condition?.GetType() == x.Value?.TargetType).Key; }
            catch { return ""; }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            GameConditional currentCondition = GetBoxedCondition(property);
            string currentKey = GetConditionKey(currentCondition);

            float height = LineHeight;

            if (currentCondition != null
                && !string.IsNullOrEmpty(currentKey)
                && conditionList.TryGetValue(currentKey, out IGameConditionalFactory factory)
                && property.isExpanded)
            {
                height += LineSpacing + factory.GetItemHeight(currentCondition, property);
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            GameConditional currentCondition = GetBoxedCondition(property);
            string currentKey = GetConditionKey(currentCondition);

            var dropdownStyle = new GUIStyle(GUI.skin.GetStyle("DropDownButton"));
            dropdownStyle.margin = new RectOffset(0, 0, 0, 0);
            dropdownStyle.padding = new RectOffset(5, 30, 0, 0);
            if (currentCondition != null) dropdownStyle.alignment = TextAnchor.MiddleLeft;
            dropdownStyle.fontStyle = FontStyle.Bold;

            Rect headerRect = new(position.x, position.y, position.width, LineHeight);
            Rect foldoutRect;

            if (currentCondition != null
                && !string.IsNullOrEmpty(currentKey)
                && conditionList.TryGetValue(currentKey, out IGameConditionalFactory factory))
            {
                property.isExpanded = EditorGUI.Foldout(headerRect, property.isExpanded, GUIContent.none);
                foldoutRect = headerRect;
                if (property.isExpanded)
                {
                    foldoutRect = new RectOffset(-(int)LineHeight, 1, 0, 0).Add(foldoutRect);

                    Rect itemRect = new(
                        position.x,
                        position.y + LineHeight + LineSpacing,
                        position.width,
                        factory.GetItemHeight(currentCondition, property));

                    EditorGUI.indentLevel++;
                    factory.DrawItem(itemRect, currentCondition, property);
                    EditorGUI.indentLevel--;
                }
                else
                {
                    var btnStyle = new GUIStyle(GUI.skin.GetStyle("TextField"));
                    btnStyle.fontStyle = FontStyle.Italic;
                    btnStyle.padding = new RectOffset(5, 5, 0, 0);
                    btnStyle.alignment = TextAnchor.MiddleLeft;
                    if (GUI.Button(new RectOffset(-18, -20, 0, 0).Add(foldoutRect), factory.GetName(currentCondition), btnStyle))
                    {
                        property.isExpanded = true;
                    }
                    foldoutRect = new RectOffset(19 - (int)foldoutRect.width, 1, 0, 0).Add(foldoutRect);
                }
            }
            else
            {
                EditorGUI.LabelField(headerRect, "");
                foldoutRect = headerRect;
            }

            if (GUI.Button(foldoutRect, string.IsNullOrEmpty(currentKey) ? "Select a conditional..." : currentKey, dropdownStyle))
            {
                var state = new AdvancedDropdownState();
                var dropdown = new GameConditionalDropdown(state, (item) =>
                {
                    propertySetTarget = property.boxedValue;
                    propertySet = item;
                });
                dropdown.Show(position);
            }

            if (!string.IsNullOrEmpty(propertySet) && propertySetTarget == currentCondition)
            {
                currentKey = propertySet;
                property.boxedValue = currentCondition = conditionList[propertySet]?.MakeItem();
                propertySetTarget = propertySet = null;
                property.isExpanded = true;
            }

            EditorGUI.EndProperty();
        }
    }

    class GameConditionalDropdown : AdvancedDropdown
    {
        private Action<string> OnSet;

        public GameConditionalDropdown(AdvancedDropdownState state, Action<string> onSet) : base(state)
        {
            OnSet = onSet;
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("Game Conditionals");

            foreach (var factory in GameConditionalDrawer.conditionList)
            {
                root.AddChild(new AdvancedDropdownItem(factory.Key));
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            OnSet(item.name);
        }
    }


    public interface IGameConditionalFactory
    {
        Type TargetType { get; }
        GameConditional MakeItem();
        float GetItemHeight(GameConditional item, SerializedProperty prop);
        void DrawItem(Rect rect, GameConditional item, SerializedProperty prop);
        string GetName(GameConditional item);
    }

    public class GameConditionFactory<T> : IGameConditionalFactory where T : GameConditional
    {
        public Type TargetType => typeof(T);
        public Func<T> MakeItemFunc;
        public Func<T, SerializedProperty, float> GetHeightFunc;
        public Action<T, SerializedProperty, Rect> DrawItemFunc;
        public Func<T, string> GetNameFunc;

        public GameConditional MakeItem() => MakeItemFunc();
        public float GetItemHeight(GameConditional item, SerializedProperty prop) =>
            GetHeightFunc((T)item, prop);
        public void DrawItem(Rect rect, GameConditional item, SerializedProperty prop) =>
            DrawItemFunc((T)item, prop, rect);
        public string GetName(GameConditional item) =>
            GetNameFunc((T)item);
    }
}
