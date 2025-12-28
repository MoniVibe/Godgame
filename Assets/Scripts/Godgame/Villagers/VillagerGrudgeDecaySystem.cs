using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// System that decays grudge intensity over time based on villager's VengefulScore.
    /// Forgiving villagers forget faster; vengeful villagers hold grudges longer.
    /// Also handles grudge cleanup and ActiveGrudgeCount updates.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct VillagerGrudgeDecaySystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();

            // Create default config if not exists
            if (!SystemAPI.HasSingleton<GrudgeSystemConfig>())
            {
                var configEntity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponentData(configEntity, GrudgeSystemConfig.Default);
            }
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            var config = SystemAPI.GetSingleton<GrudgeSystemConfig>();

            // Calculate decay delta for this frame
            // Decay is per-day, so we need to convert frame time to day fraction
            float dayFraction = timeState.FixedDeltaTime * config.TicksPerDay / 86400f; // Assuming real-time seconds

            new GrudgeDecayJob
            {
                DayFraction = dayFraction,
                Config = config,
                CurrentTick = timeState.Tick
            }.ScheduleParallel();
        }

        partial struct GrudgeDecayJob : IJobEntity
        {
            public float DayFraction;
            public GrudgeSystemConfig Config;
            public uint CurrentTick;

            void Execute(
                ref VillagerBehavior behavior,
                ref DynamicBuffer<VillagerGrudge> grudges)
            {
                if (grudges.Length == 0)
                {
                    behavior.ActiveGrudgeCount = 0;
                    return;
                }

                var decayRatePerDay = behavior.GetGrudgeDecayRatePerDay();
                var decayThisFrame = decayRatePerDay * DayFraction;
                var isForgiving = behavior.VengefulScore < -40f;

                int activeCount = 0;
                int i = 0;

                while (i < grudges.Length)
                {
                    var grudge = grudges[i];

                    // Skip already resolved grudges (will be removed)
                    if (grudge.IsResolved != 0)
                    {
                        grudges.RemoveAtSwapBack(i);
                        continue;
                    }

                    // Check for instant forgiveness for forgiving villagers
                    if (isForgiving && Config.ForgivingInstantForgiveness != 0 &&
                        grudge.Intensity < Config.InstantForgivenessThreshold)
                    {
                        grudges.RemoveAtSwapBack(i);
                        continue;
                    }

                    // Apply decay
                    grudge.Intensity -= decayThisFrame;

                    // Remove if below threshold
                    if (grudge.Intensity < Config.MinIntensityThreshold)
                    {
                        grudges.RemoveAtSwapBack(i);
                        continue;
                    }

                    grudges[i] = grudge;
                    activeCount++;
                    i++;
                }

                // Enforce max grudges limit (remove weakest if over)
                while (grudges.Length > Config.MaxGrudgesPerVillager)
                {
                    int weakestIndex = 0;
                    float weakestIntensity = float.MaxValue;

                    for (int j = 0; j < grudges.Length; j++)
                    {
                        if (grudges[j].Intensity < weakestIntensity)
                        {
                            weakestIntensity = grudges[j].Intensity;
                            weakestIndex = j;
                        }
                    }

                    grudges.RemoveAtSwapBack(weakestIndex);
                    activeCount--;
                }

                behavior.ActiveGrudgeCount = (byte)grudges.Length;
            }
        }
    }

    /// <summary>
    /// System that randomizes behavior traits on spawn for entities with RandomizeBehaviorOnSpawn.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct VillagerBehaviorRandomizationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (randomize, behavior, entity) in SystemAPI
                         .Query<RefRO<RandomizeBehaviorOnSpawn>, RefRW<VillagerBehavior>>()
                         .WithEntityAccess())
            {
                // Use entity index + tick for deterministic but varied randomness
                var seed = (uint)(entity.Index + 1) * (timeState.Tick + 1);
                var random = new Unity.Mathematics.Random(seed);

                var range = randomize.ValueRO.Range;
                behavior.ValueRW.VengefulScore = random.NextFloat(-range, range);
                behavior.ValueRW.BoldScore = random.NextFloat(-range, range);
                behavior.ValueRW.RecalculateInitiative();

                // Update combat behavior if present
                if (SystemAPI.HasComponent<VillagerCombatBehavior>(entity))
                {
                    ecb.SetComponent(entity, VillagerCombatBehavior.FromBehavior(in behavior.ValueRW));
                }

                // Remove the randomization tag
                ecb.RemoveComponent<RandomizeBehaviorOnSpawn>(entity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
