using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DungeonMaster.EditorTools
{
    [InitializeOnLoad]
    public static class UrpProjectSetup
    {
        const string Folder = "Assets/Settings";
        const string RendererPath = "Assets/Settings/ForwardRenderer.asset";
        const string PipelinePath = "Assets/Settings/URP_Asset.asset";

        static UrpProjectSetup()
        {
            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode) return;
                EnsurePipeline();
                OpenMainIfUntitled();
            };
        }

        [MenuItem("Dungeon Master/Setup URP")]
        public static void EnsurePipeline()
        {
            try
            {
                if (!AssetDatabase.IsValidFolder(Folder))
                    AssetDatabase.CreateFolder("Assets", "Settings");

                var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
                if (pipeline == null)
                {
                    var renderer = CreateRenderer();
                    pipeline = UniversalRenderPipelineAsset.Create(renderer);
                    if (AssetDatabase.LoadAssetAtPath<Object>(PipelinePath) != null)
                        AssetDatabase.DeleteAsset(PipelinePath);
                    AssetDatabase.CreateAsset(pipeline, PipelinePath);
                    Tune(pipeline);
                    AssetDatabase.SaveAssets();
                }

                GraphicsSettings.defaultRenderPipeline = pipeline;
                int current = QualitySettings.GetQualityLevel();
                for (int i = 0; i < QualitySettings.names.Length; i++)
                {
                    QualitySettings.SetQualityLevel(i, false);
                    QualitySettings.renderPipeline = pipeline;
                }
                QualitySettings.SetQualityLevel(Mathf.Clamp(current, 0, QualitySettings.names.Length - 1), true);
                Debug.Log("Dungeon Master: Universal Render Pipeline is assigned.");
            }
            catch (Exception ex)
            {
                Debug.LogError("Dungeon Master could not create the URP asset. Use Assets > Create > Rendering > URP Asset, assign it in Graphics, then press Play.\n" + ex.Message);
            }
        }

        static ScriptableRendererData CreateRenderer()
        {
            var existing = AssetDatabase.LoadAssetAtPath<ScriptableRendererData>(RendererPath);
            if (existing != null) return existing;

            var flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var method = typeof(UniversalRenderPipelineAsset).GetMethod("CreateRendererAsset", flags);
            if (method == null) throw new MissingMethodException("UniversalRenderPipelineAsset.CreateRendererAsset");
            var parameters = method.GetParameters();
            var args = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                var p = parameters[i];
                if (i == 0 && p.ParameterType == typeof(string)) args[i] = RendererPath;
                else if (p.ParameterType == typeof(string)) args[i] = "Renderer";
                else if (p.ParameterType == typeof(bool)) args[i] = false;
                else if (p.ParameterType.IsEnum) args[i] = Enum.Parse(p.ParameterType, "UniversalRenderer");
                else if (p.HasDefaultValue) args[i] = p.DefaultValue;
            }
            var created = method.Invoke(null, args) as ScriptableRendererData;
            if (created == null) throw new InvalidOperationException("Renderer asset was not created.");
            return created;
        }

        static void Tune(UniversalRenderPipelineAsset pipeline)
        {
            var so = new SerializedObject(pipeline);
            SetFloat(so, "m_ShadowDistance", 80f);
            SetInt(so, "m_AdditionalLightsPerObjectLimit", 6);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetFloat(SerializedObject so, string name, float value)
        {
            var prop = so.FindProperty(name);
            if (prop != null) prop.floatValue = value;
        }

        static void SetInt(SerializedObject so, string name, int value)
        {
            var prop = so.FindProperty(name);
            if (prop != null) prop.intValue = value;
        }

        static void OpenMainIfUntitled()
        {
            if (SessionState.GetBool("DM_OpenedMain", false)) return;
            var scene = EditorSceneManager.GetActiveScene();
            if (!string.IsNullOrEmpty(scene.path)) return;
            if (!File.Exists("Assets/Scenes/Main.unity")) return;
            SessionState.SetBool("DM_OpenedMain", true);
            EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
        }
    }
}
