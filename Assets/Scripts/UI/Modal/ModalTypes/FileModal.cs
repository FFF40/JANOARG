using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FileModal : Modal
{
    public static FileModal main;

    [Space]
    public string CurrentDirectory;
    
    [Space]
    public TMP_InputField PathField;
    public TMP_InputField TargetField;

    [Space]
    public RectTransform BookmarkViewport;
    public RectTransform BookmarkHolder;
    public RectTransform ItemViewport;
    public RectTransform ItemHolder;
    public FileModalItem ItemSample;
    public TMP_Text MessageLabel;

    [Space]
    public RectTransform FileTypeHolder;
    public TMP_Text FileTypeLabel;

    [Space]
    public Sprite FileIcon;
    public Sprite FolderIcon;
    public Sprite DriveIcon;
    public Sprite AudioFileIcon;
    public Sprite ImageFileIcon;
    public Sprite PlayableSongFileIcon;
    public Sprite ChartFileIcon;

    [Space]
    public TMP_Text HeaderLabel;
    public TMP_Text SelectLabel;

    [Space]
    public List<FileModalFileType> AcceptedTypes = new();
    FileModalFileType CurrentType;

    [Space]
    public UnityEvent OnSelect;

    List<FileModalHistoryEntry> HistoryBehind = new();
    List<FileModalHistoryEntry> HistoryAhead = new();

    List<FileModalEntry> entries = new();
    List<FileModalItem> items = new();

    int currentOffset = 0;
    float itemHeight = 0;

    [HideInInspector]
    public FileModalEntry SelectedEntry;

    public void Awake()
    {
        if (main) Close();
        else main = this;
    }

    new void Start()
    {
        base.Start();
        SetUpBookmarks();
        CurrentDirectory = Path.GetDirectoryName(Application.dataPath);
        SetFileType(AcceptedTypes[0]);
    }

    public void SetUpBookmarks()
    {
        List<FileModalEntry> bookmarks = new();

        var drives = Directory.GetLogicalDrives();

        foreach (string drive in drives)
        {
            bookmarks.Add(new FileModalEntry {
                Path = drive,
                Text = drive,
                IsFolder = true,
            });
        }

        itemHeight = ((RectTransform)ItemSample.transform).sizeDelta.y;
        BookmarkHolder.sizeDelta = new(BookmarkHolder.sizeDelta.x, bookmarks.Count * itemHeight);

        foreach (FileModalEntry entry in bookmarks)
        {
            FileModalItem item = Instantiate(ItemSample, BookmarkHolder);
            item.Parent = this;
            SetBookmark(item, entry);
        }
    }

    public void SetDirectory(string path)
    {

        if (string.IsNullOrWhiteSpace(path)) 
        {
            PathField.text = CurrentDirectory;
            return;
        }

        entries.Clear();

        try 
        {
            var folders = Directory.GetDirectories(path);
            var files = Directory.GetFiles(path);

            PathField.text = path;

            foreach (string folder in folders)
            {
                entries.Add(new FileModalEntry {
                    Path = folder,
                    Text = "<alpha=#77>\\<alpha=#ff>" + Path.GetFileName(folder),
                    IsFolder = true,
                });
            }
            foreach (string file in files)
            {
                bool valid = CurrentType.Filter.Length <= 0;
                if (!valid) foreach (string ext in CurrentType.Filter) if (file.EndsWith("." + ext))
                {
                    valid = true;
                    break;
                }
                if (valid) entries.Add(new FileModalEntry {
                    Path = file,
                    Text = Path.GetFileName(file),
                    IsFolder = false,
                });
            }

            MessageLabel.gameObject.SetActive(false);
        }
        catch (Exception e)
        {
            MessageLabel.gameObject.SetActive(true);
            
            MessageLabel.text = e.Message;
        }

        CurrentDirectory = path;

        UpdateButtons(true);
    }

    public void FileTypePopup()
    {
        ContextMenuList list = new();
        foreach (FileModalFileType type in AcceptedTypes)
        {
            list.Items.Add(new ContextMenuListAction(
                type.Description + " (" + (type.Filter.Length == 0 ? "*" : "*." + string.Join(",*.", type.Filter)) + ")",
                () => SetFileType(type), _checked: CurrentType == type
            ));
        }
        ContextMenuHolder.main.OpenRoot(list, FileTypeHolder);
    }

    public void SetFileType(FileModalFileType type)
    {
        CurrentType = type;
        FileTypeLabel.text = type.Description + " (" + (type.Filter.Length == 0 ? "*" : "*." + string.Join(",*.", type.Filter)) + ")";
        SetDirectory(CurrentDirectory);
    }

    IEnumerator UpdateButtonDelay() { yield return 0; UpdateButtons(); }

    public void UpdateButtons(bool forced = false)
    {
        if (ItemViewport.rect.height <= 0) 
        {
            StartCoroutine(UpdateButtonDelay());
            return;
        }

        itemHeight = ((RectTransform)ItemSample.transform).sizeDelta.y;
        int height = Mathf.FloorToInt(ItemViewport.rect.height / itemHeight) + 2;
        ItemHolder.sizeDelta = new (ItemHolder.sizeDelta.x, itemHeight * entries.Count);

        while (items.Count > height) 
        {
            Destroy(items[^1]);
            items.RemoveAt(items.Count - 1);
        }

        int offset = Mathf.FloorToInt(ItemHolder.anchoredPosition.y / itemHeight);
        if (forced || offset != currentOffset)
        {
            if (forced || Mathf.Abs(offset - currentOffset) >= items.Count)
            {
                for (int a = 0; a < items.Count; a++) SetItem(items[a], offset + a);
                currentOffset = offset;
            }
            else 
            {
                while (currentOffset < offset)
                {
                    FileModalItem item = items[0];
                    SetItem(item, currentOffset + items.Count);
                    items.RemoveAt(0);
                    items.Add(item);
                    currentOffset ++;
                }
                while (currentOffset > offset)
                {
                    FileModalItem item = items[^1];
                    SetItem(item, currentOffset - 1);
                    items.RemoveAt(items.Count - 1);
                    items.Insert(0, item);
                    currentOffset --;
                }
            }
        }

        while (items.Count < height) 
        {
            FileModalItem item = Instantiate(ItemSample, ItemHolder);
            item.Parent = this;
            items.Add(item);
            SetItem(item, offset + items.Count - 1);
        }
    }

    public void SetItem(FileModalItem item, int index)
    {
        item.gameObject.SetActive(index >= 0 && index < entries.Count);
        if (item.gameObject.activeSelf)
        {
            ((RectTransform)item.transform).anchoredPosition = new Vector2(0, index * -itemHeight);
            item.Entry = entries[index];
            item.Text.text = entries[index].Text;
            item.Icon.sprite = entries[index].IsFolder ? FolderIcon : GetIcon(entries[index].Text);
        }
    }

    public void SetBookmark(FileModalItem item, FileModalEntry entry)
    {
        item.gameObject.SetActive(true);
        item.Entry = entry;
        item.Button.onClick.RemoveAllListeners();
        item.Button.onClick.AddListener(() => Navigate(entry.Path));
        item.Text.text = entry.Text;
        item.Icon.sprite = DriveIcon;
    }

    public void InvokeEntry()
    {
        if (SelectedEntry != null) InvokeEntry(SelectedEntry);
    }

    public void SelectItem(FileModalEntry entry)
    {
        if (SelectedEntry == entry)
        {
            InvokeEntry(entry);
        }
        else
        {
            SelectedEntry = entry;
            TargetField.text = Path.GetFileName(entry.Path);
        }
    }

    public void InvokeEntry(FileModalEntry entry)
    {
        if (entry.IsFolder)
        {
            Navigate(entry.Path);
        }
        else
        {
            OnSelect.Invoke();
            Close();
        }
    }

    public void Navigate(string path)
    {
        if (path == CurrentDirectory) return;
        AddToHistory();
        ItemHolder.anchoredPosition = Vector2.zero;
        SetDirectory(path);
    }
    public void MoveUp()
    {
        AddToHistory();
        SetDirectory(Path.GetDirectoryName(CurrentDirectory));
    }

    public void AddToHistory()
    {
        HistoryBehind.Add(new FileModalHistoryEntry {
            Path = CurrentDirectory,
            ScrollPos = ItemHolder.anchoredPosition.y
        });
        HistoryAhead.Clear();
    }
    public void MoveHistory(List<FileModalHistoryEntry> _from, List<FileModalHistoryEntry> _to)
    {
        if (_from.Count > 0)
        {
            _to.Add(new FileModalHistoryEntry {
                Path = CurrentDirectory,
                ScrollPos = ItemHolder.anchoredPosition.y
            });
            FileModalHistoryEntry entry = _from[^1];
            SetDirectory(entry.Path);
            ItemHolder.anchoredPosition = Vector2.up * entry.ScrollPos;
            _from.RemoveAt(_from.Count - 1);
        }
    }
    public void GoBack()
    {
        MoveHistory(HistoryBehind, HistoryAhead);
    }
    public void GoForward()
    {
        MoveHistory(HistoryAhead, HistoryBehind);
    }

    public Sprite GetIcon(string path) => Path.GetExtension(path).ToLower() switch {
        ".japs" => PlayableSongFileIcon,
        ".jac" => ChartFileIcon,
        ".mp3" or ".ogg" or ".wav" => AudioFileIcon,
        ".png" or ".jpg" => ImageFileIcon,
        _ => FileIcon,
    };
}

[Serializable]
public class FileModalFileType
{
    public string Description;
    public string[] Filter;

    public FileModalFileType(string description, params string[] filter)
    {
        Description = description;
        Filter = filter;
    }
}

public class FileModalHistoryEntry
{
    public string Path;
    public float ScrollPos;
}

public class FileModalEntry
{
    public string Path;
    public string Text;
    public bool IsFolder;
}
