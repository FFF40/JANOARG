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
            if (CoverInfos.ContainsKey(songID))
            {
                CoverInfos[songID].Uses++;
                if (CoverInfos[songID].Coroutine != null) yield return null;
            }
            else
            {
                IEnumerator f_load()
                {
                    string path = $"Songs/{songID}";
                    path = Path.Combine(path, song.Cover.IconTarget);
                    if (Path.HasExtension(path)) path = Path.ChangeExtension(path, "")[0..^1];
                    ResourceRequest req = Resources.LoadAsync<Texture2D>(path);
                    yield return new WaitUntil(() => req.isDone);
                    if (req.asset)
                    {
                        Texture2D tex = (Texture2D)req.asset;
                        tex.name = songID;
                        CoverInfos[songID].Icon = tex;
                    }
                    CoverInfos[songID].Coroutine = null;
                }
                CoverInfos[songID] = new CoverInfo
                {
                    Uses = 1,
                    Coroutine = StartCoroutine(f_load())
                };
                yield return CoverInfos[songID].Coroutine;
            }
        }
        public IEnumerator RegisterUse(RawImage coverImage, string songID, PlayableSong song)
        {
            UnregisterUse(coverImage);
            coverImage.color = song.Cover.BackgroundColor;
            yield return RegisterUse(songID, song);
            if (CoverInfos.ContainsKey(songID))
            {
                CoverInfos[songID].BackgroundColor = coverImage.color;
                coverImage.texture = CoverInfos[songID].Icon;
                coverImage.color = coverImage.texture ? Color.white : CoverInfos[songID].BackgroundColor;
            }
        }
        public IEnumerator RegisterUse(RawImage coverImage, string songID)
        {
            yield return new WaitUntil(() => CoverInfos.ContainsKey(songID));
            if (CoverInfos[songID].Coroutine != null) yield return new WaitUntil(() => CoverInfos[songID].Coroutine == null);
            if (CoverInfos.ContainsKey(songID))
            {
                coverImage.texture = CoverInfos[songID].Icon;
                coverImage.color = coverImage.texture ? Color.white : CoverInfos[songID].BackgroundColor;
            }
        }

        public void UnregisterUse(string songID)
        {
            if (CoverInfos.ContainsKey(songID))
            {
                CoverInfos[songID].Uses--;
                if (CoverInfos[songID].Uses <= 0)
                {
                    Destroy(CoverInfos[songID].Icon);
                    CoverInfos.Remove(songID);
                }
            }
        }
        public void UnregisterUse(Texture2D texture)
        {
            UnregisterUse(texture.name);
        }
        public void UnregisterUse(RawImage rawImage)
        {
            if (!rawImage.texture) return;
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