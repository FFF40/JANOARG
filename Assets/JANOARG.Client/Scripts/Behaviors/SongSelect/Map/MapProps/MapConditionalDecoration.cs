

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JANOARG.Client.Behaviors.SongSelect.Shared;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Splines;
using UnityEngine.Splines.ExtrusionShapes;

namespace JANOARG.Client.Behaviors.SongSelect.Map.MapProps
{
    public class MapConditionalDecoration : MapProp, IHasGameConditional
    {
        public enum RevealOnType
        {
            Reveal,
            Unlock,
        }

        public enum RevealConditionalType
        {
            All,
            Any,
        }

        [SerializeField] private MapItem[] m_dependencies;
        [SerializeField] private RevealOnType m_revealOn;
        [SerializeField] private RevealConditionalType m_revealConditional;

        public bool isRevealed { get; private set; }
        public bool isUnlocked { get; private set; }

        public void Start()
        {
            SetDirty();
        }
    
        public MapItem[] Dependencies
        {
            get 
            {
                return m_dependencies;
            }
            set 
            {
                m_dependencies = value;
                SetDirty();
            }
        }
        public RevealOnType RevealOn
        {
            get 
            {
                return m_revealOn;
            }
            set 
            {
                m_revealOn = value;
                SetDirty();
            }
        }
        public RevealConditionalType RevealConditional
        {
            get 
            {
                return m_revealConditional;
            }
            set 
            {
                m_revealConditional = value;
                SetDirty();
            }
        }

        protected override void OnDirtyUpdate()
        {
            base.OnDirtyUpdate();
            
            Func<MapItem, bool> revealFunc = RevealOn switch {
                RevealOnType.Reveal => x => isRevealed,
                RevealOnType.Unlock => x => isUnlocked,
                _ => throw new ArgumentException($"Unknown reveal on type {RevealOn}")
            };

            isRevealed = isUnlocked = RevealConditional switch{
                RevealConditionalType.All => Dependencies.All(revealFunc),
                RevealConditionalType.Any => Dependencies.Any(revealFunc),
                _ => throw new ArgumentException($"Unknown reveal conditional type {RevealConditional}")
            };

            Debug.Log(isRevealed);

            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(isRevealed);
            }
        }

        public override IEnumerable<MapItem> GetDependencies()
        {
            return Dependencies;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }
    }
}