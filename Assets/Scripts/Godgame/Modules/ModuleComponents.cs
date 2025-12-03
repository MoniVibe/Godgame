using PureDOTS.Runtime.Skills;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Modules
{
    public enum ModuleStatus : byte
    {
        Inactive = 0,
        Operational = 1,
        Damaged = 2,
        Refit = 3,
        Offline = 4
    }

    /// <summary>
    /// Buffered requests to apply immediate damage to modules by slot index.
    /// </summary>
    public struct ModuleDamageRequest : IBufferElementData
    {
        public byte SlotIndex;
        public float Damage;
    }

    public struct ModuleSlot : IBufferElementData
    {
        public byte SlotIndex;
        public FixedString64Bytes SlotType;
        public Entity InstalledModule;
    }

    public struct ModuleData : IComponentData
    {
        public FixedString64Bytes ModuleId;
        public FixedString64Bytes SlotType;
        public ModuleStatus Status;
        public float MaxCondition;
        public float Condition;
        public float DegradationPerSecond;
        public uint LastServiceTick;
    }

    public struct ModuleRefitRequest : IComponentData
    {
        public float WorkRemaining;
        public float TargetCondition;
        public SkillId SkillId;
        public byte AutoRequested;
    }

    public struct ModuleMaintainerAssignment : IComponentData
    {
        public Entity WorkerEntity;
    }

    /// <summary>
    /// Optional host reference for modules installed into slot buffers.
    /// </summary>
    public struct ModuleHostReference : IComponentData
    {
        public Entity Host;
    }

    /// <summary>
    /// Worker entities contributing skills to module upkeep on a host entity.
    /// </summary>
    public struct ModuleMaintainerLink : IBufferElementData
    {
        public Entity Worker;
    }

    public struct ModuleMaintenanceConfig : IComponentData
    {
        public float BaseWorkRate;
        public float SkillRateBonus;
        public float WorkRequiredPerCondition;
        public float AutoRepairThreshold;
        public float CriticalThreshold;
        public float ResourceCostPerWork;
    }

    public static class ModuleMaintenanceDefaults
    {
        public static ModuleMaintenanceConfig Create()
        {
            return new ModuleMaintenanceConfig
            {
                BaseWorkRate = 6f,
                SkillRateBonus = 0.2f,
                WorkRequiredPerCondition = 0.05f,
                AutoRepairThreshold = 0.55f,
                CriticalThreshold = 0.2f,
                ResourceCostPerWork = 0f
            };
        }
    }

    /// <summary>
    /// Optional resource wallet used to pay for refit/repair work.
    /// </summary>
    public struct ModuleResourceWallet : IComponentData
    {
        public float Resources;
    }

    public static class ModuleTelemetryKeys
    {
        public static readonly FixedString64Bytes TotalModules = "modules.total";
        public static readonly FixedString64Bytes DamagedModules = "modules.damaged";
        public static readonly FixedString64Bytes OfflineModules = "modules.offline";
        public static readonly FixedString64Bytes RefitsQueued = "modules.refitsQueued";
        public static readonly FixedString64Bytes RefitCompleted = "modules.refitCompleted";
        public static readonly FixedString64Bytes RepairCompleted = "modules.repairCompleted";
    }
}
