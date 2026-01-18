using System.Collections;
using System.Collections.Generic;
using System.IO;
using JANOARG.Shared.Data.ChartInfo;
using UnityEngine;
using UnityEngine.UI;

namespace JANOARG.Client.Behaviors.SongSelect
{
    public class SongSelectCoverManager : MonoBehaviour
    {
        public static SongSelectCoverManager sMain;

        public Dictionary<string, CoverInfo> CoverInfos = new();

        public void Awake()
        {
            sMain = this;
        }

        public IEnumerator RegisterUse(string songID, PlayableSong song)
        {
            string textureID = "SONG:" + songID;
            if (CoverInfos.ContainsKey(textureID))
            {
                CoverInfos[textureID].Uses++;
                if (CoverInfos[textureID].Coroutine != null)
                    yield return null;
                else 
                    yield return CoverInfos[textureID].Coroutine;
            }
            else
            {
                IEnumerator f_load()
                {
                    string path = $"Songs/{songID}";
                    path = Path.Combine(path, song.Cover.IconTarget);
                    if (Path.HasExtension(path)) 
                        path = Path.ChangeExtension(path, "")[0..^1];
                    ResourceRequest req = Resources.LoadAsync<Texture2D>(path);
                    yield return new WaitUntil(() => req.isDone);
                    if (req.asset)
                    {
                        Texture2D tex = (Texture2D)req.asset;
                        tex.name = textureID;
                        CoverInfos[textureID].Icon = tex;
                    }
                    CoverInfos[textureID].Coroutine = null;
                }
                CoverInfos[textureID] = new CoverInfo
                {
                    Uses = 1,
                    Coroutine = StartCoroutine(f_load())
                };
                yield return CoverInfos[textureID].Coroutine;
            }
        }
        public IEnumerator RegisterUse(RawImage coverImage, string songID, PlayableSong song)
        {
            UnregisterUse(coverImage);
            coverImage.color = song.Cover.BackgroundColor;
            yield return RegisterUse(songID, song);
            string textureID = "SONG:" + songID;
            if (CoverInfos.ContainsKey(textureID))
            {
                CoverInfos[textureID].BackgroundColor = coverImage.color;
                coverImage.texture = CoverInfos[textureID].Icon;
                coverImage.color = coverImage.texture ? Color.white : CoverInfos[textureID].BackgroundColor;
            }
        }
        public IEnumerator RegisterUseSong(RawImage coverImage, string songID)
        {
            yield return new WaitUntil(() => SongSelectScreen.sMain?.PlayableSongByID.ContainsKey(songID) == true);
            yield return RegisterUse(coverImage, songID, SongSelectScreen.sMain.PlayableSongByID[songID]);
        }

        public void UnregisterUse(string textureID)
        {
            if (CoverInfos.ContainsKey(textureID))
            {
                CoverInfos[textureID].Uses--;
                if (CoverInfos[textureID].Uses <= 0)
                {
                    Resources.UnloadAsset(CoverInfos[textureID].Icon);
                    CoverInfos.Remove(textureID);
                }
            }
        }
        public void UnregisterUseSong(string songID)
        {
            UnregisterUse("SONG:" + songID);
        }
        public void UnregisterUse(Texture2D texture)
        {
            UnregisterUse(texture.name);
        }
        public void UnregisterUse(RawImage rawImage)
        {
            if (!rawImage.texture) 
                return;
            string name = rawImage.texture.name;
            rawImage.texture = null;
            UnregisterUse(name);
        }

        public class CoverInfo
        {
            public Coroutine Coroutine;
            public Texture2D Icon;
            public Color BackgroundColor;
            public int Uses;
        }
    }
}