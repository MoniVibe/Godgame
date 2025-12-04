using PureDOTS.Runtime.Components;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Miracles
{
    /// <summary>
    /// Per-hand casting state tracked by miracle input and release systems.
    /// </summary>
    public struct MiracleCasterState : IComponentData
    {
        public Entity HandEntity;
        public byte SelectedSlot;
        public byte SustainedCastHeld;
        public byte ThrowCastTriggered;
        public byte IsCasting;
        public float ChargePercent;
        public uint LastCastTick;
    }

    /// <summary>
    /// Defines an available miracle slot on a caster, including prefab/config binding.
    /// Uses PureDOTS.Runtime.Components.MiracleSlotDefinition.
    /// </summary>
    // Note: MiracleSlotDefinition is defined in PureDOTS.Runtime.Components namespace
    // This file imports that namespace, so MiracleSlotDefinition refers to PureDOTS version

    /// <summary>
    /// Configuration payload for rain miracles.
    /// </summary>
    public struct RainMiracleConfig : IComponentData
    {
        public Entity RainCloudPrefab;
        public int CloudCount;
        public float SpawnRadius;
        public float SpawnHeightOffset;
        public float SpawnSpreadAngle;
        public uint Seed;
    }

    /// <summary>
    /// Tag component marking the buffer entity that holds pending rain miracle commands.
    /// </summary>
    public struct RainMiracleCommandQueue : IComponentData { }

    /// <summary>
    /// Command consumed by weather/FX systems to spawn rain clouds.
    /// </summary>
    [InternalBufferCapacity(4)]
    public struct RainMiracleCommand : IBufferElementData
    {
        public float3 Center;
        public int CloudCount;
        public float Radius;
        public float HeightOffset;
        public Entity RainCloudPrefab;
        public uint Seed;
    }

    /// <summary>
    /// Event raised by input/presentation to trigger a miracle release.
    /// Uses PureDOTS.Runtime.Components.MiracleReleaseEvent.
    /// </summary>
    // Note: MiracleReleaseEvent is defined in PureDOTS.Runtime.Components namespace
    // This file imports that namespace, so MiracleReleaseEvent refers to PureDOTS version
}
