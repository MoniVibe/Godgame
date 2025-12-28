using PureDOTS.Runtime.AI;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Villagers
{
    /// <summary>
    /// Ensures movement debug components exist for villagers.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct VillagerMovementDebugBootstrapSystem : ISystem
    {
        private EntityQuery _missingQuery;

        public void OnCreate(ref SystemState state)
        {
            _missingQuery = SystemAPI.QueryBuilder()
                .WithAll<VillagerGoalState>()
                .WithNone<MoveIntent>()
                .Build();
            state.RequireForUpdate(_missingQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var entities = _missingQuery.ToEntityArray(state.WorldUpdateAllocator);
            if (entities.Length == 0)
            {
                return;
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var entity in entities)
            {
                ecb.AddComponent(entity, new MoveIntent
                {
                    TargetEntity = Entity.Null,
                    TargetPosition = float3.zero,
                    IntentType = MoveIntentType.None
                });
                ecb.AddComponent(entity, new MovePlan
                {
                    Mode = MovePlanMode.None,
                    DesiredVelocity = float3.zero,
                    MaxAccel = 0f,
                    EtaSeconds = 0f
                });
                ecb.AddComponent(entity, new DecisionTrace
                {
                    ReasonCode = 0,
                    ChosenTarget = Entity.Null,
                    Score = 0f,
                    BlockerEntity = Entity.Null,
                    SinceTick = 0
                });
                ecb.AddBuffer<DecisionTraceEvent>(entity);
                ecb.AddBuffer<MovementTickTrace>(entity);
                ecb.AddComponent(entity, new VillagerMovementDiagnosticsState
                {
                    LastPosition = float3.zero,
                    LastGoalDistance = 0f,
                    LastTick = 0,
                    StuckTicks = 0,
                    MaxSpeed = 0f,
                    MaxTeleport = 0f,
                    StateFlipCount = 0,
                    LastIntentType = MoveIntentType.None
                });
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
