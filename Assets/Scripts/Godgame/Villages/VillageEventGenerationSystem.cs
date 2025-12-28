using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Villages
{
    /// <summary>
    /// Generates deterministic village events (seasonal, social, crisis, miracle, threat).
    /// Evaluates thresholds and enqueues events into VillageEvent buffers.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(VillageInitiativeSystem))]
    public partial struct VillageEventGenerationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Village>();
            state.RequireForUpdate<TimeState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var tick = SystemAPI.GetSingleton<TimeState>().Tick;
            var random = new Unity.Mathematics.Random((uint)(tick + 1));

            var job = new GenerateEventsJob
            {
                Tick = tick,
                Random = random
            };
            state.Dependency = job.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        public partial struct GenerateEventsJob : IJobEntity
        {
            public uint Tick;
            public Unity.Mathematics.Random Random;

            void Execute(
                Entity entity,
                ref DynamicBuffer<VillageEvent> events,
                in VillageState state,
                in VillageResourceSummary resources,
                in VillageWorshipState worship,
                in VillageAlignmentState alignment)
            {
                // Check for crisis events (shortages, disasters)
                if (resources.FoodStored < resources.FoodUpkeep * 0.5f)
                {
                    EnqueueEvent(ref events, VillageEventFamily.Crisis, Tick, 0.8f, alignment);
                }

                if (resources.ConstructionStored < resources.ConstructionUpkeep * 0.3f)
                {
                    EnqueueEvent(ref events, VillageEventFamily.Crisis, Tick, 0.6f, alignment);
                }

                // Check for social events (festivals, births)
                if (worship.AverageMorale > 75f && resources.FoodStored > resources.FoodUpkeep * 1.3f)
                {
                    if (Random.NextFloat() < 0.1f) // 10% chance per tick when conditions met
                    {
                        EnqueueEvent(ref events, VillageEventFamily.Social, Tick, 0.5f, alignment);
                    }
                }

                // Check for threat events (raids, monsters)
                // Simplified - can be expanded with threat detection systems
            }

            private static void EnqueueEvent(
                ref DynamicBuffer<VillageEvent> events,
                VillageEventFamily family,
                uint tick,
                float magnitude,
                VillageAlignmentState alignment)
            {
                // Limit buffer size (ring buffer behavior)
                if (events.Length >= 32)
                {
                    events.RemoveAt(0);
                }

                var axesMask = (byte)((alignment.MoralAxis != 0 ? 1 : 0) |
                                     ((alignment.OrderAxis != 0 ? 1 : 0) << 1) |
                                     ((alignment.PurityAxis != 0 ? 1 : 0) << 2));

                events.Add(new VillageEvent
                {
                    EventId = (uint)events.Length,
                    TriggerTick = tick,
                    Seed = (uint)(tick * 31 + (int)family),
                    Magnitude = magnitude,
                    EventFamily = (byte)family,
                    AffectedAxesMask = axesMask
                });
            }
        }
    }
}

