using System;
using System.Collections.Generic;
using UnityEngine;

namespace JANOARG.Client.Data.Constant
{
    [CreateAssetMenu(fileName = "Story Background Constants", menuName = "JANOARG/Story/Story Background Constants")]
    public class StoryBackgroundConstants : ScriptableObject
    {
        public BackgroundInfo[] BackgroundList;
        public StoryBackgroundReference[] StoryBackgrounds;

        private Dictionary<string, Sprite> _BackgroundLookup;

        public Sprite GetBackground(string id)
        {
            if (_BackgroundLookup == null)
                BuildBackgroundList();

            return _BackgroundLookup.TryGetValue(id, out Sprite sprite)
                ? sprite
                : null;
        }

        public void BuildBackgroundList()
        {
            _BackgroundLookup = new Dictionary<string, Sprite>();
            var visited = new HashSet<StoryBackgroundConstants>();
            TraverseBackgrounds(this, visited);
        }

        private void TraverseBackgrounds(
            StoryBackgroundConstants constants,
            HashSet<StoryBackgroundConstants> visited)
        {
            if (constants == null || !visited.Add(constants))
                return;

            if (constants.BackgroundList != null)
            {
                foreach (BackgroundInfo background in constants.BackgroundList)
                {
                    if (background == null ||
                        string.IsNullOrEmpty(background.ID))
                        continue;

                    if (!_BackgroundLookup.TryAdd(
                            background.ID, background.Sprite))
                    {
                        Debug.LogWarning(
                            $"Duplicate background ID: {background.ID}",
                            constants);
                    }
                }
            }

            if (constants.StoryBackgrounds == null)
                return;

            foreach (StoryBackgroundReference reference
                    in constants.StoryBackgrounds)
            {
                if (reference?.StoryBackground == null)
                    continue;

                TraverseBackgrounds(reference.StoryBackground, visited);
            }
        }
    }

    [Serializable]
    public class BackgroundInfo
    {
        public string ID;
        public Sprite Sprite;
    }

    [Serializable]
    public class StoryBackgroundReference
    {
        public StoryBackgroundConstants StoryBackground;
    }


}