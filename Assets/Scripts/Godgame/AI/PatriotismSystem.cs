using Godgame.Bands;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.AI
{
    /// <summary>
    /// Updates villager patriotism based on time served, family, assets, and alignment.
    /// Low patriotism triggers migration consideration; very low triggers desertion.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct PatriotismUpdateSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();

            // Create config singleton if it doesn't exist
            if (!SystemAPI.HasSingleton<PatriotismConfig>())
            {
                var configEntity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponentData(configEntity, PatriotismConfig.Default);
            }
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            if (timeState.IsPaused) return;

            var config = SystemAPI.GetSingleton<PatriotismConfig>();
            var currentTick = timeState.Tick;
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            // Update patriotism for all villagers
            foreach (var (patriotism, entity) in SystemAPI.Query<RefRW<VillagerPatriotism>>()
                .WithEntityAccess())
            {
                var patriotismValue = patriotism.ValueRW;

                // Increment time in aggregate
                patriotismValue.TicksInAggregate++;

                // Recalculate score periodically (every 100 ticks)
                if ((currentTick - patriotismValue.LastUpdateTick) >= 100)
                {
                    patriotismValue.RecalculateScore();
                    patriotismValue.LastUpdateTick = currentTick;

                    // Check for migration consideration
                    if (patriotismValue.Score < config.MigrationThreshold)
                    {
                        // Add migration consideration tag if not already present
                        if (!SystemAPI.HasComponent<ConsideringMigrationTag>(entity))
                        {
                            ecb.AddComponent(entity, new ConsideringMigrationTag
                            {
                                StartedTick = currentTick,
                                TargetEntity = Entity.Null
                            });
                        }
                    }
                    else
                    {
                        // Remove migration tag if patriotism recovered
                        if (SystemAPI.HasComponent<ConsideringMigrationTag>(entity))
                        {
                            ecb.RemoveComponent<ConsideringMigrationTag>(entity);
                        }
                    }
                }

                patriotism.ValueRW = patriotismValue;
            }

            // Process migration considerations
            foreach (var (migrationTag, patriotism, entity) in SystemAPI.Query<
                RefRO<ConsideringMigrationTag>,
                RefRO<VillagerPatriotism>>()
                .WithEntityAccess())
            {
                var considerationDuration = currentTick - migrationTag.ValueRO.StartedTick;

                // If consideration period passed and patriotism still low, desert
                if (considerationDuration >= config.MigrationConsiderationPeriod &&
                    patriotism.ValueRO.Score < config.DesertionThreshold)
                {
                    // Mark as deserted
                    ecb.AddComponent(entity, new DesertedTag
                    {
                        DesertedFrom = patriotism.ValueRO.VillageEntity,
                        DesertedTick = currentTick
                    });

                    // Remove migration consideration
                    ecb.RemoveComponent<ConsideringMigrationTag>(entity);

                    // Remove band membership if present
                    if (SystemAPI.HasComponent<BandMembership>(entity))
                    {
                        ecb.RemoveComponent<BandMembership>(entity);
                    }
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    /// <summary>
    /// Applies patriotism changes from events (victories, defeats, leadership actions).
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(PatriotismUpdateSystem))]
    public partial struct PatriotismEventSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PatriotismConfig>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // This system would process patriotism-affecting events
            // Events would be created by other systems (combat, leadership, etc.)
            // For now, this is a placeholder for the event processing infrastructure
        }

        /// <summary>
        /// Static helper to apply a victory boost to a villager's patriotism.
        /// Called by other systems when shared victories occur.
        /// </summary>
        public static void ApplyVictoryBoost(ref VillagerPatriotism patriotism, byte boostAmount)
        {
            patriotism.Score = (byte)math.min(100, patriotism.Score + boostAmount);
        }

        /// <summary>
        /// Static helper to apply a defeat penalty to a villager's patriotism.
        /// Called by other systems when shared defeats occur.
        /// </summary>
        public static void ApplyDefeatPenalty(ref VillagerPatriotism patriotism, byte penaltyAmount)
        {
            patriotism.Score = (byte)math.max(0, patriotism.Score - penaltyAmount);
        }
    }
}

