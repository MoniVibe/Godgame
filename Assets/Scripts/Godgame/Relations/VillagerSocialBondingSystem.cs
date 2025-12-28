using Godgame.Villagers;
using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Relations
{
    /// <summary>
    /// Applies small relation adjustments when villagers work or socialize in close proximity.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(EntityMeetingSystem))]
    public partial struct VillagerSocialBondingSystem : ISystem
    {
        private EntityQuery _villagerQuery;
        private uint _lastUpdateTick;
        private int _currentBatchStart;

        private const uint UpdateIntervalTicks = 60;
        private const int MaxEntitiesPerBatch = 40;
        private const int MaxPairChecksPerFrame = 300;

        public void OnCreate(ref SystemState state)
        {
            _villagerQuery = SystemAPI.QueryBuilder()
                .WithAll<LocalTransform, VillagerGoalState, VillagerJobState>()
                .Build();

            state.RequireForUpdate<TimeState>();
            _lastUpdateTick = 0;
            _currentBatchStart = 0;
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            if (timeState.IsPaused)
            {
                return;
            }

            if (timeState.Tick - _lastUpdateTick < UpdateIntervalTicks)
            {
                return;
            }

            _lastUpdateTick = timeState.Tick;

            var entities = _villagerQuery.ToEntityArray(Allocator.Temp);
            var transforms = _villagerQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
            var goals = _villagerQuery.ToComponentDataArray<VillagerGoalState>(Allocator.Temp);
            var jobs = _villagerQuery.ToComponentDataArray<VillagerJobState>(Allocator.Temp);

            if (entities.Length == 0)
            {
                entities.Dispose();
                transforms.Dispose();
                goals.Dispose();
                jobs.Dispose();
                return;
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var pairChecks = 0;
            var maxDistanceSq = 6f * 6f;

            var batchEnd = math.min(_currentBatchStart + MaxEntitiesPerBatch, entities.Length);
            for (int i = _currentBatchStart; i < batchEnd && pairChecks < MaxPairChecksPerFrame; i++)
            {
                for (int j = i + 1; j < entities.Length && pairChecks < MaxPairChecksPerFrame; j++)
                {
                    pairChecks++;

                    var distSq = math.distancesq(transforms[i].Position, transforms[j].Position);
                    if (distSq > maxDistanceSq)
                    {
                        continue;
                    }

                    var delta = ResolveBondDelta(goals[i], goals[j], jobs[i], jobs[j]);
                    if (delta == 0)
                    {
                        continue;
                    }

                    var requestA = ecb.CreateEntity();
                    ecb.AddComponent(requestA, new ModifyRelationRequest
                    {
                        SourceEntity = entities[i],
                        TargetEntity = entities[j],
                        Delta = delta
                    });

                    var requestB = ecb.CreateEntity();
                    ecb.AddComponent(requestB, new ModifyRelationRequest
                    {
                        SourceEntity = entities[j],
                        TargetEntity = entities[i],
                        Delta = delta
                    });
                }
            }

            _currentBatchStart = batchEnd >= entities.Length ? 0 : batchEnd;

            entities.Dispose();
            transforms.Dispose();
            goals.Dispose();
            jobs.Dispose();

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        private static sbyte ResolveBondDelta(in VillagerGoalState aGoal, in VillagerGoalState bGoal, in VillagerJobState aJob, in VillagerJobState bJob)
        {
            if (aGoal.CurrentGoal == VillagerGoal.Socialize && bGoal.CurrentGoal == VillagerGoal.Socialize)
            {
                return 1;
            }

            if (aGoal.CurrentGoal == VillagerGoal.Work && bGoal.CurrentGoal == VillagerGoal.Work &&
                aJob.ResourceTypeIndex == bJob.ResourceTypeIndex && aJob.ResourceTypeIndex != ushort.MaxValue)
            {
                return 1;
            }

            return 0;
        }
    }
}
