
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JANOARG.Client.Behaviors.Common;
using JANOARG.Client.Behaviors.SongSelect.List.ListItemUIs;
using JANOARG.Client.Behaviors.SongSelect.Map.MapItems;
using JANOARG.Client.Behaviors.SongSelect.Map.MapItemUIs;
using JANOARG.Shared.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace JANOARG.Client.Behaviors.SongSelect.Map
{
    public class MapManager : MonoBehaviour
    {
        public static MapManager main;
        public static List<MapItem> Items = new();
        public static List<MapItemUI> ItemUIs = new();

        public static Dictionary<string, SongMapItem> SongMapItemsByID = new();
        public static Dictionary<string, SongMapItemUI> SongMapItemUIsByID = new();


        public Scene MapScene;
        [Space]
        public List<MapItemUI> ItemUISamples;
        public RectTransform ItemUIHolder;
        [Space]
        public float DragScale = 20;

        public bool IsReady { get; private set; }
        public bool IsPointerDown { get; private set; }
        public Vector2 ScrollVelocity { get; private set; }

        private Vector2 lastTouchPos;
        private MapItemUI closestItem;
        private bool isPositionDirty;

        Vector2 lastCameraPos;
        Vector2 cameraPos
        {
            get
            {
                return CommonSys.sMain.MainCamera.transform.position;
            }
            set
            {
                if (cameraPos == value) return;
                CommonSys.sMain.MainCamera.transform.position *= new Vector3Frag(x: value.x, y: value.y);
                isPositionDirty = true;
            }
        }

        public void Awake()
        {
            main = this;
        }
    
        public void Update()
        {
            if (IsReady && SongSelectScreen.sMain.IsMapView)
            {
                if (IsPointerDown)
                {
                    if (IsPointerDown)
                    {
                        ScrollVelocity = (cameraPos - lastCameraPos) / Time.deltaTime;
                        lastCameraPos = cameraPos;
                    }
                }
                else if (!SongSelectScreen.sMain.IsAnimating)
                {
                    if (ScrollVelocity.sqrMagnitude > .1f)
                    {
                        cameraPos += ScrollVelocity * Time.deltaTime;
                        ScrollVelocity *= Mathf.Pow(0.1f, Time.deltaTime);
                    }

                    // Snap camera to the nearest map item
                    if (closestItem)
                    {
                        Vector2 closestItemPosition = ((RectTransform)closestItem.transform).anchoredPosition;
                        float closestItemDistance = closestItemPosition.magnitude;
                        if (closestItemDistance >= closestItem.Parent.SafeCameraDistance)
                        {
                            ScrollVelocity *= Mathf.Pow(0.1f, Time.deltaTime);
                            Vector2 moveOffset = closestItemPosition.normalized * (closestItemDistance - closestItem.Parent.SafeCameraDistance + 0.01f);
                            cameraPos += moveOffset * ((1 - Mathf.Pow(1e-3f, Time.deltaTime)) / DragScale);
                        }
                    }
                }
            }
        }

        public void LateUpdate()
        {
            if (isPositionDirty)
            {
                UpdateAllPositions();
            }
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

            string sceneName = SongSelectScreen.sMain.Playlist.MapName + " Map";
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            MapScene = SceneManager.GetSceneByName(sceneName);

            yield return null;

            IsReady = true;
        }
        public void UnloadMap()
        {
            StartCoroutine(UnloadMapRoutine());
        }

        public List<(SongMapItemUI, SongSelectListSongUI)> GetMapToListItems(List<SongSelectListSongUI> listItems)
        {
            var listDict = listItems.Where(x => x.Target != null).ToDictionary(x => x.Target.SongID, x => x);
            var keys = listDict.Keys.Intersect(SongMapItemUIsByID.Keys);
            List<(SongMapItemUI, SongSelectListSongUI)> result = new();
            foreach (string key in keys)
            {
                if (!SongMapItemUIsByID[key].gameObject.activeSelf) continue;
                result.Add((SongMapItemUIsByID[key], listDict[key]));
            }
            return result;
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
            float closestItemDistance = float.PositiveInfinity;
            foreach (var item in ItemUIs)
            {
                if (!item.gameObject.activeSelf) continue;
                item.UpdatePosition();
                float distance = ((RectTransform)item.transform).anchoredPosition.sqrMagnitude
                    / (item.Parent.SafeCameraDistance * item.Parent.SafeCameraDistance);
                if (distance < closestItemDistance)
                {
                    closestItemDistance = distance;
                    closestItem = item;
                }
            }
            isPositionDirty = false;
        }

        public void UpdateAllStatuses()
        {
            foreach (var item in SongMapItemsByID)
            {
                item.Value.UpdateStatus();
            }
            isPositionDirty = false;
        }

        IEnumerator UnloadMapRoutine()
        {
            IsReady = false;
            if (MapScene.IsValid())
            {
                yield return SceneManager.UnloadSceneAsync(MapScene);
            }
        }




        public void OnMapPointerDown(BaseEventData data)
        {
            PointerEventData pointerData = (PointerEventData)data;
            lastTouchPos = pointerData.position;
            ScrollVelocity = Vector2.zero;
            IsPointerDown = true;
        }

        public void OnMapDrag(BaseEventData data)
        {
            if (!IsPointerDown) return;
            PointerEventData pointerData = (PointerEventData)data;
            Vector2 delta = (lastTouchPos - pointerData.position) / Screen.height * DragScale;
            cameraPos += delta;
            lastTouchPos = pointerData.position;
        }

        public void OnMapPointerUp(BaseEventData data)
        {
            IsPointerDown = false;
        }



        public void SelectSong(SongMapItem item)
        {
            IsPointerDown = false;
            if (SongSelectScreen.sMain.TargetSongAnim != null) StopCoroutine(SongSelectScreen.sMain.TargetSongAnim);
            SongSelectScreen.sMain.TargetSongAnim = StartCoroutine(SongSelectScreen.sMain.MapTargetSongShowAnim(item));
        }
    }
}