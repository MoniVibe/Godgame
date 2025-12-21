using Godgame.Economy;
using Godgame.Rendering;
using Unity.Collections;
using Unity.Mathematics;

namespace Godgame.Presentation
{
    public static class GodgamePresentationColors
    {
        private static readonly float4 OreColor = new float4(0.5f, 0.5f, 0.5f, 1f);
        private static readonly float4 WoodColor = new float4(0.55f, 0.27f, 0.07f, 1f);
        private static readonly float4 StoneColor = new float4(0.4f, 0.4f, 0.4f, 1f);
        private static readonly float4 HerbColor = new float4(0.2f, 0.6f, 0.2f, 1f);
        private static readonly float4 AgricultureColor = new float4(0.9f, 0.8f, 0.2f, 1f);

        private static readonly float4 VillageCenterColor = new float4(0.2f, 0.6f, 0.9f, 1f);
        private static readonly float4 StorehouseColor = new float4(0.9f, 0.6f, 0.2f, 1f);
        private static readonly float4 HousingColor = new float4(0.3f, 0.8f, 0.6f, 1f);
        private static readonly float4 WorshipColor = new float4(0.7f, 0.4f, 0.9f, 1f);
        private static readonly float4 ConstructionGhostColor = new float4(0.7f, 0.7f, 0.7f, 0.6f);
        private static readonly float4 BandColor = new float4(0.95f, 0.85f, 0.3f, 1f);
        private static readonly float4 DefaultColor = new float4(1f, 1f, 1f, 1f);

        private static readonly FixedString64Bytes WoodId = CreateId('w', 'o', 'o', 'd');
        private static readonly FixedString64Bytes TimberId = CreateId('t', 'i', 'm', 'b', 'e', 'r');
        private static readonly FixedString64Bytes StoneId = CreateId('s', 't', 'o', 'n', 'e');
        private static readonly FixedString64Bytes OreId = CreateId('o', 'r', 'e');
        private static readonly FixedString64Bytes IronId = CreateId('i', 'r', 'o', 'n');
        private static readonly FixedString64Bytes CopperId = CreateId('c', 'o', 'p', 'p', 'e', 'r');
        private static readonly FixedString64Bytes TinId = CreateId('t', 'i', 'n');
        private static readonly FixedString64Bytes SilverId = CreateId('s', 'i', 'l', 'v', 'e', 'r');
        private static readonly FixedString64Bytes GoldId = CreateId('g', 'o', 'l', 'd');
        private static readonly FixedString64Bytes MithrilId = CreateId('m', 'i', 't', 'h', 'r', 'i', 'l');
        private static readonly FixedString64Bytes AdamantiteId = CreateId('a', 'd', 'a', 'm', 'a', 'n', 't', 'i', 't', 'e');

        public static float4 ForBuilding(ushort semanticKey)
        {
            return semanticKey switch
            {
                GodgameSemanticKeys.VillageCenter => VillageCenterColor,
                GodgameSemanticKeys.Storehouse => StorehouseColor,
                GodgameSemanticKeys.Housing => HousingColor,
                GodgameSemanticKeys.Worship => WorshipColor,
                GodgameSemanticKeys.ConstructionGhost => ConstructionGhostColor,
                _ => DefaultColor
            };
        }

        public static float4 ForBand()
        {
            return BandColor;
        }

        public static float4 ForResourceType(ResourceType type)
        {
            byte typeValue = (byte)type;

            if (typeValue >= 1 && typeValue <= 7)
                return OreColor;
            if (typeValue >= 10 && typeValue <= 14)
                return WoodColor;
            if (typeValue >= 20 && typeValue <= 23)
                return StoneColor;
            if (typeValue >= 30 && typeValue <= 33)
                return HerbColor;
            if (typeValue >= 40 && typeValue <= 44)
                return AgricultureColor;

            return DefaultColor;
        }

        public static float4 ForResourceTypeIndex(ushort typeIndex)
        {
            return ForResourceType((ResourceType)typeIndex);
        }

        public static float4 ForResourceId(in FixedString64Bytes resourceId)
        {
            if (resourceId.Equals(WoodId) || resourceId.Equals(TimberId))
                return WoodColor;
            if (resourceId.Equals(StoneId))
                return StoneColor;
            if (resourceId.Equals(OreId)
                || resourceId.Equals(IronId)
                || resourceId.Equals(CopperId)
                || resourceId.Equals(TinId)
                || resourceId.Equals(SilverId)
                || resourceId.Equals(GoldId)
                || resourceId.Equals(MithrilId)
                || resourceId.Equals(AdamantiteId))
                return OreColor;

            return DefaultColor;
        }

        private static FixedString64Bytes CreateId(char c0, char c1, char c2)
        {
            var id = new FixedString64Bytes();
            id.Append(c0);
            id.Append(c1);
            id.Append(c2);
            return id;
        }

        private static FixedString64Bytes CreateId(char c0, char c1, char c2, char c3)
        {
            var id = new FixedString64Bytes();
            id.Append(c0);
            id.Append(c1);
            id.Append(c2);
            id.Append(c3);
            return id;
        }

        private static FixedString64Bytes CreateId(char c0, char c1, char c2, char c3, char c4)
        {
            var id = new FixedString64Bytes();
            id.Append(c0);
            id.Append(c1);
            id.Append(c2);
            id.Append(c3);
            id.Append(c4);
            return id;
        }

        private static FixedString64Bytes CreateId(char c0, char c1, char c2, char c3, char c4, char c5)
        {
            var id = new FixedString64Bytes();
            id.Append(c0);
            id.Append(c1);
            id.Append(c2);
            id.Append(c3);
            id.Append(c4);
            id.Append(c5);
            return id;
        }

        private static FixedString64Bytes CreateId(char c0, char c1, char c2, char c3, char c4, char c5, char c6)
        {
            var id = new FixedString64Bytes();
            id.Append(c0);
            id.Append(c1);
            id.Append(c2);
            id.Append(c3);
            id.Append(c4);
            id.Append(c5);
            id.Append(c6);
            return id;
        }

        private static FixedString64Bytes CreateId(char c0, char c1, char c2, char c3, char c4, char c5, char c6, char c7, char c8, char c9)
        {
            var id = new FixedString64Bytes();
            id.Append(c0);
            id.Append(c1);
            id.Append(c2);
            id.Append(c3);
            id.Append(c4);
            id.Append(c5);
            id.Append(c6);
            id.Append(c7);
            id.Append(c8);
            id.Append(c9);
            return id;
        }
    }
}
