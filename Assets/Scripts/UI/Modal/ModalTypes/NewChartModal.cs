using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class NewChartModal : Modal
{
    public static NewChartModal main;

    [Space]
    public LayoutElement FormLayout;
    public RectTransform FormHolder;
    public VerticalLayoutGroup FormHolderLayout;

    [Header("Data")]
    public string Codename;

    public Chart InitialValues;

    public void Awake()
    {
        if (main) Close();
        else main = this;
    }

    new void Start()
    {
        base.Start();
        InitForm();
    }

    public void InitForm()
    {
        FormEntryString name = null;
        FormEntryInt index = null;

        PlayableSong song = Chartmaker.main.CurrentSong;

        var codename = SpawnForm<FormEntryString, string>("Codename", () => Codename, x => Codename = x);
        codename.Field.contentType = TMP_InputField.ContentType.Alphanumeric;
        
        SpawnForm<FormEntryHeader>("Presets");

        HorizontalLayoutGroup group = new GameObject("Presets", typeof(RectTransform)).AddComponent<HorizontalLayoutGroup>();
        group.transform.SetParent(FormHolder);
        group.gameObject.AddComponent<LayoutElement>().minHeight = 24;
        group.spacing = 1;
        group.padding = new RectOffset(10, 10, 0, 0);

        string[] presetNames = {"Simple", "Normal", "Complex", "Overdrive", "Special"};
        for (int a = 0; a < presetNames.Length; a++)
        {
            int A = a;
            var button = Formmaker.main.Spawn<FormEntryButton>((RectTransform)group.transform, presetNames[a]);
            button.Button.transform.SetParent(group.transform);
            button.gameObject.SetActive(false);
            button.TitleLabel.text = presetNames[a];
            button.Button.onClick.AddListener(() => {
                Codename = A == 4 ? "" : presetNames[A].ToLower();
                InitialValues.DifficultyName = A == 4 ? "" : presetNames[A];
                InitialValues.DifficultyIndex = A == 4 ? -1 : A;
                name.Start();
                index.Start();
                codename.Start();
            });
        }
        
        SpawnForm<FormEntryHeader>("Metadata");
        name = SpawnForm<FormEntryString, string>("Chart Name", () => InitialValues.DifficultyName, x => InitialValues.DifficultyName = x);
        index = SpawnForm<FormEntryInt, int>("Sorting Index", () => InitialValues.DifficultyIndex, x => InitialValues.DifficultyIndex = x);
        SpawnForm<FormEntryString, string>("Difficulty", () => InitialValues.DifficultyLevel, x => InitialValues.DifficultyLevel = x);
        SpawnForm<FormEntryFloat, float>("Chart Constant", () => InitialValues.ChartConstant, x => InitialValues.ChartConstant = x);
        
        SpawnForm<FormEntryHeader>("Pallete");
        InitialValues.Pallete.BackgroundColor = song.BackgroundColor;
        InitialValues.Pallete.InterfaceColor = song.InterfaceColor;
        if (InitialValues.Pallete.LaneStyles.Count == 0) InitialValues.Pallete.LaneStyles.Add(new LaneStyle {
            LaneColor = song.InterfaceColor * new Color (1, 1, 1, .35f),
            JudgeColor = song.InterfaceColor,
        });
        if (InitialValues.Pallete.HitStyles.Count == 0) InitialValues.Pallete.HitStyles.Add(new HitStyle {
            NormalColor = song.InterfaceColor,
            CatchColor = Color.Lerp(song.InterfaceColor, song.BackgroundColor, .35f),
            HoldTailColor = song.InterfaceColor * new Color (1, 1, 1, .35f),
        });
        SpawnForm<FormEntryColor, Color>("Background Color", () => InitialValues.Pallete.BackgroundColor, x => InitialValues.Pallete.BackgroundColor = x);
        SpawnForm<FormEntryColor, Color>("Interface Color", () => InitialValues.Pallete.InterfaceColor, x => InitialValues.Pallete.InterfaceColor = x);
        SpawnForm<FormEntrySpace>("");
        SpawnForm<FormEntryColor, Color>("Lane Color", () => InitialValues.Pallete.LaneStyles[0].LaneColor, x => InitialValues.Pallete.LaneStyles[0].LaneColor = x);
        SpawnForm<FormEntryColor, Color>("Lane Judge Color", () => InitialValues.Pallete.LaneStyles[0].JudgeColor, x => InitialValues.Pallete.LaneStyles[0].JudgeColor = x);
        SpawnForm<FormEntrySpace>("");
        SpawnForm<FormEntryColor, Color>("Normal Hit Color", () => InitialValues.Pallete.HitStyles[0].NormalColor, x => InitialValues.Pallete.HitStyles[0].NormalColor = x);
        SpawnForm<FormEntryColor, Color>("Catch Hit Color", () => InitialValues.Pallete.HitStyles[0].CatchColor, x => InitialValues.Pallete.HitStyles[0].CatchColor = x);
        SpawnForm<FormEntryColor, Color>("Hold Tail Color", () => InitialValues.Pallete.HitStyles[0].HoldTailColor, x => InitialValues.Pallete.HitStyles[0].HoldTailColor = x);

        LayoutRebuilder.ForceRebuildLayoutImmediate(FormHolder);
        FormLayout.preferredHeight = FormHolderLayout.preferredHeight;
    }
    
    public ExternalChartMeta Execute()
    {
        if (string.IsNullOrWhiteSpace(Codename))
        {
            throw new Exception("Please specify a codename for the Chart.");
        }

        string path = Path.GetDirectoryName(Chartmaker.main.CurrentSongPath) + "/" + Codename + ".jac";
        if (File.Exists(path))
        {
            throw new Exception("There is already a Chart with the same codename as you choose for this Chart. Please change the codename to a different value.");
        }

        PlayableSong song = Chartmaker.main.CurrentSong;
        ExternalChartMeta chart = null;

        song.Charts.Add(chart = new ExternalChartMeta {
            Target = Codename,
            DifficultyName = InitialValues.DifficultyName,
            DifficultyIndex = InitialValues.DifficultyIndex,
            DifficultyLevel = InitialValues.DifficultyLevel,
            ChartConstant = InitialValues.ChartConstant,
        });

        File.WriteAllText(path, JACEncoder.Encode(InitialValues));

        Chartmaker.main.Save();

        return chart;
    }

    public Task<ExternalChartMeta> ExecuteAsync()
    {
        return Task.Run(() => Execute());
    }
    
    public IEnumerator ExecuteRoutine() {
        Chartmaker.main.Loader.SetActive(true);
        Chartmaker.main.LoaderPanel.SetSong(Chartmaker.main.CurrentSong);
        Chartmaker.main.LoaderPanel.ActionLabel.text = "Creating Chart...";
        Chartmaker.main.LoaderPanel.ProgressBar.value = 0;

        Chartmaker.main.LoaderPanel.ProgressLabel.text = "Initializing...";
        yield return new WaitForSeconds(0.5f);

        Chartmaker.main.LoaderPanel.ProgressLabel.text = "Creating .jac file...";

        Task<ExternalChartMeta> task = ExecuteAsync(); 
        yield return new WaitUntil(() => task.IsCompleted);
        if (!task.IsCompletedSuccessfully) 
        {
            Chartmaker.main.Loader.SetActive(false);
            DialogModal modal = ModalHolder.main.Spawn<DialogModal>();
            modal.SetDialog("Error", task.Exception.Message, new string[] {"Ok"}, _ => {});
            yield break;
        }
        
        Chartmaker.main.StartCoroutine(Chartmaker.main.OpenChartRoutine(task.Result));
        Close();
    }
    
    public void StartExecuteRoutine() {
        StartCoroutine(ExecuteRoutine());
    }

    T SpawnForm<T>(string title = "") where T : FormEntry
        => Formmaker.main.Spawn<T>(FormHolder, title);

    T SpawnForm<T, U>(string title, Func<U> get, Action<U> set) where T : FormEntry<U>
        => Formmaker.main.Spawn<T, U>(FormHolder, title, get, set);


}
