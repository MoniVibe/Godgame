using Godgame.Economy;
using Godgame.Presentation;
using Godgame.Villages;
using Godgame.Villagers;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Godgame.Demo
{
    /// <summary>
    /// Validation system for demo entities.
    /// Checks entity component validity and logs counts at startup.
    /// Editor-only checks (not Burst-compiled).
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(Godgame_ScenarioBootstrapSystem))]
    [UpdateAfter(typeof(Godgame_Demo01_BootstrapSystem))]
    public partial struct Godgame_DemoValidationSystem : ISystem
    {
        private bool _validated;
        private bool _loggedEmptyDemo;
        private EntityQuery _demoConfigQuery;

        public void OnCreate(ref SystemState state)
        {
            _validated = false;
            _loggedEmptyDemo = false;
            _demoConfigQuery = state.GetEntityQuery(ComponentType.ReadOnly<DemoConfigBlobReference>());
        }

        public void OnUpdate(ref SystemState state)
        {
            if (_validated)
            {
                state.Enabled = false;
                return;
            }

            _validated = true;

#if UNITY_EDITOR
            ValidateDemoEntities(ref state);
#endif

            state.Enabled = false;
        }

#if UNITY_EDITOR
        private void ValidateDemoEntities(ref SystemState state)
        {
            var em = state.EntityManager;
            int villagerCount = 0;
            int villageCount = 0;
            int nodeCount = 0;
            int chunkCount = 0;
            int villagerErrors = 0;
            int villageErrors = 0;
            int chunkErrors = 0;
            var scenarioMode = GetScenarioMode(ref state);
            var modeLabel = scenarioMode.ToString();

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
                    UnityEngine.Debug.LogWarning($"[DemoValidation] Villager {entity.Index} missing components: Transform={hasTransform}, Behavior={hasBehavior}, VisualState={hasVisualState}, LODState={hasLODState}");
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
                    UnityEngine.Debug.LogWarning($"[DemoValidation] Village {entity.Index} missing components: Village={hasVillage}, VisualState={hasVisualState}");
                }

                if (hasMemberBuffer)
                {
                    var members = em.GetBuffer<VillageMember>(entity);
                    if (members.Length == 0)
                    {
                        villageErrors++;
                        UnityEngine.Debug.LogWarning($"[DemoValidation] Village {entity.Index} has no members in VillageMember buffer");
                    }
                }
                else
                {
                    villageErrors++;
                    UnityEngine.Debug.LogWarning($"[DemoValidation] Village {entity.Index} missing VillageMember buffer");
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
                    UnityEngine.Debug.LogWarning($"[DemoValidation] ResourceNode {entity.Index} missing components: Transform={hasTransform}, Resource={hasResource}, LODState={hasLODState}");
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
                    UnityEngine.Debug.LogWarning($"[DemoValidation] ResourceChunk {entity.Index} missing components: Transform={hasTransform}, Resource={hasResource}, VisualState={hasVisualState}, LODState={hasLODState}");
                }
            }

            // Log summary
            var totalCount = villagerCount + villageCount + nodeCount + chunkCount;
            if (scenarioMode == DemoScenarioMode.Demo01 && totalCount == 0)
            {
                if (!_loggedEmptyDemo)
                {
                    UnityEngine.Debug.LogError("[DemoValidation] Demo_01: No demo entities were spawned. Ensure the demo subscene is loaded, the ScenarioPreset.Mode is set to Demo_01, and spawn gates are enabled.");
                    _loggedEmptyDemo = true;
                }
                return;
            }

            if (totalCount == 0)
            {
                UnityEngine.Debug.LogWarning($"[DemoValidation] {modeLabel}: No demo-tagged entities were found during validation.");
                return;
            }

            if (villagerErrors == 0 && villageErrors == 0 && chunkErrors == 0)
            {
                UnityEngine.Debug.Log($"[DemoValidation] {modeLabel}: {villageCount} villages, {villagerCount} villagers, {nodeCount} nodes, {chunkCount} chunks instantiated. All entities validated successfully.");
            }
            else
            {
                UnityEngine.Debug.LogWarning($"[DemoValidation] {modeLabel}: {villageCount} villages ({villageErrors} errors), {villagerCount} villagers ({villagerErrors} errors), {nodeCount} nodes, {chunkCount} chunks ({chunkErrors} errors) instantiated. Some entities have validation errors.");
            }
        }

        private DemoScenarioMode GetScenarioMode(ref SystemState state)
        {
            if (!_demoConfigQuery.IsEmptyIgnoreFilter)
            {
                var blobRef = _demoConfigQuery.GetSingleton<DemoConfigBlobReference>();
                if (blobRef.Config.IsCreated)
                {
                    return blobRef.Config.Value.Mode;
                }
            }

            return DemoScenarioMode.Demo01;
        }
#endif
    }
}
