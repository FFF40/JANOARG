using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Unity.VisualScripting;

public class OptionsPanel : MonoBehaviour
{

    public int CurrentTab;
    [Space]
    public TMP_Text SubtitleLabel;
    [Space]
    public ScrollRect ContentScroller;
    public LayoutGroup ContentLayout;
    public RectTransform ContentViewport;
    public CanvasGroup ContentGroup;
    public Scrollbar ContentScrollBar;
    [Space]
    public List<OptionsPanelTabButton> TabButtons;
    [Space]
    public RectTransform OptionItemHolder;
    public List<OptionItem> OptionItems;
    [Space]
    public GameObject AboutPane;
    [HideInInspector]
    public List<OptionItem> CurrentItems;
    [Space]
    public RenderTexture PreviewTexture;
    public Camera PreviewCamera;
    public RawImage PreviewTextureTarget;
    [Space]
    public Transform PreviewNormalCenter;
    public Transform PreviewNormalLeft;
    public Transform PreviewNormalRight;
    public MeshFilter PreviewNormalFlick;
    public Transform PreviewCatchCenter;
    public Transform PreviewCatchLeft;
    public Transform PreviewCatchRight;
    public MeshFilter PreviewCatchFlick;
    public Panel CurrentPanel;

    [HideInInspector]
    public bool IsAnimating;

    public Mesh FreeFlickIndicator { get; private set; }
    public Mesh ArrowFlickIndicator { get; private set; }

    void Start()
    {
        TabButtons[CurrentTab].SetFill(1);
        MakeTab(CurrentTab);

        PreviewTexture = new RenderTexture(Screen.width, Screen.height, 32, UnityEngine.Experimental.Rendering.DefaultFormat.LDR);
        PreviewTextureTarget.texture = PreviewCamera.targetTexture = PreviewTexture;
        PreviewCamera.fieldOfView = Camera.HorizontalToVerticalFieldOfView(110, PreviewCamera.aspect);
        
        InitFlickMeshes();
        UpdatePlayerPreview();
    }

    public void OnDestroy()
    {
        Destroy(PreviewTexture);
        Destroy(FreeFlickIndicator);
        Destroy(ArrowFlickIndicator);
    }

    public void Close()
    {
        Common.main.Storage.Save();
        Common.main.Preferences.Save();
        ProfileBar.main.UpdateLabels();
        if (CurrentTab == 1) StartCoroutine(PreviewAnim(false));
        CurrentPanel.Close();
    }

    public void SetTab(int tab)
    {
        if (!IsAnimating && tab != CurrentTab) StartCoroutine(SetTabAnim(tab));
    }

    public IEnumerator SetTabAnim(int tab)
    {
        IsAnimating = true;

        Vector2 basePos = ContentViewport.anchoredPosition;

        if (CurrentTab == 1) StartCoroutine(PreviewAnim(false));
        else if (tab == 1) StartCoroutine(PreviewAnim(true));

        yield return Ease.Animate(.2f, x => {
            float ease = Ease.Get(x, EaseFunction.Cubic, EaseMode.Out);
            TabButtons[CurrentTab].SetFill(1 - ease);
            SubtitleLabel.alpha = 1 - ease;
            SubtitleLabel.rectTransform.anchoredPosition = Vector2.left * 10 * ease;
            ContentViewport.anchoredPosition = basePos + Vector2.left * 10 * ease;
            ContentGroup.alpha = 1 - ease;
        });

        MakeTab(tab);
        CurrentTab = tab;

        yield return Ease.Animate(.2f, x => {
            float ease = Ease.Get(x, EaseFunction.Cubic, EaseMode.Out);
            TabButtons[CurrentTab].SetFill(ease);
            SubtitleLabel.alpha = ease;
            SubtitleLabel.rectTransform.anchoredPosition = Vector2.left * 10 * (1 - ease);
            ContentViewport.anchoredPosition = basePos + Vector2.left * 10 * (1 - ease);
            ContentGroup.alpha = ease;
        });

        IsAnimating = false;
    }

    public void SetScrollerWidth(float width)
    {
        rt(ContentScroller).sizeDelta = new (width, rt(ContentScroller).sizeDelta.y);
        ContentLayout.padding.right = (int)(30 - width / 20);
        rt(ContentScrollBar).anchoredPosition *= new Vector2Frag(y: width * Mathf.Tan(15 * Mathf.Deg2Rad) + 40);
    }

    public void MakeTab(int tab) 
    {
        ClearAll();
        ContentScroller.content.anchoredPosition = Vector2.zero;
        ContentScroller.velocity = Vector2.zero;
        AboutPane.SetActive(false);

        Storage Storage = Common.main.Storage;
        Storage Preferences = Common.main.Preferences;

        switch (tab)
        {
            case 0:
            {
                SubtitleLabel.text = " > General";
                SetScrollerWidth(500);

                Spawn<OptionCategoryTitle>("Profile");

                GetOptionItemSample<StringOptionInput>().Limit = 16;
                Spawn<StringOptionInput, string>("Player Name", 
                    () => Storage.Get("INFO:Name", "JANOARG"),
                    x => Storage.Set("INFO:Name", x)
                );
                
                Spawn<OptionCategoryTitle>("Audio");
                FloatOptionInput sample = GetOptionItemSample<FloatOptionInput>();
                sample.Min = 0; sample.Max = 100;
                sample.Step = 5; sample.Unit = "%";
                Spawn<FloatOptionInput, float>("UI Music Volume", 
                    () => Preferences.Get("GENR:UIMusicVolume", 100f),
                    x => Preferences.Set("GENR:UIMusicVolume", x)
                );
                Spawn<FloatOptionInput, float>("UI SFX Volume", 
                    () => Preferences.Get("GENR:UISFXVolume", 100f),
                    x => Preferences.Set("GENR:UISFXVolume", x)
                );

                // Spawn<OptionCategoryTitle>("Localization");
                // var lang = Spawn<ListOptionInput, string>("🌐 Language", 
                //     () => Storage.Get("MAIN:Language", "en"),
                //     x => Storage.Set("MAIN:Language", x)
                // );
                // lang.ValidValues.Add("en", "English");
                // lang.ValidValues.Add("fr", "Français");
                // lang.ValidValues.Add("zh_CN", "简体中文");
                // lang.ValidValues.Add("zh_TW", "繁體中文");
                // lang.ValidValues.Add("ja", "日本語");
                // lang.ValidValues.Add("ko", "한국어");
                // lang.ValidValues.Add("tok", "toki pona");
                // lang.ValidValues.Add("snale", "🐌 <alpha=#77>Snailian");

                // var altNames = Spawn<ListOptionInput, string>("Alt. Song Titles", 
                //     () => Storage.Get("MAIN:AltNameRule", "auto"),
                //     x => Storage.Set("MAIN:AltNameRule", x)
                // );
                // altNames.ValidValues.Add("auto", "Automatic (based on language)");
                // altNames.ValidValues.Add("never", "Always use original song titles");
                // altNames.ValidValues.Add("side", "Show original and alt. names side by side");
                // altNames.ValidValues.Add("always", "Always use alternative song titles");

                // var altArtist = Spawn<ListOptionInput, string>("Alt. Artist Names", 
                //     () => Storage.Get("MAIN:AltArtistRule", "auto"),
                //     x => Storage.Set("MAIN:AltArtistRule", x)
                // );
                // altArtist.ValidValues.Add("auto", "Use \"Alt. Song Titles\" setting");
                // altArtist.ValidValues.Add("never", "Always use original artist names");



                // Spawn<OptionCategoryTitle>("🐌");
                // Spawn<BooleanOptionInput, bool>("snail mode", 
                //     () => false,
                //     x => {}
                // );
                // Spawn<OptionText>(
                //     "This mode turns the game into an ✨indie✨scale✨rhythm✨game✨™, enable at your own risk."
                //     + "\nRequires a restart to reflect changes."
                // );
            }
            break;

            case 1:
            {
                SubtitleLabel.text = " > Gameplay";
                SetScrollerWidth(360);

                Spawn<OptionCategoryTitle>("Syncronization");

                FloatOptionInput sample = GetOptionItemSample<FloatOptionInput>();
                sample.Min = -500;
                sample.Max = 500;
                sample.Step = 1;
                sample.Unit = "ms";
                Spawn<JudgmentOffsetOptionInput, float>("Judgment Offset", 
                    () => Preferences.Get("PLYR:JudgmentOffset", 0f),
                    x => Preferences.Set("PLYR:JudgmentOffset", x)
                );
                Spawn<VisualOffsetOptionInput, float>("Visual Offset", 
                    () => Preferences.Get("PLYR:VisualOffset", 0f),
                    x => Preferences.Set("PLYR:VisualOffset", x)
                );

                Spawn<OptionCategoryTitle>("Audio");
                
                sample.Min = 0;
                sample.Max = 100;
                sample.Step = 5;
                sample.Unit = "%";

                MultiFloatOptionInput msample = GetOptionItemSample<MultiFloatOptionInput>();
                msample.Min = 0;
                msample.Max = 100;
                msample.Step = 5;
                msample.Unit = "%";
                msample.ValueType = MultiValueType.PerJudgment;

                Spawn<FloatOptionInput, float>("Music Volume", 
                    () => Preferences.Get("PLYR:BGMusicVolume", 100f),
                    x => Preferences.Set("PLYR:BGMusicVolume", x)
                );
                Spawn<MultiFloatOptionInput, float[]>("Hitsound Volume", 
                    () => Preferences.Get("PLYR:HitsoundVolume", new [] {60f}),
                    x => Preferences.Set("PLYR:HitsoundVolume", x)
                );

                Spawn<OptionCategoryTitle>("Visual");
                
                sample.Min = msample.Min = .2f;
                sample.Max = msample.Max = 5;
                sample.Step = msample.Step = .1f;
                sample.Unit = msample.Unit = "×";
                msample.ValueType = MultiValueType.PerHitType;

                Spawn<MultiFloatOptionInput, float[]>("Hit Object Scale", 
                    () => Preferences.Get("PLYR:HitScale", new [] {1f}),
                    x => { Preferences.Set("PLYR:HitScale", x); UpdatePlayerPreview(); }
                );
                Spawn<FloatOptionInput, float>("Flick Emblem Scale", 
                    () => Preferences.Get("PLYR:FlickScale", 1f),
                    x => { Preferences.Set("PLYR:FlickScale", x); UpdatePlayerPreview(); }
                );
            }
            break;

            case 2:
            {
                SubtitleLabel.text = " > About";
                SetScrollerWidth(600);

                AboutPane.SetActive(true);
                OptionAboutEntry entry;
                entry = Spawn<OptionAboutEntry>("LEAD PROGRAMMER / GAME DESIGNER");
                entry.BodyLabel.text = "duducat / ducdat0507";

                entry = Spawn<OptionAboutEntry>("SOUNDTRACK COMPOSERS (ORIGINAL TRACKS)");
                entry.BodyLabel.text = "Insert name of a famous artist here";

                entry = Spawn<OptionAboutEntry>("SOUNDTRACK COMPOSERS (LICENSED / FREE USE TRACKS)");
                entry.BodyLabel.text = "Sound Souler  •  mrcool909090  •  R3ality";
                
                entry = Spawn<OptionAboutEntry>("UI BACKGROUND MUSIC COMPOSERS");
                entry.BodyLabel.text = "duducat";

                entry = Spawn<OptionAboutEntry>("COVER ILLUSTRATORS");
                entry.BodyLabel.text = ":blobcat:";

                entry = Spawn<OptionAboutEntry>("CHART DESIGNERS");
                entry.BodyLabel.text = "duducat  •  M3galodon";
            }
            break;
        }
    }

    public TType GetOptionItemSample<TType>() where TType : OptionItem 
    {
        foreach (OptionItem item in OptionItems) 
        {
            if (item is TType type) return type;
        }
        return null;
    }

    public TType Spawn<TType>(string title) where TType : OptionItem 
    {
        TType item = Instantiate(GetOptionItemSample<TType>(), OptionItemHolder);
        item.transform.localEulerAngles = Vector3.forward * 15;
        item.TitleLabel.text = title;
        CurrentItems.Add(item);
        return item;
    }
    
    public void InitFlickMeshes() 
    {
        if (!FreeFlickIndicator) 
        {
            Mesh mesh = new();
            List<Vector3> verts = new();
            List<int> tris = new();

            verts.AddRange(new Vector3[] { new(-1, 0), new(0, 2), new(0, -.5f), new(1, 0), new(0, -2), new(0, .5f) });
            tris.AddRange(new [] {0, 1, 2, 3, 4, 5});

            mesh.SetVertices(verts);
            mesh.SetUVs(0, verts);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            PreviewCatchFlick.sharedMesh = FreeFlickIndicator = mesh;
        }
        if (!ArrowFlickIndicator) 
        {
            Mesh mesh = new();
            List<Vector3> verts = new();
            List<int> tris = new();

            verts.AddRange(new Vector3[] { new(-1, 0), new(0, 2.2f), new(1, 0), new(.71f, -.71f), new(0, -1), new(-.71f, -.71f) });
            tris.AddRange(new [] {0, 1, 2, 2, 3, 0, 3, 4, 0, 4, 5, 0});
            
            mesh.SetVertices(verts);
            mesh.SetUVs(0, verts);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            PreviewNormalFlick.sharedMesh = ArrowFlickIndicator = mesh;
        }
    }

    public void UpdatePlayerPreview()
    {
        float width = 5;
        var settings = new PlayerSettings();

        {
            float scale = settings.HitObjectScale[1];
            PreviewCatchCenter.localScale = new (width, .2f * scale, .2f * scale);
            PreviewCatchLeft.localScale = PreviewCatchRight.localScale = new Vector3(.2f, .4f, .4f) * scale;
            PreviewCatchRight.localPosition = Vector3.right * (width / 2);
            PreviewCatchLeft.localPosition = -PreviewCatchRight.localPosition;
        }
        {
            float scale = settings.HitObjectScale[0];
            PreviewNormalCenter.localScale = new (width - .2f * scale, .4f * scale, .4f * scale);
            PreviewNormalLeft.localScale = PreviewNormalRight.localScale = new Vector3(.2f, .4f, .4f) * scale;
            PreviewNormalRight.localPosition = Vector3.right * (width / 2 + .2f * scale);
            PreviewNormalLeft.localPosition = -PreviewNormalRight.localPosition;
        }

        PreviewCatchFlick.transform.localScale = PreviewNormalFlick.transform.localScale
            = Vector2.one * settings.FlickScale;
    }

    public TType Spawn<TType, TObject>(string title, Func<TObject> get, Action<TObject> set) where TType : OptionInput<TObject>
    {
        TType item = Instantiate(GetOptionItemSample<TType>(), OptionItemHolder);
        item.transform.localEulerAngles = Vector3.forward * 15;
        item.TitleLabel.text = title;
        item.OnGet = get;
        item.OnSet = set;
        CurrentItems.Add(item);
        return item;
    }

    public void ClearAll() 
    {
        foreach (OptionItem item in CurrentItems) 
        {
            Destroy(item.gameObject);
        }
        CurrentItems.Clear();
    }

    public void GoToSocial(string target) {
        string url = target switch {
            "discord" => "https://discord.gg/vXJTPFQBHm",
            "reddit" => "https://reddit.com/r/fff40",
            _ => "",
        };
        if (!string.IsNullOrEmpty(url)) Application.OpenURL(url);
    }

    IEnumerator PreviewAnim(bool shown) 
    {
        if (shown) 
        {
            PreviewCamera.gameObject.SetActive(true);
        }

        yield return Ease.Animate(.5f, (x) => {
            float ease1 = Ease.Get(Mathf.Pow(x, .5f), EaseFunction.Exponential, shown ? EaseMode.Out : EaseMode.In);
            PreviewCamera.transform.localPosition = new (0, (shown ? 12 : 0) - 12 * ease1, 0);
            PreviewCamera.transform.localEulerAngles = new (0, (shown ? 180 : 0) - 180 * ease1, 0);
        }); 

        if (!shown) 
        {
            PreviewCamera.gameObject.SetActive(false);
        }
    }

    static RectTransform rt(MonoBehaviour item) => (RectTransform)item.transform;
}
