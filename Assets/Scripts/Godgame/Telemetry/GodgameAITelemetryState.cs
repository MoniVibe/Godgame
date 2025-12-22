using Unity.Entities;
using VillagerAIState = PureDOTS.Runtime.Components.VillagerAIState;

namespace Godgame.Telemetry
{
    /// <summary>
    /// Tracks last-known AI role/profile + goals so telemetry events fire only when values change.
    /// </summary>
    public struct GodgameAITelemetryState : IComponentData
    {
        public VillagerAIState.Goal LastGoal;
        public uint LastGoalTick;
        public ushort LastRoleId;
        public ushort LastDoctrineId;
        public uint LastProfileHash;
        public byte ProfileLogged;
    }
}
