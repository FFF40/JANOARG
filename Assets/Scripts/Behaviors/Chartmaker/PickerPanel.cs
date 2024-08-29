using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PickerPanel : MonoBehaviour
{
    public static PickerPanel main;

    public TimelinePickerMode CurrentTimelinePickerMode;
    public List<Button> HierarchyButtons;
    public List<Button> TimelineButtons;
    
    public GameObject HierarchySongItems;
    public GameObject HierarchyChartItems;
    
    public GameObject hmmm;
    public GameObject hmmm2;

    public void Awake()
    {
        main = this;
    }

    public void Start()
    {
        for (int a = 0; a < TimelineButtons.Count; a++)
        {
            TimelinePickerMode mode = (TimelinePickerMode)a;
            TimelineButtons[a].onClick.AddListener(() => SetTimelinePickerMode(mode));
            TooltipTarget tt = TimelineButtons[a].gameObject.AddComponent<TooltipTarget>();
            tt.Text = TimelineButtons[a].name;
            tt.PositionMode = TooltipPositionMode.Right;
        }
        for (int a = 0; a < HierarchyButtons.Count; a++)
        {
            HierarchyPickerItem mode = (HierarchyPickerItem)a;
            HierarchyButtons[a].onClick.AddListener(() => ClickHierarchyPickerItem(mode));
            TooltipTarget tt = HierarchyButtons[a].gameObject.AddComponent<TooltipTarget>();
            tt.Text = HierarchyButtons[a].name;
            tt.PositionMode = TooltipPositionMode.Right;
        }
    }

    public void SetTabMode(int mode) => SetTimelinePickerMode((TimelinePickerMode)mode);

    public void SetTimelinePickerMode(TimelinePickerMode mode)
    {
        CurrentTimelinePickerMode = mode;
        UpdateButtons();
    }

    public void ClickHierarchyPickerItem(HierarchyPickerItem item) 
    {
        Chart chart = Chartmaker.main.CurrentChart;
        PlayableSong song = Chartmaker.main.CurrentSong;
        
        if (item == HierarchyPickerItem.CoverLayer)
        {
            ModalHolder.main.Spawn<NewCoverLayerModal>();
        }
        else if (item == HierarchyPickerItem.LaneStyle) 
        {
            LaneStyle target = chart.Pallete.LaneStyles.Count > 0 ? chart.Pallete.LaneStyles[0] : new LaneStyle() {
                LaneColor = song.InterfaceColor * new Color (1, 1, 1, .35f),
                JudgeColor = song.InterfaceColor,
            };
            switch (InspectorPanel.main.CurrentObject) {
                case LaneStyle ls: target = ls; break;
            }
            Chartmaker.main.AddItem(target.DeepClone());
        }
        else if (item == HierarchyPickerItem.HitStyle) 
        {
            HitStyle target = chart.Pallete.HitStyles.Count > 0 ? chart.Pallete.HitStyles[0] : new HitStyle() {
                NormalColor = song.InterfaceColor,
                CatchColor = Color.Lerp(song.InterfaceColor, song.BackgroundColor, .35f),
                HoldTailColor = song.InterfaceColor * new Color (1, 1, 1, .35f),
            };
            switch (InspectorPanel.main.CurrentObject) {
                case HitStyle ls: target = ls; break;
            }
            Chartmaker.main.AddItem(target.DeepClone());
        }
        else if (item == HierarchyPickerItem.Lane) 
        {
            string group = "";
            switch (InspectorPanel.main.CurrentObject) {
                case Lane l: group = l.Group; break;
                case LaneGroup lg: group = lg.Group; break;
            }

            Lane lane = new Lane {
                Position = new(0, -4, 0),
                Group = group,
            };
            lane.LaneSteps.Add(new LaneStep { 
                StartPos = new(-8, 0),
                EndPos = new(8, 0),
                Offset = (BeatPosition)InformationBar.main.beat
            });
            lane.LaneSteps.Add(new LaneStep { 
                StartPos = new(-8, 0),
                EndPos = new(8, 0),
                Offset = (BeatPosition)(InformationBar.main.beat + 1),
            });
            Chartmaker.main.AddItem(lane);
        }
        else if (item == HierarchyPickerItem.LaneGroup) 
        {
            string parent = "";
            switch (InspectorPanel.main.CurrentObject) {
                case Lane l: parent = l.Group; break;
                case LaneGroup lg: parent = lg.Group; break;
            }
            
            LaneGroup group = new LaneGroup {
                Group = parent,
                Name = InspectorPanel.main.GetNewGroupName("Group 1"),
            };
            Chartmaker.main.AddItem(group);
        }
    }
    

    public void UpdateButtons()
    {
        TimelineMode tMode = TimelinePanel.main.CurrentMode;
        TimelineButtons[3].gameObject.SetActive(tMode == TimelineMode.Storyboard);
        TimelineButtons[4].gameObject.SetActive(tMode == TimelineMode.Timing);
        TimelineButtons[5].gameObject.SetActive(tMode == TimelineMode.Lanes);
        TimelineButtons[6].gameObject.SetActive(tMode == TimelineMode.LaneSteps);
        TimelineButtons[7].gameObject.SetActive(tMode == TimelineMode.HitObjects);
        TimelineButtons[8].gameObject.SetActive(tMode == TimelineMode.HitObjects);

        HierarchyMode hMode = HierarchyPanel.main.CurrentMode;
        HierarchySongItems.gameObject.SetActive(hMode == HierarchyMode.PlayableSong);
        HierarchyChartItems.gameObject.SetActive(hMode == HierarchyMode.Chart);

        bool isOkay = false;
        for (int a = 0; a < TimelineButtons.Count; a++)
        {
            TimelineButtons[a].interactable = CurrentTimelinePickerMode != (TimelinePickerMode)a;
            if (TimelineButtons[a].gameObject.activeSelf && !TimelineButtons[a].interactable) isOkay = true;
        }

        if (!isOkay) SetTimelinePickerMode(TimelinePickerMode.Cursor);

        bool hmm = Random.value < 0.005;
        hmmm.SetActive(hmm);
        hmmm2.SetActive(hmm);
    }

    public void DoTheFunnyThing()
    {
        Application.OpenURL("https://file.garden/X9Xrm_GIBmpbTDCZ/omnicharting");
        hmmm.SetActive(false);
        hmmm2.SetActive(false);
    }
}

public enum TimelinePickerMode
{
    Cursor, Select, Delete,
    Timestamp, BPMStop, Lane, LaneStep, NormalHit, CatchHit
}

public enum HierarchyPickerItem
{
    CoverLayer,
    Lane, LaneGroup, LaneStyle, HitStyle
}
