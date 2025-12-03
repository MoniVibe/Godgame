using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Launch;
using PureDOTS.Runtime.Physics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Adapters.Launch
{
    /// <summary>
    /// Godgame-specific adapter for slingshot/throw mechanics.
    /// Reads Godgame input state and writes LaunchRequest entries.
    /// Also processes collision events for launched projectiles.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(PureDOTS.Systems.Launch.LaunchRequestIntakeSystem))]
    public partial struct GodgameSlingshotInputAdapter : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var rewindState = SystemAPI.GetSingleton<RewindState>();

            // Only process in Record mode
            if (rewindState.Mode != RewindMode.Record)
            {
                return;
            }

            var timeState = SystemAPI.GetSingleton<TimeState>();

            // Process slingshots with pending throw commands
            // In a real implementation, this would read from an input buffer or command queue
            // For now, this is a placeholder showing the adapter pattern

            foreach (var (config, slingshotConfig, requestBuffer, transform, entity) in
                SystemAPI.Query<RefRO<LauncherConfig>, RefRO<GodgameSlingshotConfig>, DynamicBuffer<LaunchRequest>, RefRO<LocalTransform>>()
                    .WithAll<GodgameSlingshotTag>()
                    .WithEntityAccess())
            {
                // Example: Check for pending throw commands (would come from input system)
                // ProcessThrowCommands(ref requestBuffer, config, slingshotConfig, transform, timeState.Tick);
            }
        }

        /// <summary>
        /// Helper to queue a throw from a slingshot.
        /// Called by game input systems when player triggers a throw.
        /// </summary>
        public static void QueueThrow(
            ref DynamicBuffer<LaunchRequest> requestBuffer,
            Entity payloadEntity,
            float3 targetPosition,
            float3 launcherPosition,
            float speed,
            float arcHeight)
        {
            // Calculate launch velocity for parabolic arc
            var direction = targetPosition - launcherPosition;
            var horizontalDist = math.length(new float2(direction.x, direction.z));
            var verticalDist = direction.y;

            // Simple parabolic calculation
            var horizontalDir = math.normalize(new float3(direction.x, 0, direction.z));
            var launchAngle = math.atan2(verticalDist + arcHeight * horizontalDist, horizontalDist);

            var velocity = horizontalDir * speed * math.cos(launchAngle);
            velocity.y = speed * math.sin(launchAngle);

            requestBuffer.Add(new LaunchRequest
            {
                SourceEntity = Entity.Null, // Will be set by caller if needed
                PayloadEntity = payloadEntity,
                LaunchTick = 0, // Immediate
                InitialVelocity = velocity,
                Flags = 0
            });
        }
    }

    /// <summary>
    /// Processes collision events for launched projectiles in Godgame.
    /// Translates generic collision events to Godgame-specific effects.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(PureDOTS.Systems.Launch.LaunchExecutionSystem))]
    public partial struct GodgameSlingshotCollisionAdapter : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var rewindState = SystemAPI.GetSingleton<RewindState>();

            // Only process in Record mode
            if (rewindState.Mode != RewindMode.Record)
            {
                return;
            }

            // Process collision events for launched projectiles
            foreach (var (projectileTag, collisionBuffer, entity) in
                SystemAPI.Query<RefRO<LaunchedProjectileTag>, DynamicBuffer<PhysicsCollisionEventElement>>()
                    .WithEntityAccess())
            {
                for (int i = 0; i < collisionBuffer.Length; i++)
                {
                    var collision = collisionBuffer[i];

                    // Process Godgame-specific collision effects
                    // Examples:
                    // - Apply damage to hit entity
                    // - Spawn debris/particles (via presentation bridge)
                    // - Trigger miracle effects
                    // - Update villager morale/fear

                    // This is where game-specific logic goes
                    // The adapter translates generic collision events to Godgame behavior
                }
            }
        }
    }
}

