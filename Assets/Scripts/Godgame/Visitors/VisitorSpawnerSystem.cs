using Godgame.Visitors;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Core;
using PureDOTS.Runtime.Orbits;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Visitors
{
    /// <summary>
    /// Spawns visitor objects (comets, asteroids, etc.) periodically orbiting the world.
    /// Uses seeded RNG for deterministic spawns.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct VisitorSpawnerSystem : ISystem
    {
        private const float SPAWN_INTERVAL_MINUTES = 5f; // Spawn every 5 world minutes
        private const float SPAWN_INTERVAL_SECONDS = SPAWN_INTERVAL_MINUTES * 60f;
        private float _lastSpawnTime;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TickTimeState>();
            _lastSpawnTime = -SPAWN_INTERVAL_SECONDS; // Allow immediate spawn
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<TickTimeState>(out var tickTime))
            {
                return;
            }

            var worldSeconds = tickTime.WorldSeconds;

            // Check if enough time has passed
            if (worldSeconds - _lastSpawnTime < SPAWN_INTERVAL_SECONDS)
            {
                return;
            }

            _lastSpawnTime = worldSeconds;

            // Use tick as seed for deterministic RNG
            var random = new Unity.Mathematics.Random((uint)tickTime.Tick + 12345u);

            // Decide whether to spawn (70% chance)
            if (random.NextFloat() > 0.7f)
            {
                return;
            }

            // Choose visitor kind
            var kind = (VisitorKind)random.NextInt(0, 5); // 0-4 for the 5 kinds

            // Find or create anchor entity (world center/star)
            // For now, we'll spawn at origin and let the orbit system handle positioning
            // In a real implementation, you'd find the world's star/planet entity
            Entity anchorEntity = Entity.Null;
            float3 anchorPosition = float3.zero;

            // Try to find an existing anchor (e.g., a star or planet entity)
            // For now, create a simple orbit around origin
            // TODO: Find actual world star/planet entity

            // Create orbit parameters
            float radius = random.NextFloat(50f, 200f); // Orbit radius
            float angularSpeed = random.NextFloat(0.1f, 0.5f); // Radians per second
            float initialPhase = random.NextFloat(0f, math.PI * 2f); // Random starting angle
            float inclination = random.NextFloat(-0.3f, 0.3f); // Slight tilt

            // Create visitor entity
            var visitorEntity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponent<VisitorTag>(visitorEntity);
            state.EntityManager.AddComponent<VisitorState>(visitorEntity);
            state.EntityManager.AddComponent<OrbitParams>(visitorEntity);
            state.EntityManager.AddComponent<LocalTransform>(visitorEntity);

            // Set visitor state
            state.EntityManager.SetComponentData(visitorEntity, new VisitorState
            {
                Kind = kind,
                IsPickable = false,
                IsHeld = false,
                IsInbound = false
            });

            // Set orbit parameters
            state.EntityManager.SetComponentData(visitorEntity, new OrbitParams
            {
                Anchor = anchorEntity, // Will be Entity.Null for now, orbit around origin
                LocalCenter = anchorPosition,
                Radius = radius,
                AngularSpeed = angularSpeed,
                InitialPhase = initialPhase,
                Inclination = inclination
            });

            // Set initial transform (will be updated by OrbitSystem)
            state.EntityManager.SetComponentData(visitorEntity, LocalTransform.Identity);
        }
    }
}

