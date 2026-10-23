using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;

namespace DungeonMaster
{
    public static class PrimitiveKit
    {
        struct Key
        {
            public int rgba;
            public byte metallic;
            public byte smooth;
            public byte emit;
            public byte transparent;
        }

        static readonly Dictionary<Key, Material> cache = new Dictionary<Key, Material>();
        static Shader litShader;
        static bool lookedUp;

        public static Shader LitShader()
        {
            if (lookedUp && litShader != null) return litShader;
            lookedUp = true;
            bool urp = GraphicsSettings.currentRenderPipeline != null;
            if (urp)
                litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null)
                litShader = Shader.Find("Standard");
            if (litShader == null)
                litShader = Shader.Find("Diffuse");
            if (litShader == null)
                litShader = Shader.Find("Hidden/InternalErrorShader");
            return litShader;
        }

        public static Material Mat(Color color, float metallic = 0f, float smooth = 0.35f, bool emit = false, float emitScale = 1f, bool transparent = false)
        {
            Color32 c32 = color;
            int packed = (c32.r << 24) | (c32.g << 16) | (c32.b << 8) | c32.a;
            var key = new Key
            {
                rgba = packed,
                metallic = (byte)Mathf.Clamp(Mathf.RoundToInt(metallic * 100f), 0, 255),
                smooth = (byte)Mathf.Clamp(Mathf.RoundToInt(smooth * 100f), 0, 255),
                emit = emit ? (byte)Mathf.Clamp(Mathf.RoundToInt(emitScale * 10f), 1, 255) : (byte)0,
                transparent = transparent ? (byte)1 : (byte)0
            };
            Material mat;
            if (cache.TryGetValue(key, out mat) && mat != null) return mat;

            mat = new Material(LitShader());
            mat.name = "DM_" + color;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            mat.color = color;
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smooth);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smooth);

            if (emit)
            {
                Color emission = color * emitScale;
                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emission);
                mat.EnableKeyword("_EMISSION");
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }

            if (transparent)
            {
                if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f);
                if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", 0f);
                if (mat.HasProperty("_Mode")) mat.SetFloat("_Mode", 3f);
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }

            cache[key] = mat;
            return mat;
        }

        public static GameObject Make(PrimitiveType type, string name, Transform parent, Vector3 localPos, Vector3 localScale, Material mat, bool collider = true)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = localScale;
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null && mat != null) renderer.sharedMaterial = mat;
            if (!collider)
            {
                var col = go.GetComponent<Collider>();
                if (col != null) Object.Destroy(col);
            }
            return go;
        }

        public static GameObject Make(PrimitiveType type, string name, Transform parent, Vector3 localPos, Quaternion localRot, Vector3 localScale, Material mat, bool collider = true)
        {
            var go = Make(type, name, parent, localPos, localScale, mat, collider);
            go.transform.localRotation = localRot;
            return go;
        }

        public static Light AddLight(Transform parent, Vector3 localPos, Color color, float intensity, float range, LightType type = LightType.Point)
        {
            var go = new GameObject(type + " Light");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            var light = go.AddComponent<Light>();
            light.type = type;
            light.color = color;
            TrySetUnit(light, type == LightType.Directional ? "Lux" : "Candela");
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;
            return light;
        }

        static void TrySetUnit(Light light, string unitName)
        {
            var prop = typeof(Light).GetProperty("lightUnit");
            if (prop == null) return;
            try
            {
                prop.SetValue(light, Enum.Parse(prop.PropertyType, unitName), null);
            }
            catch (Exception)
            {
                // Older editors ignore light units and use the raw intensity.
            }
        }

        public static ParticleSystem Embers(Transform parent, Color color, float rate, float size, float lifetime, float speed, float radius)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) return null;

            var go = new GameObject("Embers");
            go.transform.SetParent(parent, false);
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = color;
            main.startSize = size;
            main.startLifetime = lifetime;
            main.startSpeed = speed;
            main.maxParticles = 64;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = true;
            var emission = ps.emission;
            emission.rateOverTime = rate;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = radius;
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            var mat = new Material(shader);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            renderer.material = mat;
            return ps;
        }

        public static Font BuiltinFont()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return font;
        }
    }
}
