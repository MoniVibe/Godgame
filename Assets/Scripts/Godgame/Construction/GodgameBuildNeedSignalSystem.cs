using PureDOTS.Runtime.Aggregate;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Construction;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Construction
{
    /// <summary>
    /// Godgame-specific build need signal generation.
    /// Uses VillagerNeeds component to emit BuildNeedSignals to village/guild buffers.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(Unity.Entities.SimulationSystemGroup))]
    [UpdateAfter(typeof(PureDOTS.Systems.Construction.BuildNeedSignalSystem))]
    public partial struct GodgameBuildNeedSignalSystem : ISystem
    {
        private const float HousingEnergyThreshold = 30f; // Energy < 30% triggers housing need
        private const float HousingCheckDuration = 300u; // Check every ~3.3 seconds at 90 TPS
        private const float WorshipMoraleThreshold = 40f; // Morale < 40% triggers worship need
        private const float StorageFillThreshold = 0.8f; // Storage > 80% triggers storage need

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
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

            var currentTick = timeState.Tick;
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            // Process villagers with needs and group membership
            foreach (var (needs, membership, transform, entity) in SystemAPI.Query<
                RefRO<PureDOTS.Runtime.Components.VillagerNeeds>,
                RefRO<GroupMembership>,
                RefRO<LocalTransform>>().WithEntityAccess())
            {
                var groupEntity = membership.ValueRO.Group;
                if (groupEntity == Entity.Null || !SystemAPI.Exists(groupEntity))
                    continue;

                // Ensure group has BuildNeedSignal buffer
                if (!SystemAPI.HasBuffer<BuildNeedSignal>(groupEntity))
                {
                    ecb.AddBuffer<BuildNeedSignal>(groupEntity);
                    continue; // Buffer will be available next frame
                }

                var needsValue = needs.ValueRO;
                var position = transform.ValueRO.Position;

                // Get writable buffer for this group
                var signals = SystemAPI.GetBuffer<BuildNeedSignal>(groupEntity);

                // Housing: If Energy < 30% for extended period
                if (needsValue.EnergyFloat < HousingEnergyThreshold)
                {
                    // TODO: Check if villager has valid bed/shelter
                    // For now, emit signal if energy is low
                    var strength = math.saturate(1f - (needsValue.EnergyFloat / HousingEnergyThreshold));
                    signals.Add(new BuildNeedSignal
                    {
                        Category = BuildCategory.Housing,
                        Strength = strength,
                        Position = position,
                        SourceEntity = entity,
                        EmittedTick = currentTick
                    });
                }

                // Worship: If Morale < 40%
                if (needsValue.MoraleFloat < WorshipMoraleThreshold)
                {
                    // TODO: Check if temple/altar exists within radius
                    // For now, emit signal if morale is low
                    var strength = math.saturate(1f - (needsValue.MoraleFloat / WorshipMoraleThreshold));
                    signals.Add(new BuildNeedSignal
                    {
                        Category = BuildCategory.Worship,
                        Strength = strength,
                        Position = position,
                        SourceEntity = entity,
                        EmittedTick = currentTick
                    });
                }

                // Storage: Check resource pile overflow (stub - would check nearby piles)
                // TODO: Implement storage need detection based on resource pile fill ratio
            }
        }
    }
}

