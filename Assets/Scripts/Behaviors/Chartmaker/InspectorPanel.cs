using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InspectorPanel : MonoBehaviour
{
    public static InspectorPanel main;

    public object CurrentObject;
    public List<Timestamp> CurrentTimestamp;
    [NonSerialized]
    public Lane CurrentLane = null;

    public TMP_Text FormTitle;
    public RectTransform FormHolder;
    public RectTransform OffsetFieldHolder;

    public Button PaletteButton;
    public Button GroupsButton;
    public Button CameraButton;
    [Space]
    public Button BackButton;

    public FieldInfo CurrentMultiField;
    public ChartmakerMultiHandler MultiHandler;
    public Dictionary<Type, ChartmakerMultiHandler> MultiHandlers = new ();

    public void Awake()
    {
        main = this;
    }

    public void Start()
    {
        UpdateForm();
    }

    public void OnObjectChange()
    {
        UpdateButtons();
        UpdateForm();
        TimelinePanel.main.UpdateTabs();
        TimelinePanel.main.UpdateItems();
        Chartmaker.main.OnClipboardUpdate();
        PlayerView.main.UpdateHandles();
    }

    public void UnsetObject()
    {
        if (CurrentTimestamp?.Count > 0)
        {
            CurrentTimestamp = new ();
        }
        else
        {
            CurrentObject = null;
        }
        OnObjectChange();
    }

    public void SetObject(object obj)
    {
        if (obj is Timestamp ts)
        {
            CurrentTimestamp = new () { ts };
        }
        else if (obj is List<Timestamp> tsl)
        {
            CurrentTimestamp = tsl;
        }
        else 
        {
            CurrentObject = obj;
            CurrentTimestamp = new ();
            if (obj is Lane lane) CurrentLane = lane;
        }
        OnObjectChange();
    }

    public void ClearForm()
    {
        foreach (RectTransform rt in FormHolder)
        {
            Destroy(rt.gameObject);
        }
        foreach (RectTransform rt in OffsetFieldHolder)
        {
            Destroy(rt.gameObject);
        }
    }

    public void FocusPalette()
    {
        if (Chartmaker.main.CurrentChart != null)
            SetObject(Chartmaker.main.CurrentChart?.Pallete);
    }

    public void FocusGroups()
    {
        if (Chartmaker.main.CurrentChart != null)
            SetObject(Chartmaker.main.CurrentChart?.Groups);
    }

    public void FocusCamera()
    {
        if (Chartmaker.main.CurrentChart != null)
            SetObject(Chartmaker.main.CurrentChart?.Camera);
    }

    public void UpdateButtons()
    {
        PaletteButton.gameObject.SetActive(Chartmaker.main.CurrentChart != null);
        GroupsButton.gameObject.SetActive(Chartmaker.main.CurrentChart != null);
        CameraButton.gameObject.SetActive(Chartmaker.main.CurrentChart != null);

        BackButton.gameObject.SetActive(CurrentObject != null);

        PaletteButton.interactable = CurrentObject is not Pallete;
        GroupsButton.interactable = CurrentObject != Chartmaker.main.CurrentChart?.Groups;
        CameraButton.interactable = CurrentObject is not CameraController;
    }

    public void UpdateForm()
    {
        ClearForm();

        if (CurrentObject == null)
        {
            FormTitle.text = "No object selected";
            SpawnForm<FormEntryLabel>("Select an item to get started.");
        }
        else if (CurrentTimestamp.Count == 1)
        {
            FormTitle.text = "Timestamp";

            Timestamp ts = CurrentTimestamp[0];
            MakeOffsetEntry(() => ts.Offset, x => Chartmaker.main.SetItem(ts, "Offset", x));

            SpawnForm<FormEntryHeader>("General");
            SpawnForm<FormEntryFloat, float>("Offset", () => ts.Offset, x => Chartmaker.main.SetItem(ts, "Offset", x));
            SpawnForm<FormEntryFloat, float>("Duration", () => ts.Duration, x => Chartmaker.main.SetItem(ts, "Duration", x));
            SpawnForm<FormEntryToggleFloat, float>("From", () => ts.From, x => Chartmaker.main.SetItem(ts, "From", x));
            SpawnForm<FormEntryFloat, float>("To", () => ts.Target, x => Chartmaker.main.SetItem(ts, "Target", x));
            SpawnForm<FormEntryEasing, EasingPair>("Easing", () => new (ts.Easing, ts.EaseMode), 
                x => { Chartmaker.main.SetItem(ts, "Easing", x.Function); Chartmaker.main.SetItem(ts, "EaseMode", x.Mode); }
            );
        }
        else if (CurrentTimestamp.Count >= 1)
        {
            FormTitle.text = "Multi-select";

            MakeMultiEditForm(CurrentTimestamp);
        }
        else if (CurrentObject is IList list && CurrentObject != Chartmaker.main.CurrentChart?.Groups)
        {
            FormTitle.text = "Multi-select";

            MakeMultiEditForm(list);
        }
        else if (CurrentObject is PlayableSong song)
        {
            if (song != Chartmaker.main.CurrentSong)
            {
                SetObject(null);
                return;
            }

            FormTitle.text = "Playable Song";

            SpawnForm<FormEntryHeader>("Metadata");
            SpawnForm<FormEntryString, string>("Song Name", () => song.SongName, x => Chartmaker.main.SetItem(song, "SongName", x));
            SpawnForm<FormEntryString, string>("Alt Name", () => song.AltSongName, x => Chartmaker.main.SetItem(song, "AltSongName", x));
            SpawnForm<FormEntryString, string>("Song Artist", () => song.SongArtist, x => Chartmaker.main.SetItem(song, "SongArtist", x));
            SpawnForm<FormEntryString, string>("Alt Artist", () => song.AltSongArtist, x => Chartmaker.main.SetItem(song, "AltSongArtist", x));
            SpawnForm<FormEntryString, string>("Genre", () => song.Genre, x => Chartmaker.main.SetItem(song, "Genre", x));
            SpawnForm<FormEntryString, string>("Location", () => song.Location, x => Chartmaker.main.SetItem(song, "Location", x));
            
            SpawnForm<FormEntryHeader>("Accent Colors");
            SpawnForm<FormEntryColor, Color>("Background", () => song.BackgroundColor, x => Chartmaker.main.SetItem(song, "BackgroundColor", x));
            SpawnForm<FormEntryColor, Color>("Interface", () => song.InterfaceColor, x => Chartmaker.main.SetItem(song, "InterfaceColor", x));
        }
        else if (CurrentObject is BPMStop stop)
        {
            if (!Chartmaker.main.CurrentSong.Timing.Stops.Contains(stop))
            {
                SetObject(null);
                return;
            }

            FormTitle.text = "BPM Stop";
            MakeOffsetEntry(() => stop.Offset, x => Chartmaker.main.SetItem(stop, "Offset", x));

            SpawnForm<FormEntryHeader>("Properties");
            SpawnForm<FormEntryFloat, float>("BPM", () => stop.BPM, x => Chartmaker.main.SetItem(stop, "BPM", x));
            SpawnForm<FormEntryFloat, float>("Signature", () => stop.Signature, x => Chartmaker.main.SetItem(stop, "Signature", x));
            SpawnForm<FormEntryHeader>("Flags");
            SpawnForm<FormEntryBool, bool>("Significant", () => stop.Significant, x => Chartmaker.main.SetItem(stop, "Significant", x));
        }
        else if (CurrentObject is Chart chart)
        {
            if (chart != Chartmaker.main.CurrentChart)
            {
                SetObject(null);
                return;
            }

            ExternalChartMeta meta = Chartmaker.main.CurrentChartMeta;
            FormTitle.text = "Chart";

            SpawnForm<FormEntryHeader>("Metadata");
            SpawnForm<FormEntryString, string>("Chart Name", () => chart.DifficultyName, 
                x => Chartmaker.main.SetItem(chart, "DifficultyName", meta.DifficultyName = x));
            SpawnForm<FormEntryInt, int>("Sorting Index", () => chart.DifficultyIndex, 
                x => Chartmaker.main.SetItem(chart, "DifficultyIndex", meta.DifficultyIndex = x));
            SpawnForm<FormEntryString, string>("Difficulty", () => chart.DifficultyLevel, 
                x => Chartmaker.main.SetItem(chart, "DifficultyLevel", meta.DifficultyLevel = x));
            SpawnForm<FormEntryFloat, float>("Chart Constant", () => chart.ChartConstant, 
                x => Chartmaker.main.SetItem(chart, "ChartConstant", meta.ChartConstant = x));
        }
        else if (CurrentObject is Pallete pallete)
        {
            if (pallete != Chartmaker.main.CurrentChart?.Pallete)
            {
                SetObject(null);
                return;
            }

            FormTitle.text = "Palette";

            SpawnForm<FormEntryHeader>("Colors");
            var bgColor = SpawnForm<FormEntryColor, Color>("Background", () => pallete.BackgroundColor, x => Chartmaker.main.SetItem(pallete, "BackgroundColor", x));
            var fgColor = SpawnForm<FormEntryColor, Color>("Interface", () => pallete.InterfaceColor, x => Chartmaker.main.SetItem(pallete, "InterfaceColor", x));
            var copy = SpawnForm<FormEntryButton>("Copy from Playable Song");
            copy.Button.onClick.AddListener(() => {
                Chartmaker.main.SetItem(pallete, "BackgroundColor", Chartmaker.main.CurrentSong.BackgroundColor);
                bgColor.Start();
                Chartmaker.main.SetItem(pallete, "InterfaceColor", Chartmaker.main.CurrentSong.InterfaceColor);
                fgColor.Start();
            });

            var laneStyles = SpawnForm<FormEntryListHeader>("Lane Styles");
            laneStyles.Button.onClick.AddListener(() => {
                Chartmaker.main.AddItem(pallete.LaneStyles[0].DeepClone());
            });
            int index = 0;
            foreach (LaneStyle style in pallete.LaneStyles)
            {
                LaneStyle Style = style;
                var btn = SpawnForm<FormEntryListItem>("ID " + index);
                btn.Button.onClick.AddListener(() => {
                    SetObject(Style);
                });
                btn.RemoveButton.onClick.AddListener(() => {
                    Chartmaker.main.DeleteItem(Style, false);
                });
                index++;
            }

            var hitStyles = SpawnForm<FormEntryListHeader>("Hit Styles");
            hitStyles.Button.onClick.AddListener(() => {
                Chartmaker.main.AddItem(pallete.HitStyles[0].DeepClone());
            });
            index = 0;
            foreach (HitStyle style in pallete.HitStyles)
            {
                HitStyle Style = style;
                var btn = SpawnForm<FormEntryListItem>("ID " + index);
                btn.Button.onClick.AddListener(() => {
                    SetObject(Style);
                });
                btn.RemoveButton.onClick.AddListener(() => {
                    Chartmaker.main.DeleteItem(Style, false);
                });
                index++;
            }
        }
        else if (CurrentObject is LaneStyle laneStyle)
        {
            if (Chartmaker.main.CurrentChart?.Pallete.LaneStyles.Contains(laneStyle) != true)
            {
                SetObject(null);
                return;
            }

            FormTitle.text = "Lane Style";

            SpawnForm<FormEntryHeader>("Lane");
            SpawnForm<FormEntryColor, Color>("Color", () => laneStyle.LaneColor, x => Chartmaker.main.SetItem(laneStyle, "LaneColor", x));

            SpawnForm<FormEntryHeader>("Judge");
            SpawnForm<FormEntryColor, Color>("Color", () => laneStyle.JudgeColor, x => Chartmaker.main.SetItem(laneStyle, "JudgeColor", x));
        }
        else if (CurrentObject is HitStyle hitStyle)
        {
            if (Chartmaker.main.CurrentChart?.Pallete.HitStyles.Contains(hitStyle) != true)
            {
                SetObject(null);
                return;
            }

            FormTitle.text = "Hit Style";

            SpawnForm<FormEntryHeader>("Hit Body");
            SpawnForm<FormEntryColor, Color>("Normal Color", () => hitStyle.NormalColor, x => Chartmaker.main.SetItem(hitStyle, "NormalColor", x));
            SpawnForm<FormEntryColor, Color>("Catch Color", () => hitStyle.CatchColor, x => Chartmaker.main.SetItem(hitStyle, "CatchColor", x));

            SpawnForm<FormEntryHeader>("Hold Tail");
            SpawnForm<FormEntryColor, Color>("Color", () => hitStyle.HoldTailColor, x => Chartmaker.main.SetItem(hitStyle, "HoldTailColor", x));
        }
        else if (CurrentObject == Chartmaker.main.CurrentChart?.Groups)
        {
            FormTitle.text = "Lane Groups";

            List<LaneGroup> groups = Chartmaker.main.CurrentChart?.Groups;

            var listHeader = SpawnForm<FormEntryListHeader>("Groups");
            listHeader.Button.onClick.AddListener(() => {
                LaneGroup group = new();
                int index = 1;
                while (Chartmaker.main.CurrentChart.Groups.FindIndex(x => x.Name == "Group " + index) >= 0) index++;
                group.Name = "Group " + index;
                Chartmaker.main.AddItem(group);
            });
            int index = 0;
            foreach (LaneGroup group in groups)
            {
                LaneGroup Group = group;
                var btn = SpawnForm<FormEntryListItem>(group.Name);
                btn.Button.onClick.AddListener(() => {
                    SetObject(Group);
                });
                btn.RemoveButton.onClick.AddListener(() => {
                    Chartmaker.main.DeleteItem(Group, false);
                });
                index++;
            }
        }
        else if (CurrentObject is LaneGroup group)
        {
            if (Chartmaker.main.CurrentChart?.Groups.Contains(group) != true)
            {
                SetObject(null);
                return;
            }

            FormTitle.text = "Lane Group";

            SpawnForm<FormEntryHeader>("Transform");
            SpawnForm<FormEntryVector3, Vector3>("Position", () => group.Position, x => Chartmaker.main.SetItem(group, "Position", x));
            SpawnForm<FormEntryVector3, Vector3>("Rotation", () => group.Rotation, x => Chartmaker.main.SetItem(group, "Rotation", x));
            MakeLaneGroupEntry(group);
        }
        else if (CurrentObject is CameraController camera)
        {
            if (camera != Chartmaker.main.CurrentChart?.Camera)
            {
                SetObject(null);
                return;
            }

            FormTitle.text = "Camera Controller";

            SpawnForm<FormEntryHeader>("Pivot");
            SpawnForm<FormEntryVector3, Vector3>("Position", () => camera.CameraPivot, x => Chartmaker.main.SetItem(camera, "CameraPivot", x));
            SpawnForm<FormEntryVector3, Vector3>("Rotation", () => camera.CameraRotation, x => Chartmaker.main.SetItem(camera, "CameraRotation", x));
            SpawnForm<FormEntryFloat, float>("Distance", () => camera.PivotDistance, x => Chartmaker.main.SetItem(camera, "PivotDistance", x));
        }
        else if (CurrentObject is Lane lane)
        {
            if (Chartmaker.main.CurrentChart?.Lanes.Contains(lane) != true)
            {
                SetObject(null);
                return;
            }

            FormTitle.text = "Lane";

            SpawnForm<FormEntryHeader>("Transform");
            SpawnForm<FormEntryVector3, Vector3>("Position", () => lane.Position, x => Chartmaker.main.SetItem(lane, "Position", x));
            SpawnForm<FormEntryVector3, Vector3>("Rotation", () => lane.Rotation, x => Chartmaker.main.SetItem(lane, "Rotation", x));
            MakeLaneGroupEntry(lane);
            SpawnForm<FormEntryHeader>("Appearance");
            MakeLaneStyleEntry(lane);
        }
        else if (CurrentObject is LaneStep step)
        {
            if (CurrentLane?.LaneSteps.Contains(step) != true)
            {
                SetObject(null);
                return;
            }

            FormTitle.text = "Lane Step";
            MakeOffsetEntry(() => step.Offset, x => Chartmaker.main.SetItem(step, "Offset", x));

            SpawnForm<FormEntryHeader>("Transform");
            SpawnForm<FormEntryVector2, Vector2>("Start Pos", () => step.StartPos, x => Chartmaker.main.SetItem(step, "StartPos", x));
            SetEase2(
                SpawnForm<FormEntryEasing, EasingPair>("", () => new (step.StartEaseX, step.StartEaseXMode), 
                    x => { Chartmaker.main.SetItem(step, "StartEaseX", x.Function); Chartmaker.main.SetItem(step, "StartEaseXMode", x.Mode);  }
                ),
                SpawnForm<FormEntryEasing, EasingPair>("", () => new (step.StartEaseY, step.StartEaseYMode), 
                    x => { Chartmaker.main.SetItem(step, "StartEaseY", x.Function); Chartmaker.main.SetItem(step, "StartEaseYMode", x.Mode);  }
                )
            );

            SpawnForm<FormEntryVector2, Vector2>("End Pos", () => step.EndPos, x => Chartmaker.main.SetItem(step, "EndPos", x));
            SetEase2(
                SpawnForm<FormEntryEasing, EasingPair>("", () => new (step.EndEaseX, step.EndEaseXMode), 
                    x => { Chartmaker.main.SetItem(step, "EndEaseX", x.Function); Chartmaker.main.SetItem(step, "EndEaseXMode", x.Mode);  }
                ),
                SpawnForm<FormEntryEasing, EasingPair>("", () => new (step.EndEaseY, step.EndEaseYMode), 
                    x => { Chartmaker.main.SetItem(step, "EndEaseY", x.Function); Chartmaker.main.SetItem(step, "EndEaseYMode", x.Mode);  }
                )
            );

            SpawnForm<FormEntryFloat, float>("Speed", () => step.Speed, x => Chartmaker.main.SetItem(step, "Speed", x));
        }
        else if (CurrentObject is HitObject hit)
        {
            if (CurrentLane?.Objects.Contains(hit) != true)
            {
                SetObject(null);
                return;
            }

            FormTitle.text = "Hit Object";
            MakeOffsetEntry(() => hit.Offset, x => Chartmaker.main.SetItem(hit, "Offset", x));
            
            SpawnForm<FormEntryHeader>("Type");
            var typeDropdown = SpawnForm<FormEntryDropdown, object>("", () => hit.Type, x => hit.Type = (HitObject.HitType)x);
            typeDropdown.TargetEnum(typeof(HitObject.HitType));
            typeDropdown.TitleLabel.gameObject.SetActive(false);
            typeDropdown.GetComponent<HorizontalLayoutGroup>().padding.left = 10;

            SpawnForm<FormEntryHeader>("Transform");
            SpawnForm<FormEntryFloat, float>("Position", () => hit.Position, x => Chartmaker.main.SetItem(hit, "Position", x));
            SpawnForm<FormEntryFloat, float>("Width", () => hit.Length, x => Chartmaker.main.SetItem(hit, "Length", x));
            SpawnForm<FormEntryFloat, float>("Hold Length", () => hit.HoldLength, x => Chartmaker.main.SetItem(hit, "HoldLength", x));

            SpawnForm<FormEntryHeader>("Appearance");
            MakeHitStyleEntry(hit);

            FormEntryToggleFloat dirField = null;
            SpawnForm<FormEntryHeader>("Behavior");
            SpawnForm<FormEntryBool, bool>("Flickable", () => hit.Flickable, x => {
                Chartmaker.main.SetItem(hit, "Flickable", x);
                dirField?.gameObject.SetActive(x);
            });
            dirField = SpawnForm<FormEntryToggleFloat, float>("Direction", () => hit.FlickDirection, x => Chartmaker.main.SetItem(hit, "FlickDirection", x));
            dirField.gameObject.SetActive(hit.Flickable);
        }
        else 
        {
            FormTitle.text = "?????";

            SpawnForm<FormEntryLabel>("Unsupported object " + CurrentObject.GetType());
        }
    }

    public void MakeOffsetEntry(Func<float> get, Action<float> set)
    {
        var field = SpawnForm<FormEntryFloat, float>("", get, set);
        field.transform.SetParent(OffsetFieldHolder);
        field.TitleLabel.gameObject.SetActive(false);
    }

    public void MakeOffsetEntry(Func<BeatPosition> get, Action<BeatPosition> set)
    {
        var field = SpawnForm<FormEntryBeatPosition, BeatPosition>("", get, set);
        field.transform.SetParent(OffsetFieldHolder);
        field.TitleLabel.gameObject.SetActive(false);
    }

    public void MakeLaneStyleEntry(Lane lane)
    {
        var dropdown = SpawnForm<FormEntryDropdown, object>("Style Index", () => lane.StyleIndex, x => Chartmaker.main.SetItem(lane, "StyleIndex", x));
        for (int a = 0; a < Chartmaker.main.CurrentChart.Pallete.LaneStyles.Count; a++) dropdown.ValidValues.Add(a, "ID " + a);
        dropdown.ValidValues.Add(-1, "<i>Invisible</i>");
    }

    public void MakeHitStyleEntry(HitObject hit)
    {
        var dropdown = SpawnForm<FormEntryDropdown, object>("Style Index", () => hit.StyleIndex, x => Chartmaker.main.SetItem(hit, "StyleIndex", x));
        for (int a = 0; a < Chartmaker.main.CurrentChart.Pallete.HitStyles.Count; a++) dropdown.ValidValues.Add(a, "ID " + a);
        dropdown.ValidValues.Add(-1, "<i>Invisible</i>");
    }

    public void MakeLaneGroupEntry(LaneGroup group)
    {
        var dropdown = SpawnForm<FormEntryDropdown, object>("Parent", () => group.Group ?? "", x => Chartmaker.main.SetItem(group, "Group", x));
        foreach (LaneGroup p in Chartmaker.main.CurrentChart.Groups) if (p != group) dropdown.ValidValues.Add(p.Name, p.Name);
        dropdown.ValidValues.Add("", "<i>None</i>");
    }

    public void MakeLaneGroupEntry(Lane lane)
    {
        var dropdown = SpawnForm<FormEntryDropdown, object>("Group", () => lane.Group ?? "", x => Chartmaker.main.SetItem(lane, "Group", x));
        foreach (LaneGroup p in Chartmaker.main.CurrentChart.Groups) dropdown.ValidValues.Add(p.Name, p.Name);
        dropdown.ValidValues.Add("", "<i>None</i>");
    }

    public void MakeMultiEditForm(IList thing)
    {
        SpawnForm<FormEntrySpace>("");
        SpawnForm<FormEntryLabel>(Chartmaker.GetItemName(thing));

        SpawnForm<FormEntryHeader>("Multi-edit");

        var fields = Array.FindAll(thing[0].GetType().GetFields(), field => !(
            typeof(IEnumerable).IsAssignableFrom(field.FieldType)
            || typeof(Storyboard) == field.FieldType
            || field.IsStatic || field.IsLiteral || !field.IsPublic
        ));
        if (Array.IndexOf(fields, CurrentMultiField) < 0) SetMultiField(fields[0]);

        var dropdown = SpawnForm<FormEntryDropdown, object>("Target", () => CurrentMultiField, x => {
            SetMultiField((FieldInfo)x);
            UpdateForm();
        });
        foreach (FieldInfo field in fields)
        {
            dropdown.ValidValues.Add(field, field.Name);
        }

        SpawnForm<FormEntrySpace>("");
        
        void MakeLerpableEditor<T>(LerpableMultiHandler<T> lerpHandler)
        {
            bool advanced = float.IsFinite(lerpHandler.From);
            SpawnForm<FormEntryBool, bool>("Advanced", () => advanced, x => { 
                lerpHandler.From = x ? lerpHandler.To : float.NaN; 
                if (x) lerpHandler.SetLerp(thing);
                UpdateForm();
            });
            if (advanced) SpawnForm<FormEntryFloat, float>("From", () => lerpHandler.From, x => { lerpHandler.From = x; });
            SpawnForm<FormEntryFloat, float>("To", () => lerpHandler.To, x => { lerpHandler.To = x; });
            
            if (advanced) 
            {
                var lerpDropdown = SpawnForm<FormEntryDropdown, object>("Lerp Source", () => lerpHandler.LerpSource, x => {
                    lerpHandler.LerpSource = (string)x; lerpHandler.SetLerp(thing);
                });
                foreach (FieldInfo field in fields)
                {
                    if (typeof(float).IsAssignableFrom(field.FieldType) || field.FieldType == typeof(BeatPosition))
                        lerpDropdown.ValidValues.Add(field.Name, field.Name);
                }
                SpawnForm<FormEntryEasing, EasingPair>("Lerp Easing", () => new (lerpHandler.LerpEasing, lerpHandler.LerpEaseMode), 
                    x => { lerpHandler.LerpEasing = x.Function; lerpHandler.LerpEaseMode = x.Mode; }
                );
            }
            
            SpawnForm<FormEntryDropdown, object>("Operation", () => lerpHandler.Operation, 
                x => { lerpHandler.Operation = (LerpableOperation)x; }
            ).TargetEnum(typeof(LerpableOperation));
        }
        void MakeBeatPositionEditor(ChartmakerMultiHandlerBeatPosition beatHandler)
        {
            bool advanced = !BeatPosition.IsNaN(beatHandler.From);
            SpawnForm<FormEntryBool, bool>("Advanced", () => advanced, x => { 
                beatHandler.From = x ? beatHandler.To : BeatPosition.NaN; 
                if (x) beatHandler.SetLerp(thing);
                UpdateForm();
            });
            if (advanced) SpawnForm<FormEntryBeatPosition, BeatPosition>("From", () => beatHandler.From, x => { beatHandler.From = x; });
            SpawnForm<FormEntryBeatPosition, BeatPosition>("To", () => beatHandler.To, x => { beatHandler.To = x; });
            
            if (advanced) 
            {
                var lerpDropdown = SpawnForm<FormEntryDropdown, object>("Lerp Source", () => beatHandler.LerpSource, x => {
                    beatHandler.LerpSource = (string)x; beatHandler.SetLerp(thing);
                });
                foreach (FieldInfo field in fields)
                {
                    if (typeof(float).IsAssignableFrom(field.FieldType) || field.FieldType == typeof(BeatPosition))
                        lerpDropdown.ValidValues.Add(field.Name, field.Name);
                }
                SpawnForm<FormEntryEasing, EasingPair>("Lerp Easing", () => new (beatHandler.LerpEasing, beatHandler.LerpEaseMode), 
                    x => { beatHandler.LerpEasing = x.Function; beatHandler.LerpEaseMode = x.Mode; }
                );
            }
            
            SpawnForm<FormEntryDropdown, object>("Operation", () => beatHandler.Operation, 
                x => { beatHandler.Operation = (BeatPositionOperation)x; }
            ).TargetEnum(typeof(BeatPositionOperation));
        }

        if (MultiHandler is ChartmakerMultiHandlerBoolean boolHandler) {
            SpawnForm<FormEntryDropdown, object>("To", () => boolHandler.To == null ? 2 : (bool)boolHandler.To ? 1 : 0, x => {
                boolHandler.To = new bool?[] {true, false, null}[(int)x];
            }).TargetList("False", "True", "Toggle");
        } else if (MultiHandler is ChartmakerMultiHandlerBeatPosition beatHandler) {
            MakeBeatPositionEditor(beatHandler);
        } else if (MultiHandler is ChartmakerMultiHandlerFloat floatHandler) {
            MakeLerpableEditor(floatHandler);
        } else if (MultiHandler is ChartmakerMultiHandlerVector2 v2Handler) {
            SpawnForm<FormEntryDropdown, object>("Axis", () => v2Handler.Axis, x => { v2Handler.Axis = (int)x; }).TargetList("X", "Y");
            SpawnForm<FormEntrySpace>("");
            MakeLerpableEditor(v2Handler);
        } else if (MultiHandler is ChartmakerMultiHandlerVector3 v3Handler) {
            SpawnForm<FormEntryDropdown, object>("Axis", () => v3Handler.Axis, x => { v3Handler.Axis = (int)x; }).TargetList("X", "Y", "Z");
            SpawnForm<FormEntrySpace>("");
            MakeLerpableEditor(v3Handler);
        } else if (MultiHandler is ChartmakerMultiHandler<int> intHandler) {
            MultiHandler.To ??= 0;
            SpawnForm<FormEntryInt, int>("To", () => (int)intHandler.To, x => { intHandler.To = x; });
        } else if (MultiHandler is ChartmakerMultiHandler<string> stringHandler) {
            MultiHandler.To ??= "";
            SpawnForm<FormEntryString, string>("To", () => (string)stringHandler.To, x => { stringHandler.To = x; });
        } else if (MultiHandler.TargetType.IsEnum) {
            MultiHandler.To ??= MultiHandler.TargetType.GetEnumValues().GetValue(0);
            SpawnForm<FormEntryDropdown, object>("To", () => MultiHandler.To, x => {
                MultiHandler.To = x;
            }).TargetEnum(MultiHandler.TargetType);
        } else {
            SpawnForm<FormEntryLabel>("Unknown field type " + CurrentMultiField?.FieldType);
        }

        SpawnForm<FormEntrySpace>("");

        var button = SpawnForm<FormEntryButton>("Execute");
        button.Button.onClick.AddListener(() => {
            ExecuteMulti(thing, Chartmaker.main.History);
        });
    }
    
    public void ExecuteMulti(IList items, ChartmakerHistory history) {

        ChartmakerMultiEditAction action = new ChartmakerMultiEditAction() 
        { 
            Keyword = CurrentMultiField.Name 
        };

        foreach(object obj in items) {
            ChartmakerMultiEditActionItem item = new ChartmakerMultiEditActionItem
            {
                Target = obj,
                From = CurrentMultiField.GetValue(obj),
            };
            item.To = MultiHandler.Get(item.From, obj);
            action.Targets.Add(item);
        }
        action.Redo();
        history.ActionsBehind.Push(action);
        history.ActionsAhead.Clear();
        Chartmaker.main.OnHistoryDo();
        Chartmaker.main.OnHistoryUpdate();
    }


    public void SetMultiField(FieldInfo field)
    {
        MultiHandler = MultiHandlers.ContainsKey(field.FieldType) ? MultiHandlers[field.FieldType] : MakeNewHandler(field.FieldType);
        CurrentMultiField = field;
    }

    private ChartmakerMultiHandler MakeNewHandler(Type type)
    {
        if (type ==  typeof(bool)) 
        {
            return new ChartmakerMultiHandlerBoolean();
        }
        else if (type == typeof(BeatPosition)) 
        {
            return new ChartmakerMultiHandlerBeatPosition();
        }
        else if (type == typeof(float)) 
        {
            return new ChartmakerMultiHandlerFloat();
        }
        else if (type == typeof(Vector2)) 
        {
            return new ChartmakerMultiHandlerVector2();
        }
        else if (type == typeof(Vector3)) 
        {
            return new ChartmakerMultiHandlerVector3();
        }
        else 
        {
            return Activator.CreateInstance(typeof(ChartmakerMultiHandler<>).MakeGenericType(type)) as ChartmakerMultiHandler;
        }
    }

    public void GoBack()
    {
        if (CurrentTimestamp.Count > 0) SetObject(CurrentObject);
        if (CurrentObject is LaneStyle or HitStyle) SetObject(Chartmaker.main.CurrentChart?.Pallete);
        else SetObject(null);
    }

    public bool IsSelected(object obj)
    {
        return obj is Timestamp ts ? CurrentTimestamp?.Contains(ts) == true : 
            CurrentObject is IList list ? list.Contains(obj) : CurrentObject == obj;
    }

    T SpawnForm<T>(string title = "") where T : FormEntry
        => Formmaker.main.Spawn<T>(FormHolder, title);

    T SpawnForm<T, U>(string title, Func<U> get, Action<U> set) where T : FormEntry<U>
        => Formmaker.main.Spawn<T, U>(FormHolder, title, get, set);

    void SetEase2(FormEntryEasing easeX, FormEntryEasing easeY)
    {
        easeX.TitleLabel.gameObject.SetActive(false);
        easeY.TitleLabel.gameObject.SetActive(false);
        easeY.EaseFunctionButton.transform.parent.SetParent(easeX.transform);
        easeY.GetComponent<LayoutElement>().minHeight = 0;
        var lg = easeX.GetComponent<HorizontalLayoutGroup>();
        lg.padding.left = 10;
        lg.spacing = 2;
    }
}