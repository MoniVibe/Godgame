using Godgame.Visitors;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Visitors
{
    /// <summary>
    /// Buffer element for visitor impact events.
    /// Consumed by climate/environment systems to apply effects.
    /// </summary>
    [InternalBufferCapacity(4)]
    public struct VisitorImpactEvent : IBufferElementData
    {
        public VisitorKind Kind;
        public float3 Position;
        public float Radius;
        public float Energy;
    }

    /// <summary>
    /// Detects when thrown visitors hit terrain and creates impact events.
    /// Also handles impact effects on climate and environment.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct VisitorImpactSystem : ISystem
    {
        private const float GROUND_HEIGHT_THRESHOLD = 1f; // Height threshold for impact detection
        private const float IMPACT_RADIUS_BASE = 10f; // Base impact radius
        private const float IMPACT_ENERGY_BASE = 100f; // Base impact energy

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VisitorTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Find or create impact event buffer singleton
            Entity impactEventEntity;
            if (!SystemAPI.TryGetSingletonEntity<VisitorImpactEvent>(out impactEventEntity))
            {
                impactEventEntity = state.EntityManager.CreateEntity();
                state.EntityManager.AddBuffer<VisitorImpactEvent>(impactEventEntity);
            }

            var impactEvents = state.EntityManager.GetBuffer<VisitorImpactEvent>(impactEventEntity);

            foreach (var (visitorState, transform, entity) in SystemAPI.Query<RefRW<VisitorState>, RefRO<LocalTransform>>()
                .WithAll<VisitorTag>()
                .WithEntityAccess())
            {
                var stateRef = visitorState.ValueRO;

                // Skip if not inbound (on impact trajectory)
                if (!stateRef.IsInbound)
                {
                    // Check if visitor should be marked as inbound (falling toward ground)
                    float3 visitorPos = transform.ValueRO.Position;
                    if (visitorPos.y <= GROUND_HEIGHT_THRESHOLD && !stateRef.IsHeld)
                    {
                        stateRef.IsInbound = true;
                        visitorState.ValueRW = stateRef;
                    }
                    continue;
                }

                // Check if impact occurred (visitor hit ground)
                float3 pos = transform.ValueRO.Position;
                if (pos.y <= GROUND_HEIGHT_THRESHOLD)
                {
                    // Create impact event
                    float radius = IMPACT_RADIUS_BASE;
                    float energy = IMPACT_ENERGY_BASE;

                    // Scale by visitor kind
                    switch (stateRef.Kind)
                    {
                        case VisitorKind.Comet:
                            radius *= 1.5f; // Larger impact
                            energy *= 1.2f;
                            break;
                        case VisitorKind.Asteroid:
                            radius *= 2f; // Largest impact
                            energy *= 2f;
                            break;
                        case VisitorKind.IceChunk:
                            radius *= 0.8f; // Smaller impact
                            energy *= 0.6f;
                            break;
                    }

                    impactEvents.Add(new VisitorImpactEvent
                    {
                        Kind = stateRef.Kind,
                        Position = pos,
                        Radius = radius,
                        Energy = energy
                    });

                    // Mark visitor as no longer inbound (impacted)
                    stateRef.IsInbound = false;
                    visitorState.ValueRW = stateRef;

                    // Destroy or disable visitor entity after impact
                    // For now, just remove the visitor tag so it's ignored
                    state.EntityManager.RemoveComponent<VisitorTag>(entity);
                }
            }

            // Process impact events and apply climate effects
            ProcessImpactEvents(ref state, impactEvents);
            impactEvents.Clear();
        }

        [BurstCompile]
        private void ProcessImpactEvents(ref SystemState state, DynamicBuffer<VisitorImpactEvent> events)
        {
            // Apply climate/environment effects based on impact events
            // This is a simplified version - full implementation would modify ClimateState, MoistureGrid, etc.
            for (int i = 0; i < events.Length; i++)
            {
                var impact = events[i];

                switch (impact.Kind)
                {
                    case VisitorKind.Comet:
                        // Increase water level and moisture in radius
                        // TODO: Modify ClimateState / MoistureGrid
                        break;

                    case VisitorKind.Asteroid:
                        // Increase fire state, damage entities, spawn resources
                        // TODO: Modify FireState, damage entities, spawn resource deposits
                        break;

                    case VisitorKind.IceChunk:
                        // Targeted water injection
                        // TODO: Modify MoistureGrid
                        break;
                }
            }
        }
    }
}

