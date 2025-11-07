
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JANOARG.Client.Behaviors.Common;
using JANOARG.Client.Behaviors.SongSelect.List.ListItemUIs;
using JANOARG.Client.Behaviors.SongSelect.Map.MapItems;
using JANOARG.Client.Behaviors.SongSelect.Map.MapItemUIs;
using JANOARG.Client.Data.Playlist;
using JANOARG.Shared.Data.ChartInfo;
using JANOARG.Shared.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace JANOARG.Client.Behaviors.SongSelect.Map
{
    public class MapManager : MonoBehaviour
    {
        public static MapManager sMain;
        public static HashSet<MapItem> sItems = new();
        public static HashSet<MapItemUI> sItemUIs = new();
        public static HashSet<MapProp> sProps = new();

        public static Stack<Playlist> sPlaylistStack = new();

        public static Dictionary<string, SongMapItem> sSongMapItemsByID = new();
        public static Dictionary<string, SongMapItemUI> sSongMapItemUIsByID = new();
        public static Dictionary<string, PlaylistMapItem> sPlaylistMapItemsByID = new();
        public static Dictionary<string, PlaylistMapItemUI> sPlaylistMapItemUIsByID = new();
        public static Dictionary<MapItem, HashSet<MapProp>> sPropsByDependency = new();


        public Scene MapScene;
        [Space]
        public List<MapItemUI> ItemUISamples;
        public RectTransform ItemUIHolder;
        [Space]
        public float DragScale = 20;

        public bool isReady { get; private set; }
        public bool isPointerDown { get; private set; }
        public Vector2 scrollVelocity { get; private set; }

        private Vector2 _LastTouchPos;
        private MapItemUI _ClosestItem;
        private bool _IsPositionDirty;

        private Vector2 _LastCameraPos;
        private Vector2 cameraPos
        {
            get
            {
                return CommonSys.sMain.MainCamera.transform.position;
            }
            set
            {
                if (cameraPos == value) return;
                CommonSys.sMain.MainCamera.transform.position *= new Vector3Frag(x: value.x, y: value.y);
                _IsPositionDirty = true;
            }
        }

        public void Awake()
        {
            sMain = this;
        }
    
        public void Update()
        {
            if (isReady && SongSelectScreen.sMain.IsMapView)
            {
                if (isPointerDown)
                {
                    if (isPointerDown)
                    {
                        scrollVelocity = (cameraPos - _LastCameraPos) / Time.deltaTime;
                        _LastCameraPos = cameraPos;
                    }
                }
                else if (!SongSelectScreen.sMain.IsAnimating)
                {
                    if (scrollVelocity.sqrMagnitude > .1f)
                    {
                        cameraPos += scrollVelocity * Time.deltaTime;
                        scrollVelocity *= Mathf.Pow(0.1f, Time.deltaTime);
                    }

                    // Snap camera to the nearest map item
                    if (_ClosestItem)
                    {
                        Vector2 closestItemPosition = ((RectTransform)_ClosestItem.transform).anchoredPosition;
                        float closestItemDistance = closestItemPosition.magnitude;
                        if (closestItemDistance >= _ClosestItem.parent.SafeCameraDistance)
                        {
                            scrollVelocity *= Mathf.Pow(0.1f, Time.deltaTime);
                            Vector2 moveOffset = closestItemPosition.normalized * (closestItemDistance - _ClosestItem.parent.SafeCameraDistance + 0.01f);
                            cameraPos += moveOffset * ((1 - Mathf.Pow(1e-3f, Time.deltaTime)) / DragScale);
                        }
                    }
                }
            }
        }

        public void LateUpdate()
        {
            if (_IsPositionDirty)
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
            isReady = false;

            if (MapScene.IsValid())
            {
                yield return SceneManager.UnloadSceneAsync(MapScene);
            }

            string sceneName = SongSelectScreen.sMain.Playlist.MapName + " Map";
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            MapScene = SceneManager.GetSceneByName(sceneName);

            yield return null;

            isReady = true;
        }
        public void UnloadMap()
        {
            StartCoroutine(UnloadMapRoutine());
        }

        public void NavigateToMap(PlaylistReference playlist)
        {
            isReady = false;
            StartCoroutine(NavigateToMapAnim(playlist));
        }

        public IEnumerator NavigateToMapAnim(PlaylistReference playlist)
        {
            var targetItem = sPlaylistMapItemsByID.Values.FirstOrDefault(p => p.Target == playlist);

            StartCoroutine(Ease.Animate(0.3f, (t) => {
                float lerp1 = 1 - Ease.Get(t, EaseFunction.Cubic, EaseMode.Out);
                ProfileBar.sMain.SetVisibility(lerp1);
                SongSelectScreen.sMain.LerpActions(lerp1);
            }));
            yield return SongSelectScreen.sMain.LeaveInAnim(targetItem.transform);
            SongSelectScreen.sMain.MapCover.color 
                = CommonSys.sMain.MainCamera.backgroundColor
                = playlist.Playlist.BackgroundColor;

            SongSelectScreen.sMain.ClearPlaylist();
            yield return UnloadMapRoutine();

            sPlaylistStack.Push(playlist.Playlist);
            SongSelectScreen.sMain.Playlist = playlist.Playlist;
            yield return new WaitForSeconds(0.2f);

            yield return SongSelectScreen.sMain.InitPlaylist();
            yield return new WaitForSeconds(0.7f);
            SongSelectScreen.sMain.UpdateButtons();
            StartCoroutine(Ease.Animate(0.3f, (t) => {
                float lerp1 = Ease.Get(t, EaseFunction.Cubic, EaseMode.Out);
                ProfileBar.sMain.SetVisibility(lerp1);
                SongSelectScreen.sMain.LerpActions(lerp1);
            }));
        }

        public void NavigatePreviousMap()
        {
            isReady = false;
            StartCoroutine(NavigatePreviousMapAnim());
        }

        public IEnumerator NavigatePreviousMapAnim()
        {
            StartCoroutine(Ease.Animate(0.3f, (t) => {
                float lerp1 = 1 - Ease.Get(t, EaseFunction.Cubic, EaseMode.Out);
                ProfileBar.sMain.SetVisibility(lerp1);
                SongSelectScreen.sMain.LerpActions(lerp1);
            }));
            yield return SongSelectScreen.sMain.LeaveOutAnim();

            SongSelectScreen.sMain.ClearPlaylist();
            yield return UnloadMapRoutine();

            var lastPlaylist = sPlaylistStack.Pop();
            SongSelectScreen.sMain.Playlist = sPlaylistStack.Peek();
            SongSelectScreen.sMoveBackFrom = () => sPlaylistMapItemsByID.Values.FirstOrDefault(x => x.Target.Playlist == lastPlaylist).transform;
            yield return new WaitForSeconds(0.2f);

            yield return SongSelectScreen.sMain.InitPlaylist();
            yield return new WaitForSeconds(0.7f);
            SongSelectScreen.sMain.UpdateButtons();
            StartCoroutine(Ease.Animate(0.3f, (t) => {
                float lerp1 = Ease.Get(t, EaseFunction.Cubic, EaseMode.Out);
                ProfileBar.sMain.SetVisibility(lerp1);
                SongSelectScreen.sMain.LerpActions(lerp1);
            }));
        }

        public List<(SongMapItemUI, SongSelectListSongUI)> GetMapToListItems(List<SongSelectListSongUI> listItems)
        {
            var listDict = listItems.Where(x => x.Target != null).ToDictionary(x => x.Target.SongID, x => x);

            var keys = listDict.Keys.Intersect(sSongMapItemUIsByID.Keys);

            var rect = ItemUIHolder.rect;
            rect.size += new Vector2(50, 50);
            rect.center = Vector2.zero;

            List<(SongMapItemUI, SongSelectListSongUI)> result = new();
            foreach (string key in keys)
            {
                var songItem = sSongMapItemUIsByID[key];
                var listItem = listDict[key];

                // Do not add if the map item is disabled
                if (!songItem.gameObject.activeSelf) continue;

                // Do not add if the map item is outside the screen
                if (!rect.Contains(((RectTransform)songItem.transform).anchoredPosition)) continue;
                

                // If all passes, add the item to the list
                result.Add((songItem, listItem));
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
            sItemUIs.Add(item);
            return item;
        }
        public TItem MakeItemUI<TItem, TParent>(TParent parent) where TParent : MapItem where TItem : MapItemUI<TParent>
        {
            var item = Instantiate(GetItemUISample<TItem, TParent>(), ItemUIHolder);
            item.SetParent(parent);
            sItemUIs.Add(item);
            return item;
        }

        public void UpdateAllPositions()
        {
            float closestItemDistance = float.PositiveInfinity;
            foreach (var item in sItemUIs)
            {
                if (!item.gameObject.activeSelf) continue;
                item.UpdatePosition();
                float distance = ((RectTransform)item.transform).anchoredPosition.sqrMagnitude
                    / (item.parent.SafeCameraDistance * item.parent.SafeCameraDistance);
                if (distance < closestItemDistance)
                {
                    closestItemDistance = distance;
                    _ClosestItem = item;
                }
            }
            _IsPositionDirty = false;
        }

        public void UpdateAllStatuses()
        {
            foreach (var item in sSongMapItemsByID)
            {
                item.Value.UpdateStatus();
            }
            _IsPositionDirty = false;
        }

        public void UpdateAllProps()
        {
            foreach (var item in sProps)
            {
                item.IsDirty = true;
            }
        }

        public void UpdateProps(MapItem depencency)
        {
            if (sPropsByDependency.ContainsKey(depencency))
            {
                foreach (var item in sPropsByDependency[depencency])
                {
                    item.IsDirty = true;
                }
            }
        }

        IEnumerator UnloadMapRoutine()
        {
            isReady = false;
            if (MapScene.IsValid())
            {
                yield return SceneManager.UnloadSceneAsync(MapScene);
            }
        }




        public void OnMapPointerDown(BaseEventData data)
        {
            PointerEventData pointerData = (PointerEventData)data;
            _LastTouchPos = pointerData.position;
            scrollVelocity = Vector2.zero;
            isPointerDown = true;
        }

        public void OnMapDrag(BaseEventData data)
        {
            if (!isPointerDown) return;
            PointerEventData pointerData = (PointerEventData)data;
            Vector2 delta = (_LastTouchPos - pointerData.position) / Screen.height * DragScale;
            cameraPos += delta;
            _LastTouchPos = pointerData.position;
        }

        public void OnMapPointerUp(BaseEventData data)
        {
            isPointerDown = false;
        }



        public void SelectSong(SongMapItem item)
        {
            isPointerDown = false;
            if (SongSelectScreen.sMain.TargetSongAnim != null) StopCoroutine(SongSelectScreen.sMain.TargetSongAnim);
            SongSelectScreen.sMain.TargetSongAnim = StartCoroutine(SongSelectScreen.sMain.MapTargetSongShowAnim(item));
        }
    }
}