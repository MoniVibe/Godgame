using Godgame.Villages;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Telemetry;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Telemetry
{
    /// <summary>
    /// Samples command queue pressure and writes audit records.
    /// Currently inspects village expansion requests because they already persist request ticks.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(VillageAIDecisionSystem))]
    public partial struct GodgameQueueTelemetrySystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Village>();
            state.RequireForUpdate<BehaviorTelemetryState>();
            state.RequireForUpdate<TimeState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var telemetryEntity = SystemAPI.GetSingletonEntity<BehaviorTelemetryState>();
            if (!state.EntityManager.HasBuffer<GodgameQueuePressureRecord>(telemetryEntity))
            {
                return;
            }

            var buffer = state.EntityManager.GetBuffer<GodgameQueuePressureRecord>(telemetryEntity);
            var tick = SystemAPI.GetSingleton<TimeState>().Tick;
            const uint staleThresholdTicks = 600;

            foreach (var (village, requests, entity) in SystemAPI
                         .Query<RefRO<Village>, DynamicBuffer<VillageExpansionRequest>>()
                         .WithEntityAccess())
            {
                if (requests.Length == 0)
                {
                    continue;
                }

                var seenKeys = new NativeParallelHashSet<int>(requests.Length, Allocator.Temp);
                ushort duplicateCount = 0;
                ushort staleCount = 0;
                double waitSum = 0;

                for (int i = 0; i < requests.Length; i++)
                {
                    var request = requests[i];
                    var key = BuildRequestKey(ref request);
                    if (!seenKeys.Add(key))
                    {
                        duplicateCount++;
                    }

                    if (tick - request.RequestTick > staleThresholdTicks)
                    {
                        staleCount++;
                    }

                    waitSum += tick - request.RequestTick;
                }

                seenKeys.Dispose();

                FixedString64Bytes queueName = default;
                queueName.Append("village-expansion/");
                queueName.Append(village.ValueRO.VillageId);

                buffer.Add(new GodgameQueuePressureRecord
                {
                    Tick = tick,
                    QueueName = queueName,
                    Length = (ushort)requests.Length,
                    DuplicateCount = duplicateCount,
                    StaleCount = staleCount,
                    AverageWaitTicks = (float)(waitSum / requests.Length)
                });
            }
        }

        private static int BuildRequestKey(ref VillageExpansionRequest request)
        {
            var hash = (int)math.hash(request.Position);
            return (request.BuildingType << 24) ^ hash;
        }
    }
}
