// [TRI-STUB] This is an ahead-of-time stub. Safe to compile, does nothing at runtime.
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Runtime
{
    public struct MiracleRequest : IComponentData
    {
        public int RequestId;
        public Entity Target;
    }

    public struct VillagerNeed : IComponentData
    {
        public byte NeedType;
        public byte Urgency;
    }

    public struct NeedSatisfaction : IComponentData
    {
        public byte NeedType;
        public float Value;
    }

    public struct BandCohesionState : IComponentData
    {
        public byte Cohesion;
    }

    public struct VillageBoundary : IComponentData
    {
        public float Radius;
    }

    public struct ConstructionPhase : IComponentData
    {
        public byte Phase;
    }

    public struct AlertState : IComponentData
    {
        public byte Level;
    }

    public struct GrudgeEntry : IBufferElementData
    {
        public Entity Target;
        public byte Severity;
    }

    public struct LanguageAffinity : IComponentData
    {
        public byte Affinity;
    }

    public struct HandInputIntent : IComponentData
    {
        public byte Intent;
    }

    public struct VillagerBehaviorTreeRef : IComponentData
    {
        public int TreeId;
        public byte Mode;
    }

    public struct VillagerBehaviorState : IComponentData
    {
        public int ActiveNodeId;
        public byte Status;
    }

    public struct VillagerBehaviorNodeState : IBufferElementData
    {
        public int NodeId;
        public byte Flags;
    }

    public struct VillagerPerceptionConfig : IComponentData
    {
        public float Range;
        public byte ChannelMask;
        public byte MaxStimuli;
    }

    public struct VillagerPerceptionStimulus : IBufferElementData
    {
        public Entity Source;
        public float Strength;
        public byte Channel;
        public uint Timestamp;
    }

    public struct VillagerInitiativeState : IComponentData
    {
        public float Charge;
    }

    public struct VillagerNeedElement : IBufferElementData
    {
        public byte NeedType;
        public float Urgency;
    }

    public struct PathIntent : IComponentData
    {
        public Entity Destination;
        public byte Mode;
    }

    public struct DestinationWaypoint : IComponentData
    {
        public float3 Position;
    }

    public struct NavigationTicketHandle : IComponentData
    {
        public Entity Ticket;
    }

    public struct VillagerSensorRig : IComponentData
    {
        public byte ChannelsMask;
    }

    public struct VillagerInterruptTicket : IComponentData
    {
        public byte Category;
    }

    public struct GodgameTimeCommand : IComponentData
    {
        public byte Command;
    }

    public struct GodgameSituationAnchor : IComponentData
    {
        public int SituationId;
    }

    public struct TelemetryStreamHook : IComponentData
    {
        public int StreamId;
    }

    public struct SaveSlotRequest : IComponentData
    {
        public int SlotIndex;
    }

    public struct BandHandle : IComponentData
    {
        public int BandId;
    }

    public struct BandMembershipElement : IBufferElementData
    {
        public Entity Member;
        public byte Role;
    }

    public struct GuildHandle : IComponentData
    {
        public int GuildId;
    }

    public struct InterceptState : IComponentData
    {
        public byte Active;
    }

    public struct WorkshopRecipeHandle : IComponentData
    {
        public int RecipeId;
    }

    public struct StorehouseInventoryTag : IComponentData { }

    public struct VillagerCraftJob : IComponentData
    {
        public Entity TargetFacility;
    }

    public struct MiracleFuelStockpile : IComponentData
    {
        public float Amount;
    }

    public struct VillageEconomySnapshot : IComponentData
    {
        public float ProductionValue;
        public float ConsumptionValue;
    }

    public struct CraftingQueueEntry : IComponentData
    {
        public int RecipeId;
    }

    public struct CraftQualityState : IComponentData
    {
        public float QualityScore;
    }
}
