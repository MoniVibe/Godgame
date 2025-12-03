using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Registry;
using PureDOTS.Runtime.Spatial;
using PureDOTS.Runtime.Transport;
using PureDOTS.Systems;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Registry
{
    /// <summary>
    /// Clamps and timestamps logistics requests before the shared registry builds entries.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(TransportPhaseGroup))]
    [UpdateBefore(typeof(LogisticsRequestRegistrySystem))]
    public partial struct GodgameLogisticsSyncSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LogisticsRequestRegistry>();
            state.RequireForUpdate<LogisticsRequest>();
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            var rewindState = SystemAPI.GetSingleton<RewindState>();
            if (timeState.IsPaused || rewindState.Mode != RewindMode.Record)
            {
                return;
            }

            var registryEntity = SystemAPI.GetSingletonEntity<LogisticsRequestRegistry>();
            var metadata = SystemAPI.GetComponentRW<RegistryMetadata>(registryEntity);
            if (metadata.ValueRO.ArchetypeId == 0)
            {
                metadata.ValueRW.ArchetypeId = GodgameRegistryIds.LogisticsRequestArchetype;
            }

            var hasSpatialGrid = SystemAPI.TryGetSingleton<SpatialGridConfig>(out var spatialConfig) &&
                                 SystemAPI.TryGetSingleton<SpatialGridState>(out var spatialState) &&
                                 spatialConfig.CellCount > 0 &&
                                 spatialConfig.CellSize > 0f &&
                                 spatialState.Version > 0;

            foreach (var request in SystemAPI.Query<RefRW<LogisticsRequest>>())
            {
                var value = request.ValueRO;

                // Clamp resource index to non-negative range (no catalog mapping needed here).
                value.ResourceTypeIndex = (ushort)math.max(0, value.ResourceTypeIndex);

                // Refresh timestamps so registry metadata reflects the current tick.
                if (value.CreatedTick == 0)
                {
                    value.CreatedTick = timeState.Tick;
                }
                value.LastUpdateTick = timeState.Tick;

                // Ensure positions remain finite so spatial classification does not throw.
                if (!math.all(math.isfinite(value.SourcePosition)))
                {
                    value.SourcePosition = float3.zero;
                }

                if (!math.all(math.isfinite(value.DestinationPosition)))
                {
                    value.DestinationPosition = hasSpatialGrid
                        ? spatialConfig.WorldMin + (spatialConfig.WorldMax - spatialConfig.WorldMin) * 0.5f
                        : float3.zero;
                }

                request.ValueRW = value;
            }
        }
    }
}
