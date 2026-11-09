using System;
using System.Collections.Generic;
using UnityEngine;

namespace JANOARG.Client.Data.Constant
{
    // Main Audio Constants List that can be nested like how Playlist works
    // so there will not be a long list of audio
    [CreateAssetMenu(fileName = "Story Audio Constants", menuName = "JANOARG/Story/Story Audio Constants")]
    public class StoryAudioConstants : ScriptableObject
    {
        public Music[] MusicList;
        public SoundEffects[] SFXList;
        public StoryAudioReference[] StoryAudios;

        private Dictionary<string, AudioClip> _MusicLookup;
        private Dictionary<string, AudioClip> _SoundLookup;

        #region Music Lookup
        public AudioClip GetMusic(string id)
        {
            if (_MusicLookup == null)
                BuildList();

            return _MusicLookup.TryGetValue(id, out AudioClip clip)
                ? clip
                : null;
        }

        public AudioClip GetSound(string id)
        {
            if (_SoundLookup == null)
                BuildList();

            return _SoundLookup.TryGetValue(id, out AudioClip clip)
                ? clip
                : null;
        }

        public void BuildList()
        {
            _MusicLookup = new Dictionary<string, AudioClip>();
            _SoundLookup = new Dictionary<string, AudioClip>();
            var visited = new HashSet<StoryAudioConstants>();
            TraverseBackgrounds(this, visited);
        }

        private void TraverseBackgrounds(
            StoryAudioConstants constants,
            HashSet<StoryAudioConstants> visited)
        {
            if (constants == null || !visited.Add(constants))
                return;

            if (constants.MusicList != null)
            {
                foreach (Music music in constants.MusicList)
                {
                    if (music == null || string.IsNullOrEmpty(music.ID))
                        continue;

                    if (!_MusicLookup.TryAdd(music.ID, music.MusicClip))
                    {
                        Debug.LogWarning(
                            $"Duplicate music ID: {music.ID}",
                            constants);
                    }
                }
            }

            if (constants.SFXList != null)
            {
                foreach (SoundEffects sound in constants.SFXList)
                {
                    if (sound == null || string.IsNullOrEmpty(sound.ID))
                        continue;

                    if (!_SoundLookup.TryAdd(sound.ID, sound.SoundEffect))
                    {
                        Debug.LogWarning(
                            $"Duplicate sound ID: {sound.ID}",
                            constants);
                    }
                }
            }
            
            if (constants.MusicList == null && constants.SFXList == null)
                return;

            foreach (StoryAudioReference reference
                    in constants.StoryAudios)
            {
                if (reference?.StoryAudio == null)
                    continue;

                TraverseBackgrounds(reference.StoryAudio, visited);
            }
        }
    
        #endregion
    }

    [Serializable]
    public class Music
    {
        public string ID;
        public AudioClip MusicClip;
    }

    [Serializable]
    public class SoundEffects
    {
        public string ID;
        public AudioClip SoundEffect;
    }

    [Serializable]
    public class StoryAudioReference
    {
        public StoryAudioConstants StoryAudio;
    }


}