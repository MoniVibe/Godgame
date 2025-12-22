using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Logistics
{
    public enum LogisticsHaulPhase : byte
    {
        Idle = 0,
        MoveToBoard = 1,
        Claiming = 2,
        MoveToSource = 3,
        Pickup = 4,
        MoveToSite = 5,
        Dropoff = 6
    }

    public struct LogisticsHaulerTag : IComponentData
    {
    }

    public struct LogisticsHauler : IComponentData
    {
        public float CarryCapacity;
        public float MoveSpeed;
        public float InteractRange;
        public uint ClaimCooldownTicks;
    }

    public struct LogisticsHaulState : IComponentData
    {
        public LogisticsHaulPhase Phase;
        public Entity BoardEntity;
        public Entity SiteEntity;
        public Entity SourceEntity;
        public ushort ResourceTypeIndex;
        public float ReservedUnits;
        public float CarryingUnits;
        public uint ReservationId;
        public uint LastClaimTick;
        public uint LastProgressTick;
    }
}
