using Godgame.AI;
using Godgame.MoveAct;
using PureDOTS.Runtime.Bands;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.AI;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.MoveAct
{
    /// <summary>
    /// Materialises band spawn requests into minimal registry-compatible entities.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(BandSpawnInputSystem))]
    public partial struct BandSpawnSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BandSpawnConfig>();
            state.RequireForUpdate<BandSpawnRequest>();
            state.RequireForUpdate<BandSpawnState>();
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
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

            var configEntity = SystemAPI.GetSingletonEntity<BandSpawnConfig>();
            var config = SystemAPI.GetComponent<BandSpawnConfig>(configEntity);
            var spawnState = SystemAPI.GetComponent<BandSpawnState>(configEntity);
            var requests = SystemAPI.GetBuffer<BandSpawnRequest>(configEntity);

            if (requests.Length == 0)
            {
                return;
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            Entity lastSpawned = Entity.Null;
            float3 lastPosition = float3.zero;

            for (int i = 0; i < requests.Length; i++)
            {
                var request = requests[i];
                var bandEntity = ecb.CreateEntity();

                ecb.AddComponent(bandEntity, new BandId
                {
                    Value = spawnState.NextBandId++,
                    FactionId = config.DefaultFactionId,
                    Leader = Entity.Null
                });

                ecb.AddComponent(bandEntity, new BandStats
                {
                    MemberCount = config.DefaultMemberCount,
                    AverageDiscipline = config.DefaultDiscipline,
                    Morale = config.DefaultMorale,
                    Cohesion = config.DefaultCohesion,
                    Fatigue = 0f,
                    Flags = BandStatusFlags.Idle,
                    LastUpdateTick = timeState.Tick
                });

                ecb.AddComponent(bandEntity, LocalTransform.FromPositionRotationScale(
                    request.Position,
                    quaternion.identity,
                    1f));

                ecb.AddComponent(bandEntity, new BandFormation
                {
                    Formation = BandFormationType.Column,
                    Spacing = math.max(0.1f, config.DefaultSpacing),
                    Width = math.max(config.DefaultSpacing, 1f),
                    Depth = math.max(config.DefaultSpacing, 1f),
                    Facing = new float3(0f, 0f, 1f),
                    Anchor = request.Position,
                    Stability = 1f,
                    LastSolveTick = timeState.Tick
                });

                ecb.AddBuffer<BandMember>(bandEntity);

                var roleAssignment = GodgameAIRoleDefinitions.ResolveForBand();
                ecb.AddComponent(bandEntity, new AIRole { RoleId = roleAssignment.RoleId });
                ecb.AddComponent(bandEntity, new AIDoctrine { DoctrineId = roleAssignment.DoctrineId });
                ecb.AddComponent(bandEntity, new AIBehaviorProfile
                {
                    ProfileId = roleAssignment.ProfileId,
                    ProfileHash = roleAssignment.ProfileHash,
                    ProfileEntity = Entity.Null,
                    SourceId = GodgameAIRoleDefinitions.SourceScenario
                });

                lastSpawned = bandEntity;
                lastPosition = request.Position;
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();

            requests.Clear();
            SystemAPI.SetComponent(configEntity, spawnState);

            if (lastSpawned != Entity.Null && SystemAPI.TryGetSingletonEntity<BandSelection>(out var selectionEntity))
            {
                state.EntityManager.SetComponentData(selectionEntity, new BandSelection
                {
                    Selected = lastSpawned,
                    Position = lastPosition,
                    SelectionTick = timeState.Tick
                });
            }
        }
    }
}
