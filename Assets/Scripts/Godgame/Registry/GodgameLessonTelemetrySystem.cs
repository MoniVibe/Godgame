using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Telemetry;
using PureDOTS.Runtime.Registry;
using PureDOTS.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using VillagerLessonRegistryEntry = PureDOTS.Runtime.Components.VillagerLessonRegistryEntry;

namespace Godgame.Registry
{
    /// <summary>
    /// Emits high-level lesson diagnostics for Godgame designers via the shared telemetry stream.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct GodgameLessonTelemetrySystem : ISystem
    {
        private EntityQuery _telemetryQuery;

        public void OnCreate(ref SystemState state)
        {
            _telemetryQuery = SystemAPI.QueryBuilder()
                .WithAll<TelemetryStream>()
                .Build();

            state.RequireForUpdate<TelemetryStream>();
            state.RequireForUpdate<VillagerRegistry>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!_telemetryQuery.TryGetSingletonEntity<TelemetryStream>(out var telemetryEntity))
            {
                return;
            }

            if (!SystemAPI.TryGetSingletonEntity<VillagerRegistry>(out var villagerRegistryEntity) ||
                !state.EntityManager.HasBuffer<VillagerLessonRegistryEntry>(villagerRegistryEntity))
            {
                return;
            }

            var lessons = state.EntityManager.GetBuffer<VillagerLessonRegistryEntry>(villagerRegistryEntity);
            if (!lessons.IsCreated || lessons.Length == 0)
            {
                return;
            }

            var buffer = state.EntityManager.GetBuffer<TelemetryMetric>(telemetryEntity);
            VillagerLessonTelemetry.AddMetrics(ref buffer, lessons, new FixedString64Bytes("godgame"));
        }
    }

    internal static class VillagerLessonTelemetry
    {
        private static readonly FixedString64Bytes s_TotalSuffix = new FixedString64Bytes(".villagers.lessons.total");
        private static readonly FixedString64Bytes s_CompletedSuffix = new FixedString64Bytes(".villagers.lessons.completed");
        private static readonly FixedString64Bytes s_AxisPrefix = new FixedString64Bytes(".villagers.lessons.axis.");

        private struct AxisAggregate
        {
            public float Progress;
            public int Count;
        }

        public static void AddMetrics(
            ref DynamicBuffer<TelemetryMetric> buffer,
            DynamicBuffer<VillagerLessonRegistryEntry> lessons,
            in FixedString64Bytes prefix)
        {
            if (!lessons.IsCreated || lessons.Length == 0)
            {
                return;
            }

            var completed = 0;
            var axisMap = new NativeHashMap<FixedString64Bytes, AxisAggregate>(math.max(lessons.Length, 8), Allocator.Temp);
            try
            {
                for (var i = 0; i < lessons.Length; i++)
                {
                    var lesson = lessons[i];
                    if (lesson.Progress >= 0.99f)
                    {
                        completed++;
                    }

                    if (lesson.AxisId.Length == 0)
                    {
                        continue;
                    }

                    axisMap.TryGetValue(lesson.AxisId, out var aggregate);
                    aggregate.Count++;
                    aggregate.Progress += lesson.Progress;
                    axisMap[lesson.AxisId] = aggregate;
                }

                var totalKey = new FixedString64Bytes(prefix);
                totalKey.Append(s_TotalSuffix);
                buffer.AddMetric(totalKey, lessons.Length);

                var completedKey = new FixedString64Bytes(prefix);
                completedKey.Append(s_CompletedSuffix);
                buffer.AddMetric(completedKey, completed);

                var kv = axisMap.GetKeyValueArrays(Allocator.Temp);
                try
                {
                    for (var i = 0; i < kv.Length; i++)
                    {
                        var aggregate = kv.Values[i];
                        if (aggregate.Count <= 0)
                        {
                            continue;
                        }

                        var key = new FixedString64Bytes(prefix);
                        key.Append(s_AxisPrefix);
                        key.Append(kv.Keys[i]);
                        var average = aggregate.Progress / aggregate.Count;
                        buffer.AddMetric(key, average, TelemetryMetricUnit.Ratio);
                    }
                }
                finally
                {
                    kv.Dispose();
                }
            }
            finally
            {
                axisMap.Dispose();
            }
        }
    }
}
