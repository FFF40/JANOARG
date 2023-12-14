using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HelpModal : Modal
{
    public static HelpModal main;

    public RectTransform EntryListHolder;
    public List<HelpEntry> HelpEntries;
    public HelpEntry HelpEntrySample;

    public RectTransform ContentHolder;

    public TMP_Text TitleLabel;
    public TMP_Text Title2Label;
    public TMP_Text ContentLabel;
    public LayoutElement LayoutHolder;
    public Image LayoutBox;
    public Image LayoutBorder;
    public TMP_Text LayoutText;

    TMP_Text currentLabel;
    LayoutElement currentLayout;
    Image currentBox;

    bool isLoading = false;

    public void Awake()
    {
        if (main) Close();
        else main = this;
    }

    public new void Start()
    {
        base.Start();
        StartCoroutine(InitHelp());
    }

    IEnumerator InitHelp()
    {
        ResourceRequest request = Resources.LoadAsync<TextAsset>("Help/$index");
        yield return new WaitUntil(() => request.isDone);
        TextAsset index = request.asset as TextAsset;

        HelpEntry prev = null;
        
        foreach (string line in index.text.Split('\n'))
        {
            string data = line;
            int indent = 0;
            int pos = line.IndexOf(' ');
            while (pos == 0) 
            {
                data = data[1..];
                indent++;
                pos = data.IndexOf(' ');
            }
            string path = data[..pos];
            string name = data[(pos + 1)..];

            HelpEntry entry = Instantiate(HelpEntrySample, EntryListHolder);
            entry.SetItem(name, path, indent);
            entry.Parent = this;
            entry.gameObject.SetActive(indent == 0);
            if (prev) prev.ExpandButton.gameObject.SetActive(prev.Indent < indent);

            HelpEntries.Add(entry);
            prev = entry;
        }

        LoadEntry("index");
    }

    public void UpdateEntries() 
    {
        int activeIndent = 0;

        foreach (HelpEntry entry in HelpEntries) 
        {
            entry.gameObject.SetActive(activeIndent >= entry.Indent);
            activeIndent = Mathf.Min(activeIndent, entry.Indent);
            if (entry.gameObject.activeSelf && entry.Expanded) activeIndent = entry.Indent + 1;
        }
    }

    public void LoadEntry(string target) 
    {
        if (isLoading) return;
        isLoading = true;
        StartCoroutine(LoadEntryRoutine(target));
    }

    IEnumerator LoadEntryRoutine(string target)
    {
        foreach (Transform child in ContentHolder) Destroy(child.gameObject);

        foreach (HelpEntry entry in HelpEntries) entry.Button.interactable = entry.Target != target;

        ResourceRequest request = Resources.LoadAsync<HelpPage>("Help/" + target);
        yield return new WaitUntil(() => request.isDone);
        HelpPage page = request.asset as HelpPage;

        string currentMode = "";

        if (!page) 
        {
            isLoading = false;
            yield break;
        }

        foreach (string line in page.Lines)
        {
            if (line == "$title")
            {
                currentLabel = Instantiate(TitleLabel, ContentHolder);
                currentLabel.text = "";
                currentMode = "title";
            } 
            else if (line == "$t2")
            {
                currentLabel = Instantiate(Title2Label, ContentHolder);
                currentLabel.text = "";
                currentMode = "t2";
            } 
            else if (line == "$")
            {
                currentLabel = Instantiate(ContentLabel, ContentHolder);
                currentLabel.text = "";
                currentMode = "p";
            }
            else if (line == "$pre")
            {
                currentLabel = Instantiate(ContentLabel, ContentHolder);
                currentLabel.text = "";
                currentMode = "pre";
            }
            else if (line == "$layout")
            {
                currentLayout = Instantiate(LayoutHolder, ContentHolder);
                currentMode = "layout";
            }
            else if (line == "$n")
            {
                currentLabel.text += "\n";
            }
            else 
            {
                if (currentMode == "layout") 
                {
                    string[] cmds = line.Split(' ');
                    if (cmds.Length <= 0) continue;

                    if (cmds[0] == "Box") currentBox = Instantiate(LayoutBox, currentLayout.transform);
                    else continue;

                    Vector2 position = new();
                    float.TryParse(cmds[1], out position.x);
                    position.x += 10;
                    float.TryParse(cmds[2], out position.y);
                    position.y = -position.y - 10;
                    currentBox.rectTransform.anchoredPosition = position;

                    Vector2 size = new();
                    float.TryParse(cmds[3], out size.x);
                    float.TryParse(cmds[4], out size.y);
                    currentBox.rectTransform.sizeDelta = size;

                    currentLayout.minHeight = Mathf.Max(currentLayout.minHeight, size.y - position.y + 6);

                    int pos = 5;
                    currentBox.gameObject.SetActive(false);
                    while (pos < cmds.Length)
                    {
                        if (cmds[pos] == "color")
                        {
                            GraphicThemeable themeable = currentBox.gameObject.AddComponent<GraphicThemeable>();
                            themeable.Target = currentBox;
                            themeable.ID = cmds[pos+1];
                            pos += 2;
                        }
                        else if (cmds[pos] == "text")
                        {
                            TMP_Text text = Instantiate(LayoutText, currentBox.transform);
                            text.rectTransform.anchoredPosition = Vector2.zero;
                            
                            GraphicThemeable themeable = currentBox.gameObject.AddComponent<GraphicThemeable>();
                            themeable.Target = text;
                            themeable.ID = cmds[pos+1];
                            
                            pos += 2;

                            if (cmds[pos].StartsWith('"'))
                            {
                                text.text = cmds[pos][1..];
                                pos++;
                                while (pos < cmds.Length)
                                {
                                    if (cmds[pos].EndsWith('"'))
                                    {
                                        text.text += " " + cmds[pos][..^1];
                                        pos++;
                                        break;
                                    }
                                    else 
                                    {
                                        text.text += " " + cmds[pos];
                                        pos++;
                                    }
                                }
                            }
                            else 
                            {
                                text.text = cmds[pos];
                                pos++;
                            }

                        }
                        else if (cmds[pos] == "border")
                        {
                            var border = Instantiate(LayoutBorder, currentBox.transform);
                            border.rectTransform.anchoredPosition = Vector2.zero;
                            border.rectTransform.sizeDelta = Vector2.zero;

                            GraphicThemeable themeable = border.gameObject.AddComponent<GraphicThemeable>();
                            themeable.Target = border;
                            themeable.ID = cmds[pos+1];

                            pos += 2;
                        }
                        else if (cmds[pos] == "shadow")
                        {
                            var shadow = currentBox.gameObject.AddComponent<Shadow>();
                            shadow.effectDistance = new(2, -2);

                            ShadowThemeable themeable = currentBox.gameObject.AddComponent<ShadowThemeable>();
                            themeable.Target = shadow;
                            themeable.ID = cmds[pos+1];
                            
                            currentLayout.minHeight = Mathf.Max(currentLayout.minHeight, size.y - position.y + 8);

                            pos += 2;
                        }
                        else 
                        {
                            break;
                        }
                    }
                    currentBox.gameObject.SetActive(true);
                }
                else 
                {
                    currentLabel.text += line;
                    if (currentMode == "pre") currentLabel.text += "\n";
                }
            }
        }

        ContentHolder.anchoredPosition = Vector2.zero;

        isLoading = false;
    }

}
