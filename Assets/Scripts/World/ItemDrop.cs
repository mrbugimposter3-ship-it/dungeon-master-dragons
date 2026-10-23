using UnityEngine;

namespace DungeonMaster
{
    public static class ItemDrop
    {
        public static WorldInteract Spawn(ItemId id, Vector3 worldPos, Transform parent)
        {
            var def = ItemCatalog.Get(id);
            var go = new GameObject(def.displayName);
            if (parent != null) go.transform.SetParent(parent, true);
            go.transform.position = worldPos;

            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            BuildVisual(id, visual.transform);
            visual.AddComponent<Bob>();

            var interact = go.AddComponent<WorldInteract>();
            interact.item = id;
            interact.kind = id == ItemId.Torch ? InteractKind.Torch : InteractKind.Pickup;
            interact.promptColor = def.color;
            interact.prompt = id == ItemId.Torch
                ? "Light the torch  [E]"
                : "Take " + def.displayName + "  [E]";
            return interact;
        }

        public static void BuildVisual(ItemId id, Transform root)
        {
            var def = ItemCatalog.Get(id);
            var mat = PrimitiveKit.Mat(def.color, 0.35f, 0.45f, id == ItemId.Torch || id == ItemId.IceWhip || id == ItemId.AntiFireCape, 1.6f);
            switch (id)
            {
                case ItemId.Torch:
                    PrimitiveKit.Make(PrimitiveType.Cylinder, "Stick", root, new Vector3(0, 0.25f, 0), new Vector3(0.06f, 0.28f, 0.06f), PrimitiveKit.Mat(new Color(0.35f, 0.2f, 0.1f)), false);
                    PrimitiveKit.Make(PrimitiveType.Sphere, "Flame", root, new Vector3(0, 0.62f, 0), Vector3.one * 0.14f, PrimitiveKit.Mat(new Color(1f, 0.5f, 0.12f), 0f, 0.4f, true, 3f), false);
                    break;
                case ItemId.ClothArmour:
                    PrimitiveKit.Make(PrimitiveType.Cube, "Fold", root, new Vector3(0, 0.15f, 0), new Vector3(0.55f, 0.18f, 0.4f), mat, false);
                    break;
                case ItemId.MithrilAxe:
                    PrimitiveKit.Make(PrimitiveType.Cylinder, "Haft", root, new Vector3(0, 0.35f, 0), new Vector3(0.05f, 0.35f, 0.05f), PrimitiveKit.Mat(new Color(0.3f, 0.18f, 0.1f)), false);
                    PrimitiveKit.Make(PrimitiveType.Cube, "Head", root, new Vector3(0.12f, 0.7f, 0), new Vector3(0.28f, 0.16f, 0.08f), PrimitiveKit.Mat(def.color, 0.8f, 0.7f, true, 0.6f), false);
                    break;
                case ItemId.IronSword:
                    PrimitiveKit.Make(PrimitiveType.Cube, "Blade", root, new Vector3(0, 0.55f, 0), new Vector3(0.06f, 0.55f, 0.14f), PrimitiveKit.Mat(def.color, 0.85f, 0.7f), false);
                    PrimitiveKit.Make(PrimitiveType.Cylinder, "Grip", root, new Vector3(0, 0.12f, 0), new Vector3(0.05f, 0.1f, 0.05f), PrimitiveKit.Mat(new Color(0.25f, 0.14f, 0.08f)), false);
                    break;
                case ItemId.IceWhip:
                    for (int i = 0; i < 6; i++)
                    {
                        float t = i / 5f;
                        PrimitiveKit.Make(PrimitiveType.Sphere, "Link", root, new Vector3(t * 0.15f, 0.45f - t * 0.4f, t * 0.2f), Vector3.one * 0.08f, mat, false);
                    }
                    break;
                case ItemId.SewerKey:
                case ItemId.ChestKey:
                    PrimitiveKit.Make(PrimitiveType.Cylinder, "Shaft", root, new Vector3(0, 0.22f, 0), new Vector3(0.04f, 0.18f, 0.04f), mat, false);
                    PrimitiveKit.Make(PrimitiveType.Cube, "Tooth", root, new Vector3(0.06f, 0.08f, 0), new Vector3(0.1f, 0.05f, 0.04f), mat, false);
                    PrimitiveKit.Make(PrimitiveType.Sphere, "Bow", root, new Vector3(0, 0.42f, 0), new Vector3(0.12f, 0.12f, 0.04f), mat, false);
                    break;
                case ItemId.Kiteshield:
                    PrimitiveKit.Make(PrimitiveType.Cube, "Shield", root, new Vector3(0, 0.4f, 0), new Vector3(0.08f, 0.7f, 0.5f), PrimitiveKit.Mat(def.color, 0.45f, 0.55f), false);
                    break;
                case ItemId.PoisonCheese:
                    PrimitiveKit.Make(PrimitiveType.Cube, "Wedge", root, new Vector3(0, 0.12f, 0), new Vector3(0.28f, 0.16f, 0.22f), mat, false);
                    break;
                case ItemId.BucketOfWater:
                    PrimitiveKit.Make(PrimitiveType.Cylinder, "Bucket", root, new Vector3(0, 0.16f, 0), new Vector3(0.18f, 0.14f, 0.18f), mat, false);
                    PrimitiveKit.Make(PrimitiveType.Cylinder, "Handle", root, new Vector3(0, 0.36f, 0), new Vector3(0.04f, 0.1f, 0.04f), mat, false);
                    break;
                case ItemId.AntiFireCape:
                    PrimitiveKit.Make(PrimitiveType.Cube, "Cape", root, new Vector3(0, 0.45f, 0), new Vector3(0.45f, 0.7f, 0.06f), PrimitiveKit.Mat(def.color, 0.15f, 0.4f, true, 0.5f), false);
                    break;
            }
        }
    }

    public class ChestLid : MonoBehaviour
    {
        Quaternion shut;
        bool ready;

        void Awake()
        {
            shut = transform.localRotation;
            ready = true;
        }

        public void Open()
        {
            if (!ready) shut = transform.localRotation;
            transform.localRotation = shut * Quaternion.Euler(-75f, 0f, 0f);
        }
    }

    public class Brazier : MonoBehaviour
    {
        public bool doused;

        public void Douse()
        {
            doused = true;
            var lights = GetComponentsInChildren<Light>();
            for (int i = 0; i < lights.Length; i++) lights[i].enabled = false;
            var ps = GetComponentsInChildren<ParticleSystem>();
            for (int i = 0; i < ps.Length; i++) ps[i].Stop(true, ParticleSystemStopBehavior.StopEmitting);
            var renderers = GetComponentsInChildren<Renderer>();
            var cold = PrimitiveKit.Mat(new Color(0.2f, 0.18f, 0.16f), 0.1f, 0.3f);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i].gameObject.name.Contains("Flame") || renderers[i].gameObject.name.Contains("Coal"))
                    renderers[i].sharedMaterial = cold;
            }
        }
    }
}
