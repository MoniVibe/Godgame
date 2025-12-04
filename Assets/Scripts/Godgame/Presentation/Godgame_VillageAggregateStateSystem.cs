using Godgame.Miracles;
using Godgame.Villages;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Presentation
{
    /// <summary>
    /// System that updates village aggregate visual state based on village data.
    /// Reads Village, VillageResource buffer, and nearby MiracleEffect to determine aggregate state.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(Godgame_VillagePresentationSystem))]
    public partial struct Godgame_VillageAggregateStateSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VillageCenterVisualState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var miracleLookup = SystemAPI.GetComponentLookup<MiracleEffect>(true);
            var transformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);
            miracleLookup.Update(ref state);
            transformLookup.Update(ref state);

            // Collect all miracle effect entities in OnUpdate (SystemAPI.Query allowed here)
            var miracleEntities = SystemAPI.QueryBuilder()
                .WithAll<MiracleEffect>()
                .Build()
                .ToEntityArray(state.WorldUpdateAllocator);

            var job = new UpdateVillageAggregateStateJob
            {
                MiracleEffectLookup = miracleLookup,
                TransformLookup = transformLookup,
                MiracleEffectEntities = miracleEntities
            };
            job.ScheduleParallel();
        }
    }

    /// <summary>
    /// Job to update village aggregate state from sim components.
    /// </summary>
    [BurstCompile]
    public partial struct UpdateVillageAggregateStateJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<MiracleEffect> MiracleEffectLookup;
        [ReadOnly] public ComponentLookup<LocalTransform> TransformLookup;
        [ReadOnly] public NativeArray<Entity> MiracleEffectEntities;

        public void Execute(
            ref VillageCenterVisualState visualState,
            in Village village,
            in DynamicBuffer<VillageResource> resources,
            in LocalTransform transform,
            in VillageCenterPresentationTag tag)
        {
            VillageAggregateState aggregateState = VillageAggregateState.Normal;
            float intensity = 0f;

            // Check for nearby miracle effects first (highest priority)
            bool hasNearbyMiracle = false;
            float maxMiracleIntensity = 0f;

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

                if (distance <= village.InfluenceRadius)
                {
                    hasNearbyMiracle = true;
                    maxMiracleIntensity = math.max(maxMiracleIntensity, miracleEffect.Intensity);
                }
            }

            if (hasNearbyMiracle)
            {
                aggregateState = VillageAggregateState.UnderMiracle;
                intensity = maxMiracleIntensity;
            }
            else
            {
                // Determine state based on village phase and resources
                // Find food resource (assuming ResourceType.Wheat or similar is food)
                int foodAmount = 0;
                int totalResources = 0;

                for (int i = 0; i < resources.Length; i++)
                {
                    var resource = resources[i];
                    totalResources += resource.Quantity;

                    // Check if this is a food resource (ResourceType >= 40 is agricultural)
                    if (resource.ResourceTypeIndex >= 40 && resource.ResourceTypeIndex <= 44)
                    {
                        foodAmount += resource.Quantity;
                    }
                }

                // Determine aggregate state
                if (village.Phase == VillagePhase.Crisis)
                {
                    aggregateState = VillageAggregateState.Crisis;
                    intensity = 1f;
                }
                else if (foodAmount < 10 || village.Phase == VillagePhase.Declining)
                {
                    aggregateState = VillageAggregateState.Starving;
                    intensity = math.saturate(1f - (foodAmount / 10f));
                }
                else if (totalResources > 100 && village.Phase == VillagePhase.Growing || village.Phase == VillagePhase.Expanding)
                {
                    aggregateState = VillageAggregateState.Prosperous;
                    intensity = math.saturate((totalResources - 100f) / 200f);
                }
                else
                {
                    aggregateState = VillageAggregateState.Normal;
                    intensity = 0.5f;
                }
            }

            visualState.AggregateState = aggregateState;
            visualState.AggregateStateIntensity = intensity;
        }
    }
}

