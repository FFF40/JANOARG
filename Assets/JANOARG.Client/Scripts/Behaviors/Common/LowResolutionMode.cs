using UnityEngine;

namespace JANOARG.Client.Behaviors.Common
{
    /// <summary>
    ///     Universal low-resolution mode: renders the game's camera into a scaled RenderTexture and blits
    ///     it back to the screen, so the 3D scene costs fewer pixels while screen-space-overlay UI stays
    ///     at native resolution.
    /// </summary>
    /// <remarks>
    ///     The source camera is deliberately kept as the "logical" camera rather than rendering through a
    ///     scaled target: code positions HUD elements and hitboxes with
    ///     <see cref="Camera.WorldToScreenPoint" /> / <see cref="Camera.ScreenToWorldPoint" />, which would
    ///     return low-resolution coordinates if the target texture were attached to it. Instead its culling
    ///     mask is cleared (so it draws nothing) and a separate render camera mirrors its transform and
    ///     projection into the texture. Screen-space-overlay canvases render after every camera, so they
    ///     stay sharp automatically.
    /// </remarks>
    public class LowResolutionMode : MonoBehaviour
    {
        Camera        _source;
        Camera        _renderCamera;
        Camera        _blitCamera;
        RenderTexture _texture;

        CameraClearFlags _savedClearFlags;
        int              _savedCullingMask;

        /// <summary>Filtering used when the low-resolution texture is upscaled to the screen.</summary>
        public FilterMode UpscaleFilter = FilterMode.Bilinear;

        /// <summary>Whether the low-resolution path is currently rendering.</summary>
        public bool IsActive { get; private set; }

        /// <summary>Current scale (1 = native, lower = fewer rendered pixels).</summary>
        public float Scale { get; private set; } = 1f;

        /// <summary>The texture the 3D scene is rendered into; null while inactive.</summary>
        public RenderTexture Texture => _texture;

        public static LowResolutionMode Create(Camera source)
        {
            var renderGo = new GameObject("Low Resolution Render Camera");
            Object.DontDestroyOnLoad(renderGo);

            var renderCamera = renderGo.AddComponent<Camera>();
            renderCamera.enabled = false;

            var mode = renderGo.AddComponent<LowResolutionMode>();
            mode.Init(source, renderCamera);

            return mode;
        }

        void Init(Camera source, Camera renderCamera)
        {
            _source       = source;
            _renderCamera = renderCamera;

            var blitGo = new GameObject("Low Resolution Upscaler");
            Object.DontDestroyOnLoad(blitGo);

            _blitCamera = blitGo.AddComponent<Camera>();
            _blitCamera.enabled            = false;
            _blitCamera.cullingMask        = 0;
            _blitCamera.clearFlags         = CameraClearFlags.SolidColor;
            _blitCamera.backgroundColor    = Color.black;
            _blitCamera.depth              = 10000f;
            _blitCamera.useOcclusionCulling = false;
            _blitCamera.allowHDR           = false;
            _blitCamera.allowMSAA          = false;
            _blitCamera.orthographic       = true;

            // Low-resolution mode is exactly where HDR/MSAA cost the most; keep the offscreen
            // scene pass cheap and let the upscale blur stand in for AA.
            _renderCamera.allowHDR  = false;
            _renderCamera.allowMSAA = false;

            var upscaler = blitGo.AddComponent<LowResUpscaler>();
            upscaler.Mode = this;
        }

        public void SetScale(float scale)
        {
            Scale = Mathf.Clamp(scale, 0.25f, 1f);

            if (IsActive)
                EnsureTexture();
        }

        public void SetActive(bool active)
        {
            if (IsActive == active)
                return;

            IsActive = active;

            if (active)
            {
                _savedCullingMask = _source.cullingMask;
                _savedClearFlags  = _source.clearFlags;

                // The logical camera must keep its transform/projection for WorldToScreenPoint, but it
                // must not actually draw the scene at native resolution any more.
                _source.cullingMask = 0;
                _source.clearFlags  = CameraClearFlags.Nothing;

                EnsureTexture();

                _renderCamera.enabled = true;
                _blitCamera.enabled   = true;
            }
            else
            {
                _renderCamera.enabled = false;
                _blitCamera.enabled   = false;

                _source.cullingMask = _savedCullingMask;
                _source.clearFlags  = _savedClearFlags;

                _renderCamera.targetTexture = null;
                ReleaseTexture();
            }
        }

        void OnPreRender()
        {
            if (!IsActive || _source == null)
                return;

            EnsureTexture();
            CopySourceToRenderCamera();
        }

        void CopySourceToRenderCamera()
        {
            Transform sourceTransform = _source.transform;
            _renderCamera.transform.SetPositionAndRotation(sourceTransform.position, sourceTransform.rotation);

            _renderCamera.orthographic        = _source.orthographic;
            _renderCamera.orthographicSize    = _source.orthographicSize;
            _renderCamera.fieldOfView         = _source.fieldOfView;
            _renderCamera.nearClipPlane       = _source.nearClipPlane;
            _renderCamera.farClipPlane        = _source.farClipPlane;
            _renderCamera.cullingMask         = _savedCullingMask;
            _renderCamera.clearFlags          = _savedClearFlags;
            _renderCamera.backgroundColor     = _source.backgroundColor;
            _renderCamera.renderingPath       = _source.renderingPath;
            _renderCamera.targetDisplay       = _source.targetDisplay;
            _renderCamera.useOcclusionCulling = _source.useOcclusionCulling;

            // The texture contains the screen, so the camera covers the whole of it.
            _renderCamera.rect = new Rect(0, 0, 1, 1);
        }

        void EnsureTexture()
        {
            int width  = Mathf.Max(1, Mathf.RoundToInt(Screen.width * Scale));
            int height = Mathf.Max(1, Mathf.RoundToInt(Screen.height * Scale));

            if (_texture != null && _texture.width == width && _texture.height == height)
            {
                // Let the player switch filtering without forcing a full recreation.
                if (_texture.filterMode != UpscaleFilter)
                    _texture.filterMode = UpscaleFilter;

                return;
            }

            _renderCamera.targetTexture = null;
            ReleaseTexture();

            _texture = new RenderTexture(width, height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default)
            {
                name             = "Low Resolution",
                filterMode       = UpscaleFilter,
                wrapMode         = TextureWrapMode.Clamp,
                antiAliasing     = 1,
                useMipMap        = false,
                autoGenerateMips = false,
            };

            _texture.Create();
            _renderCamera.targetTexture = _texture;
        }

        void ReleaseTexture()
        {
            if (_texture == null)
                return;

            _texture.Release();
            Destroy(_texture);
            _texture = null;
        }

        internal void Upscale(RenderTexture destination)
        {
            if (_texture != null)
                Graphics.Blit(_texture, destination);
        }

        void OnDestroy()
        {
            // If the owner tears us down mid-session, put the source camera back the way it was so
            // the screen does not stay permanently unrendered.
            if (IsActive && _source != null)
            {
                _source.cullingMask = _savedCullingMask;
                _source.clearFlags  = _savedClearFlags;
            }

            ReleaseTexture();

            if (_blitCamera != null)
                Destroy(_blitCamera.gameObject);
        }
    }

    /// <summary>Presents the low-resolution texture to the backbuffer from its dedicated camera.</summary>
    [RequireComponent(typeof(Camera))]
    internal sealed class LowResUpscaler : MonoBehaviour
    {
        public LowResolutionMode Mode;

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (Mode != null && Mode.IsActive && Mode.Texture != null)
                Mode.Upscale(destination);
            else
                Graphics.Blit(source, destination);
        }
    }
}
