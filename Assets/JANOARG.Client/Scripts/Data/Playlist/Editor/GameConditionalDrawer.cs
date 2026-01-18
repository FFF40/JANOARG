using System;
using System.Collections.Generic;
using System.Linq;
using JANOARG.Client.Data.Playlist.Conditionals;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace JANOARG.Client.Data.Playlist.Editor
{
    [CustomPropertyDrawer(typeof(GameConditional))]
    public class GameConditionalDrawer : PropertyDrawer
    {
        public static readonly Dictionary<string, IGameConditionalFactory> conditionList = new()
        {
            ["Has Score Entry"] = new GameConditionFactory<ScoreStoreGameConditional>
            {
                MakeItemFunc = () => new ScoreStoreGameConditional(),
                DrawItemFunc = (item, prop) =>
                {
                    EditorGUILayout.PropertyField(prop.FindPropertyRelative("SongID"));
                    EditorGUILayout.PropertyField(prop.FindPropertyRelative("Achievement"));
                    if (item.Achievement < 0)
                    {
                        EditorGUILayout.PropertyField(prop.FindPropertyRelative("Threshold"), new GUIContent(""));
                    }
                    EditorGUILayout.PropertyField(prop.FindPropertyRelative("Difficulty"));
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
                DrawItemFunc = (item, prop) =>
                {
                    var flagProp = prop.FindPropertyRelative("Flag");
                    EditorGUILayout.PropertyField(prop.FindPropertyRelative("Flag"));
                },
                GetNameFunc = (item) =>
                {
                    return $"Has \"{item.Flag}\" flag in save";
                }
            }
        };

        private List<string> conditionNames => conditionList.Keys.ToList();

        object propertySetTarget = null;
        string propertySet = null;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            GUILayout.Space(-EditorGUIUtility.singleLineHeight - 2);
            GameConditional currentCondition = (GameConditional)property.boxedValue;

            string currentKey = "";
            try { currentKey = conditionList.FirstOrDefault(x => currentCondition?.GetType() == x.Value?.TargetType).Key; }
            catch { }
            int currentIndex = conditionNames.IndexOf(currentKey);

            var dropdownStyle = new GUIStyle(GUI.skin.GetStyle("DropDownButton"));
            dropdownStyle.margin = new RectOffset(0, 0, 0, 0);
            dropdownStyle.padding = new RectOffset(5, 30, 0, 0);
            if (currentCondition != null) dropdownStyle.alignment = TextAnchor.MiddleLeft;
            dropdownStyle.fontStyle = FontStyle.Bold;

            Rect foldoutRect = new Rect();
            if (currentCondition != null && conditionList[currentKey] is { } factory)
            {
                property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, "");
                foldoutRect = GUILayoutUtility.GetLastRect();
                if (property.isExpanded)
                {
                    foldoutRect = new RectOffset(-(int)EditorGUIUtility.singleLineHeight, 1, 0, 0).Add(foldoutRect);
                    EditorGUI.indentLevel++;
                    factory.DrawItem(currentCondition, property);
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
                EditorGUILayout.LabelField("");
                foldoutRect = GUILayoutUtility.GetLastRect();
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
        void DrawItem(GameConditional item, SerializedProperty prop);
        string GetName(GameConditional item);
    }

    public class GameConditionFactory<T> : IGameConditionalFactory where T : GameConditional
    {
        public Type TargetType => typeof(T);
        public Func<T> MakeItemFunc;
        public Action<T, SerializedProperty> DrawItemFunc;
        public Func<T, string> GetNameFunc;

        public GameConditional MakeItem() => MakeItemFunc();
        public void DrawItem(GameConditional item, SerializedProperty prop) =>
            DrawItemFunc((T)item, prop);
        public string GetName(GameConditional item) =>
            GetNameFunc((T)item);
    }
}