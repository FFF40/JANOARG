using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace JANOARG.Client.Behaviors.Common
{
    /// <summary>
    ///     Low-resolution mode implemented with the URP asset's Render Scale. The pipeline renders the
    ///     camera at a smaller resolution and upscales it; screen-space-overlay UI stays at native
    ///     resolution.
    /// </summary>
    public class LowResolutionMode : MonoBehaviour
    {
        /// <summary>Whether the low-resolution path is currently active.</summary>
        public bool IsActive { get; private set; }

        /// <summary>Current render scale (1 = native, lower = fewer rendered pixels).</summary>
        public float Scale { get; private set; } = 1f;

        int _fullResolutionRequests;

        public static LowResolutionMode Create()
        {
            var go = new GameObject("Low Resolution Mode");
            DontDestroyOnLoad(go);

            return go.AddComponent<LowResolutionMode>();
        }

        public void SetScale(float scale)
        {
            Scale = Mathf.Clamp(scale, 0.5f, 1f);
            Apply();
        }

        public void SetActive(bool active)
        {
            if (IsActive == active)
                return;

            IsActive = active;
            Apply();
        }

        /// <summary>Render at native resolution until the matching pop (previews, screenshots).</summary>
        public void PushFullResolution()
        {
            _fullResolutionRequests++;
            Apply();
        }

        public void PopFullResolution()
        {
            if (_fullResolutionRequests == 0)
                return;

            _fullResolutionRequests--;
            Apply();
        }

        void Apply()
        {
            UniversalRenderPipelineAsset pipeline = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (pipeline == null)
                return;

            float target = IsActive && _fullResolutionRequests == 0 ? Scale : 1f;
            if (!Mathf.Approximately(pipeline.renderScale, target))
                pipeline.renderScale = target;
        }

        void OnDestroy()
        {
            if (!IsActive)
                return;

            IsActive = false;
            Apply();
        }
    }
}
