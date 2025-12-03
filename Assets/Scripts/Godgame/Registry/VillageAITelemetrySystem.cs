using Godgame.Villages;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Telemetry;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Registry
{
    /// <summary>
    /// Publishes village AI telemetry (initiative, alignment, events) to shared telemetry stream.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct VillageAITelemetrySystem : ISystem
    {
        private static readonly FixedString64Bytes MetricVillageInitiative = new FixedString64Bytes("godgame.village.initiative");
        private static readonly FixedString64Bytes MetricVillageAlignmentMoral = new FixedString64Bytes("godgame.village.alignment.moral");
        private static readonly FixedString64Bytes MetricVillageAlignmentOrder = new FixedString64Bytes("godgame.village.alignment.order");
        private static readonly FixedString64Bytes MetricVillageAlignmentPurity = new FixedString64Bytes("godgame.village.alignment.purity");
        private static readonly FixedString64Bytes MetricVillageState = new FixedString64Bytes("godgame.village.state");
        private static readonly FixedString64Bytes MetricVillageEvents = new FixedString64Bytes("godgame.village.events");

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Village>();
            state.RequireForUpdate<TelemetryStream>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var buffer = SystemAPI.GetSingletonBuffer<TelemetryMetric>();

            foreach (var (village, initiative, alignment, villageState, events) in SystemAPI
                         .Query<RefRO<Village>, RefRO<VillageInitiativeState>, RefRO<VillageAlignmentState>, RefRO<VillageState>, DynamicBuffer<VillageEvent>>())
            {
                var villageId = village.ValueRO.VillageId;

                buffer.AddMetric(MetricVillageInitiative, initiative.ValueRO.CurrentInitiative, TelemetryMetricUnit.Ratio);
                buffer.AddMetric(MetricVillageAlignmentMoral, alignment.ValueRO.MoralAxis, TelemetryMetricUnit.Count);
                buffer.AddMetric(MetricVillageAlignmentOrder, alignment.ValueRO.OrderAxis, TelemetryMetricUnit.Count);
                buffer.AddMetric(MetricVillageAlignmentPurity, alignment.ValueRO.PurityAxis, TelemetryMetricUnit.Count);
                buffer.AddMetric(MetricVillageState, (byte)villageState.ValueRO.CurrentState, TelemetryMetricUnit.Count);
                buffer.AddMetric(MetricVillageEvents, events.Length, TelemetryMetricUnit.Count);
            }
        }
    }
}
