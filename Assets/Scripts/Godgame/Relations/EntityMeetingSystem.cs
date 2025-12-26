using Godgame.Villagers;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Relations
{
    /// <summary>
    /// System that detects when two entities meet for the first time
    /// and creates initial relations based on alignment/personality/context.
    /// Rate-limited and entity-budget-capped to prevent O(n²) freeze on large populations.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct EntityMeetingSystem : ISystem
    {
        private EntityQuery _entityQuery;
        private uint _lastUpdateTick;
        private int _currentBatchStart;

        // Rate limit: only run every N ticks (e.g., every 30 ticks = ~0.5 seconds at 60fps)
        private const uint UpdateIntervalTicks = 30;
        // Max entities to check per update to avoid freezing
        private const int MaxEntitiesPerBatch = 50;
        // Max pair comparisons per frame
        private const int MaxPairChecksPerFrame = 500;

        public void OnCreate(ref SystemState state)
        {
            // Query entities that can form relations (have transform and alignment)
            _entityQuery = SystemAPI.QueryBuilder()
                .WithAll<LocalTransform, VillagerAlignment>()
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
                return;

            uint currentTick = timeState.Tick;
            
            // Rate limiting: only process meetings periodically
            if (currentTick - _lastUpdateTick < UpdateIntervalTicks)
                return;
            
            _lastUpdateTick = currentTick;

            // Get config
            var config = SystemAPI.HasSingleton<RelationSystemConfig>()
                ? SystemAPI.GetSingleton<RelationSystemConfig>()
                : RelationSystemConfig.Default;

            float meetingDistanceSq = config.MeetingDistance * config.MeetingDistance;

            // Collect all entities for meeting checks
            var entities = _entityQuery.ToEntityArray(Allocator.Temp);
            var transforms = _entityQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
            var alignments = _entityQuery.ToComponentDataArray<VillagerAlignment>(Allocator.Temp);

            // Safety check: if too many entities, use batching
            int entityCount = entities.Length;
            if (entityCount == 0)
            {
                entities.Dispose();
                transforms.Dispose();
                alignments.Dispose();
                return;
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            int pairChecks = 0;

            // Determine batch range (wrap around if needed)
            int batchEnd = math.min(_currentBatchStart + MaxEntitiesPerBatch, entityCount);
            
            // Check pairs only for entities in current batch against all others
            for (int i = _currentBatchStart; i < batchEnd && pairChecks < MaxPairChecksPerFrame; i++)
            {
                for (int j = i + 1; j < entityCount && pairChecks < MaxPairChecksPerFrame; j++)
                {
                    pairChecks++;
                    
                    // Check distance first (cheapest check)
                    float distSq = math.distancesq(transforms[i].Position, transforms[j].Position);
                    if (distSq > meetingDistanceSq)
                        continue;

                    // Check if already met
                    bool alreadyMet = false;

                    if (SystemAPI.HasBuffer<EntityRelation>(entities[i]))
                    {
                        var relations = SystemAPI.GetBuffer<EntityRelation>(entities[i]);
                        for (int r = 0; r < relations.Length; r++)
                        {
                            if (relations[r].OtherEntity == entities[j])
                            {
                                alreadyMet = true;
                                break;
                            }
                        }
                    }

                    if (alreadyMet)
                        continue;

                    // Create meeting
                    CreateMeeting(
                        ref state,
                        ref ecb,
                        entities[i], entities[j],
                        alignments[i], alignments[j],
                        MeetingContext.VillageNeighbor, // Default context
                        currentTick);
                }
            }

            // Advance batch position for next update
            _currentBatchStart = batchEnd >= entityCount ? 0 : batchEnd;

            entities.Dispose();
            transforms.Dispose();
            alignments.Dispose();

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        private void CreateMeeting(
            ref SystemState state,
            ref EntityCommandBuffer ecb,
            Entity entity1, Entity entity2,
            VillagerAlignment align1, VillagerAlignment align2,
            MeetingContext context,
            uint tick)
        {
            // Calculate initial relation using new four-layer identity system
            // Falls back to legacy if new components not available
            uint seed = (uint)(entity1.Index * 31337 + entity2.Index * 17 + tick);
            sbyte initialRelation = RelationCalculator.CalculateInitialRelation(
                entity1, entity2,
                state.EntityManager,
                context,
                KinshipType.None,
                seed);

            // Create bidirectional relations
            AddRelation(ref state, ref ecb, entity1, entity2, initialRelation, context, tick);
            AddRelation(ref state, ref ecb, entity2, entity1, initialRelation, context, tick);
        }

        private void AddRelation(
            ref SystemState state,
            ref EntityCommandBuffer ecb,
            Entity owner, Entity other,
            sbyte relationValue,
            MeetingContext context,
            uint tick)
        {
            // Ensure entity has relation buffer
            if (!SystemAPI.HasBuffer<EntityRelation>(owner))
            {
                ecb.AddBuffer<EntityRelation>(owner);
                ecb.AddComponent(owner, new HasRelationsTag());
                ecb.AppendToBuffer(owner, EntityRelation.Create(other, relationValue, context, tick));
                return;
            }

            var buffer = SystemAPI.GetBuffer<EntityRelation>(owner);
            buffer.Add(EntityRelation.Create(other, relationValue, context, tick));
        }
    }

    /// <summary>
    /// System that processes relation modification requests.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(EntityMeetingSystem))]
    public partial struct RelationModificationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            uint currentTick = timeState.Tick;

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            // Process modification requests
            foreach (var (request, entity) in SystemAPI.Query<RefRO<ModifyRelationRequest>>().WithEntityAccess())
            {
                var source = request.ValueRO.SourceEntity;
                var target = request.ValueRO.TargetEntity;
                var delta = request.ValueRO.Delta;

                if (SystemAPI.HasBuffer<EntityRelation>(source))
                {
                    var relations = SystemAPI.GetBuffer<EntityRelation>(source);
                    for (int i = 0; i < relations.Length; i++)
                    {
                        if (relations[i].OtherEntity == target)
                        {
                            var relation = relations[i];
                            relation.ModifyRelation(delta, currentTick);
                            relations[i] = relation;
                            break;
                        }
                    }
                }

                ecb.DestroyEntity(entity);
            }

            // Process creation requests
            foreach (var (request, entity) in SystemAPI.Query<RefRO<CreateRelationRequest>>().WithEntityAccess())
            {
                // Would create relations between specified entities
                // For now just destroy the request
                ecb.DestroyEntity(entity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    /// <summary>
    /// Helper methods for relation operations.
    /// </summary>
    public static class RelationHelper
    {
        /// <summary>
        /// Create a request to modify a relation.
        /// </summary>
        public static Entity CreateModifyRequest(
            ref EntityCommandBuffer ecb,
            Entity source, Entity target, sbyte delta)
        {
            var entity = ecb.CreateEntity();
            ecb.AddComponent(entity, new ModifyRelationRequest
            {
                SourceEntity = source,
                TargetEntity = target,
                Delta = delta
            });
            return entity;
        }

        /// <summary>
        /// Get relation value between two entities (if exists).
        /// </summary>
        public static bool TryGetRelation(
            ref SystemState state,
            Entity owner, Entity other,
            out sbyte relationValue)
        {
            relationValue = 0;

            var entityManager = state.EntityManager;

            if (!entityManager.HasBuffer<EntityRelation>(owner))
                return false;

            var relations = entityManager.GetBuffer<EntityRelation>(owner);
            for (int i = 0; i < relations.Length; i++)
            {
                if (relations[i].OtherEntity == other)
                {
                    relationValue = relations[i].RelationValue;
                    return true;
                }
            }

            return false;
        }
    }
}
