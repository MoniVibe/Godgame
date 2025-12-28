using Godgame.Miracles;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Registry;
using PureDOTS.Systems;
using Unity.Burst;
using Unity.Entities;

namespace Godgame.Registry
{
    /// <summary>
    /// Bridges Godgame-specific miracle components to PureDOTS canonical MiracleDefinition and MiracleRuntimeState components.
    /// Follows projection pattern: if entity has PureDOTS MiracleDefinition/MiracleRuntimeState, leave it alone.
    /// If entity has Godgame miracle components but not canonical, project/add canonical components.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(GameplaySystemGroup))]
    public partial struct GodgameMiracleSyncSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            // Require PureDOTS registry and time state
            state.RequireForUpdate<MiracleRegistry>();
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            var rewindState = SystemAPI.GetSingleton<RewindState>();
            
            // Skip if paused or not recording
            if (timeState.IsPaused || rewindState.Mode != RewindMode.Record)
            {
                return;
            }

            var ecb = SystemAPI
                .GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            // 1. PROJECT: Query entities with Godgame miracle components but no MiracleDefinition/MiracleRuntimeState
            foreach (var (ggMiracle, entity) in SystemAPI
                     .Query<RefRO<Godgame.Miracles.MiracleToken>>()
                     .WithNone<MiracleDefinition, MiracleRuntimeState>()
                     .WithEntityAccess())
            {
                ecb.AddComponent(entity, new MiracleDefinition
                {
                    Type = ggMiracle.ValueRO.Type,
                    CastingMode = ggMiracle.ValueRO.CastingMode,
                    BaseRadius = ggMiracle.ValueRO.BaseRadius,
                    BaseIntensity = ggMiracle.ValueRO.BaseIntensity,
                    BaseCost = ggMiracle.ValueRO.BaseCost,
                    SustainedCostPerSecond = ggMiracle.ValueRO.SustainedCostPerSecond
                });
                
                ecb.AddComponent(entity, new MiracleRuntimeState
                {
                    Lifecycle = ggMiracle.ValueRO.Lifecycle,
                    ChargePercent = ggMiracle.ValueRO.ChargePercent,
                    CurrentRadius = ggMiracle.ValueRO.CurrentRadius,
                    CurrentIntensity = ggMiracle.ValueRO.CurrentIntensity,
                    CooldownSecondsRemaining = ggMiracle.ValueRO.CooldownSecondsRemaining,
                    LastCastTick = ggMiracle.ValueRO.LastCastTick,
                    AlignmentDelta = ggMiracle.ValueRO.AlignmentDelta
                });
                
                ecb.AddComponent<SyncedFromGodgame>(entity);
            }

            // 2. UPDATE: Query entities with both Godgame and canonical components
            foreach (var (ggMiracle, definition, runtime) in SystemAPI
                     .Query<RefRO<Godgame.Miracles.MiracleToken>, RefRW<MiracleDefinition>, RefRW<MiracleRuntimeState>>()
                     .WithChangeFilter<Godgame.Miracles.MiracleToken>())
            {
                definition.ValueRW.Type = ggMiracle.ValueRO.Type;
                definition.ValueRW.CastingMode = ggMiracle.ValueRO.CastingMode;
                definition.ValueRW.BaseRadius = ggMiracle.ValueRO.BaseRadius;
                definition.ValueRW.BaseIntensity = ggMiracle.ValueRO.BaseIntensity;
                definition.ValueRW.BaseCost = ggMiracle.ValueRO.BaseCost;
                definition.ValueRW.SustainedCostPerSecond = ggMiracle.ValueRO.SustainedCostPerSecond;
                
                runtime.ValueRW.Lifecycle = ggMiracle.ValueRO.Lifecycle;
                runtime.ValueRW.ChargePercent = ggMiracle.ValueRO.ChargePercent;
                runtime.ValueRW.CurrentRadius = ggMiracle.ValueRO.CurrentRadius;
                runtime.ValueRW.CurrentIntensity = ggMiracle.ValueRO.CurrentIntensity;
                runtime.ValueRW.CooldownSecondsRemaining = ggMiracle.ValueRO.CooldownSecondsRemaining;
                runtime.ValueRW.LastCastTick = ggMiracle.ValueRO.LastCastTick;
                runtime.ValueRW.AlignmentDelta = ggMiracle.ValueRO.AlignmentDelta;
            }

            // 3. CLEANUP: Query entities with MiracleDefinition/MiracleRuntimeState + SyncedFromGodgame but no Godgame source
            foreach (var (definition, runtime, entity) in SystemAPI
                     .Query<RefRO<MiracleDefinition>, RefRO<MiracleRuntimeState>>()
                     .WithAll<SyncedFromGodgame>()
                     .WithNone<Godgame.Miracles.MiracleToken>()
                     .WithEntityAccess())
            {
                ecb.RemoveComponent<MiracleDefinition>(entity);
                ecb.RemoveComponent<MiracleRuntimeState>(entity);
                ecb.RemoveComponent<SyncedFromGodgame>(entity);
            }
        }
    }
}
