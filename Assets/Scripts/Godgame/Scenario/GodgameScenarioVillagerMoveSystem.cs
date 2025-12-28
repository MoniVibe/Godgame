#if !UNITY_DISABLE_AUTOMATIC_SYSTEM_BOOTSTRAP
using PureDOTS.Runtime.Components;
using PureDOTS.Systems;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Scenario
{
    /// <summary>
    /// Simple 2.5D (XZ plane) movement for the scenario settlement loop.
    /// Moves villagers toward the current VillagerAIState.TargetEntity so the scenario has visible motion
    /// and headless runs generate meaningful time-driven state changes.
    /// </summary>
    [DisableAutoCreation]
    [UpdateInGroup(typeof(VillagerSystemGroup))]
    [UpdateAfter(typeof(GodgameScenarioVillagerBehaviorSystem))]
    public partial struct GodgameScenarioVillagerMoveSystem : ISystem
    {
        private uint _lastTick;
        private byte _tickInitialized;
        private ComponentLookup<LocalTransform> _transformLookup;

        public void OnCreate(ref SystemState state)
        {
            // Hard-disabled: scenario loop should not simulate missing behaviors.
            state.Enabled = false;
            if (!state.Enabled)
            {
                return;
            }

            state.RequireForUpdate<ScenarioSceneTag>();
            state.RequireForUpdate<SettlementConfig>();
            state.RequireForUpdate<GodgameScenarioConfigBlobReference>();
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
            state.RequireForUpdate<SettlementVillagerState>();

            _tickInitialized = 0;
            _lastTick = 0;
            _transformLookup = state.GetComponentLookup<LocalTransform>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!IsScenario01(ref state))
            {
                state.Enabled = false;
                return;
            }

            var time = SystemAPI.GetSingleton<TimeState>();
            var deltaTime = ResolveDeltaTime(time);
            if (deltaTime <= 0f)
            {
                return;
            }
            if (time.IsPaused)
            {
                return;
            }

            var rewind = SystemAPI.GetSingleton<RewindState>();
            if (rewind.Mode != RewindMode.Record)
            {
                return;
            }

            _transformLookup.Update(ref state);

            const float speedUnitsPerSecond = 6.0f;
            var maxStep = speedUnitsPerSecond * deltaTime;
            var arrivedThresholdSq = 0.36f;

            foreach (var (settlementState, transform) in SystemAPI
                         .Query<RefRO<SettlementVillagerState>, RefRW<LocalTransform>>())
            {
                var phase = settlementState.ValueRO.Phase;
                if (phase != SettlementVillagerPhase.ToResource && phase != SettlementVillagerPhase.ToDepot)
                {
                    continue;
                }

                var target = phase == SettlementVillagerPhase.ToResource
                    ? settlementState.ValueRO.CurrentResourceNode
                    : settlementState.ValueRO.CurrentDepot;

                if (target == Entity.Null || !_transformLookup.HasComponent(target))
                {
                    continue;
                }

                var current = transform.ValueRO.Position;
                var targetPos = _transformLookup[target].Position;

                // 2.5D: keep Y fixed, move on XZ plane.
                targetPos.y = current.y;

                var toTarget = targetPos - current;
                toTarget.y = 0f;
                var distSq = math.lengthsq(toTarget);
                if (distSq <= arrivedThresholdSq)
                {
                    continue;
                }

                var dist = math.sqrt(distSq);
                var step = math.min(maxStep, dist);
                var dir = toTarget / math.max(1e-5f, dist);
                var next = current + dir * step;

                var updated = transform.ValueRO;
                updated.Position = next;
                transform.ValueRW = updated;
            }
        }

        private bool IsScenario01(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<GodgameScenarioConfigBlobReference>(out var configRef))
            {
                return false;
            }

            if (!configRef.Config.IsCreated)
            {
                return false;
            }

            return configRef.Config.Value.Mode == GodgameScenarioMode.Scenario01;
        }

        private float ResolveDeltaTime(in TimeState timeState)
        {
            var tick = timeState.Tick;
            if (_tickInitialized == 0)
            {
                _tickInitialized = 1;
                _lastTick = tick;
                return 0f;
            }

            var deltaTicks = tick >= _lastTick ? tick - _lastTick : 0u;
            _lastTick = tick;

            if (deltaTicks == 0u)
            {
                return 0f;
            }

            var fixedDt = math.max(timeState.FixedDeltaTime, 1e-4f);
            return fixedDt * deltaTicks;
        }
    }
}
#endif
