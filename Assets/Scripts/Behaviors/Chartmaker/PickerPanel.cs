using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PickerPanel : MonoBehaviour
{
    public static PickerPanel main;

    public PickerMode CurrentMode;
    public List<Button> Buttons;

    public GameObject hmmm;
    public GameObject hmmm2;

    public void Awake()
    {
        main = this;
    }
    public void Start()
    {
        for (int a = 0; a < Buttons.Count; a++)
        {
            PickerMode mode = (PickerMode)a;
            Buttons[a].onClick.AddListener(() => SetTabMode(mode));
            TooltipTarget tt = Buttons[a].gameObject.AddComponent<TooltipTarget>();
            tt.Text = Buttons[a].name;
            tt.PositionMode = TooltipPositionMode.Right;
        }
    }

    public void SetTabMode(int mode) => SetTabMode((PickerMode)mode);

    public void SetTabMode(PickerMode mode)
    {
        CurrentMode = mode;
        UpdateButtons();
    }
    

    public void UpdateButtons()
    {
        TimelineMode mode = TimelinePanel.main.CurrentMode;
        Buttons[3].gameObject.SetActive(mode == TimelineMode.Storyboard);
        Buttons[4].gameObject.SetActive(mode == TimelineMode.Timing);
        Buttons[5].gameObject.SetActive(mode == TimelineMode.Lanes);
        Buttons[6].gameObject.SetActive(mode == TimelineMode.LaneSteps);
        Buttons[7].gameObject.SetActive(mode == TimelineMode.HitObjects);
        Buttons[8].gameObject.SetActive(mode == TimelineMode.HitObjects);

        bool isOkay = false;
        for (int a = 0; a < Buttons.Count; a++)
        {
            Buttons[a].interactable = CurrentMode != (PickerMode)a;
            if (Buttons[a].gameObject.activeSelf && !Buttons[a].interactable) isOkay = true;
        }

        if (!isOkay) SetTabMode(PickerMode.Cursor);

        bool hmm = Random.value < 0.005;
        hmmm.SetActive(hmm);
        hmmm2.SetActive(hmm);
    }

    public void DoTheFunnyThing()
    {
        Application.OpenURL("https://cdn.discordapp.com/attachments/845255908408950814/1143937770352545955/RPReplay_Final1681660404.mov");
        hmmm.SetActive(false);
        hmmm2.SetActive(false);
    }
}

public enum PickerMode
{
    Cursor, Select, Delete,
    Timestamp, BPMStop, Lane, LaneStep, NormalHit, CatchHit
}
