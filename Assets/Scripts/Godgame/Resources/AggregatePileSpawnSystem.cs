using Godgame.Presentation;
using PureDOTS.Runtime.Components;
using PureDOTS.Rendering;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Resources
{
    /// <summary>
    /// Request component to spawn a new aggregate pile or add resources to an existing one.
    /// </summary>
    public struct PileSpawnRequest : IComponentData
    {
        public FixedString64Bytes ResourceTypeId;
        public ushort ResourceTypeIndex;
        public float Amount;
        public float3 Position;
    }

    /// <summary>
    /// System that spawns aggregate piles and handles pile requests.
    /// Implements v1.0 (spawn/grow) from Aggregate_Piles.md iteration plan.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct AggregatePileSpawnSystem : ISystem
    {
        private EntityQuery _pileQuery;
        private EntityQuery _requestQuery;

        public void OnCreate(ref SystemState state)
        {
            _pileQuery = SystemAPI.QueryBuilder()
                .WithAll<AggregatePile, LocalTransform>()
                .Build();

            _requestQuery = SystemAPI.QueryBuilder()
                .WithAll<PileSpawnRequest>()
                .Build();

            state.RequireForUpdate<TimeState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (_requestQuery.IsEmptyIgnoreFilter)
                return;

            var timeState = SystemAPI.GetSingleton<TimeState>();
            uint currentTick = timeState.Tick;

            // Get or create config
            var config = SystemAPI.HasSingleton<AggregatePileConfig>()
                ? SystemAPI.GetSingleton<AggregatePileConfig>()
                : AggregatePileConfig.Default;

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            // Collect existing piles
            var pileEntities = _pileQuery.ToEntityArray(Allocator.Temp);
            var pileTransforms = _pileQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
            var pileData = _pileQuery.ToComponentDataArray<AggregatePile>(Allocator.Temp);

            // Count current piles
            int currentPileCount = pileEntities.Length;

            // Process spawn requests
            foreach (var (request, requestEntity) in SystemAPI.Query<RefRO<PileSpawnRequest>>().WithEntityAccess())
            {
                ProcessSpawnRequest(
                    ref state,
                    ref ecb,
                    in request.ValueRO,
                    in config,
                    currentTick,
                    ref currentPileCount,
                    in pileEntities,
                    in pileTransforms,
                    ref pileData);

                // Remove processed request
                ecb.DestroyEntity(requestEntity);
            }

            // Update pile data (for entities we modified)
            for (int i = 0; i < pileEntities.Length; i++)
            {
                state.EntityManager.SetComponentData(pileEntities[i], pileData[i]);
            }

            pileEntities.Dispose();
            pileTransforms.Dispose();
            pileData.Dispose();

            ecb.Playback(state.EntityManager);
            ecb.Dispose();

            // Update stats
            UpdateStats(ref state, currentPileCount);
        }

        private void ProcessSpawnRequest(
            ref SystemState state,
            ref EntityCommandBuffer ecb,
            in PileSpawnRequest request,
            in AggregatePileConfig config,
            uint currentTick,
            ref int currentPileCount,
            in NativeArray<Entity> pileEntities,
            in NativeArray<LocalTransform> pileTransforms,
            ref NativeArray<AggregatePile> pileData)
        {
            // Check if we can add to existing pile nearby
            int nearestPileIndex = -1;
            float nearestDistance = float.MaxValue;

            for (int i = 0; i < pileEntities.Length; i++)
            {
                // Must be same resource type
                if (pileData[i].ResourceTypeIndex != request.ResourceTypeIndex)
                    continue;

                // Check distance
                float distance = math.distance(pileTransforms[i].Position, request.Position);
                if (distance < config.MergeRadius && distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestPileIndex = i;
                }
            }

            float remainingAmount = request.Amount;

            // Add to existing pile if found
            if (nearestPileIndex >= 0)
            {
                var pile = pileData[nearestPileIndex];
                float accepted = pile.Add(remainingAmount, currentTick);
                remainingAmount -= accepted;
                pileData[nearestPileIndex] = pile;
            }

            // Spawn new pile if still have resources and under cap
            if (remainingAmount > 0 && currentPileCount < config.MaxActivePiles)
            {
                SpawnNewPile(ref ecb, in request, remainingAmount, currentTick, config.GlobalMaxCapacity);
                currentPileCount++;
            }
        }

        private void SpawnNewPile(
            ref EntityCommandBuffer ecb,
            in PileSpawnRequest request,
            float amount,
            uint currentTick,
            float maxCapacity)
        {
            var entity = ecb.CreateEntity();

            var pile = AggregatePile.Create(
                in request.ResourceTypeId,
                request.ResourceTypeIndex,
                amount,
                currentTick);
            pile.MaxCapacity = maxCapacity;

            ecb.AddComponent(entity, pile);
            ecb.AddComponent(entity, new LocalTransform
            {
                Position = request.Position,
                Rotation = quaternion.identity,
                Scale = pile.GetVisualScale()
            });

            // Add tag for presentation system to pick up
            ecb.AddComponent(entity, new AggregatePileTag());
        }

        private void UpdateStats(ref SystemState state, int totalPiles)
        {
            if (!SystemAPI.HasSingleton<AggregatePileStats>())
            {
                var statsEntity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponentData(statsEntity, new AggregatePileStats());
            }

            ref var stats = ref SystemAPI.GetSingletonRW<AggregatePileStats>().ValueRW;
            stats.TotalPiles = totalPiles;

            // Calculate total resources
            float totalAmount = 0f;
            foreach (var pile in SystemAPI.Query<RefRO<AggregatePile>>())
            {
                totalAmount += pile.ValueRO.Amount;
            }
            stats.TotalResourceAmount = totalAmount;
        }
    }

    /// <summary>
    /// Tag component for aggregate pile entities.
    /// </summary>
    public struct AggregatePileTag : IComponentData { }

    /// <summary>
    /// System that updates pile visual scale when amount changes.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(AggregatePileSpawnSystem))]
    public partial struct AggregatePileVisualSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (pile, transform, tint) in SystemAPI
                         .Query<RefRO<AggregatePile>, RefRW<LocalTransform>, RefRW<RenderTint>>())
            {
                float expectedScale = pile.ValueRO.GetVisualScale();
                if (math.abs(transform.ValueRO.Scale - expectedScale) > 0.01f)
                {
                    transform.ValueRW.Scale = expectedScale;
                }

                tint.ValueRW.Value = GodgamePresentationColors.ForResourceTypeIndex(pile.ValueRO.ResourceTypeIndex);
            }
        }
    }

    /// <summary>
    /// System that cleans up empty piles.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(AggregatePileVisualSystem))]
    public partial struct AggregatePileCleanupSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (pile, entity) in SystemAPI.Query<RefRO<AggregatePile>>().WithEntityAccess())
            {
                if (pile.ValueRO.IsEmpty)
                {
                    ecb.DestroyEntity(entity);
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    /// <summary>
    /// Helper to create pile spawn requests.
    /// </summary>
    public static class AggregatePileHelper
    {
        /// <summary>
        /// Create a request to spawn a pile (or add to existing nearby).
        /// </summary>
        public static Entity CreateSpawnRequest(
            ref EntityCommandBuffer ecb,
            in FixedString64Bytes resourceTypeId,
            ushort typeIndex,
            float amount,
            float3 position)
        {
            var entity = ecb.CreateEntity();
            ecb.AddComponent(entity, new PileSpawnRequest
            {
                ResourceTypeId = resourceTypeId,
                ResourceTypeIndex = typeIndex,
                Amount = amount,
                Position = position
            });
            return entity;
        }
    }
}
