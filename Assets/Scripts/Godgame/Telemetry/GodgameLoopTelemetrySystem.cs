using Godgame.Registry;
using Godgame.Villagers;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Resource;
using PureDOTS.Runtime.Telemetry;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Telemetry
{
    /// <summary>
    /// Emits loop telemetry metrics for headless observability.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct GodgameLoopTelemetrySystem : ISystem
    {
        private FixedString64Bytes _extractWorkersKey;
        private FixedString64Bytes _extractOutputKey;
        private FixedString64Bytes _extractBufferKey;
        private FixedString64Bytes _extractNodesKey;
        private FixedString64Bytes _logisticsInTransitKey;
        private FixedString64Bytes _logisticsThroughputKey;
        private FixedString64Bytes _logisticsUtilizationKey;
        private FixedString64Bytes _logisticsBacklogKey;

        private float _lastStored;
        private uint _lastTick;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TelemetryStream>();
            state.RequireForUpdate<TelemetryExportConfig>();
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<StorehouseRegistry>();

            _extractWorkersKey = "loop.extract.activeWorkers";
            _extractOutputKey = "loop.extract.outputPerTick";
            _extractBufferKey = "loop.extract.buffer";
            _extractNodesKey = "loop.extract.nodes.active";
            _logisticsInTransitKey = "loop.logistics.inTransit";
            _logisticsThroughputKey = "loop.logistics.throughput";
            _logisticsUtilizationKey = "loop.logistics.storage.utilization";
            _logisticsBacklogKey = "loop.logistics.backlog";
        }

        public void OnDestroy(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
            var config = SystemAPI.GetSingleton<TelemetryExportConfig>();
            if (config.Enabled == 0 || (config.Flags & TelemetryExportFlags.IncludeTelemetryMetrics) == 0)
            {
                return;
            }

            var telemetryEntity = SystemAPI.GetSingletonEntity<TelemetryStream>();
            if (!state.EntityManager.HasBuffer<TelemetryMetric>(telemetryEntity))
            {
                return;
            }

            var tick = SystemAPI.GetSingleton<TimeState>().Tick;
            var cadence = config.CadenceTicks > 0 ? config.CadenceTicks : 30u;
            if (tick % cadence != 0)
            {
                return;
            }
            var registry = SystemAPI.GetSingleton<StorehouseRegistry>();

            var utilization = registry.TotalCapacity > 0f
                ? registry.TotalStored / registry.TotalCapacity
                : 0f;

            var throughput = 0f;
            if (_lastTick > 0 && tick > _lastTick)
            {
                var delta = math.max(0f, registry.TotalStored - _lastStored);
                throughput = delta / (tick - _lastTick);
            }

            _lastStored = registry.TotalStored;
            _lastTick = tick;

            var activeWorkers = 0;
            var inTransit = 0;
            foreach (var job in SystemAPI.Query<RefRO<VillagerJobState>>())
            {
                if (job.ValueRO.Type != JobType.Gather)
                {
                    continue;
                }

                if (job.ValueRO.Phase == JobPhase.Gather)
                {
                    activeWorkers++;
                }
                else if (job.ValueRO.Phase == JobPhase.NavigateToStorehouse || job.ValueRO.Phase == JobPhase.Deliver)
                {
                    inTransit++;
                }
            }

            var bufferAmount = 0f;
            var backlog = 0;
            foreach (var carry in SystemAPI.Query<RefRO<VillagerCarrying>>())
            {
                backlog++;
                bufferAmount += math.max(0f, carry.ValueRO.Amount);
            }

            var nodesActive = 0;
            foreach (var node in SystemAPI.Query<RefRO<GodgameResourceNode>>())
            {
                if (node.ValueRO.RemainingAmount > 0f)
                {
                    nodesActive++;
                }
            }

            var buffer = state.EntityManager.GetBuffer<TelemetryMetric>(telemetryEntity);
            buffer.AddMetric(_extractWorkersKey, activeWorkers, TelemetryMetricUnit.Count);
            buffer.AddMetric(_extractOutputKey, throughput, TelemetryMetricUnit.Custom);
            buffer.AddMetric(_extractBufferKey, bufferAmount, TelemetryMetricUnit.Custom);
            buffer.AddMetric(_extractNodesKey, nodesActive, TelemetryMetricUnit.Count);
            buffer.AddMetric(_logisticsInTransitKey, inTransit, TelemetryMetricUnit.Count);
            buffer.AddMetric(_logisticsThroughputKey, throughput, TelemetryMetricUnit.Custom);
            buffer.AddMetric(_logisticsUtilizationKey, utilization, TelemetryMetricUnit.Ratio);
            buffer.AddMetric(_logisticsBacklogKey, backlog, TelemetryMetricUnit.Count);
        }
    }
}
