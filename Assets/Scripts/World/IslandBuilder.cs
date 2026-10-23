using UnityEngine;
using UnityEngine.UI;

namespace DungeonMaster
{
    public class WorldRefs : MonoBehaviour
    {
        public Transform dropAnchor;
        public Transform gateLeft;
        public Transform gateRight;
        public Transform volcano;
        public Transform chestPoint;
        public DragonAvatar dragon;
        public Light lairLight;
        public ExplorerController player;
        public ChestLid chestLid;
    }

    public class IslandBuilder : MonoBehaviour
    {
        public WorldRefs refs;

        static readonly Color Bone = new Color(0.93f, 0.86f, 0.72f);
        static readonly Color Ember = new Color(1f, 0.55f, 0.25f);
        static readonly Color Poison = new Color(0.7f, 0.9f, 0.35f);
        static readonly Color Sea = new Color(0.45f, 0.85f, 0.75f);
        static readonly Color Danger = new Color(1f, 0.4f, 0.25f);

        public void Build()
        {
            refs = gameObject.AddComponent<WorldRefs>();
            Ground();
            Water();
            Paths();
            Coast();
            Forest();
            Volcano();
            Mine();
            Farms();
            Keep();
            Guild();
            BasiliskArea();
            Lava();
            DropRoom();
            Raft();
            DragonGate();
            Spawn();
        }

        void Ground()
        {
            var rock = PrimitiveKit.Mat(new Color(0.16f, 0.1f, 0.08f), 0.04f, 0.18f);
            var slab = PrimitiveKit.Make(PrimitiveType.Cube, "Island", transform, new Vector3(0f, -2f, 0f), new Vector3(250f, 4f, 250f), rock, true);
            slab.isStatic = true;
        }

        void Water()
        {
            var water = PrimitiveKit.Mat(new Color(0.05f, 0.08f, 0.12f, 0.88f), 0.1f, 0.8f, true, 0.15f, true);
            var sea = PrimitiveKit.Make(PrimitiveType.Cube, "Sea", transform, new Vector3(0f, -1.4f, 0f), new Vector3(520f, 0.4f, 520f), water, false);
            sea.transform.position = new Vector3(0f, -1.4f, 0f);
        }

        void Paths()
        {
            var mat = PrimitiveKit.Mat(new Color(0.3f, 0.2f, 0.14f), 0.02f, 0.16f);
            for (int r = 0; r < MapLayout.Size; r++)
            {
                for (int c = 0; c < MapLayout.Size; c++)
                {
                    Vector3 a = MapLayout.CellCenter(c, r);
                    if (c < MapLayout.Size - 1) Segment(a, MapLayout.CellCenter(c + 1, r), mat);
                    if (r < MapLayout.Size - 1) Segment(a, MapLayout.CellCenter(c, r + 1), mat);
                }
            }
        }

        void Segment(Vector3 a, Vector3 b, Material mat)
        {
            Vector3 mid = (a + b) * 0.5f;
            mid.y = 0.05f;
            var go = PrimitiveKit.Make(PrimitiveType.Cube, "Path", transform, mid, new Vector3(3.4f, 0.08f, MapLayout.Cell), mat, false);
            go.transform.rotation = Quaternion.LookRotation((b - a).normalized, Vector3.up);
        }

        void Coast()
        {
            var rock = PrimitiveKit.Mat(new Color(0.12f, 0.07f, 0.06f), 0.05f, 0.2f);
            var hot = PrimitiveKit.Mat(new Color(0.7f, 0.18f, 0.05f), 0.1f, 0.4f, true, 1.4f);
            int count = 32;
            for (int i = 0; i < count; i++)
            {
                float a = i / (float)count * Mathf.PI * 2f;
                Vector3 p = new Vector3(Mathf.Cos(a) * 108f, 1.5f, Mathf.Sin(a) * 108f);
                float h = 3.2f + (i % 4) * 0.45f;
                var wall = PrimitiveKit.Make(PrimitiveType.Cube, "Rim", transform, p, new Vector3(24f, h, 5f), i % 5 == 0 ? hot : rock, true);
                wall.transform.rotation = Quaternion.LookRotation(new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)), Vector3.up);
                wall.tag = "Boundary";
            }
        }

        void Forest()
        {
            var rng = new System.Random(2014);
            for (int r = 0; r < MapLayout.Size; r++)
            {
                for (int c = 0; c < MapLayout.Size; c++)
                {
                    Vector3 center = MapLayout.CellCenter(c, r);
                    int trees = MapLayout.At(c, r) == AreaId.Wilds ? 7 : 3;
                    if (c == 2 && r == 2) trees = 4;
                    for (int i = 0; i < trees; i++)
                    {
                        float ang = (float)rng.NextDouble() * Mathf.PI * 2f;
                        float rad = 8f + (float)rng.NextDouble() * 10f;
                        Vector3 p = center + new Vector3(Mathf.Cos(ang) * rad, 0f, Mathf.Sin(ang) * rad);
                        if (p.magnitude > 96f || OnPath(p) || NearLandmark(p, 10f)) continue;
                        Tree(p, 2.8f + (float)rng.NextDouble() * 2.6f, rng.Next(0, 2));
                    }
                }
            }
        }

        static bool OnPath(Vector3 p)
        {
            return NearGrid(p.x) || NearGrid(p.z);
        }

        static bool NearGrid(float v)
        {
            for (int i = 0; i < MapLayout.Size; i++)
            {
                float line = (i - 2) * MapLayout.Cell;
                if (Mathf.Abs(v - line) < 2.6f) return true;
            }
            return false;
        }

        static bool NearLandmark(Vector3 p, float radius)
        {
            for (int r = 0; r < MapLayout.Size; r++)
            {
                for (int c = 0; c < MapLayout.Size; c++)
                {
                    if (MapLayout.At(c, r) == AreaId.Wilds) continue;
                    if (Vector3.Distance(new Vector3(p.x, 0f, p.z), MapLayout.CellCenter(c, r)) < radius) return true;
                }
            }
            if (p.magnitude < 7f) return true;
            return false;
        }

        void Tree(Vector3 pos, float height, int kind)
        {
            var trunk = PrimitiveKit.Mat(new Color(0.2f, 0.12f, 0.08f), 0.05f, 0.2f);
            var leaf = PrimitiveKit.Mat(kind == 0 ? new Color(0.1f, 0.18f, 0.09f) : new Color(0.16f, 0.14f, 0.08f), 0.02f, 0.15f);
            PrimitiveKit.Make(PrimitiveType.Cylinder, "Trunk", transform, pos + Vector3.up * (height * 0.5f), new Vector3(0.28f, height * 0.5f, 0.28f), trunk, true);
            PrimitiveKit.Make(PrimitiveType.Sphere, "Canopy", transform, pos + Vector3.up * (height + 0.5f), new Vector3(2.1f, 1.35f, 2.1f), leaf, false);
        }

        void Volcano()
        {
            var site = new GameObject("Volcano").transform;
            site.SetParent(transform, false);
            site.position = new Vector3(0f, 0f, 128f);
            refs.volcano = site;
            var rock = PrimitiveKit.Mat(new Color(0.14f, 0.08f, 0.07f), 0.08f, 0.22f);
            var lava = PrimitiveKit.Mat(new Color(1f, 0.35f, 0.05f), 0.1f, 0.5f, true, 3.2f);
            PrimitiveKit.Make(PrimitiveType.Cylinder, "Slope", site, new Vector3(0f, 6f, 0f), new Vector3(34f, 6f, 34f), rock, true);
            PrimitiveKit.Make(PrimitiveType.Cylinder, "Cone", site, new Vector3(0f, 16f, 0f), new Vector3(20f, 5f, 20f), rock, true);
            PrimitiveKit.Make(PrimitiveType.Cylinder, "Crown", site, new Vector3(0f, 24f, 0f), new Vector3(12f, 3.5f, 12f), rock, true);
            PrimitiveKit.Make(PrimitiveType.Cylinder, "Crater", site, new Vector3(0f, 28f, 0f), new Vector3(6f, 0.6f, 6f), lava, false);
            PrimitiveKit.AddLight(site, new Vector3(0f, 30f, 0f), new Color(1f, 0.4f, 0.1f), 40f, 70f);
            PrimitiveKit.Embers(site, new Color(1f, 0.4f, 0.1f, 0.5f), 6f, 0.2f, 3f, 1.2f, 4f);
        }

        void Mine()
        {
            var site = Site("Mithril Mine", 1, 1);
            var wall = PrimitiveKit.Mat(new Color(0.18f, 0.16f, 0.18f), 0.15f, 0.25f);
            var ore = PrimitiveKit.Mat(new Color(0.45f, 0.75f, 0.9f), 0.6f, 0.7f, true, 1.2f);
            RoomShell(site, wall, 7f, true);
            PrimitiveKit.Make(PrimitiveType.Cube, "Crystal", site, new Vector3(-2f, 1.4f, 2f), new Vector3(0.3f, 0.9f, 0.3f), ore, false);
            PrimitiveKit.Make(PrimitiveType.Cube, "Crystal", site, new Vector3(2.2f, 1.1f, 1.2f), Quaternion.Euler(0f, 20f, 18f), new Vector3(0.25f, 0.7f, 0.25f), ore, false);
            PrimitiveKit.Embers(site, new Color(0.5f, 0.8f, 1f, 0.4f), 2f, 0.05f, 1.5f, 0.2f, 1.5f);
            Pedestal(site, new Vector3(-1.6f, 0f, 1.5f), ItemId.ClothArmour);
            Pedestal(site, new Vector3(1.6f, 0f, 1.5f), ItemId.MithrilAxe);
            DarkDoor(site.position + new Vector3(0f, 1.6f, -4.8f), new Vector3(13.5f, 3.4f, 2.4f), Vector3.back);
            Sign(site.position + new Vector3(0f, 0f, -8f), "MITHRIL MINE", 0f, new Color(0.7f, 0.9f, 1f));
        }

        void Farms()
        {
            var site = Site("Dark Farms", 3, 1);
            var soil = PrimitiveKit.Mat(new Color(0.18f, 0.14f, 0.08f), 0.02f, 0.1f);
            var dead = PrimitiveKit.Mat(new Color(0.28f, 0.24f, 0.12f), 0.02f, 0.12f);
            var barn = PrimitiveKit.Mat(new Color(0.22f, 0.12f, 0.1f), 0.05f, 0.2f);
            for (int i = 0; i < 5; i++)
            {
                PrimitiveKit.Make(PrimitiveType.Cube, "Row", site, new Vector3(-6f, 0.25f, -2f + i * 1.1f), new Vector3(4.5f, 0.35f, 0.35f), dead, false);
            }
            PrimitiveKit.Make(PrimitiveType.Cube, "Soil", site, new Vector3(-6f, 0.05f, 0.2f), new Vector3(6f, 0.08f, 6f), soil, false);
            // Barn on the north side of the cell. Door faces south.
            PrimitiveKit.Make(PrimitiveType.Cube, "BarnWallN", site, new Vector3(3.5f, 2f, 5.5f), new Vector3(8f, 4f, 0.4f), barn, true);
            PrimitiveKit.Make(PrimitiveType.Cube, "BarnWallE", site, new Vector3(7.3f, 2f, 2.2f), new Vector3(0.4f, 4f, 7f), barn, true);
            PrimitiveKit.Make(PrimitiveType.Cube, "BarnWallW", site, new Vector3(-0.3f, 2f, 2.2f), new Vector3(0.4f, 4f, 7f), barn, true);
            PrimitiveKit.Make(PrimitiveType.Cube, "BarnRoof", site, new Vector3(3.5f, 4.1f, 2.2f), new Vector3(8.2f, 0.3f, 7.2f), barn, true);
            PrimitiveKit.Make(PrimitiveType.Cube, "DoorL", site, new Vector3(1.5f, 1.6f, -1.2f), new Vector3(3.2f, 3.2f, 0.4f), barn, true);
            PrimitiveKit.Make(PrimitiveType.Cube, "DoorR", site, new Vector3(6.2f, 1.6f, -1.2f), new Vector3(2.2f, 3.2f, 0.4f), barn, true);
            Pedestal(site, new Vector3(2.4f, 0f, 3.2f), ItemId.IronSword);
            Pedestal(site, new Vector3(4.8f, 0f, 3.2f), ItemId.IceWhip);
            DarkDoor(site.position + new Vector3(4.1f, 1.6f, -1.2f), new Vector3(2.8f, 3.4f, 1.6f), Vector3.back);
            Sign(site.position + new Vector3(3.5f, 0f, -3.4f), "DARK FARMS", 0f, new Color(0.75f, 0.7f, 0.4f));
        }

        void Keep()
        {
            var site = Site("Adamantine Keep", 1, 3);
            var marble = PrimitiveKit.Mat(new Color(0.82f, 0.8f, 0.74f), 0.15f, 0.78f);
            var floor = PrimitiveKit.Mat(new Color(0.7f, 0.68f, 0.62f), 0.2f, 0.65f);
            PrimitiveKit.Make(PrimitiveType.Cube, "Floor", site, new Vector3(0f, 0.08f, 0f), new Vector3(14f, 0.16f, 14f), floor, false);
            for (int i = 0; i < 4; i++)
            {
                float x = i < 2 ? -4.5f : 4.5f;
                float z = i % 2 == 0 ? -4.5f : 4.5f;
                PrimitiveKit.Make(PrimitiveType.Cylinder, "Column", site, new Vector3(x, 2.2f, z), new Vector3(0.7f, 2.2f, 0.7f), marble, true);
            }
            PrimitiveKit.Make(PrimitiveType.Cube, "Lintel", site, new Vector3(0f, 4.5f, 0f), new Vector3(12f, 0.35f, 12f), marble, true);
            PrimitiveKit.Make(PrimitiveType.Cube, "Grate", site, new Vector3(0f, 0.12f, 2.5f), new Vector3(1.4f, 0.08f, 1.4f), PrimitiveKit.Mat(new Color(0.15f, 0.15f, 0.16f), 0.7f, 0.5f), false);
            Pedestal(site, new Vector3(-2.2f, 0f, -1f), ItemId.SewerKey);
            Pedestal(site, new Vector3(2.2f, 0f, -1f), ItemId.Kiteshield);
            Pedestal(site, new Vector3(-2.2f, 0f, 2f), ItemId.PoisonCheese);
            Pedestal(site, new Vector3(2.2f, 0f, 2f), ItemId.BucketOfWater);
            Sign(site.position + new Vector3(0f, 0f, -8f), "ADAMANTINE KEEP", 0f, Bone);
        }

        void Guild()
        {
            var site = Site("Guild of Scrolls", 3, 3);
            var emerald = PrimitiveKit.Mat(new Color(0.1f, 0.38f, 0.28f), 0.25f, 0.55f);
            var scroll = PrimitiveKit.Mat(new Color(0.78f, 0.68f, 0.42f), 0.05f, 0.25f);
            PrimitiveKit.Make(PrimitiveType.Cube, "Floor", site, new Vector3(0f, 0.08f, 0f), new Vector3(12f, 0.16f, 12f), emerald, false);
            PrimitiveKit.Make(PrimitiveType.Cube, "WallN", site, new Vector3(0f, 2f, 5.5f), new Vector3(12f, 4f, 0.4f), emerald, true);
            PrimitiveKit.Make(PrimitiveType.Cube, "WallE", site, new Vector3(5.8f, 2f, 0f), new Vector3(0.4f, 4f, 11f), emerald, true);
            PrimitiveKit.Make(PrimitiveType.Cube, "WallW", site, new Vector3(-5.8f, 2f, 0f), new Vector3(0.4f, 4f, 11f), emerald, true);
            for (int i = 0; i < 4; i++)
            {
                PrimitiveKit.Make(PrimitiveType.Cube, "Scroll", site, new Vector3(-4f + i * 2.4f, 2.4f, 5.2f), new Vector3(0.9f, 1.4f, 0.06f), scroll, false);
            }
            PrimitiveKit.AddLight(site, new Vector3(0f, 3f, 0f), new Color(0.4f, 1f, 0.6f), 6f, 12f);
            Pedestal(site, new Vector3(-2.4f, 0f, -1f), ItemId.ChestKey);

            var chest = new GameObject("Chest").transform;
            chest.SetParent(site, false);
            chest.localPosition = new Vector3(2.2f, 0f, 0.4f);
            var wood = PrimitiveKit.Mat(new Color(0.32f, 0.18f, 0.08f), 0.15f, 0.35f);
            var band = PrimitiveKit.Mat(new Color(0.75f, 0.62f, 0.2f), 0.7f, 0.6f);
            PrimitiveKit.Make(PrimitiveType.Cube, "ChestBody", chest, new Vector3(0f, 0.45f, 0f), new Vector3(1.2f, 0.7f, 0.7f), wood, true);
            PrimitiveKit.Make(PrimitiveType.Cube, "Band", chest, new Vector3(0f, 0.5f, 0f), new Vector3(1.24f, 0.12f, 0.74f), band, false);
            var lid = PrimitiveKit.Make(PrimitiveType.Cube, "Lid", chest, new Vector3(0f, 0.9f, -0.28f), new Vector3(1.24f, 0.18f, 0.74f), wood, false);
            lid.transform.localPosition = new Vector3(0f, 0.9f, -0.28f);
            refs.chestLid = lid.AddComponent<ChestLid>();
            refs.chestPoint = chest;
            var interact = chest.gameObject.AddComponent<WorldInteract>();
            interact.kind = InteractKind.Chest;
            interact.prompt = "Open the chest  [E]";
            interact.promptColor = new Color(0.4f, 0.9f, 0.55f);
            Sign(site.position + new Vector3(0f, 0f, -8f), "GUILD OF SCROLLS", 0f, new Color(0.5f, 1f, 0.7f));
        }

        void BasiliskArea()
        {
            var site = Site("Basilisk", 0, 2);
            var pool = PrimitiveKit.Mat(new Color(0.45f, 0.7f, 0.15f, 0.75f), 0.05f, 0.4f, true, 0.8f, true);
            var bone = PrimitiveKit.Mat(new Color(0.55f, 0.58f, 0.4f), 0.1f, 0.25f);
            var bar = PrimitiveKit.Mat(new Color(0.2f, 0.22f, 0.18f), 0.6f, 0.4f);
            PrimitiveKit.Make(PrimitiveType.Cylinder, "Lake", site, new Vector3(0f, 0.08f, 0f), new Vector3(10f, 0.08f, 10f), pool, false);
            PrimitiveKit.Embers(site, new Color(0.7f, 0.85f, 0.3f, 0.35f), 5f, 0.15f, 2.2f, 0.4f, 3f);
            PrimitiveKit.AddLight(site, new Vector3(0f, 1.5f, 0f), new Color(0.6f, 0.8f, 0.2f), 8f, 14f);
            for (int i = 0; i < 8; i++)
            {
                float a = i / 8f * Mathf.PI * 2f;
                PrimitiveKit.Make(PrimitiveType.Cylinder, "Bar", site, new Vector3(Mathf.Cos(a) * 2.2f, 1.6f, Mathf.Sin(a) * 2.2f), new Vector3(0.08f, 1.6f, 0.08f), bar, true);
            }
            // Basilisk: long body, potato nose, teeth, root feet. Distinct from the dragon.
            var hide = PrimitiveKit.Mat(new Color(0.25f, 0.32f, 0.12f), 0.1f, 0.3f);
            PrimitiveKit.Make(PrimitiveType.Capsule, "Body", site, new Vector3(0f, 0.7f, 0f), Quaternion.Euler(0f, 0f, 90f), new Vector3(0.7f, 1.3f, 0.7f), hide, false);
            PrimitiveKit.Make(PrimitiveType.Sphere, "Nose", site, new Vector3(1.5f, 0.85f, 0f), new Vector3(0.45f, 0.32f, 0.32f), PrimitiveKit.Mat(new Color(0.7f, 0.65f, 0.3f), 0.05f, 0.2f), false);
            for (int i = 0; i < 3; i++)
            {
                PrimitiveKit.Make(PrimitiveType.Cube, "Tooth", site, new Vector3(1.15f, 0.7f, -0.12f + i * 0.12f), new Vector3(0.16f, 0.08f, 0.05f), bone, false);
            }
            for (int i = 0; i < 4; i++)
            {
                float x = i < 2 ? -0.4f : 0.5f;
                float z = i % 2 == 0 ? -0.35f : 0.35f;
                PrimitiveKit.Make(PrimitiveType.Cylinder, "Root", site, new Vector3(x, 0.25f, z), Quaternion.Euler(0f, 0f, 40f), new Vector3(0.08f, 0.3f, 0.08f), hide, false);
            }
            var marker = Marker(site, new Vector3(0f, 0f, -4.2f), InteractKind.Basilisk, "Confront the basilisk  [E]", Poison);
            marker.item = ItemId.Torch;
            Sign(site.position + new Vector3(0f, 0f, -6.5f), "DO NOT MEET ITS GAZE", 0f, Poison);
        }

        void Lava()
        {
            var site = Site("Lava Falls", 4, 2);
            var rock = PrimitiveKit.Mat(new Color(0.15f, 0.08f, 0.07f), 0.08f, 0.2f);
            var lava = PrimitiveKit.Mat(new Color(1f, 0.32f, 0.04f), 0.05f, 0.45f, true, 2.8f);
            PrimitiveKit.Make(PrimitiveType.Cube, "Cliff", site, new Vector3(0f, 2.2f, 2f), new Vector3(8f, 4.4f, 4f), rock, true);
            PrimitiveKit.Make(PrimitiveType.Cube, "Pool", site, new Vector3(0f, 0.15f, -2.5f), new Vector3(7f, 0.2f, 4f), lava, false);
            for (int i = 0; i < 3; i++)
            {
                float x = -2f + i * 2f;
                for (int s = 0; s < 4; s++)
                {
                    var sheet = PrimitiveKit.Make(PrimitiveType.Cube, "Fall", site, new Vector3(x, 3.2f - s * 0.15f, 0.2f), new Vector3(0.7f, 0.9f, 0.18f), lava, false);
                    var flow = sheet.AddComponent<LavaStream>();
                    flow.speed = 1.6f + i * 0.2f;
                    flow.length = 2.4f;
                }
            }
            PrimitiveKit.AddLight(site, new Vector3(0f, 2f, -1f), new Color(1f, 0.4f, 0.1f), 16f, 16f);
            PrimitiveKit.Embers(site, new Color(1f, 0.45f, 0.1f, 0.7f), 8f, 0.08f, 1.2f, 0.6f, 1.2f);

            var brazier = new GameObject("Brazier").transform;
            brazier.SetParent(site, false);
            brazier.localPosition = new Vector3(-4.2f, 0f, -1f);
            PrimitiveKit.Make(PrimitiveType.Cylinder, "Bowl", brazier, new Vector3(0f, 0.7f, 0f), new Vector3(0.7f, 0.15f, 0.7f), rock, true);
            PrimitiveKit.Make(PrimitiveType.Sphere, "Flame", brazier, new Vector3(0f, 1.05f, 0f), Vector3.one * 0.28f, lava, false);
            PrimitiveKit.AddLight(brazier, new Vector3(0f, 1.2f, 0f), new Color(1f, 0.45f, 0.1f), 8f, 8f);
            PrimitiveKit.Embers(brazier, new Color(1f, 0.4f, 0.1f, 0.8f), 4f, 0.05f, 0.8f, 0.4f, 0.15f);
            brazier.gameObject.AddComponent<Brazier>();
            var brazierUse = brazier.gameObject.AddComponent<WorldInteract>();
            brazierUse.kind = InteractKind.Brazier;
            brazierUse.prompt = "The brazier  [E]";
            brazierUse.promptColor = Ember;

            Marker(site, new Vector3(0f, 2.4f, -0.4f), InteractKind.Lava, "Jump the Lava Falls  [E]", Danger);
            Sign(site.position + new Vector3(0f, 0f, -6f), "LAVA FALLS", 0f, Danger);
        }

        void DropRoom()
        {
            var site = Site("Drop Room", 0, 4);
            var stone = PrimitiveKit.Mat(new Color(0.35f, 0.28f, 0.22f), 0.1f, 0.3f);
            for (int i = 0; i < 8; i++)
            {
                float a = i / 8f * Mathf.PI * 2f;
                PrimitiveKit.Make(PrimitiveType.Cube, "Cairn", site, new Vector3(Mathf.Cos(a) * 3.2f, 0.6f, Mathf.Sin(a) * 3.2f), new Vector3(0.8f, 1.2f, 0.8f), stone, true);
            }
            refs.dropAnchor = new GameObject("DropAnchor").transform;
            refs.dropAnchor.SetParent(site, false);
            refs.dropAnchor.localPosition = new Vector3(-1.5f, 0f, -1f);
            Sign(site.position + new Vector3(0f, 0f, -5.5f), "DROP ROOM", 0f, new Color(0.85f, 0.7f, 0.45f));
        }

        void Raft()
        {
            var site = Site("Bamboo Shore", 2, 4);
            var sand = PrimitiveKit.Mat(new Color(0.42f, 0.36f, 0.24f), 0.02f, 0.15f);
            var bamboo = PrimitiveKit.Mat(new Color(0.55f, 0.62f, 0.28f), 0.05f, 0.25f);
            var lash = PrimitiveKit.Mat(new Color(0.35f, 0.22f, 0.1f), 0.05f, 0.2f);
            PrimitiveKit.Make(PrimitiveType.Cube, "Sand", site, new Vector3(0f, 0.06f, -1f), new Vector3(12f, 0.1f, 10f), sand, false);
            for (int i = 0; i < 6; i++)
            {
                var log = PrimitiveKit.Make(PrimitiveType.Cylinder, "Bamboo", site, new Vector3(-1.5f + i * 0.55f, 0.35f, 1.2f), Quaternion.Euler(0f, 0f, 90f), new Vector3(0.12f, 1.6f, 0.12f), bamboo, false);
                log.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            }
            PrimitiveKit.Make(PrimitiveType.Cube, "Lashing", site, new Vector3(0.4f, 0.4f, 1.2f), new Vector3(3.2f, 0.06f, 0.15f), lash, false);
            Marker(site, new Vector3(0.4f, 0f, 1.2f), InteractKind.Raft, "Ride the bamboo shaft  [E]", Sea);
            Sign(site.position + new Vector3(0f, 0f, -4.5f), "BAMBOO SHORE", 0f, Sea);
        }

        void DragonGate()
        {
            var site = Site("Dragon Gate", 2, 0);
            var stone = PrimitiveKit.Mat(new Color(0.18f, 0.12f, 0.12f), 0.12f, 0.28f);
            var metal = PrimitiveKit.Mat(new Color(0.45f, 0.32f, 0.16f), 0.7f, 0.55f);
            PrimitiveKit.Make(PrimitiveType.Cube, "PillarL", site, new Vector3(-4.2f, 3f, 1f), new Vector3(1.4f, 6f, 1.6f), stone, true);
            PrimitiveKit.Make(PrimitiveType.Cube, "PillarR", site, new Vector3(4.2f, 3f, 1f), new Vector3(1.4f, 6f, 1.6f), stone, true);
            PrimitiveKit.Make(PrimitiveType.Cube, "Lintel", site, new Vector3(0f, 6.2f, 1f), new Vector3(10f, 1f, 1.8f), stone, true);
            var left = PrimitiveKit.Make(PrimitiveType.Cube, "DoorL", site, new Vector3(-1.7f, 2.6f, 1f), new Vector3(3.2f, 4.8f, 0.5f), stone, true);
            var right = PrimitiveKit.Make(PrimitiveType.Cube, "DoorR", site, new Vector3(1.7f, 2.6f, 1f), new Vector3(3.2f, 4.8f, 0.5f), stone, true);
            PrimitiveKit.Make(PrimitiveType.Cube, "Lock", site, new Vector3(0f, 2.4f, 1.3f), new Vector3(0.35f, 0.5f, 0.2f), metal, false);
            refs.gateLeft = left.transform;
            refs.gateRight = right.transform;

            // Arena behind the gate, toward the volcano.
            var bowl = PrimitiveKit.Mat(new Color(0.1f, 0.06f, 0.06f), 0.1f, 0.2f);
            PrimitiveKit.Make(PrimitiveType.Cylinder, "Arena", site, new Vector3(0f, 0.08f, 12f), new Vector3(14f, 0.1f, 14f), bowl, false);
            for (int i = 0; i < 10; i++)
            {
                float a = i / 10f * Mathf.PI * 2f;
                if (Mathf.Sin(a) < -0.2f) continue;
                PrimitiveKit.Make(PrimitiveType.Cube, "ArenaRock", site, new Vector3(Mathf.Cos(a) * 7f, 1.2f, 12f + Mathf.Sin(a) * 7f), new Vector3(1.4f, 2.4f, 1.4f), stone, true);
            }
            refs.dragon = DragonAvatar.Build(site, new Vector3(0f, 0f, 12f));
            refs.lairLight = PrimitiveKit.AddLight(site, new Vector3(0f, 5f, 12f), new Color(1f, 0.35f, 0.08f), 22f, 22f);
            refs.lairLight.enabled = false;

            Marker(site, new Vector3(0f, 0f, -3.2f), InteractKind.Gate, "The Dragon's Gate  [E]", Ember);
            Sign(site.position + new Vector3(0f, 0f, -6f), "DRAGON'S GATE", 0f, Ember);
        }

        void Spawn()
        {
            var playerGo = new GameObject("Explorer");
            playerGo.transform.SetParent(transform, false);
            playerGo.transform.position = new Vector3(0f, 0.15f, 0f);
            refs.player = playerGo.AddComponent<ExplorerController>();
            refs.player.BuildBody();

            ItemDrop.Spawn(ItemId.Torch, new Vector3(0f, 0.2f, 2.1f), transform);
            var stone = PrimitiveKit.Mat(new Color(0.28f, 0.22f, 0.18f), 0.1f, 0.3f);
            PrimitiveKit.Make(PrimitiveType.Cube, "StoryStone", transform, new Vector3(0f, 0.7f, -2.4f), new Vector3(1.4f, 1.4f, 0.4f), stone, true);
            Marker(null, new Vector3(0f, 0f, -2.4f), InteractKind.Story, "Read the carved stone  [E]", Bone);

            var ring = PrimitiveKit.Mat(new Color(0.22f, 0.16f, 0.12f), 0.05f, 0.2f);
            for (int i = 0; i < 6; i++)
            {
                float a = i / 6f * Mathf.PI * 2f;
                PrimitiveKit.Make(PrimitiveType.Cube, "Ring", transform, new Vector3(Mathf.Cos(a) * 3.4f, 0.35f, Mathf.Sin(a) * 3.4f), new Vector3(0.6f, 0.7f, 0.6f), ring, true);
            }
        }

        Transform Site(string name, int col, int row)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.transform.position = MapLayout.CellCenter(col, row);
            return go.transform;
        }

        void RoomShell(Transform site, Material wall, float extent, bool roof)
        {
            PrimitiveKit.Make(PrimitiveType.Cube, "Back", site, new Vector3(0f, 2f, extent), new Vector3(extent * 2f, 4f, 0.5f), wall, true);
            PrimitiveKit.Make(PrimitiveType.Cube, "Left", site, new Vector3(-extent, 2f, 1f), new Vector3(0.5f, 4f, extent * 1.6f), wall, true);
            PrimitiveKit.Make(PrimitiveType.Cube, "Right", site, new Vector3(extent, 2f, 1f), new Vector3(0.5f, 4f, extent * 1.6f), wall, true);
            if (roof)
                PrimitiveKit.Make(PrimitiveType.Cube, "Roof", site, new Vector3(0f, 4.1f, 1f), new Vector3(extent * 2f, 0.3f, extent * 1.8f), wall, true);
        }

        void Pedestal(Transform site, Vector3 local, ItemId id)
        {
            var stone = PrimitiveKit.Mat(new Color(0.24f, 0.2f, 0.18f), 0.1f, 0.35f);
            PrimitiveKit.Make(PrimitiveType.Cylinder, "Pedestal", site, local + Vector3.up * 0.45f, new Vector3(0.65f, 0.45f, 0.65f), stone, true);
            ItemDrop.Spawn(id, site.TransformPoint(local + Vector3.up * 1.15f), transform);
        }

        void DarkDoor(Vector3 worldPos, Vector3 size, Vector3 push)
        {
            var go = new GameObject("TorchGate");
            go.transform.SetParent(transform, false);
            go.transform.position = worldPos;
            var box = go.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = size;
            var body = go.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            var gate = go.AddComponent<TorchGate>();
            gate.push = push;
        }

        WorldInteract Marker(Transform site, Vector3 localOrWorld, InteractKind kind, string prompt, Color color)
        {
            var go = new GameObject(kind.ToString());
            go.transform.SetParent(site != null ? site : transform, false);
            if (site != null) go.transform.localPosition = localOrWorld;
            else go.transform.position = localOrWorld;
            var interact = go.AddComponent<WorldInteract>();
            interact.kind = kind;
            interact.prompt = prompt;
            interact.promptColor = color;
            return interact;
        }

        void Sign(Vector3 worldPos, string text, float yaw, Color color)
        {
            var go = new GameObject("Sign");
            go.transform.SetParent(transform, false);
            go.transform.position = worldPos + Vector3.up * 2.6f;
            // World-space UI faces along -forward, so yaw 0 looks south, toward the path you arrive on.
            go.transform.rotation = Quaternion.Euler(0f, yaw + 180f, 0f);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(520f, 90f);
            go.transform.localScale = Vector3.one * 0.012f;

            var bgGo = new GameObject("Bg");
            bgGo.transform.SetParent(go.transform, false);
            var bg = bgGo.AddComponent<Image>();
            var white = Texture2D.whiteTexture;
            bg.sprite = Sprite.Create(white, new Rect(0f, 0f, white.width, white.height), new Vector2(0.5f, 0.5f), 100f);
            bg.color = new Color(0.04f, 0.02f, 0.02f, 0.78f);
            bg.rectTransform.sizeDelta = new Vector2(520f, 90f);
            bg.raycastTarget = false;

            var textGo = new GameObject("Label");
            textGo.transform.SetParent(go.transform, false);
            var ui = textGo.AddComponent<Text>();
            ui.font = PrimitiveKit.BuiltinFont();
            ui.fontSize = 40;
            ui.alignment = TextAnchor.MiddleCenter;
            ui.color = color;
            ui.text = text;
            ui.rectTransform.sizeDelta = new Vector2(500f, 80f);
            ui.raycastTarget = false;
        }
    }
}
