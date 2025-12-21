using Godgame.Scenario;
using Unity.Entities;

namespace Godgame.Villages
{
    public enum VillageBuildingType : byte
    {
        None = 0,
        Housing = 1,
        Storehouse = 2,
        Worship = 3
    }

    public enum VillageNeedChannel : byte
    {
        Hunger = 0,
        Rest = 1,
        Faith = 2,
        Safety = 3,
        Social = 4,
        Work = 5
    }

    /// <summary>
    /// Aggregated, village-level awareness of individual needs. This is a delayed/smoothed view (what authorities "know"),
    /// and can be influenced by a leader/seat entity near the village center.
    /// </summary>
    public struct VillageNeedAwareness : IComponentData
    {
        public float Hunger;
        public float Rest;
        public float Faith;
        public float Safety;
        public float Social;
        public float Work;
        public float MaxNeed;
        public VillageNeedChannel DominantNeed;
        public int SampleCount;
        public Entity InfluenceEntity;
        public byte DerivedMembers;
        public uint LastSampleTick;
    }

    /// <summary>
    /// Tracks the currently active village-issued construction project.
    /// </summary>
    public struct VillageConstructionRuntime : IComponentData
    {
        public Entity ActiveSite;
        public Entity ActiveWorker;
        public VillageBuildingType ActiveBuildingType;
        public uint LastIssuedTick;
    }

    /// <summary>
    /// Marks a jobsite as being issued by a village, with a coarse building type for future conversion.
    /// </summary>
    public struct VillageConstructionSite : IComponentData
    {
        public Entity Village;
        public VillageBuildingType BuildingType;
        public byte Priority;
        public uint IssuedTick;
    }

    /// <summary>
    /// Marks a villager as temporarily controlled by a village-level construction order.
    /// </summary>
    public struct VillageConstructionWorker : IComponentData
    {
        public Entity Village;
        public Entity Site;
        public Entity ResumeSettlement;
        public uint ResumeRandomState;
        public byte HadSettlementState;
        public float MoveSpeed;
        public float BuildRatePerSecond;
        public float ArrivalDistanceSq;
    }

    /// <summary>
    /// Optional runtime config for enabling the village build slice in headless runs.
    /// Presentation runs do not require this component.
    /// </summary>
    public struct VillageBuildSliceConfig : IComponentData
    {
        public byte EnableInHeadless;
    }
}
