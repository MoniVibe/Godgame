using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Villages
{
    /// <summary>
    /// Singleton carrying effect IDs used by presentation systems. Burst-safe because it is ECS data.
    /// </summary>
    public struct PresentationEffectIdsSingleton : IComponentData
    {
        public FixedString64Bytes VillageInfluenceEffectId;
        public FixedString64Bytes BandEffectId;
    }

    /// <summary>
    /// Updates village presentation visuals based on village state and AI decisions.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(VillageAIDecisionSystem))]
    public partial struct VillagePresentationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            PresentationEffectIdsBootstrap.EnsurePresentationEffectIdsSingleton(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            if (timeState.IsPaused)
            {
                return;
            }

            var effectIds = SystemAPI.GetSingleton<PresentationEffectIdsSingleton>();

            foreach (var (village, decision, presentation) in SystemAPI.Query<
                RefRO<Village>,
                RefRO<VillageAIDecision>,
                RefRW<VillagePresentation>>())
            {
                var villageValue = village.ValueRO;
                var decisionValue = decision.ValueRO;
                var presentationValue = presentation.ValueRW;

                var intensity = villageValue.Phase switch
                {
                    VillagePhase.Forming => 30,
                    VillagePhase.Growing => 60,
                    VillagePhase.Stable => 50,
                    VillagePhase.Expanding => 80,
                    VillagePhase.Crisis => 90,
                    VillagePhase.Declining => 20,
                    _ => 50
                };

                intensity = math.min(100, intensity + decisionValue.CurrentPriority / 2);

                presentationValue.VisualPosition = villageValue.CenterPosition;
                presentationValue.VisualRadius = villageValue.InfluenceRadius;
                presentationValue.VisualIntensity = (byte)intensity;
                presentationValue.LastVisualUpdateTick = timeState.Tick;
                presentationValue.EffectId = effectIds.VillageInfluenceEffectId;

                presentation.ValueRW = presentationValue;
            }
        }
    }

    /// <summary>
    /// Updates band presentation visuals.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct BandPresentationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            PresentationEffectIdsBootstrap.EnsurePresentationEffectIdsSingleton(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            if (timeState.IsPaused)
            {
                return;
            }

            var effectIds = SystemAPI.GetSingleton<PresentationEffectIdsSingleton>();

            foreach (var (bandPresentation, transform) in SystemAPI.Query<
                RefRW<BandPresentation>,
                RefRO<Unity.Transforms.LocalTransform>>())
            {
                var presentationValue = bandPresentation.ValueRO;
                var position = transform.ValueRO.Position;

                presentationValue.VisualPosition = position;
                presentationValue.VisualIntensity = 70;
                presentationValue.LastVisualUpdateTick = timeState.Tick;
                presentationValue.EffectId = effectIds.BandEffectId;

                bandPresentation.ValueRW = presentationValue;
            }
        }
    }

    internal static class PresentationEffectIdsBootstrap
    {
        /// <summary>
        /// Ensures the singleton carrying presentation effect IDs exists.
        /// </summary>
        internal static void EnsurePresentationEffectIdsSingleton(ref SystemState state)
        {
            var query = state.GetEntityQuery(ComponentType.ReadOnly<PresentationEffectIdsSingleton>());
            if (!query.IsEmptyIgnoreFilter)
            {
                return;
            }

            var entity = state.EntityManager.CreateEntity(typeof(PresentationEffectIdsSingleton));
            state.EntityManager.SetComponentData(entity, new PresentationEffectIdsSingleton
            {
                VillageInfluenceEffectId = new FixedString64Bytes("village.influence"),
                BandEffectId = new FixedString64Bytes("band.formation")
            });
        }
    }
}
