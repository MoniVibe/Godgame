using Godgame.Economy;
using Godgame.Presentation;
using Godgame.Villages;
using Godgame.Villagers;
using PureDOTS.Runtime;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Godgame.Scenario
{
    /// <summary>
    /// Lightweight smoke validation that inspects spawned gameplay entities once per scene load.
    /// Editor-only checks (not Burst-compiled) that run after the scenario bootstrap.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(Godgame_ScenarioBootstrapSystem))]
    public partial struct Godgame_SmokeValidationSystem : ISystem
    {
        private bool _validated;

        public void OnCreate(ref SystemState state)
        {
            _validated = false;
            state.RequireForUpdate<ScenarioSceneTag>();
            state.RequireForUpdate<ScenarioState>();
            state.RequireForUpdate<SettlementConfig>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (_validated)
            {
                state.Enabled = false;
                return;
            }

            if (!SystemAPI.TryGetSingleton<ScenarioState>(out var scenario) ||
                !scenario.EnableGodgame ||
                !scenario.IsInitialized)
            {
                return;
            }

            if (!SystemAPI.HasSingleton<SettlementConfig>())
            {
                return;
            }

            _validated = true;

#if UNITY_EDITOR
            ValidateSmokeEntities(ref state);
#endif

            state.Enabled = false;
        }

#if UNITY_EDITOR
        private void ValidateSmokeEntities(ref SystemState state)
        {
            var em = state.EntityManager;
            int villagerCount = 0;
            int villageCount = 0;
            int nodeCount = 0;
            int chunkCount = 0;
            int villagerErrors = 0;
            int villageErrors = 0;
            int chunkErrors = 0;

            // Validate villagers
            foreach (var (_, entity) in SystemAPI.Query<RefRO<VillagerPresentationTag>>().WithEntityAccess())
            {
                villagerCount++;
                bool hasTransform = em.HasComponent<LocalTransform>(entity);
                bool hasBehavior = em.HasComponent<VillagerBehavior>(entity);
                bool hasVisualState = em.HasComponent<VillagerVisualState>(entity);
                bool hasLODState = em.HasComponent<PresentationLODState>(entity);

                if (!hasTransform || !hasBehavior || !hasVisualState || !hasLODState)
                {
                    villagerErrors++;
                    UnityEngine.Debug.LogWarning($"[SmokeValidation] Villager {entity.Index} missing components: Transform={hasTransform}, Behavior={hasBehavior}, VisualState={hasVisualState}, LODState={hasLODState}");
                }
            }

            // Validate villages
            foreach (var (_, entity) in SystemAPI.Query<RefRO<VillageCenterPresentationTag>>().WithEntityAccess())
            {
                villageCount++;
                bool hasVillage = em.HasComponent<Village>(entity);
                bool hasMemberBuffer = em.HasBuffer<VillageMember>(entity);
                bool hasVisualState = em.HasComponent<VillageCenterVisualState>(entity);

                if (!hasVillage || !hasVisualState)
                {
                    villageErrors++;
                    UnityEngine.Debug.LogWarning($"[SmokeValidation] Village {entity.Index} missing components: Village={hasVillage}, VisualState={hasVisualState}");
                }

                if (hasMemberBuffer)
                {
                    var members = em.GetBuffer<VillageMember>(entity);
                    if (members.Length == 0)
                    {
                        villageErrors++;
                        UnityEngine.Debug.LogWarning($"[SmokeValidation] Village {entity.Index} has no members in VillageMember buffer");
                    }
                }
                else
                {
                    villageErrors++;
                    UnityEngine.Debug.LogWarning($"[SmokeValidation] Village {entity.Index} missing VillageMember buffer");
                }
            }

            // Validate resource nodes
            foreach (var (_, entity) in SystemAPI.Query<RefRO<ResourceNodePresentationTag>>().WithEntityAccess())
            {
                nodeCount++;
                bool hasTransform = em.HasComponent<LocalTransform>(entity);
                bool hasResource = em.HasComponent<ExtractedResource>(entity);
                bool hasLODState = em.HasComponent<PresentationLODState>(entity);

                if (!hasTransform || !hasResource || !hasLODState)
                {
                    UnityEngine.Debug.LogWarning($"[SmokeValidation] ResourceNode {entity.Index} missing components: Transform={hasTransform}, Resource={hasResource}, LODState={hasLODState}");
                }
            }

            // Validate resource chunks
            foreach (var (_, entity) in SystemAPI.Query<RefRO<ResourceChunkPresentationTag>>().WithEntityAccess())
            {
                chunkCount++;
                bool hasTransform = em.HasComponent<LocalTransform>(entity);
                bool hasResource = em.HasComponent<ExtractedResource>(entity);
                bool hasVisualState = em.HasComponent<ResourceChunkVisualState>(entity);
                bool hasLODState = em.HasComponent<PresentationLODState>(entity);

                if (!hasTransform || !hasResource || !hasVisualState || !hasLODState)
                {
                    chunkErrors++;
                    UnityEngine.Debug.LogWarning($"[SmokeValidation] ResourceChunk {entity.Index} missing components: Transform={hasTransform}, Resource={hasResource}, VisualState={hasVisualState}, LODState={hasLODState}");
                }
            }

            // Log summary
            var totalCount = villagerCount + villageCount + nodeCount + chunkCount;
            if (totalCount == 0)
            {
                UnityEngine.Debug.LogWarning("[SmokeValidation] No smoke-tagged entities were found during validation. Verify ScenarioBootstrap and associated subscenes are enabled.");
                return;
            }

            if (villagerErrors == 0 && villageErrors == 0 && chunkErrors == 0)
            {
                UnityEngine.Debug.Log($"[SmokeValidation] {villageCount} villages, {villagerCount} villagers, {nodeCount} nodes, {chunkCount} chunks instantiated. All entities validated successfully.");
            }
            else
            {
                UnityEngine.Debug.LogWarning($"[SmokeValidation] {villageCount} villages ({villageErrors} issues), {villagerCount} villagers ({villagerErrors} issues), {nodeCount} nodes, {chunkCount} chunks ({chunkErrors} issues) instantiated. Some entities have validation errors.");
            }
        }
#endif
    }
}
