using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace JANOARG.Client.Editor
{
    /// <summary>
    ///     One-time setup for the Built-in -> URP migration. Creates the URP asset/renderer,
    ///     assigns it globally and to every quality level, and converts the one built-in
    ///     Standard material used by the game.
    /// </summary>
    public static class URPMigrationSetup
    {
        const string SettingsFolder = "Assets/JANOARG.Client/Settings";
        const string RendererPath   = SettingsFolder + "/JANOARG Universal Renderer.asset";
        const string PipelinePath   = SettingsFolder + "/JANOARG Universal Render Pipeline Asset.asset";
        const string IntroLanePath  = "Assets/JANOARG.Client/Resources/Materials/Intro Lane.mat";

        [MenuItem("Tools/JANOARG/Migrate to URP")]
        public static void Migrate()
        {
            if (!AssetDatabase.IsValidFolder(SettingsFolder))
                AssetDatabase.CreateFolder("Assets/JANOARG.Client", "Settings");

            UniversalRendererData rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
            if (rendererData == null)
            {
                rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
                rendererData.postProcessData = GetDefaultPostProcessData();
                ResourceReloader.ReloadAllNullIn(rendererData, UniversalRenderPipelineAsset.packagePath);
                AssetDatabase.CreateAsset(rendererData, RendererPath);
            }

            UniversalRenderPipelineAsset pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            if (pipeline == null)
            {
                pipeline = UniversalRenderPipelineAsset.Create(rendererData);
                ConfigurePipeline(pipeline);
                AssetDatabase.CreateAsset(pipeline, PipelinePath);
            }
            else
            {
                ConfigurePipeline(pipeline);
                EditorUtility.SetDirty(pipeline);
            }

            AssetDatabase.SaveAssets();

            // Default pipeline + every quality level. The project currently has one asset for all
            // levels; split per-level later if MSAA/shadow tiers diverge.
            GraphicsSettings.defaultRenderPipeline = pipeline;

            int originalQuality = QualitySettings.GetQualityLevel();
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = pipeline;
            }
            QualitySettings.SetQualityLevel(originalQuality, false);

            EnsureGlobalSettings();
            ConvertIntroLaneMaterial();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[URP Migration] Assigned {AssetDatabase.GetAssetPath(pipeline)}");
        }

        static void ConfigurePipeline(UniversalRenderPipelineAsset pipeline)
        {
            // The game is unlit, Gamma, LDR and already manages its own resolution scaling.
            // Keep the URP asset as lean as Built-in was.
            pipeline.supportsHDR               = false;
            pipeline.msaaSampleCount           = 1; // MsaaQuality.Disabled
            pipeline.renderScale               = 1f;
            pipeline.supportsCameraDepthTexture  = false;
            pipeline.supportsCameraOpaqueTexture = false;
            pipeline.shadowDistance            = 0f;

            // These setters are internal in URP, so set the serialized fields directly.
            SerializedObject so = new(pipeline);
            SetBool(so, "m_MainLightShadowsSupported",       false);
            SetBool(so, "m_AdditionalLightShadowsSupported", false);
            SetBool(so, "m_AnyShadowsSupported",             false);
            SetBool(so, "m_SoftShadowsSupported",            false);
            SetBool(so, "m_SupportsLightCookies",            false);
            SetBool(so, "m_SupportsLightLayers",             false);
            SetBool(so, "m_MixedLightingSupported",          false);
            SetBool(so, "m_RequireDepthTexture",             false);
            SetBool(so, "m_RequireOpaqueTexture",            false);
            SetBool(so, "m_SupportsHDR",                     false);
            SetInt(so,   "m_MSAA",                           1);
            SetFloat(so, "m_RenderScale",                    1f);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetBool(SerializedObject so, string name, bool value)
        {
            SerializedProperty property = so.FindProperty(name);
            if (property != null)
                property.boolValue = value;
        }

        static void SetInt(SerializedObject so, string name, int value)
        {
            SerializedProperty property = so.FindProperty(name);
            if (property != null)
                property.intValue = value;
        }

        static void SetFloat(SerializedObject so, string name, float value)
        {
            SerializedProperty property = so.FindProperty(name);
            if (property != null)
                property.floatValue = value;
        }

        static PostProcessData GetDefaultPostProcessData()
        {
            MethodInfo method = typeof(PostProcessData).GetMethod(
                "GetDefaultPostProcessData",
                BindingFlags.Static | BindingFlags.NonPublic);

            return method?.Invoke(null, null) as PostProcessData;
        }

        static void EnsureGlobalSettings()
        {
            System.Type type = typeof(UniversalRenderPipelineAsset).Assembly.GetType(
                "UnityEngine.Rendering.Universal.UniversalRenderPipelineGlobalSettings");

            MethodInfo method = type?.GetMethod(
                "Ensure",
                BindingFlags.Static | BindingFlags.NonPublic);

            method?.Invoke(null, new object[] { true });
        }

        static void ConvertIntroLaneMaterial()
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(IntroLanePath);
            if (material == null)
            {
                Debug.LogWarning($"[URP Migration] Missing material: {IntroLanePath}");
                return;
            }

            Shader lit = Shader.Find("Universal Render Pipeline/Lit");
            if (lit == null)
            {
                Debug.LogError("[URP Migration] Universal Render Pipeline/Lit not found.");
                return;
            }

            // Standard's `_Color`/`_Glossiness` are also declared (obsolete) by URP Lit, so the
            // original values can be read before the shader swap.
            Color baseColor = material.HasProperty("_Color") ? material.GetColor("_Color") : new Color(1, 1, 1, 0.6f);
            float metallic   = material.HasProperty("_Metallic") ? material.GetFloat("_Metallic") : 1f;
            float smoothness = material.HasProperty("_Glossiness") ? material.GetFloat("_Glossiness") : 0f;

            material.shader = lit;

            material.SetColor("_BaseColor", baseColor);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            material.SetFloat("_WorkflowMode", 0f); // Metallic

            // Standard Fade -> URP Lit transparent alpha blend (matches BaseShaderGUI).
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
            material.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.SetFloat("_Cull", (float)CullMode.Back);
            material.SetFloat("_AlphaClip", 0f);
            material.SetFloat("_BlendModePreserveSpecular", 0f);

            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.DisableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.DisableKeyword("_ALPHAMODULATE_ON");
            material.DisableKeyword("_SPECULAR_SETUP");

            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = 2999;

            EditorUtility.SetDirty(material);
            Debug.Log($"[URP Migration] Converted {IntroLanePath} to Universal Render Pipeline/Lit.");
        }
    }
}