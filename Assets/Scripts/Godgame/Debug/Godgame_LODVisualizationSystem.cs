using Unity.Burst;
using Unity.Entities;
using Godgame.Presentation;

namespace Godgame.Debugging
{
    [BurstCompile]
    public partial struct ApplyVillagerLODTintJob : IJobEntity
    {
        void Execute(ref VillagerVisualState visualState, in PresentationLODState lodState)
        {
            // Update visual state based on LOD
            // Adjust effect intensity based on distance
            visualState.EffectIntensity = lodState.CurrentLOD switch
            {
                PresentationLOD.LOD0_Full => 1.0f,
                PresentationLOD.LOD1_Mid => 0.7f,
                PresentationLOD.LOD2_Far => 0.4f,
                _ => 0.0f  // Culled
            };
        }
    }

    [BurstCompile]
    public partial struct ApplyChunkLODTintJob : IJobEntity
    {
        void Execute(ref ResourceChunkVisualState visualState, in PresentationLODState lodState)
        {
            // Adjust quantity scale based on LOD
            if (lodState.CurrentLOD > PresentationLOD.LOD0_Full)
            {
                visualState.QuantityScale *= 0.8f; // Reduce scale for lower LODs
            }
        }
    }

    [BurstCompile]
    public partial struct ApplyVillageCenterLODTintJob : IJobEntity
    {
        void Execute(ref VillageCenterVisualState visualState, in PresentationLODState lodState)
        {
            // Adjust intensity based on LOD
            visualState.Intensity = lodState.CurrentLOD switch
            {
                PresentationLOD.LOD0_Full => 1.0f,
                PresentationLOD.LOD1_Mid => 0.8f,
                PresentationLOD.LOD2_Far => 0.6f,
                _ => 0.0f  // Culled
            };
        }
    }

    [UpdateInGroup(typeof(Unity.Entities.PresentationSystemGroup))]
    public partial struct Godgame_LODVisualizationSystem : ISystem
    {
        public void OnCreate(ref SystemState state) {}
        [BurstCompile] public void OnDestroy(ref SystemState state) {}

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new ApplyVillagerLODTintJob().ScheduleParallel();
            new ApplyChunkLODTintJob().ScheduleParallel();
            new ApplyVillageCenterLODTintJob().ScheduleParallel();
        }
    }
}

