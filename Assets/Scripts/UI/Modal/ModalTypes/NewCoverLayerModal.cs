using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class NewCoverLayerModal : Modal
{
    public static NewCoverLayerModal main;

    public Image Preview;
    public GameObject Placeholder;
    public TMP_Text Information;
    public Texture2D ImageData;

    public MetadataReader Metadata;

    public RectTransform ImageFromMetadataButton;

    public TMP_InputField TargetField;
    public string Extension = ".png";
    public TMP_Text ExtensionLabel;
    public RectTransform ExtensionButton;

    public void Awake()
    {
        if (main) Close();
        else main = this;
    }

    new void Start()
    {
        base.Start();
        ExtensionLabel.text = Extension;
    }

    public void OnDestroy() 
    {
        UnloadImage();
    }

    public void OpenExtensionDropdown()
    {
        ContextMenuList list = new ContextMenuList();
        foreach (string ext in new string[] { ".png", ".jpg" })
        {
            list.Items.Add(new ContextMenuListAction(ext, () => {
                ExtensionLabel.text = Extension = ext;
            }, _checked: Extension == ext));
        }
        ContextMenuHolder.main.OpenRoot(list, ExtensionButton);
    }

    public void ExtractFromMetadata()
    {
        Metadata ??= new (Path.Combine(Path.GetDirectoryName(Chartmaker.main.CurrentSongPath), Chartmaker.main.CurrentSong.ClipPath));
        
        ContextMenuList list = new ContextMenuList();
        if (Metadata.Attachments.Count <= 0) 
        {
            list.Items.Add(new ContextMenuListAction("No images found", () => {}, _enabled: false));
        }
        foreach (MetadataReader.AttachmentData item in Metadata.Attachments)
        {
            list.Items.Add(new ContextMenuListAction(item.Name, () => {
                UnloadImage();
				Debug.Log(item.Data.Length + " | " + item.Data[0] + item.Data[01] + item.Data[02] + item.Data[03]);
                ImageData = new(1, 1);
                ImageConversion.LoadImage(ImageData, item.Data);
                Sprite sprite = Sprite.Create(ImageData, new Rect(0, 0, ImageData.width, ImageData.height), new Vector2(.5f, .5f));
                Preview.sprite = sprite;
                Preview.gameObject.SetActive(true);
                Placeholder.gameObject.SetActive(false);
                Information.text = ImageData.width + "×" + ImageData.height + " " + item.Type;
            }));
        }
        ContextMenuHolder.main.OpenRoot(list, ImageFromMetadataButton);
    }

    public void SelectFromImageFile() 
    {
        FileModal modal = ModalHolder.main.Spawn<FileModal>();
        modal.AcceptedTypes = new () {
            new FileModalFileType("Supported image files", "png", "jpg", "jpeg"),
            new FileModalFileType("All files"),
        };
        modal.HeaderLabel.text = "Select Image...";
        modal.SelectLabel.text = "Select";
        modal.OnSelect.AddListener(() => {
            UnloadImage();
            byte[] data = File.ReadAllBytes(modal.SelectedEntry.Path);
            ImageData = new(1, 1);
            ImageConversion.LoadImage(ImageData, data);
            Sprite sprite = Sprite.Create(ImageData, new Rect(0, 0, ImageData.width, ImageData.height), new Vector2(.5f, .5f));
            Preview.sprite = sprite;
            Preview.gameObject.SetActive(true);
            Placeholder.gameObject.SetActive(false);
            Information.text = ImageData.width + "×" + ImageData.height + " image/" + Path.GetExtension(modal.SelectedEntry.Path);
        });
    }
    
    public IEnumerator SaveImageRoutine() {
        
        PlayableSong song = Chartmaker.main.CurrentSong;
        string target = TargetField.text + Extension;
        if (!ImageData)
        {
            DialogModal modal = ModalHolder.main.Spawn<DialogModal>();
            modal.SetDialog("Error", "Please specify an image to be used by the cover layer.", new string[] {"Ok"}, _ => {});
            yield break;
        }
        if (string.IsNullOrWhiteSpace(TargetField.text))
        {
            DialogModal modal = ModalHolder.main.Spawn<DialogModal>();
            modal.SetDialog("Error", "Please specify a file target for the image.", new string[] {"Ok"}, _ => {});
            yield break;
        }
        if (Chartmaker.main.CurrentSong.Cover.Layers.Find(x => x.Target == target) != null)
        {
            DialogModal modal = ModalHolder.main.Spawn<DialogModal>();
            modal.SetDialog("Error", "There is already a cover layer with the same file target.", new string[] {"Ok"}, _ => {});
            yield break;
        }
        string path = Path.Combine(Path.GetDirectoryName(Chartmaker.main.CurrentSongPath), target);
        if (File.Exists(path))
        {
            int choice = 0;
            DialogModal modal = ModalHolder.main.Spawn<DialogModal>();
            modal.SetDialog("Overwrite File?", "There is already a file on this path. Would you like to overwrite this file?", new string[] {"Overwrite", "Cancel"}, x => {
                choice = x;
            });
            yield return new WaitWhile(() => modal);
            if (choice == 1) yield break;
        }

        Chartmaker.main.Loader.SetActive(true);
        Chartmaker.main.LoaderPanel.ActionLabel.text = "Saving image...";
        Chartmaker.main.LoaderPanel.ProgressBar.value = 0;

        Chartmaker.main.LoaderPanel.ProgressLabel.text = "Initializing...";
        yield return new WaitForSeconds(0.5f);
        
        Chartmaker.main.LoaderPanel.ProgressLabel.text = "Saving image...";

        byte[] data = Extension == ".jpg" ? ImageConversion.EncodeToJPG(ImageData) : ImageConversion.EncodeToPNG(ImageData);
        Task task = File.WriteAllBytesAsync(path, data);
        yield return new WaitUntil(() => task.IsCompleted);
        if (!task.IsCompletedSuccessfully) 
        {
            Chartmaker.main.Loader.SetActive(false);
            DialogModal modal = ModalHolder.main.Spawn<DialogModal>();
            modal.SetDialog("Error", task.Exception.Message, new string[] {"Ok"}, _ => {});
            yield break;
        }

        song.Cover.Layers.Add(new () {
            Target = target,
            Texture = ImageData,
        });
        
        Task saveTask = Chartmaker.main.SaveAsync();
        yield return new WaitUntil(() => saveTask.IsCompleted);
        if (!saveTask.IsCompletedSuccessfully) 
        {
            Chartmaker.main.Loader.SetActive(false);
            DialogModal modal = ModalHolder.main.Spawn<DialogModal>();
            modal.SetDialog("Error", saveTask.Exception.Message, new string[] {"Ok"}, _ => {});
            yield break;
        }
        
        try 
        {
            PlayerView.main.UpdateIconFile();
            InspectorPanel.main.IsCoverDirty = false;
        }
        catch (Exception)
        {
            Chartmaker.main.Loader.SetActive(false);
            DialogModal modal = ModalHolder.main.Spawn<DialogModal>();
            modal.SetDialog("Error", saveTask.Exception.Message, new string[] {"Ok"}, _ => {});
            yield break;
        }

        Chartmaker.main.Loader.SetActive(false);
        ImageData = null;
        Close();
    }

    public void StartSaveImage() 
    {
        StartCoroutine(SaveImageRoutine());
    }

    public void UnloadImage() 
    {
        if (!ImageData) return;
        if (Preview.sprite) 
        {
            Sprite spr = Preview.sprite;
            Preview.sprite = null;
            Destroy(spr);
        }
        Destroy(ImageData);
        Preview.gameObject.SetActive(false);
    }
}
