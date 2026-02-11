using Unity.Collections;
using Unity.Entities;

namespace Godgame.Bands
{
    /// <summary>
    /// Ensures all band entities have doctrine runtime components and default module weights.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(BeginInitializationEntityCommandBufferSystem))]
    public partial struct BandDoctrineBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Band>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var entityManager = state.EntityManager;

            using var missingProfiles = new NativeList<Entity>(Allocator.Temp);
            using var missingContexts = new NativeList<Entity>(Allocator.Temp);
            using var missingSelections = new NativeList<Entity>(Allocator.Temp);
            using var missingProjections = new NativeList<Entity>(Allocator.Temp);
            using var missingPresetStates = new NativeList<Entity>(Allocator.Temp);
            using var missingWeights = new NativeList<Entity>(Allocator.Temp);
            using var missingHierarchy = new NativeList<Entity>(Allocator.Temp);
            using var missingSocial = new NativeList<Entity>(Allocator.Temp);
            using var missingAutonomy = new NativeList<Entity>(Allocator.Temp);
            using var missingRequests = new NativeList<Entity>(Allocator.Temp);
            using var missingDecisionEvents = new NativeList<Entity>(Allocator.Temp);
            using var missingEscalationEvents = new NativeList<Entity>(Allocator.Temp);
            using var missingSocioProfiles = new NativeList<Entity>(Allocator.Temp);
            using var missingDisciplineStates = new NativeList<Entity>(Allocator.Temp);
            using var missingOrderClimate = new NativeList<Entity>(Allocator.Temp);
            using var missingResourceMorality = new NativeList<Entity>(Allocator.Temp);
            using var missingGovernancePulse = new NativeList<Entity>(Allocator.Temp);
            using var missingSplinterMeans = new NativeList<Entity>(Allocator.Temp);
            using var missingSplinterStates = new NativeList<Entity>(Allocator.Temp);
            using var missingOrderEvents = new NativeList<Entity>(Allocator.Temp);
            using var missingJusticeEvents = new NativeList<Entity>(Allocator.Temp);
            using var missingIntelReports = new NativeList<Entity>(Allocator.Temp);
            using var missingMemoryEvents = new NativeList<Entity>(Allocator.Temp);
            using var missingDisciplineEvents = new NativeList<Entity>(Allocator.Temp);
            using var missingSplinterEvents = new NativeList<Entity>(Allocator.Temp);

            foreach (var (_, entity) in SystemAPI.Query<RefRO<Band>>().WithNone<BandDoctrineProfile>().WithEntityAccess())
            {
                missingProfiles.Add(entity);
            }

            foreach (var (_, entity) in SystemAPI.Query<RefRO<Band>>().WithNone<BandDoctrineContext>().WithEntityAccess())
            {
                missingContexts.Add(entity);
            }

            foreach (var (_, entity) in SystemAPI.Query<RefRO<Band>>().WithNone<BandDoctrineSelection>().WithEntityAccess())
            {
                missingSelections.Add(entity);
            }

            foreach (var (_, entity) in SystemAPI.Query<RefRO<Band>>().WithNone<BandDoctrineProjection>().WithEntityAccess())
            {
                missingProjections.Add(entity);
            }

            foreach (var (_, entity) in SystemAPI.Query<RefRO<Band>>().WithNone<BandDoctrinePresetState>().WithEntityAccess())
            {
                missingPresetStates.Add(entity);
            }

            foreach (var (_, entity) in SystemAPI.Query<RefRO<Band>>().WithNone<BandDoctrineWeight>().WithEntityAccess())
            {
                missingWeights.Add(entity);
            }

            foreach (var (_, entity) in SystemAPI.Query<RefRO<Band>>().WithNone<BandCommandHierarchy>().WithEntityAccess())
            {
                missingHierarchy.Add(entity);
            }

            foreach (var (_, entity) in SystemAPI.Query<RefRO<Band>>().WithNone<BandSocialState>().WithEntityAccess())
            {
                missingSocial.Add(entity);
            }

            foreach (var (_, entity) in SystemAPI.Query<RefRO<Band>>().WithNone<BandCommandAutonomy>().WithEntityAccess())
            {
                missingAutonomy.Add(entity);
            }

            foreach (var (_, entity) in SystemAPI.Query<RefRO<Band>>().WithNone<BandDoctrineRequest>().WithEntityAccess())
            {
                missingRequests.Add(entity);
            }

            foreach (var (_, entity) in SystemAPI.Query<RefRO<Band>>().WithNone<BandDoctrineDecisionEvent>().WithEntityAccess())
            {
                missingDecisionEvents.Add(entity);
            }

            foreach (var (_, entity) in SystemAPI.Query<RefRO<Band>>().WithNone<BandCommandEscalationEvent>().WithEntityAccess())
            {
                missingEscalationEvents.Add(entity);
            }

            foreach (var (_, entity) in SystemAPI.Query<RefRO<Band>>().WithNone<BandSocioProfile>().WithEntityAccess())
            {
                missingSocioProfiles.Add(entity);
            }

            foreach (var (_, entity) in SystemAPI.Query<RefRO<Band>>().WithNone<BandDisciplineState>().WithEntityAccess())
            {
                missingDisciplineStates.Add(entity);
            }

            foreach (var (_, entity) in SystemAPI.Query<RefRO<Band>>().WithNone<BandOrderClimate>().WithEntityAccess())
            {
                missingOrderClimate.Add(entity);
            }

            foreach (var (_, entity) in SystemAPI.Query<RefRO<Band>>().WithNone<BandResourceMorality>().WithEntityAccess())
            {
                missingResourceMorality.Add(entity);
            }

            foreach (var (_, entity) in SystemAPI.Query<RefRO<Band>>().WithNone<BandGovernancePulse>().WithEntityAccess())
            {
                missingGovernancePulse.Add(entity);
            }

            foreach (var (_, entity) in SystemAPI.Query<RefRO<Band>>().WithNone<BandSplinterMeans>().WithEntityAccess())
            {
                missingSplinterMeans.Add(entity);
            }

            foreach (var (_, entity) in SystemAPI.Query<RefRO<Band>>().WithNone<BandSplinterIntentState>().WithEntityAccess())
            {
                missingSplinterStates.Add(entity);
            }

            foreach (var (_, entity) in SystemAPI.Query<RefRO<Band>>().WithNone<BandOrderEvent>().WithEntityAccess())
            {
                missingOrderEvents.Add(entity);
            }

            foreach (var (_, entity) in SystemAPI.Query<RefRO<Band>>().WithNone<BandJusticeEvent>().WithEntityAccess())
            {
                missingJusticeEvents.Add(entity);
            }

            foreach (var (_, entity) in SystemAPI.Query<RefRO<Band>>().WithNone<BandIntelReport>().WithEntityAccess())
            {
                missingIntelReports.Add(entity);
            }

            foreach (var (_, entity) in SystemAPI.Query<RefRO<Band>>().WithNone<BandMemoryEvent>().WithEntityAccess())
            {
                missingMemoryEvents.Add(entity);
            }

            foreach (var (_, entity) in SystemAPI.Query<RefRO<Band>>().WithNone<BandDisciplineEvent>().WithEntityAccess())
            {
                missingDisciplineEvents.Add(entity);
            }

            foreach (var (_, entity) in SystemAPI.Query<RefRO<Band>>().WithNone<BandSplinterIntentEvent>().WithEntityAccess())
            {
                missingSplinterEvents.Add(entity);
            }

            for (var i = 0; i < missingProfiles.Length; i++)
            {
                entityManager.AddComponentData(missingProfiles[i], BandDoctrineProfile.Default);
            }

            for (var i = 0; i < missingContexts.Length; i++)
            {
                entityManager.AddComponentData(missingContexts[i], BandDoctrineContext.Default);
            }

            for (var i = 0; i < missingSelections.Length; i++)
            {
                entityManager.AddComponentData(missingSelections[i], default(BandDoctrineSelection));
            }

            for (var i = 0; i < missingProjections.Length; i++)
            {
                entityManager.AddComponentData(missingProjections[i], default(BandDoctrineProjection));
            }

            for (var i = 0; i < missingPresetStates.Length; i++)
            {
                entityManager.AddComponentData(missingPresetStates[i], BandDoctrinePresetState.Default);
            }

            for (var i = 0; i < missingWeights.Length; i++)
            {
                var weights = entityManager.AddBuffer<BandDoctrineWeight>(missingWeights[i]);
                EnsureModuleCoverage(weights);
            }

            for (var i = 0; i < missingHierarchy.Length; i++)
            {
                entityManager.AddComponentData(missingHierarchy[i], BandCommandHierarchy.Default);
            }

            for (var i = 0; i < missingSocial.Length; i++)
            {
                entityManager.AddComponentData(missingSocial[i], BandSocialState.Default);
            }

            for (var i = 0; i < missingAutonomy.Length; i++)
            {
                entityManager.AddComponentData(missingAutonomy[i], BandCommandAutonomy.Default);
            }

            for (var i = 0; i < missingRequests.Length; i++)
            {
                entityManager.AddBuffer<BandDoctrineRequest>(missingRequests[i]);
            }

            for (var i = 0; i < missingDecisionEvents.Length; i++)
            {
                entityManager.AddBuffer<BandDoctrineDecisionEvent>(missingDecisionEvents[i]);
            }

            for (var i = 0; i < missingEscalationEvents.Length; i++)
            {
                entityManager.AddBuffer<BandCommandEscalationEvent>(missingEscalationEvents[i]);
            }

            for (var i = 0; i < missingSocioProfiles.Length; i++)
            {
                entityManager.AddComponentData(missingSocioProfiles[i], BandSocioProfile.Default);
            }

            for (var i = 0; i < missingDisciplineStates.Length; i++)
            {
                entityManager.AddComponentData(missingDisciplineStates[i], BandDisciplineState.Default);
            }

            for (var i = 0; i < missingOrderClimate.Length; i++)
            {
                entityManager.AddComponentData(missingOrderClimate[i], BandOrderClimate.Default);
            }

            for (var i = 0; i < missingResourceMorality.Length; i++)
            {
                entityManager.AddComponentData(missingResourceMorality[i], BandResourceMorality.Default);
            }

            for (var i = 0; i < missingGovernancePulse.Length; i++)
            {
                entityManager.AddComponentData(missingGovernancePulse[i], BandGovernancePulse.Default);
            }

            for (var i = 0; i < missingSplinterMeans.Length; i++)
            {
                entityManager.AddComponentData(missingSplinterMeans[i], BandSplinterMeans.Default);
            }

            for (var i = 0; i < missingSplinterStates.Length; i++)
            {
                entityManager.AddComponentData(missingSplinterStates[i], BandSplinterIntentState.Default);
            }

            for (var i = 0; i < missingOrderEvents.Length; i++)
            {
                entityManager.AddBuffer<BandOrderEvent>(missingOrderEvents[i]);
            }

            for (var i = 0; i < missingJusticeEvents.Length; i++)
            {
                entityManager.AddBuffer<BandJusticeEvent>(missingJusticeEvents[i]);
            }

            for (var i = 0; i < missingIntelReports.Length; i++)
            {
                entityManager.AddBuffer<BandIntelReport>(missingIntelReports[i]);
            }

            for (var i = 0; i < missingMemoryEvents.Length; i++)
            {
                entityManager.AddBuffer<BandMemoryEvent>(missingMemoryEvents[i]);
            }

            for (var i = 0; i < missingDisciplineEvents.Length; i++)
            {
                entityManager.AddBuffer<BandDisciplineEvent>(missingDisciplineEvents[i]);
            }

            for (var i = 0; i < missingSplinterEvents.Length; i++)
            {
                entityManager.AddBuffer<BandSplinterIntentEvent>(missingSplinterEvents[i]);
            }
        }

        internal static void EnsureModuleCoverage(DynamicBuffer<BandDoctrineWeight> weights)
        {
            EnsureWeight(weights, BandDoctrineModuleType.Rotation);
            EnsureWeight(weights, BandDoctrineModuleType.ElasticFront);
            EnsureWeight(weights, BandDoctrineModuleType.SplitShell);
            EnsureWeight(weights, BandDoctrineModuleType.SortieWindow);
            EnsureWeight(weights, BandDoctrineModuleType.ReservePulse);
            EnsureWeight(weights, BandDoctrineModuleType.SacrificialScreen);
        }

        private static void EnsureWeight(DynamicBuffer<BandDoctrineWeight> weights, BandDoctrineModuleType module)
        {
            for (var i = 0; i < weights.Length; i++)
            {
                if (weights[i].Module == module)
                {
                    return;
                }
            }

            weights.Add(BandDoctrineWeight.CreateDefault(module));
        }
    }
}
