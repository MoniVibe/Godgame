using Unity.Collections;
using Unity.Entities;

namespace Godgame.Scenario
{
    public enum GodgameScenarioActionKind : byte
    {
        EconomyEnable = 0,
        ProdCreateBusiness = 1,
        ProdAddItem = 2,
        ProdRequest = 3
    }

    public struct GodgameScenarioAction : IBufferElementData
    {
        public uint ExecuteTick;
        public GodgameScenarioActionKind Kind;
        public FixedString64Bytes BusinessId;
        public FixedString64Bytes ItemId;
        public FixedString64Bytes RecipeId;
        public float Quantity;
        public float Capacity;
        public byte BusinessType;
        public byte Executed;
    }

    public struct GodgameScenarioBusinessId : IComponentData
    {
        public FixedString64Bytes Value;
    }
}
