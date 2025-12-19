using Godgame.Scenario;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using PureDOTS.Runtime;
using PureDOTS.Runtime.Physics;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Godgame.Rendering;

namespace Godgame.Scenario
{
    /// <summary>
    /// Spawns a handful of rock resource nodes around the starting village/terrain area.
    /// Uses rock authoring pattern with ResourceDeposit for mining integration.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct GodgameRockSpawnerSystem : ISystem
    {
        private bool _initialized;

        private const int RockCount = 5;
        private const float SpawnRadius = 15f;
        private const float ResourceAmount = 50f;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ScenarioSceneTag>();
            Debug.Log("[GodgameRockSpawnerSystem] OnCreate");
            state.RequireForUpdate<ScenarioState>();
        }

        // NOTE: Not Burst compiled because DemoRenderUtil.MakeRenderable uses managed code
        public void OnUpdate(ref SystemState state)
        {
            // Check scenario - only run in AllSystemsShowcase or GodgamePhysicsOnly
            var scenario = SystemAPI.GetSingleton<ScenarioState>().Current;
            if (scenario != ScenarioKind.AllSystemsShowcase && scenario != ScenarioKind.GodgamePhysicsOnly)
            {
                return;
            }

            if (_initialized)
            {
                Debug.Log("[GodgameRockSpawnerSystem] Already initialized, disabling.");
                state.Enabled = false;
                return;
            }

            Debug.Log($"[GodgameRockSpawnerSystem] Spawning {RockCount} rocks around village area");

            var em = state.EntityManager;
            var centerPos = new float3(0f, 0f, 0f); // Village center (adjust as needed)

            for (int i = 0; i < RockCount; i++)
            {
                // Spawn rocks in a circle around center
                var angle = (float)i / RockCount * math.PI * 2f;
                var radius = SpawnRadius + (i % 2) * 3f; // Vary radius slightly
                var position = centerPos + new float3(
                    math.cos(angle) * radius,
                    0f,
                    math.sin(angle) * radius
                );

                CreateRock(em, position, i);
            }

            Debug.Log($"[GodgameRockSpawnerSystem] Spawned {RockCount} rocks with ResourceDeposit components.");

            _initialized = true;
            state.Enabled = false;
        }

        private void CreateRock(EntityManager em, float3 position, int index)
        {
            var rockEntity = em.CreateEntity();

            // Basic transform (render hookup TBD via proper authoring/prefab)
            em.AddComponentData(rockEntity, LocalTransform.FromPositionRotationScale(position, quaternion.identity, 1.5f));

            // Add rock tags
            em.AddComponent<RockTag>(rockEntity);
            em.AddComponent<ThrowableTag>(rockEntity);
            em.AddComponent<ResourceNodeTag>(rockEntity);

            // Add ResourceDeposit (Stone resource)
            em.AddComponentData(rockEntity, new ResourceDeposit
            {
                ResourceTypeId = 0, // Stone (would need proper mapping)
                CurrentAmount = ResourceAmount,
                MaxAmount = ResourceAmount,
                RegenPerSecond = 0f
            });

            // Add ResourceSourceConfig for ResourceRegistrySystem
            em.AddComponentData(rockEntity, new Godgame.Resources.ResourceSourceConfig
            {
                ResourceTypeId = new FixedString64Bytes("stone"),
                Amount = ResourceAmount,
                MaxAmount = ResourceAmount,
                RegenRate = 0f
            });

            // Add MaterialStats (rock defaults)
            em.AddComponentData(rockEntity, new MaterialStats
            {
                Hardness = 2f,
                Fragility = 0.5f,
                Density = 3f
            });

            // Add Destructible
            em.AddComponentData(rockEntity, new Destructible
            {
                HitPoints = 100f,
                MaxHitPoints = 100f
            });

            // Add ImpactDamage
            em.AddComponentData(rockEntity, new ImpactDamage
            {
                DamagePerImpulse = 10f,
                MinImpulse = 1f
            });

            // Add PureDOTS physics components
            em.AddComponentData(rockEntity, new RequiresPhysics
            {
                Priority = 100,
                Flags = PhysicsInteractionFlags.Collidable
            });

            em.AddComponentData(rockEntity, new PhysicsInteractionConfig
            {
                Mass = 1f,
                CollisionRadius = 1.5f,
                Restitution = 0f,
                Friction = 0.5f, // Ground friction
                LinearDamping = 0.1f,
                AngularDamping = 0.1f
            });

            // Add collision event buffer
            em.AddBuffer<PhysicsCollisionEventElement>(rockEntity);

            GodgamePresentationUtility.ApplyScenarioRenderContract(em, rockEntity, GodgameSemanticKeys.ResourceNode);
        }
    }
}
#endif
