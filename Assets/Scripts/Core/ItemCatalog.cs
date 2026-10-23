using System.Collections.Generic;
using UnityEngine;

namespace DungeonMaster
{
    public enum ItemId
    {
        Torch,
        ClothArmour,
        MithrilAxe,
        IronSword,
        IceWhip,
        SewerKey,
        Kiteshield,
        PoisonCheese,
        BucketOfWater,
        ChestKey,
        AntiFireCape
    }

    public sealed class ItemDef
    {
        public ItemId id;
        public string displayName;
        public string blurb;
        public int attack;
        public int defence;
        public int fireRes;
        public Color color;
    }

    public static class ItemCatalog
    {
        static readonly Dictionary<ItemId, ItemDef> all = new Dictionary<ItemId, ItemDef>();

        static ItemCatalog()
        {
            Add(ItemId.Torch, "torch", "A lit torch. Without it the dark closes, and the hidden beast is no longer afraid.", 0, 0, 0, new Color(0.45f, 0.28f, 0.12f));
            Add(ItemId.ClothArmour, "cloth armour", "Weak against fire, light. +2 defence, -1 fire resistance.", 0, 2, -1, new Color(0.62f, 0.50f, 0.36f));
            Add(ItemId.MithrilAxe, "mithril axe", "The notch in the Dragon's Gate was cut for this. +3 attack.", 3, 0, 0, new Color(0.62f, 0.86f, 0.95f));
            Add(ItemId.IronSword, "iron sword", "Medium strength, heavy. +5 attack.", 5, 0, 0, new Color(0.75f, 0.76f, 0.78f));
            Add(ItemId.IceWhip, "Zamorak ice whip", "Very strong, light. +10 attack.", 10, 0, 0, new Color(0.45f, 0.9f, 1f));
            Add(ItemId.SewerKey, "sewer key", "Opens the Dragon's Gate.", 0, 0, 0, new Color(0.86f, 0.7f, 0.22f));
            Add(ItemId.Kiteshield, "Zamorak kiteshield", "Very high strength, heavy. +10 defence.", 0, 10, 0, new Color(0.72f, 0.12f, 0.1f));
            Add(ItemId.PoisonCheese, "poison cheese", "Looks tasty. It is poison.", 0, 0, 0, new Color(0.95f, 0.84f, 0.2f));
            Add(ItemId.BucketOfWater, "bucket of water", "Useful for putting out a small fire.", 0, 0, 0, new Color(0.25f, 0.45f, 0.75f));
            Add(ItemId.ChestKey, "chest key", "Opens the chest in the Guild of Scrolls.", 0, 0, 0, new Color(0.82f, 0.84f, 0.88f));
            Add(ItemId.AntiFireCape, "anti-fire cape", "Excellent against fire. +10 fire resistance, +10 defence.", 0, 10, 10, new Color(0.9f, 0.28f, 0.08f));
        }

        static void Add(ItemId id, string name, string blurb, int attack, int defence, int fireRes, Color color)
        {
            all[id] = new ItemDef
            {
                id = id,
                displayName = name,
                blurb = blurb,
                attack = attack,
                defence = defence,
                fireRes = fireRes,
                color = color
            };
        }

        public static ItemDef Get(ItemId id)
        {
            return all[id];
        }

        public static string StatLine(ItemDef def)
        {
            var parts = new List<string>();
            if (def.attack != 0) parts.Add((def.attack > 0 ? "+" : "") + def.attack + " attack");
            if (def.defence != 0) parts.Add((def.defence > 0 ? "+" : "") + def.defence + " defence");
            if (def.fireRes != 0) parts.Add((def.fireRes > 0 ? "+" : "") + def.fireRes + " fire resistance");
            if (parts.Count == 0) return "No combat stats.";
            return string.Join(", ", parts.ToArray());
        }
    }
}
