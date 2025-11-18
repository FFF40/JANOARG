

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JANOARG.Client.Behaviors.SongSelect.Shared;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Splines;
using UnityEngine.Splines.ExtrusionShapes;

namespace JANOARG.Client.Behaviors.SongSelect.Map.MapProps
{
    [RequireComponent(typeof(SplineContainer))]
    [RequireComponent(typeof(SplineExtrude))]
    [RequireComponent(typeof(MeshRenderer))]
    [ExecuteInEditMode]
    public class MapItemConnection : MapProp, IHasConditional
    {
        [SerializeField] private MapItem m_from;
        [SerializeField] private MapItem m_to;
        [Space]
        [Range(0, 1)]
        [SerializeField] private float m_alpha = 1;

        public bool isRevealed { get; private set; }
        public bool isUnlocked { get; private set; }

        private SplineContainer splineContainer;
        private SplineExtrude splineExtrude;
        private MeshRenderer meshRenderer;
        private MaterialPropertyBlock rendererProps;

        private bool isRendererDirty = false;
    
        public MapItem From
        {
            get 
            {
                return m_from;
            }
            set 
            {
                m_from = value;
                SetDirty();
            }
        }
        public MapItem To
        {
            get
            {
                return m_to;
            }
            set
            {
                m_to = value;
                SetDirty();
            }
        }
        public float Alpha
        {
            get
            {
                return m_alpha;
            }
            set
            {
                m_alpha = value;
                SetRendererDirty();
            }
        }

        protected void EnsureSplineContainerExists()
        {
            if (!splineContainer) 
            {
                splineContainer = GetComponent<SplineContainer>();
                SetDirty();
            }
            if (!splineExtrude) 
            {
                splineExtrude = GetComponent<SplineExtrude>();
                SetDirty();
            }
            if (!meshRenderer)
            {
                meshRenderer = GetComponent<MeshRenderer>();
                SetRendererDirty();
            }
        }

        protected override void OnDirtyUpdate()
        {
            base.OnDirtyUpdate();
            EnsureSplineContainerExists();

            // Update conditions
            isRevealed = From.isRevealed && To.isRevealed;
            isUnlocked = From.isUnlocked && To.isUnlocked;

            // Update start and end points
            var firstKnot = splineContainer.Spline[0];
            var lastKnot = splineContainer.Spline[^1];
            firstKnot.Position = From.transform.position;
            lastKnot.Position = To.transform.position;
            splineContainer.Spline.SetKnotNoNotify(0, firstKnot);
            splineContainer.Spline[^1] = lastKnot;

            // Update self's name
            gameObject.name = $"{From.gameObject.name} > {To.gameObject.name}";
        }

        protected void SetRendererDirty()
        {
            if (!isRendererDirty)
            {
                IEnumerator ScheduleDirty()
                {
                    yield return null;
                    OnRendererUpdate();   
                }
                StartCoroutine(ScheduleDirty());
                isRendererDirty = true;
            }
        }

        protected virtual void OnRendererUpdate()
        {
            rendererProps ??= new();
            rendererProps.SetFloat("_Alpha", m_alpha * (isRevealed ? (isUnlocked ? 1 : 0.5f) : 0));
            meshRenderer.SetPropertyBlock(rendererProps);
            isRendererDirty = false;
        }

        public override IEnumerable<MapItem> GetDependencies()
        {
            yield return From;
            yield return To;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            EnsureSplineContainerExists();
            Spline.Changed += OnSplineChange;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Spline.Changed -= OnSplineChange;
        }

        private void OnSplineChange(Spline spline, int arg2, SplineModification modification)
        {
            if (spline == splineContainer.Spline)
            {
                SetDirty();
            }
        }
    }
}