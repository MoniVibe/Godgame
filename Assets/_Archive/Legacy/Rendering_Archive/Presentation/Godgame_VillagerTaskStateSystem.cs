using Godgame.AI;
using Godgame.Miracles;
using Godgame.Villagers;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

// Type aliases to resolve MiracleEffect ambiguity
using RuntimeMiracleEffect = PureDOTS.Runtime.Components.MiracleEffect;
using GameMiracleEffect = Godgame.Miracles.MiracleEffect;

namespace Godgame.Presentation
{
    /// <summary>
    /// System that updates villager visual task state based on sim components.
    /// Reads VillagerBehavior, VillagerRoutine, HandHeldTag, and nearby MiracleEffect to determine task state.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(Godgame_VillagerPresentationSystem))]
    public partial struct Godgame_VillagerTaskStateSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VillagerVisualState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Update task state from VillagerJob
            var taskStateJob = new UpdateVillagerTaskStateJob();
            taskStateJob.ScheduleParallel();

            // Update miracle effects (requires lookups)
            // Collect all miracle effect entities in OnUpdate (SystemAPI.Query allowed here)
            var miracleEntities = SystemAPI.QueryBuilder()
                .WithAll<GameMiracleEffect>()
                .Build()
                .ToEntityArray(state.WorldUpdateAllocator);
            
            var miracleJob = new UpdateVillagerMiracleEffectJob
            {
                MiracleEffectLookup = SystemAPI.GetComponentLookup<GameMiracleEffect>(true),
                TransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true),
                MiracleEffectEntities = miracleEntities
            };
            miracleJob.ScheduleParallel();
        }
    }

    /// <summary>
    /// Job to update villager task state from sim components.
    /// Reads VillagerJob.Phase to determine task state.
    /// </summary>
    [BurstCompile]
    public partial struct UpdateVillagerTaskStateJob : IJobEntity
    {
        public void Execute(
            ref VillagerVisualState visualState,
            in VillagerJob job,
            in VillagerPresentationTag tag)
        {
            // Map VillagerJob.Phase to VillagerTaskState
            VillagerTaskState taskState = VillagerTaskState.Idle;
            float intensity = 0f;

            switch (job.Phase)
            {
                case VillagerJob.JobPhase.Idle:
                    taskState = VillagerTaskState.Idle;
                    intensity = 0f;
                    break;

                case VillagerJob.JobPhase.Assigned:
                    // Assigned includes navigating to target, treat as walking
                    taskState = VillagerTaskState.Walking;
                    intensity = 0.5f;
                    break;

                case VillagerJob.JobPhase.Gathering:
                    taskState = VillagerTaskState.Gathering;
                    intensity = 0.8f;
                    break;

                case VillagerJob.JobPhase.Delivering:
                    taskState = VillagerTaskState.Carrying;
                    intensity = 0.7f;
                    break;

                case VillagerJob.JobPhase.Completed:
                    taskState = VillagerTaskState.Idle;
                    intensity = 0f;
                    break;

                default:
                    taskState = VillagerTaskState.Idle;
                    intensity = 0f;
                    break;
            }

            visualState.TaskState = taskState;
            visualState.TaskStateIntensity = intensity;
        }
    }

    /// <summary>
    /// Job that checks for miracle effects nearby and overrides task state.
    /// Runs after UpdateVillagerTaskStateJob to apply miracle effects.
    /// </summary>
    [BurstCompile]
    public partial struct UpdateVillagerMiracleEffectJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<GameMiracleEffect> MiracleEffectLookup;
        [ReadOnly] public ComponentLookup<LocalTransform> TransformLookup;
        [ReadOnly] public NativeArray<Entity> MiracleEffectEntities;

        public void Execute(
            ref VillagerVisualState visualState,
            in VillagerPresentationTag tag,
            in LocalTransform transform)
        {
            // Check for nearby miracle effects using lookups (no SystemAPI.Query in Execute)
            bool hasNearbyMiracle = false;
            float maxIntensity = 0f;

            // Iterate through all miracle effect entities (collected in OnUpdate)
            for (int i = 0; i < MiracleEffectEntities.Length; i++)
            {
                var miracleEntity = MiracleEffectEntities[i];
                if (!TransformLookup.HasComponent(miracleEntity) || !MiracleEffectLookup.HasComponent(miracleEntity))
                {
                    continue;
                }

                var miracleEffect = MiracleEffectLookup[miracleEntity];
                var miracleTransform = TransformLookup[miracleEntity];
                float distance = math.distance(transform.Position, miracleTransform.Position);

                if (distance <= miracleEffect.Radius)
                {
                    hasNearbyMiracle = true;
                    maxIntensity = math.max(maxIntensity, miracleEffect.Intensity);
                }
            }

            if (hasNearbyMiracle)
            {
                // Override task state with miracle effect
                visualState.TaskState = VillagerTaskState.MiracleAffected;
                visualState.TaskStateIntensity = maxIntensity;
                visualState.EffectIntensity = maxIntensity;
            }
        }
    }
}

