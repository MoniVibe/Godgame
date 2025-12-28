using Godgame.Input;
using Godgame.Presentation;
using PureDOTS.Input;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Miracles;
using PureDOTS.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using EntitiesPresentationSystemGroup = Unity.Entities.PresentationSystemGroup;
using PureDotsPresentationSystemGroup = PureDOTS.Systems.PresentationSystemGroup;
using MiracleType = PureDOTS.Runtime.Components.MiracleType;

namespace Godgame.Miracles
{
    /// <summary>
    /// Bridges the new GodgameInputReader ECS components to the existing miracle system.
    /// Reads MiracleInput from the input singleton and updates MiracleCasterState.
    /// Also handles direct miracle spawning when no DivineHand system is active.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    // Removed invalid UpdateBefore: GodgameMiracleInputSystem runs in HandSystemGroup; cross-group ordering must be configured at group level.
    public partial struct Godgame_MiracleInputBridgeSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GodgameInputSingleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // Get the input singleton
            if (!SystemAPI.TryGetSingleton<MiracleInput>(out var miracleInput))
            {
                return;
            }

            // Update any existing MiracleCasterState entities with the new input
            foreach (var (casterStateRef, slotBuffer, runtimeStateRef, selectionRef) in SystemAPI.Query<RefRW<MiracleCasterState>, DynamicBuffer<MiracleSlotDefinition>, RefRW<MiracleRuntimeStateNew>, RefRW<MiracleCasterSelection>>())
            {
                ref var casterState = ref casterStateRef.ValueRW;
                ref var runtimeState = ref runtimeStateRef.ValueRW;
                ref var selection = ref selectionRef.ValueRW;

                // Update selected slot from input
                casterState.SelectedSlot = miracleInput.SelectedSlot;
                selection.SelectedSlot = miracleInput.SelectedSlot;

                // Update sustained cast state
                casterState.SustainedCastHeld = miracleInput.SustainedCastHeld;

                // Handle throw cast trigger (edge detection already done in input reader)
                if (miracleInput.ThrowCastTriggered == 1)
                {
                    casterState.ThrowCastTriggered = 1;
                }

                runtimeState.SelectedId = ResolveSelectedMiracleId(miracleInput.SelectedSlot, slotBuffer);
                runtimeState.IsActivating = miracleInput.CastTriggered;
                runtimeState.IsSustained = miracleInput.SustainedCastHeld;
            }

            // If there's a cast triggered and no MiracleCasterState exists, 
            // spawn a miracle directly at the target position
            if (miracleInput.CastTriggered == 1 && miracleInput.HasValidTarget == 1)
            {
                bool hasCaster = false;
                foreach (var _ in SystemAPI.Query<RefRO<MiracleCasterState>>())
                {
                    hasCaster = true;
                    break;
                }

                if (!hasCaster)
                {
                    SpawnDirectMiracle(ref state, miracleInput);
                }
            }
        }

        private static MiracleId ResolveSelectedMiracleId(byte slot, DynamicBuffer<MiracleSlotDefinition> slots)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].SlotIndex == slot)
                {
                    return (MiracleId)slots[i].Type;
                }
            }

            return (MiracleId)slot;
        }

        private void SpawnDirectMiracle(ref SystemState state, MiracleInput miracleInput)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            // Create a miracle effect entity at the target position
            var miracleEntity = ecb.CreateEntity();
            ecb.AddComponent(miracleEntity, LocalTransform.FromPosition(miracleInput.TargetPosition));
            ecb.AddComponent(miracleEntity, new MiracleEffect
            {
                Type = (MiracleType)miracleInput.SelectedSlot,
                Position = miracleInput.TargetPosition,
                Radius = 10f, // Default radius
                Intensity = 1f,
                Duration = 5f,
                RemainingDuration = 5f
            });

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    /// <summary>
    /// Component for active miracle effects in the world.
    /// Used for visual feedback and gameplay effects.
    /// </summary>
    public struct MiracleEffect : IComponentData
    {
        public MiracleType Type;
        public float3 Position;
        public float Radius;
        public float Intensity;
        public float Duration;
        public float RemainingDuration;
    }

    /// <summary>
    /// Tag component for entities affected by a miracle (temporary, removed when effect expires).
    /// </summary>
    public struct MiracleAffectedTag : IComponentData
    {
        public Entity MiracleEntity;
        public float EffectIntensity;
    }

    /// <summary>
    /// Component for area-of-effect ring visualization (decal entity).
    /// </summary>
    public struct MiracleAreaOfEffect : IComponentData
    {
        public Entity MiracleEntity;
        public float Radius;
        public float Intensity;
    }

    /// <summary>
    /// Presentation system for miracle effects.
    /// Reads miracle effects and updates visual state, spawns AOE rings, and tags affected entities.
    /// </summary>
    [UpdateInGroup(typeof(EntitiesPresentationSystemGroup))]
    public partial struct Godgame_MiraclePresentationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MiracleEffect>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            // Update miracle effects and spawn AOE rings
            foreach (var (effectRef, transform, entity) in SystemAPI.Query<RefRW<MiracleEffect>, RefRO<LocalTransform>>().WithEntityAccess())
            {
                ref var effect = ref effectRef.ValueRW;

                // Decrease remaining duration
                effect.RemainingDuration -= deltaTime;

                // Fade out intensity as duration decreases
                float progress = effect.RemainingDuration / effect.Duration;
                effect.Intensity = math.saturate(progress);

                // Ensure AOE ring entity exists
                EnsureAOERing(ref state, ref ecb, entity, effect, transform.ValueRO.Position);

                // Tag affected entities within radius
                TagAffectedEntities(ref state, ref ecb, entity, effect, transform.ValueRO.Position);

                // Destroy effect when duration expires
                if (effect.RemainingDuration <= 0)
                {
                    // Remove affected tags
                    RemoveAffectedTags(ref state, ref ecb, entity);
                    // Destroy AOE ring
                    DestroyAOERing(ref state, ref ecb, entity);
                    ecb.DestroyEntity(entity);
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        private void EnsureAOERing(ref SystemState state, ref EntityCommandBuffer ecb, Entity miracleEntity, MiracleEffect effect, float3 position)
        {
            // Check if AOE ring already exists
            bool hasAOE = false;
            Entity aoeEntity = Entity.Null;

            foreach (var (aoe, entity) in SystemAPI.Query<RefRO<MiracleAreaOfEffect>>().WithEntityAccess())
            {
                if (aoe.ValueRO.MiracleEntity == miracleEntity)
                {
                    hasAOE = true;
                    aoeEntity = entity;
                    break;
                }
            }

            if (!hasAOE)
            {
                // Spawn AOE ring entity
                aoeEntity = ecb.CreateEntity();
                ecb.AddComponent(aoeEntity, LocalTransform.FromPosition(position));
                ecb.AddComponent(aoeEntity, new MiracleAreaOfEffect
                {
                    MiracleEntity = miracleEntity,
                    Radius = effect.Radius,
                    Intensity = effect.Intensity
                });
#if UNITY_EDITOR
                // Use ECB to set name, not EntityManager (entity is deferred until playback)
                ecb.SetName(aoeEntity, $"MiracleAOE_{miracleEntity.Index}");
#endif
            }
            else
            {
                // Update existing AOE ring
                var aoe = state.EntityManager.GetComponentData<MiracleAreaOfEffect>(aoeEntity);
                aoe.Radius = effect.Radius;
                aoe.Intensity = effect.Intensity;
                state.EntityManager.SetComponentData(aoeEntity, aoe);
            }
        }

        private void TagAffectedEntities(ref SystemState state, ref EntityCommandBuffer ecb, Entity miracleEntity, MiracleEffect effect, float3 position)
        {
            // Tag villagers within radius
            foreach (var (villagerTransform, villagerEntity) in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<VillagerPresentationTag>().WithEntityAccess())
            {
                float distance = math.distance(position, villagerTransform.ValueRO.Position);
                if (distance <= effect.Radius)
                {
                    if (!state.EntityManager.HasComponent<MiracleAffectedTag>(villagerEntity))
                    {
                        ecb.AddComponent(villagerEntity, new MiracleAffectedTag
                        {
                            MiracleEntity = miracleEntity,
                            EffectIntensity = effect.Intensity
                        });
                    }
                    else
                    {
                        // Update intensity
                        var tag = state.EntityManager.GetComponentData<MiracleAffectedTag>(villagerEntity);
                        tag.EffectIntensity = effect.Intensity;
                        state.EntityManager.SetComponentData(villagerEntity, tag);
                    }
                }
            }

            // Tag resource chunks within radius
            foreach (var (chunkTransform, chunkEntity) in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<ResourceChunkPresentationTag>().WithEntityAccess())
            {
                float distance = math.distance(position, chunkTransform.ValueRO.Position);
                if (distance <= effect.Radius)
                {
                    if (!state.EntityManager.HasComponent<MiracleAffectedTag>(chunkEntity))
                    {
                        ecb.AddComponent(chunkEntity, new MiracleAffectedTag
                        {
                            MiracleEntity = miracleEntity,
                            EffectIntensity = effect.Intensity
                        });
                    }
                }
            }

            // Tag villages within radius
            foreach (var (villageTransform, villageEntity) in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<VillageCenterPresentationTag>().WithEntityAccess())
            {
                float distance = math.distance(position, villageTransform.ValueRO.Position);
                if (distance <= effect.Radius)
                {
                    if (!state.EntityManager.HasComponent<MiracleAffectedTag>(villageEntity))
                    {
                        ecb.AddComponent(villageEntity, new MiracleAffectedTag
                        {
                            MiracleEntity = miracleEntity,
                            EffectIntensity = effect.Intensity
                        });
                    }
                }
            }
        }

        private void RemoveAffectedTags(ref SystemState state, ref EntityCommandBuffer ecb, Entity miracleEntity)
        {
            // Remove tags from all entities affected by this miracle
            foreach (var (tag, entity) in SystemAPI.Query<RefRO<MiracleAffectedTag>>().WithEntityAccess())
            {
                if (tag.ValueRO.MiracleEntity == miracleEntity)
                {
                    ecb.RemoveComponent<MiracleAffectedTag>(entity);
                }
            }
        }

        private void DestroyAOERing(ref SystemState state, ref EntityCommandBuffer ecb, Entity miracleEntity)
        {
            // Find and destroy AOE ring for this miracle
            foreach (var (aoe, entity) in SystemAPI.Query<RefRO<MiracleAreaOfEffect>>().WithEntityAccess())
            {
                if (aoe.ValueRO.MiracleEntity == miracleEntity)
                {
                    ecb.DestroyEntity(entity);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// System that applies visual tinting to entities with MiracleAffectedTag.
    /// Updates VillagerVisualState and ResourceChunkVisualState with miracle tint overlay.
    /// </summary>
    [UpdateInGroup(typeof(EntitiesPresentationSystemGroup))]
    [UpdateAfter(typeof(Godgame_MiraclePresentationSystem))]
    public partial struct Godgame_MiracleTintingSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MiracleAffectedTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var job = new ApplyMiracleTintJob();
            job.ScheduleParallel();
        }
    }

    /// <summary>
    /// Job that applies miracle tint to villagers.
    /// </summary>
    [BurstCompile]
    public partial struct ApplyMiracleTintJob : IJobEntity
    {
        public void Execute(
            ref VillagerVisualState visualState,
            in MiracleAffectedTag miracleTag)
        {
            // Update visual state to show miracle effect
            visualState.EffectIntensity = miracleTag.EffectIntensity;
            visualState.TaskState = (int)Godgame.Presentation.VillagerTaskState.None; // Could map to specific task state if needed
        }
    }
}
