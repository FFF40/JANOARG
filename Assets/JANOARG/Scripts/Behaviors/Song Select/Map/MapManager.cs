using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
{
    public static MapManager main;
    public static List<MapItem> Items = new();
    public static List<MapItemUI> ItemUIs = new();


    public Scene MapScene;
    [Space]
    public List<MapItemUI> ItemUISamples;
    public RectTransform ItemUIHolder;


    public bool IsReady { get; private set; }

    public void Awake()
    {
        main = this;
    }



    public void LoadMap()
    {
        StartCoroutine(LoadMapRoutine());
    }

    IEnumerator LoadMapRoutine()
    {
        IsReady = false;

        if (MapScene.IsValid())
        {
            yield return SceneManager.UnloadSceneAsync(MapScene);
        }

        string sceneName = SongSelectScreen.main.Playlist.MapName + " Map";
        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        MapScene = SceneManager.GetSceneByName(sceneName);

        yield return null;

        IsReady = true;
    }
    public void UnloadMap()
    {
        StartCoroutine(UnloadMapRoutine());
    }
    

    public TItem GetItemUISample<TItem>() where TItem : MapItemUI
    {
        foreach (var item in ItemUISamples)
        {
            if (item is TItem type) return type;
        }
        return null;
    }
    public TItem GetItemUISample<TItem, TParent>() where TParent : MapItem where TItem : MapItemUI<TParent>
    {
        foreach (var item in ItemUISamples)
        {
            if (item is TItem type) return type;
        }
        return null;
    }
    public TItem MakeItemUI<TItem>() where TItem : MapItemUI
    {
        var item = Instantiate(GetItemUISample<TItem>(), ItemUIHolder);
        ItemUIs.Add(item);
        return item;
    }
    public TItem MakeItemUI<TItem, TParent>(TParent parent) where TParent : MapItem where TItem : MapItemUI<TParent>
    {
        var item = Instantiate(GetItemUISample<TItem, TParent>(), ItemUIHolder);
        item.SetParent(parent);
        ItemUIs.Add(item);
        return item;
    }

    public void UpdateAllPositions()
    {
        foreach (var item in ItemUIs)
        {
            item.UpdatePosition();
        }
    }


    IEnumerator UnloadMapRoutine()
    {
        IsReady = false;
        if (MapScene.IsValid())
        {
            yield return SceneManager.UnloadSceneAsync(MapScene);
        }
    }
}