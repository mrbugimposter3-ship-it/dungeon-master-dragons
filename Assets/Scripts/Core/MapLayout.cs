using UnityEngine;

namespace DungeonMaster
{
    public enum AreaId
    {
        Wilds,
        Mine,
        Farms,
        Keep,
        Guild,
        Basilisk,
        Lava,
        Drop,
        Raft,
        Dragon
    }

    /// <summary>
    /// The 2014 map, row 0 at the north. Letters in the original minimap were
    /// P player, M F A G D R, and a single H for three different places.
    /// Those three are now B, L, and T so the basilisk, the falls, and the raft
    /// are not the same room.
    /// </summary>
    public static class MapLayout
    {
        public const int Size = 5;
        public const float Cell = 36f;

        static readonly AreaId[,] areas = new AreaId[Size, Size];

        static MapLayout()
        {
            for (int r = 0; r < Size; r++)
                for (int c = 0; c < Size; c++)
                    areas[r, c] = AreaId.Wilds;

            areas[0, 2] = AreaId.Dragon;
            areas[1, 1] = AreaId.Mine;
            areas[1, 3] = AreaId.Farms;
            areas[2, 0] = AreaId.Basilisk;
            areas[2, 4] = AreaId.Lava;
            areas[3, 1] = AreaId.Keep;
            areas[3, 3] = AreaId.Guild;
            areas[4, 0] = AreaId.Drop;
            areas[4, 2] = AreaId.Raft;
        }

        public static AreaId At(int col, int row)
        {
            if (col < 0 || row < 0 || col >= Size || row >= Size) return AreaId.Wilds;
            return areas[row, col];
        }

        public static Vector3 CellCenter(int col, int row)
        {
            return new Vector3((col - 2) * Cell, 0f, (2 - row) * Cell);
        }

        public static bool WorldToCell(Vector3 pos, out int col, out int row)
        {
            col = Mathf.RoundToInt(pos.x / Cell + 2f);
            row = Mathf.RoundToInt(2f - pos.z / Cell);
            return col >= 0 && col < Size && row >= 0 && row < Size;
        }

        public static string Letter(AreaId id)
        {
            switch (id)
            {
                case AreaId.Mine: return "M";
                case AreaId.Farms: return "F";
                case AreaId.Keep: return "A";
                case AreaId.Guild: return "G";
                case AreaId.Dragon: return "D";
                case AreaId.Drop: return "R";
                case AreaId.Basilisk: return "B";
                case AreaId.Lava: return "L";
                case AreaId.Raft: return "T";
                default: return "";
            }
        }

        public static string DisplayName(AreaId id)
        {
            switch (id)
            {
                case AreaId.Mine: return "Mithril Mine";
                case AreaId.Farms: return "Dark Farms";
                case AreaId.Keep: return "Adamantine Keep";
                case AreaId.Guild: return "Guild of Scrolls";
                case AreaId.Dragon: return "Dragon's Gate";
                case AreaId.Drop: return "Drop Room";
                case AreaId.Basilisk: return "Hidden Basilisk";
                case AreaId.Lava: return "Lava Falls";
                case AreaId.Raft: return "Bamboo Shore";
                default: return "Volcanic Wilds";
            }
        }

        public static Color ColorOf(AreaId id)
        {
            switch (id)
            {
                case AreaId.Mine: return new Color(0.35f, 0.55f, 0.7f);
                case AreaId.Farms: return new Color(0.28f, 0.36f, 0.18f);
                case AreaId.Keep: return new Color(0.78f, 0.76f, 0.7f);
                case AreaId.Guild: return new Color(0.2f, 0.55f, 0.38f);
                case AreaId.Dragon: return new Color(0.7f, 0.18f, 0.12f);
                case AreaId.Drop: return new Color(0.55f, 0.42f, 0.28f);
                case AreaId.Basilisk: return new Color(0.55f, 0.7f, 0.22f);
                case AreaId.Lava: return new Color(0.9f, 0.35f, 0.08f);
                case AreaId.Raft: return new Color(0.35f, 0.7f, 0.62f);
                default: return new Color(0.16f, 0.12f, 0.11f);
            }
        }

        public static string ArrivalLine(AreaId id)
        {
            switch (id)
            {
                case AreaId.Mine:
                    return "You enter the Mithril Mine, a humid climate.";
                case AreaId.Farms:
                    return "You enter the Dark Farms. It is dark, but your lantern allows you to see.";
                case AreaId.Keep:
                    return "You enter the Adamantine Keep. An old palace with reflective marble pillars and floors.";
                case AreaId.Guild:
                    return "You enter the Guild of Scrolls. An emerald room with ancient scrolls hanging on the walls.";
                case AreaId.Drop:
                    return "You can find your dropped items here.";
                case AreaId.Basilisk:
                    return "A sulfurous lake boils under the cage. This is the basilisk, not the dragon.";
                case AreaId.Lava:
                    return "The Lava Falls. Largest fall of molten lava, three great falls over the rocks.";
                case AreaId.Raft:
                    return "You see a bamboo shaft which can carry you off the island. That is not a victory.";
                case AreaId.Dragon:
                    return "The Dragon's Gate. The sewer mouth waits at the foot of the volcano.";
                default:
                    return "";
            }
        }
    }
}
