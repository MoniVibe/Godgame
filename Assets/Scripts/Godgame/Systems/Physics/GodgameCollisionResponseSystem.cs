using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Physics;
using PureDOTS.Runtime.Time;
using PureDOTS.Systems.Physics;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Systems.Physics
{
    /// <summary>
    /// Processes physics collision events for Godgame and applies damage based on impulse.
    /// Handles villagers, buildings, and other game entities.
    /// </summary>
    [UpdateInGroup(typeof(PhysicsPostEventSystemGroup))]
    [UpdateAfter(typeof(PureDOTS.Systems.Physics.PhysicsEventSystem))]
    public partial struct GodgameCollisionResponseSystem : ISystem
    {
        // Damage per unit of impulse
        private const float DamagePerImpulse = 0.1f;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
            state.RequireForUpdate<PhysicsConfig>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            var rewindState = SystemAPI.GetSingleton<RewindState>();
            var config = SystemAPI.GetSingleton<PhysicsConfig>();

            // Skip during rewind playback
            if (rewindState.Mode == RewindMode.Playback)
            {
                return;
            }

            // Skip if Godgame physics is disabled
            if (!config.IsGodgamePhysicsEnabled)
            {
                return;
            }

            // Skip during post-rewind settle frames
            if (PhysicsConfigHelpers.IsPostRewindSettleFrame(in config, timeState.Tick))
            {
                return;
            }

            bool logCollisions = config.LogCollisions != 0;

            // Process collision events for all entities with PhysicsCollisionEventElement buffers
            foreach (var (events, entity) in SystemAPI.Query<DynamicBuffer<PhysicsCollisionEventElement>>()
                .WithEntityAccess())
            {
                for (int i = 0; i < events.Length; i++)
                {
                    var evt = events[i];
                    
                    // Calculate damage from impulse
                    float damage = evt.Impulse * DamagePerImpulse;

                    // Apply damage (placeholder - can be extended with health components)
                    // For now, just log the collision
                    if (logCollisions)
                    {
                        UnityEngine.Debug.Log($"[GodgameCollision] Entity {entity.Index} hit Entity {evt.OtherEntity.Index} impulse={evt.Impulse:G2} damage={damage:G2}");
                    }

                    // TODO: Apply damage to health components if they exist
                    // For villagers, could apply debuff/stun
                    // if (SystemAPI.HasComponent<VillagerHealth>(entity))
                    // {
                    //     var health = SystemAPI.GetComponentRW<VillagerHealth>(entity);
                    //     health.ValueRW.CurrentHealth -= damage;
                    //     
                    //     // Apply brief stun/knockdown if damage is significant
                    //     if (damage > threshold)
                    //     {
                    //         // Add stun component
                    //     }
                    // }
                }
            }
        }
    }
}
