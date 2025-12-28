using Godgame.Presentation;
using Godgame.Villages;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Miracles
{
    /// <summary>
    /// Component for tracking miracle telemetry data.
    /// </summary>
    public struct MiracleTelemetryData : IComponentData
    {
        /// <summary>Miracles cast this session</summary>
        public int MiraclesCastThisSession;
        /// <summary>Region miracles cast</summary>
        public int RegionMiraclesCast;
        /// <summary>Village miracles cast</summary>
        public int VillageMiraclesCast;
    }

    /// <summary>
    /// Component for tracking before/after snapshots for a miracle effect.
    /// </summary>
    public struct MiracleSnapshot : IComponentData
    {
        /// <summary>Village entity this snapshot is for</summary>
        public Entity VillageEntity;
        /// <summary>Tick when snapshot was taken</summary>
        public uint SnapshotTick;
        /// <summary>Population at snapshot time</summary>
        public int Population;
        /// <summary>Food amount at snapshot time</summary>
        public int Food;
        /// <summary>Wealth at snapshot time</summary>
        public int Wealth;
    }

    /// <summary>
    /// System that tracks miracle casts and captures before/after metrics.
    /// Cheap and aggregated: Only tracks village-level stats, not per-villager.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(Godgame_MiraclePresentationSystem))]
    public partial struct Godgame_MiracleTelemetrySystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            // Create telemetry singleton if not exists
            var query = state.GetEntityQuery(typeof(MiracleTelemetryData));
            if (query.IsEmpty)
            {
                var telemetryEntity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponentData(telemetryEntity, new MiracleTelemetryData());
                state.EntityManager.SetName(telemetryEntity, "MiracleTelemetrySingleton");
            }
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<TimeState>(out var timeState))
            {
                return;
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            // Track new miracle casts
            foreach (var (effect, entity) in SystemAPI.Query<RefRO<MiracleEffect>>().WithEntityAccess())
            {
                // Check if this is a new miracle (just spawned)
                // In a full implementation, we'd track spawned miracles and increment counters
                // For now, this is a placeholder
            }

            // Process snapshots and compare after N ticks
            foreach (var (snapshotRef, entity) in SystemAPI.Query<RefRW<MiracleSnapshot>>().WithEntityAccess())
            {
                ref var snapshot = ref snapshotRef.ValueRW;
                uint ticksSinceSnapshot = timeState.Tick - snapshot.SnapshotTick;

                // After 300 ticks (10 seconds at 30 TPS), compare and log
                if (ticksSinceSnapshot >= 300)
                {
                    if (state.EntityManager.Exists(snapshot.VillageEntity))
                    {
                        CompareSnapshot(ref state, snapshot);
                    }

                    ecb.DestroyEntity(entity);
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        [BurstCompile]
        private void CompareSnapshot(ref SystemState state, MiracleSnapshot snapshot)
        {
            if (!state.EntityManager.HasComponent<Village>(snapshot.VillageEntity))
            {
                return;
            }

            var village = state.EntityManager.GetComponentData<Village>(snapshot.VillageEntity);
            int currentPopulation = (int)village.MemberCount;

            int food = 0;
            if (state.EntityManager.HasBuffer<VillageResource>(snapshot.VillageEntity))
            {
                var resources = state.EntityManager.GetBuffer<VillageResource>(snapshot.VillageEntity);
                for (int i = 0; i < resources.Length; i++)
                {
                    if (resources[i].ResourceTypeIndex >= 40 && resources[i].ResourceTypeIndex <= 44)
                    {
                        food += resources[i].Quantity;
                    }
                }
            }

            int populationChange = currentPopulation - snapshot.Population;
            int foodChange = food - snapshot.Food;

            // Log comparison (in a full implementation, this would go to telemetry/metrics)
            // For now, this is a placeholder
        }
    }
}

