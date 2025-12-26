using Unity.Entities;

namespace Godgame.Villagers
{
    public enum VillagerGoalReasonKind : byte
    {
        None = 0,
        Threat = 1,
        NeedHunger = 2,
        NeedRest = 3,
        NeedFaith = 4,
        NeedSafety = 5,
        NeedSocial = 6,
        NeedWork = 7,
        FocusLocked = 8
    }

    /// <summary>
    /// Captures the current motivation/why behind a villager goal.
    /// </summary>
    public struct VillagerGoalReason : IComponentData
    {
        public VillagerGoalReasonKind Kind;
        public float Urgency;
        public Entity SourceEntity;
        public uint SetTick;
    }
}
