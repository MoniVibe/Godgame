using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Time;

namespace Godgame.Temporal
{
    /// <summary>
    /// High-level API for time control in Godgame.
    /// Wraps PureDOTS time systems for game-specific use.
    /// 
    /// SINGLE-PLAYER ONLY: This API is designed for single-player mode.
    /// In multiplayer, these methods will route through per-player time authority.
    /// All commands use PlayerId = TimePlayerIds.SinglePlayer (0) and Scope = Global or LocalBubble.
    /// </summary>
    public static class GodgameTimeAPI
    {
        /// <summary>
        /// Requests a global rewind to the specified number of ticks in the past.
        /// 
        /// SINGLE-PLAYER ONLY: Convenience wrapper that calls the player-aware overload with playerId = 0.
        /// </summary>
        /// <param name="world">The ECS world.</param>
        /// <param name="ticksBack">Number of ticks to rewind.</param>
        /// <param name="sourceMiracleId">ID of the miracle that triggered the rewind (0 for player input).</param>
        /// <returns>True if the request was accepted.</returns>
        public static bool RequestGlobalRewind(World world, uint ticksBack, uint sourceMiracleId = 0)
        {
            return RequestGlobalRewind(world, ticksBack, sourceMiracleId, TimePlayerIds.SinglePlayer);
        }

        /// <summary>
        /// Requests a global rewind to the specified number of ticks in the past.
        /// 
        /// MULTIPLAYER-AWARE: Takes explicit playerId parameter. In SP, use TimePlayerIds.SinglePlayer (0).
        /// In MP, this will route through per-player time authority. Uses Scope = Global.
        /// </summary>
        /// <param name="world">The ECS world.</param>
        /// <param name="ticksBack">Number of ticks to rewind.</param>
        /// <param name="sourceMiracleId">ID of the miracle that triggered the rewind (0 for player input).</param>
        /// <param name="playerId">Player ID making the request (0 for single-player).</param>
        /// <returns>True if the request was accepted.</returns>
        public static bool RequestGlobalRewind(World world, uint ticksBack, uint sourceMiracleId, byte playerId)
        {
            if (world == null || !world.IsCreated)
            {
                return false;
            }

            var entityManager = world.EntityManager;

            // Get current tick
            using var timeQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<TimeState>());
            if (timeQuery.IsEmptyIgnoreFilter)
            {
                return false;
            }

            var timeState = timeQuery.GetSingleton<TimeState>();
            uint targetTick = timeState.Tick > ticksBack ? timeState.Tick - ticksBack : 0;

            // Find the command buffer entity
            using var rewindQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<RewindState>());
            if (rewindQuery.IsEmptyIgnoreFilter)
            {
                return false;
            }

            var rewindEntity = rewindQuery.GetSingletonEntity();
            if (!entityManager.HasBuffer<TimeControlCommand>(rewindEntity))
            {
                return false;
            }

            var commandBuffer = entityManager.GetBuffer<TimeControlCommand>(rewindEntity);
            commandBuffer.Add(new TimeControlCommand
            {
                Type = TimeControlCommandType.StartRewind,
                UintParam = targetTick,
                Scope = TimeControlScope.Global,
                Source = sourceMiracleId > 0 ? TimeControlSource.Miracle : TimeControlSource.Player,
                SourceId = sourceMiracleId,
                PlayerId = playerId,
                Priority = 0
            });

            return true;
        }

        /// <summary>
        /// Spawns a time bubble at the specified position.
        /// 
        /// SINGLE-PLAYER ONLY: Convenience wrapper that calls the player-aware overload with ownerPlayerId = 0.
        /// </summary>
        /// <param name="world">The ECS world.</param>
        /// <param name="center">Center position of the bubble.</param>
        /// <param name="radius">Radius of the bubble.</param>
        /// <param name="mode">Time mode for the bubble.</param>
        /// <param name="scale">Time scale (for Scale/FastForward modes).</param>
        /// <param name="durationTicks">Duration in ticks (0 = permanent until removed).</param>
        /// <param name="priority">Priority for overlap resolution.</param>
        /// <param name="sourceMiracleEntity">Entity of the miracle that created this bubble.</param>
        /// <returns>The created bubble entity, or Entity.Null on failure.</returns>
        public static Entity SpawnTimeBubble(World world, float3 center, float radius, 
            TimeBubbleMode mode = TimeBubbleMode.Scale, float scale = 0.5f, 
            uint durationTicks = 0, byte priority = 100, Entity sourceMiracleEntity = default)
        {
            return SpawnTimeBubble(world, center, radius, mode, scale, durationTicks, priority, sourceMiracleEntity, TimePlayerIds.SinglePlayer);
        }

        /// <summary>
        /// Spawns a time bubble at the specified position.
        /// 
        /// MULTIPLAYER-AWARE: Takes explicit ownerPlayerId parameter. In SP, use TimePlayerIds.SinglePlayer (0).
        /// In MP, this determines which player owns the bubble. Uses Scope = LocalBubble.
        /// </summary>
        /// <param name="world">The ECS world.</param>
        /// <param name="center">Center position of the bubble.</param>
        /// <param name="radius">Radius of the bubble.</param>
        /// <param name="mode">Time mode for the bubble.</param>
        /// <param name="scale">Time scale (for Scale/FastForward modes).</param>
        /// <param name="durationTicks">Duration in ticks (0 = permanent until removed).</param>
        /// <param name="priority">Priority for overlap resolution.</param>
        /// <param name="sourceMiracleEntity">Entity of the miracle that created this bubble.</param>
        /// <param name="ownerPlayerId">Owner player ID (0 for single-player).</param>
        /// <returns>The created bubble entity, or Entity.Null on failure.</returns>
        public static Entity SpawnTimeBubble(World world, float3 center, float radius, 
            TimeBubbleMode mode, float scale, 
            uint durationTicks, byte priority, Entity sourceMiracleEntity, byte ownerPlayerId)
        {
            if (world == null || !world.IsCreated)
            {
                return Entity.Null;
            }

            var entityManager = world.EntityManager;

            // Get next bubble ID
            using var systemStateQuery = entityManager.CreateEntityQuery(ComponentType.ReadWrite<TimeBubbleSystemState>());
            uint bubbleId;
            if (systemStateQuery.IsEmptyIgnoreFilter)
            {
                // Create system state if it doesn't exist
                var stateEntity = entityManager.CreateEntity(typeof(TimeBubbleSystemState));
                entityManager.SetComponentData(stateEntity, new TimeBubbleSystemState
                {
                    NextBubbleId = 2,
                    ActiveBubbleCount = 0,
                    AffectedEntityCount = 0,
                    LastUpdateTick = 0
                });
                bubbleId = 1;
            }
            else
            {
                var stateEntity = systemStateQuery.GetSingletonEntity();
                var state = entityManager.GetComponentData<TimeBubbleSystemState>(stateEntity);
                bubbleId = state.NextBubbleId;
                state.NextBubbleId++;
                state.ActiveBubbleCount++;
                entityManager.SetComponentData(stateEntity, state);
            }

            // Get current tick for creation timestamp
            uint currentTick = 0;
            using var timeQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<TimeState>());
            if (!timeQuery.IsEmptyIgnoreFilter)
            {
                currentTick = timeQuery.GetSingleton<TimeState>().Tick;
            }

            // Create the bubble entity
            var bubbleEntity = entityManager.CreateEntity();
            
            entityManager.AddComponentData(bubbleEntity, TimeBubbleId.Create(bubbleId, new FixedString32Bytes("Miracle")));
            
            entityManager.AddComponentData(bubbleEntity, new TimeBubbleParams
            {
                BubbleId = bubbleId,
                Mode = mode,
                Scale = scale,
                RewindOffsetTicks = 0,
                PlaybackTick = 0,
                Priority = priority,
                OwnerPlayerId = ownerPlayerId,
                AffectsOwnedEntitiesOnly = false,
                SourceEntity = sourceMiracleEntity,
                DurationTicks = durationTicks,
                CreatedAtTick = currentTick,
                IsActive = true,
                AllowMembershipChanges = mode != TimeBubbleMode.Stasis
            });

            entityManager.AddComponentData(bubbleEntity, TimeBubbleVolume.CreateSphere(center, radius));

            return bubbleEntity;
        }

        /// <summary>
        /// Removes a time bubble.
        /// </summary>
        /// <param name="world">The ECS world.</param>
        /// <param name="bubbleEntity">The bubble entity to remove.</param>
        /// <returns>True if the bubble was found and removed.</returns>
        public static bool RemoveTimeBubble(World world, Entity bubbleEntity)
        {
            if (world == null || !world.IsCreated || bubbleEntity == Entity.Null)
            {
                return false;
            }

            var entityManager = world.EntityManager;

            if (!entityManager.Exists(bubbleEntity))
            {
                return false;
            }

            // Get bubble ID to clean up memberships
            uint bubbleId = 0;
            if (entityManager.HasComponent<TimeBubbleId>(bubbleEntity))
            {
                bubbleId = entityManager.GetComponentData<TimeBubbleId>(bubbleEntity).Id;
            }

            // Remove membership from affected entities
            if (bubbleId > 0)
            {
                using var membershipQuery = entityManager.CreateEntityQuery(ComponentType.ReadWrite<TimeBubbleMembership>());
                var entities = membershipQuery.ToEntityArray(Allocator.Temp);
                
                foreach (var entity in entities)
                {
                    var membership = entityManager.GetComponentData<TimeBubbleMembership>(entity);
                    if (membership.BubbleId == bubbleId)
                    {
                        entityManager.RemoveComponent<TimeBubbleMembership>(entity);
                        if (entityManager.HasComponent<StasisTag>(entity))
                        {
                            entityManager.RemoveComponent<StasisTag>(entity);
                        }
                    }
                }
                
                entities.Dispose();
            }

            // Update system state
            using var systemStateQuery = entityManager.CreateEntityQuery(ComponentType.ReadWrite<TimeBubbleSystemState>());
            if (!systemStateQuery.IsEmptyIgnoreFilter)
            {
                var stateEntity = systemStateQuery.GetSingletonEntity();
                var state = entityManager.GetComponentData<TimeBubbleSystemState>(stateEntity);
                state.ActiveBubbleCount = math.max(0, state.ActiveBubbleCount - 1);
                entityManager.SetComponentData(stateEntity, state);
            }

            // Destroy the bubble entity
            entityManager.DestroyEntity(bubbleEntity);
            return true;
        }

        /// <summary>
        /// Gets the time bubble entity for a miracle.
        /// </summary>
        /// <param name="world">The ECS world.</param>
        /// <param name="miracleEntity">The miracle entity that created the bubble.</param>
        /// <returns>The bubble entity if found, otherwise Entity.Null.</returns>
        public static Entity GetTimeBubbleForMiracle(World world, Entity miracleEntity)
        {
            if (world == null || !world.IsCreated || miracleEntity == Entity.Null)
            {
                return Entity.Null;
            }

            var entityManager = world.EntityManager;
            using var bubbleQuery = entityManager.CreateEntityQuery(
                ComponentType.ReadOnly<TimeBubbleParams>(),
                ComponentType.ReadOnly<TimeBubbleId>());

            var entities = bubbleQuery.ToEntityArray(Allocator.Temp);
            var paramsArray = bubbleQuery.ToComponentDataArray<TimeBubbleParams>(Allocator.Temp);

            Entity result = Entity.Null;
            for (int i = 0; i < entities.Length; i++)
            {
                if (paramsArray[i].SourceEntity == miracleEntity && paramsArray[i].IsActive)
                {
                    result = entities[i];
                    break;
                }
            }

            entities.Dispose();
            paramsArray.Dispose();
            return result;
        }

        /// <summary>
        /// Sets the global time speed.
        /// 
        /// SINGLE-PLAYER ONLY: Convenience wrapper that calls the player-aware overload with playerId = 0.
        /// </summary>
        /// <param name="world">The ECS world.</param>
        /// <param name="speedMultiplier">Speed multiplier (0.01 to 16.0).</param>
        /// <returns>True if the command was accepted.</returns>
        public static bool SetGlobalTimeSpeed(World world, float speedMultiplier)
        {
            return SetGlobalTimeSpeed(world, speedMultiplier, TimePlayerIds.SinglePlayer);
        }

        /// <summary>
        /// Sets the global time speed.
        /// 
        /// MULTIPLAYER-AWARE: Takes explicit playerId parameter. In SP, use TimePlayerIds.SinglePlayer (0).
        /// In MP, this will route through per-player time authority. Uses Scope = Global.
        /// </summary>
        /// <param name="world">The ECS world.</param>
        /// <param name="speedMultiplier">Speed multiplier (0.01 to 16.0).</param>
        /// <param name="playerId">Player ID making the request (0 for single-player).</param>
        /// <returns>True if the command was accepted.</returns>
        public static bool SetGlobalTimeSpeed(World world, float speedMultiplier, byte playerId)
        {
            if (world == null || !world.IsCreated)
            {
                return false;
            }

            var entityManager = world.EntityManager;
            using var rewindQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<RewindState>());
            if (rewindQuery.IsEmptyIgnoreFilter)
            {
                return false;
            }

            var rewindEntity = rewindQuery.GetSingletonEntity();
            if (!entityManager.HasBuffer<TimeControlCommand>(rewindEntity))
            {
                return false;
            }

            var commandBuffer = entityManager.GetBuffer<TimeControlCommand>(rewindEntity);
            commandBuffer.Add(new TimeControlCommand
            {
                Type = TimeControlCommandType.SetSpeed,
                FloatParam = TimeHelpers.ClampSpeed(speedMultiplier),
                Scope = TimeControlScope.Global,
                Source = TimeControlSource.Player,
                PlayerId = playerId,
                Priority = 0
            });

            return true;
        }

        /// <summary>
        /// Toggles global pause state.
        /// 
        /// SINGLE-PLAYER ONLY: Convenience wrapper that calls the player-aware overload with playerId = 0.
        /// </summary>
        /// <param name="world">The ECS world.</param>
        /// <returns>True if the command was accepted.</returns>
        public static bool TogglePause(World world)
        {
            return TogglePause(world, TimePlayerIds.SinglePlayer);
        }

        /// <summary>
        /// Toggles global pause state.
        /// 
        /// MULTIPLAYER-AWARE: Takes explicit playerId parameter. In SP, use TimePlayerIds.SinglePlayer (0).
        /// In MP, this will route through per-player time authority. Uses Scope = Global.
        /// </summary>
        /// <param name="world">The ECS world.</param>
        /// <param name="playerId">Player ID making the request (0 for single-player).</param>
        /// <returns>True if the command was accepted.</returns>
        public static bool TogglePause(World world, byte playerId)
        {
            if (world == null || !world.IsCreated)
            {
                return false;
            }

            var entityManager = world.EntityManager;
            
            // Check current pause state
            using var timeQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<TimeState>());
            if (timeQuery.IsEmptyIgnoreFilter)
            {
                return false;
            }

            var timeState = timeQuery.GetSingleton<TimeState>();
            
            using var rewindQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<RewindState>());
            if (rewindQuery.IsEmptyIgnoreFilter)
            {
                return false;
            }

            var rewindEntity = rewindQuery.GetSingletonEntity();
            if (!entityManager.HasBuffer<TimeControlCommand>(rewindEntity))
            {
                return false;
            }

            var commandBuffer = entityManager.GetBuffer<TimeControlCommand>(rewindEntity);
            commandBuffer.Add(new TimeControlCommand
            {
                Type = timeState.IsPaused ? TimeControlCommandType.Resume : TimeControlCommandType.Pause,
                Scope = TimeControlScope.Global,
                Source = TimeControlSource.Player,
                PlayerId = playerId,
                Priority = 0
            });

            return true;
        }

        /// <summary>
        /// Steps the simulation forward by the specified number of ticks.
        /// 
        /// SINGLE-PLAYER ONLY: Convenience wrapper that calls the player-aware overload with playerId = 0.
        /// </summary>
        /// <param name="world">The ECS world.</param>
        /// <param name="ticks">Number of ticks to advance.</param>
        /// <returns>True if the command was accepted.</returns>
        public static bool StepForward(World world, uint ticks = 1)
        {
            return StepForward(world, ticks, TimePlayerIds.SinglePlayer);
        }

        /// <summary>
        /// Steps the simulation forward by the specified number of ticks.
        /// 
        /// MULTIPLAYER-AWARE: Takes explicit playerId parameter. In SP, use TimePlayerIds.SinglePlayer (0).
        /// In MP, this will route through per-player time authority. Uses Scope = Global.
        /// </summary>
        /// <param name="world">The ECS world.</param>
        /// <param name="ticks">Number of ticks to advance.</param>
        /// <param name="playerId">Player ID making the request (0 for single-player).</param>
        /// <returns>True if the command was accepted.</returns>
        public static bool StepForward(World world, uint ticks, byte playerId)
        {
            if (world == null || !world.IsCreated)
            {
                return false;
            }

            var entityManager = world.EntityManager;
            using var rewindQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<RewindState>());
            if (rewindQuery.IsEmptyIgnoreFilter)
            {
                return false;
            }

            var rewindEntity = rewindQuery.GetSingletonEntity();
            if (!entityManager.HasBuffer<TimeControlCommand>(rewindEntity))
            {
                return false;
            }

            var commandBuffer = entityManager.GetBuffer<TimeControlCommand>(rewindEntity);
            commandBuffer.Add(new TimeControlCommand
            {
                Type = TimeControlCommandType.StepTicks,
                UintParam = ticks,
                Scope = TimeControlScope.Global,
                Source = TimeControlSource.Player,
                PlayerId = playerId,
                Priority = 0
            });

            return true;
        }

        /// <summary>
        /// Creates a slow-time miracle bubble (Temporal Veil).
        /// SINGLE-PLAYER ONLY: Convenience wrapper with ownerPlayerId = 0.
        /// </summary>
        public static Entity CreateTemporalVeilBubble(World world, float3 center, float radius, 
            uint durationTicks, Entity miracleEntity)
        {
            return CreateTemporalVeilBubble(world, center, radius, durationTicks, miracleEntity, TimePlayerIds.SinglePlayer);
        }

        /// <summary>
        /// Creates a slow-time miracle bubble (Temporal Veil).
        /// MULTIPLAYER-AWARE: Takes explicit ownerPlayerId parameter.
        /// </summary>
        /// <param name="world">The ECS world.</param>
        /// <param name="center">Center position of the bubble.</param>
        /// <param name="radius">Radius of the bubble.</param>
        /// <param name="durationTicks">Duration in ticks.</param>
        /// <param name="miracleEntity">Entity of the miracle that created this bubble.</param>
        /// <param name="ownerPlayerId">Owner player ID (0 for single-player).</param>
        public static Entity CreateTemporalVeilBubble(World world, float3 center, float radius, 
            uint durationTicks, Entity miracleEntity, byte ownerPlayerId)
        {
            return SpawnTimeBubble(world, center, radius, TimeBubbleMode.Scale, 0.25f, 
                durationTicks, 150, miracleEntity, ownerPlayerId);
        }

        /// <summary>
        /// Creates a stasis miracle bubble (complete freeze).
        /// SINGLE-PLAYER ONLY: Convenience wrapper with ownerPlayerId = 0.
        /// </summary>
        public static Entity CreateStasisBubble(World world, float3 center, float radius, 
            uint durationTicks, Entity miracleEntity)
        {
            return CreateStasisBubble(world, center, radius, durationTicks, miracleEntity, TimePlayerIds.SinglePlayer);
        }

        /// <summary>
        /// Creates a stasis miracle bubble (complete freeze).
        /// MULTIPLAYER-AWARE: Takes explicit ownerPlayerId parameter.
        /// </summary>
        /// <param name="world">The ECS world.</param>
        /// <param name="center">Center position of the bubble.</param>
        /// <param name="radius">Radius of the bubble.</param>
        /// <param name="durationTicks">Duration in ticks.</param>
        /// <param name="miracleEntity">Entity of the miracle that created this bubble.</param>
        /// <param name="ownerPlayerId">Owner player ID (0 for single-player).</param>
        public static Entity CreateStasisBubble(World world, float3 center, float radius, 
            uint durationTicks, Entity miracleEntity, byte ownerPlayerId)
        {
            return SpawnTimeBubble(world, center, radius, TimeBubbleMode.Stasis, 0f, 
                durationTicks, 200, miracleEntity, ownerPlayerId);
        }

        /// <summary>
        /// Creates a fast-forward bubble (accelerated growth/activity).
        /// SINGLE-PLAYER ONLY: Convenience wrapper with ownerPlayerId = 0.
        /// </summary>
        public static Entity CreateAccelerationBubble(World world, float3 center, float radius, 
            float speedMultiplier, uint durationTicks, Entity miracleEntity)
        {
            return CreateAccelerationBubble(world, center, radius, speedMultiplier, durationTicks, miracleEntity, TimePlayerIds.SinglePlayer);
        }

        /// <summary>
        /// Creates a fast-forward bubble (accelerated growth/activity).
        /// MULTIPLAYER-AWARE: Takes explicit ownerPlayerId parameter.
        /// </summary>
        /// <param name="world">The ECS world.</param>
        /// <param name="center">Center position of the bubble.</param>
        /// <param name="radius">Radius of the bubble.</param>
        /// <param name="speedMultiplier">Speed multiplier for the bubble.</param>
        /// <param name="durationTicks">Duration in ticks.</param>
        /// <param name="miracleEntity">Entity of the miracle that created this bubble.</param>
        /// <param name="ownerPlayerId">Owner player ID (0 for single-player).</param>
        public static Entity CreateAccelerationBubble(World world, float3 center, float radius, 
            float speedMultiplier, uint durationTicks, Entity miracleEntity, byte ownerPlayerId)
        {
            return SpawnTimeBubble(world, center, radius, TimeBubbleMode.FastForward, speedMultiplier, 
                durationTicks, 125, miracleEntity, ownerPlayerId);
        }
    }
}

