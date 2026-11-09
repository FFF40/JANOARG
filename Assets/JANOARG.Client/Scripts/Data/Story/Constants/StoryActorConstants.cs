using System;
using System.Collections.Generic;
using UnityEngine;

namespace JANOARG.Client.Data.Constant
{
    [CreateAssetMenu(fileName = "Story Actor Constants", menuName = "JANOARG/Story/Story Actors")]
    public class StoryActorConstants : ScriptableObject
    {
        public ActorInfo[] ActorList;
        public StoryActorReference[] StoryActors;

        private Dictionary<string, ActorInfo> _ActorLookup;

        public ActorInfo GetActor(string id)
        {
            if (_ActorLookup == null)
                BuildActorList();

            return _ActorLookup.TryGetValue(id, out ActorInfo actor)
                ? actor
                : null;
        }

        public void BuildActorList()
        {
            _ActorLookup = new Dictionary<string, ActorInfo>();
            var visited = new HashSet<StoryActorConstants>();
            TraverseBackgrounds(this, visited);
        }

        private void TraverseBackgrounds(
            StoryActorConstants constants,
            HashSet<StoryActorConstants> visited)
        {
            if (constants == null || !visited.Add(constants))
                return;

            if (constants.ActorList != null)
            {
                foreach (ActorInfo actor in constants.ActorList)
                {
                    if (actor == null ||
                        string.IsNullOrEmpty(actor.Alias))
                        continue;

                    if (!_ActorLookup.TryAdd(
                            actor.Alias, actor))
                    {
                        Debug.LogWarning(
                            $"Duplicate background ID: {actor.Alias}",
                            constants);
                    }
                }
            }

            if (constants.StoryActors == null)
                return;

            foreach (StoryActorReference reference
                    in constants.StoryActors)
            {
                if (reference?.StoryActors == null)
                    continue;

                TraverseBackgrounds(reference.StoryActors, visited);
            }
        }

        public Sprite GetActorSprite(string id, string spriteID)
        {
            ActorInfo actor = GetActor(id);

            if (actor == null)
            {
                Debug.LogWarning($"Actor not found: {id}");
                return null;
            }

            ActorSprite sprite = Array.Find(
                actor.Sprites,
                s => s.ID == spriteID);

            if (sprite == null || sprite.Sprite == null)
            {
                Debug.LogWarning(
                    $"Sprite '{spriteID}' not found for '{id}'");
                return null;
            }

            return sprite.Sprite;
            
        }
    }

    [Serializable]
    public class ActorInfo
    {
        public string Name;         // Displayed name in Storyteller
        public string Alias;        // used in .story
        public string TextPrefix;   // For rich text
        public ActorSprite[] Sprites; 
    }

    [Serializable]
    public class ActorSprite
    {
        public string Name;         
        public string ID;           // used in .story
        public Sprite Sprite;
    }

    [Serializable]
    public class StoryActorReference
    {
        public StoryActorConstants StoryActors;
    }


}