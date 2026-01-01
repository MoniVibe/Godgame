using Godgame.Visitors;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Interaction;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Visitors
{
    /// <summary>
    /// Updates visitor pickability based on proximity to player world.
    /// Sets IsPickable and adds PickableTag when visitor is near the playable region.
    /// </summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct VisitorPickabilitySystem : ISystem
    {
        private const float PICKABLE_THRESHOLD_DISTANCE = 100f; // Distance threshold for pickability
        private const float PICKABLE_HEIGHT_THRESHOLD = 50f; // Height above biome region

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VisitorTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Get world center (for now, use origin; in real implementation, find player world instance)
            float3 worldCenter = float3.zero;
            float worldRadius = 50f; // Approximate playable region radius

            foreach (var (visitorState, transform, entity) in SystemAPI.Query<RefRW<VisitorState>, RefRO<LocalTransform>>()
                .WithAll<VisitorTag>()
                .WithEntityAccess())
            {
                var stateRef = visitorState.ValueRO;

                // Skip if already held
                if (stateRef.IsHeld)
                {
                    // Remove pickable tag if held
                    if (state.EntityManager.HasComponent<PickableTag>(entity))
                    {
                        state.EntityManager.RemoveComponent<PickableTag>(entity);
                    }
                    continue;
                }

                float3 visitorPos = transform.ValueRO.Position;
                
                // Check horizontal distance to world center
                float2 horizontal = new float2(visitorPos.x - worldCenter.x, visitorPos.z - worldCenter.z);
                float horizontalDistance = math.length(horizontal);

                // Check vertical distance (height above world)
                float verticalDistance = visitorPos.y - worldCenter.y;

                // Visitor is pickable if:
                // 1. Within horizontal threshold
                // 2. Above the world (positive Y)
                // 3. Not too high (within height threshold)
                bool isPickable = horizontalDistance <= worldRadius + PICKABLE_THRESHOLD_DISTANCE
                    && verticalDistance > 0f
                    && verticalDistance <= PICKABLE_HEIGHT_THRESHOLD;

                // Update state
                if (isPickable != stateRef.IsPickable)
                {
                    stateRef.IsPickable = isPickable;
                    visitorState.ValueRW = stateRef;

                    // Add or remove PickableTag
                    if (isPickable && !state.EntityManager.HasComponent<PickableTag>(entity))
                    {
                        state.EntityManager.AddComponent<PickableTag>(entity);
                        if (!state.EntityManager.HasComponent<HeldByPlayer>(entity))
                        {
                            state.EntityManager.AddComponent<HeldByPlayer>(entity);
                        }
                        state.EntityManager.SetComponentEnabled<HeldByPlayer>(entity, false);
                        if (!state.EntityManager.HasComponent<MovementSuppressed>(entity))
                        {
                            state.EntityManager.AddComponent<MovementSuppressed>(entity);
                        }
                        state.EntityManager.SetComponentEnabled<MovementSuppressed>(entity, false);
                        if (!state.EntityManager.HasComponent<BeingThrown>(entity))
                        {
                            state.EntityManager.AddComponent<BeingThrown>(entity);
                        }
                        state.EntityManager.SetComponentEnabled<BeingThrown>(entity, false);
                    }
                    else if (!isPickable && state.EntityManager.HasComponent<PickableTag>(entity))
                    {
                        state.EntityManager.RemoveComponent<PickableTag>(entity);
                    }
                }
            }
        }
    }
}
