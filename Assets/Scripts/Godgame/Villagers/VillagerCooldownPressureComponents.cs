using System;
using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Reasons for clearing a work cooldown early.
    /// </summary>
    public enum VillagerCooldownClearReason : byte
    {
        None = 0,
        Pressure = 1,
        Threat = 2,
        Food = 3,
        Manual = 4
    }

    [Flags]
    public enum VillagerCooldownPressureMask : byte
    {
        None = 0,
        Work = 1 << 0,
        Threat = 1 << 1,
        Food = 1 << 2
    }

    /// <summary>
    /// Tracks hysteresis state for cooldown pressure signals.
    /// </summary>
    public struct VillagerCooldownPressureState : IComponentData
    {
        public VillagerCooldownPressureMask ActiveMask;
        public VillagerCooldownClearReason LastClearReason;
        public uint LastClearTick;
    }
}
