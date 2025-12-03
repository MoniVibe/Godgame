using Godgame.Runtime;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Registry;
using PureDOTS.Systems;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Registry
{
    /// <summary>
    /// Normalizes miracle data before the registry bridge builds its snapshot.
    /// Keeps values finite and non-negative so the PureDOTS registry can operate headless.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(GameplaySystemGroup))]
    [UpdateBefore(typeof(GodgameRegistryBridgeSystem))]
    public partial struct GodgameMiracleSyncSystem : ISystem
    {
        private ComponentLookup<MiracleTarget> _targetLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MiracleRegistry>();
            state.RequireForUpdate<MiracleDefinition>();
            state.RequireForUpdate<MiracleRuntimeState>();
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();

            _targetLookup = state.GetComponentLookup<MiracleTarget>(isReadOnly: false);
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

            _targetLookup.Update(ref state);

            foreach (var (definition, runtime, entity) in SystemAPI
                         .Query<RefRW<MiracleDefinition>, RefRW<MiracleRuntimeState>>()
                         .WithEntityAccess())
            {
                var def = definition.ValueRO;
                def.BaseCost = math.max(0f, def.BaseCost);
                def.SustainedCostPerSecond = math.max(0f, def.SustainedCostPerSecond);
                def.BaseRadius = math.max(0f, def.BaseRadius);
                def.BaseIntensity = math.max(0f, def.BaseIntensity);
                definition.ValueRW = def;

                var runtimeState = runtime.ValueRO;
                runtimeState.CooldownSecondsRemaining = math.max(0f, runtimeState.CooldownSecondsRemaining);
                runtimeState.CurrentRadius = math.max(0f, runtimeState.CurrentRadius);
                runtimeState.CurrentIntensity = math.max(0f, runtimeState.CurrentIntensity);
                runtimeState.ChargePercent = math.clamp(runtimeState.ChargePercent, 0f, 1f);
                runtime.ValueRW = runtimeState;

                if (_targetLookup.HasComponent(entity))
                {
                    var target = _targetLookup[entity];
                    var finite = math.isfinite(target.TargetPosition);
                    if (!(finite.x && finite.y && finite.z))
                    {
                        target.TargetPosition = float3.zero;
                    }

                    _targetLookup[entity] = target;
                }
            }
        }
    }
}
