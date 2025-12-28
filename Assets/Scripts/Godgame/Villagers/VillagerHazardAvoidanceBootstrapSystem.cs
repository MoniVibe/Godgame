using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Combat;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Core;
using PureDOTS.Runtime.Telemetry;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Godgame.Villagers
{
    /// <summary>
    /// Ensures every baked villager has the shared hazard avoidance components used by PureDOTS.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(BehaviorConfigBootstrapSystem))]
    public partial struct VillagerHazardAvoidanceBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameWorldTag>();
            state.RequireForUpdate<TimeState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var query = SystemAPI.QueryBuilder()
                .WithAll<VillagerId, LocalTransform>()
                .WithNone<HazardRaycastProbe>()
                .Build();

            var villagers = query.ToEntityArray(state.WorldUpdateAllocator);
            if (villagers.Length == 0)
            {
                state.Enabled = false;
                return;
            }

            var hasBehaviorConfig = SystemAPI.TryGetSingleton<BehaviorConfigRegistry>(out var behaviorConfig);
            var hazardConfig = hasBehaviorConfig ? behaviorConfig.HazardDodge : default;

            var timeState = SystemAPI.GetSingleton<TimeState>();
            var cooldownSeconds = hazardConfig.RaycastCooldownTicks > 0
                ? hazardConfig.RaycastCooldownTicks * math.max(timeState.FixedDeltaTime, 1e-4f)
                : 0f;

            var collisionFilter = CollisionFilter.Default;
            collisionFilter.BelongsTo = ~0u;
            collisionFilter.CollidesWith = ~0u;

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            for (int i = 0; i < villagers.Length; i++)
            {
                var entity = villagers[i];
                var probe = new HazardRaycastProbe
                {
                    RayLength = 30f,
                    SphereRadius = 2f,
                    SampleCount = 3,
                    SpreadAngleDeg = 25f,
                    UrgencyFalloff = 0.75f,
                    CollisionFilter = collisionFilter,
                    CooldownSeconds = cooldownSeconds,
                    LastSampleTick = 0
                };
                if (hazardConfig.SampleCount > 0)
                {
                    probe.SampleCount = (byte)math.max(1, hazardConfig.SampleCount);
                }

                ecb.AddComponent(entity, probe);
                ecb.AddComponent(entity, new AvoidanceProfile
                {
                    LookaheadSec = 1.2f,
                    ReactionSec = 0.2f,
                    BreakFormationThresh = 0.35f,
                    LooseSpacingMin = 4f,
                    LooseSpacingMax = 15f,
                    RiskWeightAoE = 1f,
                    RiskWeightChain = 0.6f,
                    RiskWeightPlague = 0.5f,
                    RiskWeightHoming = 0.8f,
                    RiskWeightSpray = 0.5f
                });
                ecb.AddComponent(entity, new HazardAvoidanceState
                {
                    CurrentAdjustment = float3.zero,
                    AvoidanceUrgency = 0f,
                    AvoidingEntity = Entity.Null
                });
                ecb.AddComponent(entity, new HazardRaycastState());
                ecb.AddComponent(entity, new AvoidanceReactionState());
                ecb.AddBuffer<AvoidanceReactionSample>(entity);
                ecb.AddComponent(entity, new HazardDodgeTelemetry
                {
                    WasAvoidingLastTick = 0
                });
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();

            // Only needs to run once per world
            state.Enabled = false;
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}
