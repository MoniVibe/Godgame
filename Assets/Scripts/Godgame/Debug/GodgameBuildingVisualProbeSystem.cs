#if UNITY_EDITOR || DEVELOPMENT_BUILD
using Godgame.Rendering;
using Godgame.Scenario;
using PureDOTS.Runtime.Core;
using PureDOTS.Rendering;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace Godgame.DebugSystems
{
    /// <summary>
    /// Logs building render key/presenter state once after the settlement scenario spawns.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(Godgame.Scenario.SettlementSpawnSystem))]
    [UpdateAfter(typeof(Godgame.Scenario.GodgameScenarioSpawnSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.Default)]
    public partial struct GodgameBuildingVisualProbeSystem : ISystem
    {
        private EntityQuery _semanticQuery;
        private EntityQuery _settlementQuery;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameWorldTag>();
            _semanticQuery = state.GetEntityQuery(new EntityQueryDesc
            {
                All = new[] { ComponentType.ReadOnly<RenderSemanticKey>() },
                None = new[] { ComponentType.ReadOnly<Prefab>() }
            });
            _settlementQuery = state.GetEntityQuery(ComponentType.ReadOnly<SettlementRuntime>());
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            if (_settlementQuery.IsEmpty)
            {
                return;
            }

            var hasSpawned = false;
            using (var runtimes = _settlementQuery.ToComponentDataArray<SettlementRuntime>(Allocator.Temp))
            {
                for (var i = 0; i < runtimes.Length; i++)
                {
                    if (runtimes[i].HasSpawned != 0)
                    {
                        hasSpawned = true;
                        break;
                    }
                }
            }

            if (!hasSpawned)
            {
                return;
            }

            using var entities = _semanticQuery.ToEntityArray(Allocator.Temp);
            using var semantics = _semanticQuery.ToComponentDataArray<RenderSemanticKey>(Allocator.Temp);

            var villageCenterKey = GodgameSemanticKeys.VillageCenter;
            var storehouseKey = GodgameSemanticKeys.Storehouse;
            var housingKey = GodgameSemanticKeys.Housing;
            var worshipKey = GodgameSemanticKeys.Worship;

            var villageCenterCount = 0;
            var storehouseCount = 0;
            var housingCount = 0;
            var worshipCount = 0;

            var villageCenterEntity = Entity.Null;
            var storehouseEntity = Entity.Null;
            var housingEntity = Entity.Null;
            var worshipEntity = Entity.Null;

            for (var i = 0; i < entities.Length; i++)
            {
                var key = semantics[i].Value;
                if (key == villageCenterKey)
                {
                    villageCenterCount++;
                    if (villageCenterEntity == Entity.Null)
                    {
                        villageCenterEntity = entities[i];
                    }
                }
                else if (key == storehouseKey)
                {
                    storehouseCount++;
                    if (storehouseEntity == Entity.Null)
                    {
                        storehouseEntity = entities[i];
                    }
                }
                else if (key == housingKey)
                {
                    housingCount++;
                    if (housingEntity == Entity.Null)
                    {
                        housingEntity = entities[i];
                    }
                }
                else if (key == worshipKey)
                {
                    worshipCount++;
                    if (worshipEntity == Entity.Null)
                    {
                        worshipEntity = entities[i];
                    }
                }
            }

            Debug.Log("[GodgameBuildingProbe] Counts " +
                      "VillageCenter=" + villageCenterCount +
                      " Storehouse=" + storehouseCount +
                      " Housing=" + housingCount +
                      " Worship=" + worshipCount);

            LogSample(ref state, "VillageCenter", villageCenterKey, villageCenterEntity, villageCenterCount);
            LogSample(ref state, "Storehouse", storehouseKey, storehouseEntity, storehouseCount);
            LogSample(ref state, "Housing", housingKey, housingEntity, housingCount);
            LogSample(ref state, "Worship", worshipKey, worshipEntity, worshipCount);

            state.Enabled = false;
        }

        private static void LogSample(ref SystemState state, string label, ushort key, Entity entity, int count)
        {
            if (count == 0 || entity == Entity.Null)
            {
                Debug.Log("[GodgameBuildingProbe] " + label + " key=" + key + " count=0");
                return;
            }

            var entityManager = state.EntityManager;
            var semanticValue = entityManager.GetComponentData<RenderSemanticKey>(entity).Value;
            var hasVariant = entityManager.HasComponent<RenderVariantKey>(entity);
            var variantValue = hasVariant ? entityManager.GetComponentData<RenderVariantKey>(entity).Value : -1;
            var hasMeshPresenter = entityManager.HasComponent<MeshPresenter>(entity);
            var meshPresenterEnabled = hasMeshPresenter && entityManager.IsComponentEnabled<MeshPresenter>(entity);
            var meshDefIndex = hasMeshPresenter ? entityManager.GetComponentData<MeshPresenter>(entity).DefIndex : (ushort)0;
            var hasMaterialMesh = entityManager.HasComponent<MaterialMeshInfo>(entity);
            var hasLocalTransform = entityManager.HasComponent<LocalTransform>(entity);
            var scale = hasLocalTransform ? entityManager.GetComponentData<LocalTransform>(entity).Scale : 0f;

            Debug.Log("[GodgameBuildingProbe] " + label +
                      " key=" + key +
                      " count=" + count +
                      " entity=" + entity +
                      " semantic=" + semanticValue +
                      " variant=" + variantValue +
                      " meshPresenter=" + hasMeshPresenter +
                      " meshPresenterEnabled=" + meshPresenterEnabled +
                      " meshDefIndex=" + meshDefIndex +
                      " materialMeshInfo=" + hasMaterialMesh +
                      " scale=" + scale);
        }
    }

    /// <summary>
    /// Logs building and villager render state after variant resolution has applied.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(PureDOTS.Rendering.ApplyRenderVariantSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.Default)]
    public partial struct GodgameBuildingVisualPostProbeSystem : ISystem
    {
        private EntityQuery _semanticQuery;
        private EntityQuery _settlementQuery;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameWorldTag>();
            _semanticQuery = state.GetEntityQuery(new EntityQueryDesc
            {
                All = new[] { ComponentType.ReadOnly<RenderSemanticKey>() },
                None = new[] { ComponentType.ReadOnly<Prefab>() }
            });
            _settlementQuery = state.GetEntityQuery(ComponentType.ReadOnly<SettlementRuntime>());
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            if (_settlementQuery.IsEmpty)
            {
                return;
            }

            var hasSpawned = false;
            using (var runtimes = _settlementQuery.ToComponentDataArray<SettlementRuntime>(Allocator.Temp))
            {
                for (var i = 0; i < runtimes.Length; i++)
                {
                    if (runtimes[i].HasSpawned != 0)
                    {
                        hasSpawned = true;
                        break;
                    }
                }
            }

            if (!hasSpawned)
            {
                return;
            }

            using var entities = _semanticQuery.ToEntityArray(Allocator.Temp);
            using var semantics = _semanticQuery.ToComponentDataArray<RenderSemanticKey>(Allocator.Temp);

            var villageCenterKey = GodgameSemanticKeys.VillageCenter;
            var storehouseKey = GodgameSemanticKeys.Storehouse;
            var housingKey = GodgameSemanticKeys.Housing;
            var worshipKey = GodgameSemanticKeys.Worship;

            var villageCenterCount = 0;
            var storehouseCount = 0;
            var housingCount = 0;
            var worshipCount = 0;
            var villagerCount = 0;

            var villageCenterEntity = Entity.Null;
            var storehouseEntity = Entity.Null;
            var housingEntity = Entity.Null;
            var worshipEntity = Entity.Null;
            var villagerEntity = Entity.Null;
            ushort villagerKeySample = 0;

            for (var i = 0; i < entities.Length; i++)
            {
                var key = semantics[i].Value;
                if (key == villageCenterKey)
                {
                    villageCenterCount++;
                    if (villageCenterEntity == Entity.Null)
                    {
                        villageCenterEntity = entities[i];
                    }
                }
                else if (key == storehouseKey)
                {
                    storehouseCount++;
                    if (storehouseEntity == Entity.Null)
                    {
                        storehouseEntity = entities[i];
                    }
                }
                else if (key == housingKey)
                {
                    housingCount++;
                    if (housingEntity == Entity.Null)
                    {
                        housingEntity = entities[i];
                    }
                }
                else if (key == worshipKey)
                {
                    worshipCount++;
                    if (worshipEntity == Entity.Null)
                    {
                        worshipEntity = entities[i];
                    }
                }
                else if (key >= GodgameSemanticKeys.VillagerMiner && key <= GodgameSemanticKeys.VillagerCombatant)
                {
                    villagerCount++;
                    if (villagerEntity == Entity.Null)
                    {
                        villagerEntity = entities[i];
                        villagerKeySample = key;
                    }
                }
            }

            Debug.Log("[GodgameBuildingPostProbe] Counts " +
                      "VillageCenter=" + villageCenterCount +
                      " Storehouse=" + storehouseCount +
                      " Housing=" + housingCount +
                      " Worship=" + worshipCount +
                      " Villager=" + villagerCount);

            LogSample(ref state, "VillageCenter", villageCenterKey, villageCenterEntity, villageCenterCount);
            LogSample(ref state, "Storehouse", storehouseKey, storehouseEntity, storehouseCount);
            LogSample(ref state, "Housing", housingKey, housingEntity, housingCount);
            LogSample(ref state, "Worship", worshipKey, worshipEntity, worshipCount);
            LogSample(ref state, "Villager", villagerKeySample, villagerEntity, villagerCount);

            state.Enabled = false;
        }

        private static void LogSample(ref SystemState state, string label, ushort key, Entity entity, int count)
        {
            if (count == 0 || entity == Entity.Null)
            {
                Debug.Log("[GodgameBuildingPostProbe] " + label + " key=" + key + " count=0");
                return;
            }

            var entityManager = state.EntityManager;
            var semanticValue = entityManager.GetComponentData<RenderSemanticKey>(entity).Value;
            var hasVariant = entityManager.HasComponent<RenderVariantKey>(entity);
            var variantValue = hasVariant ? entityManager.GetComponentData<RenderVariantKey>(entity).Value : -1;
            var hasMeshPresenter = entityManager.HasComponent<MeshPresenter>(entity);
            var meshPresenterEnabled = hasMeshPresenter && entityManager.IsComponentEnabled<MeshPresenter>(entity);
            var meshDefIndex = hasMeshPresenter ? entityManager.GetComponentData<MeshPresenter>(entity).DefIndex : (ushort)0;
            var hasMaterialMesh = entityManager.HasComponent<MaterialMeshInfo>(entity);
            var material = "n/a";
            var mesh = "n/a";
            var subMesh = -1;
            var hasMaterialMeshIndexRange = false;
            var materialMeshIndexRange = "n/a";
            var materialIndex = "n/a";
            var meshIndex = "n/a";
            if (hasMaterialMesh)
            {
                var mmi = entityManager.GetComponentData<MaterialMeshInfo>(entity);
                material = mmi.Material.ToString();
                mesh = mmi.Mesh.ToString();
                subMesh = mmi.SubMesh;
                hasMaterialMeshIndexRange = mmi.HasMaterialMeshIndexRange;
                if (hasMaterialMeshIndexRange)
                {
                    materialMeshIndexRange = mmi.MaterialMeshIndexRange.ToString();
                }
                if (mmi.Material < 0)
                {
                    materialIndex = MaterialMeshInfo.StaticIndexToArrayIndex(mmi.Material).ToString();
                }
                if (mmi.Mesh < 0)
                {
                    meshIndex = MaterialMeshInfo.StaticIndexToArrayIndex(mmi.Mesh).ToString();
                }
            }
            var hasLocalTransform = entityManager.HasComponent<LocalTransform>(entity);
            var scale = hasLocalTransform ? entityManager.GetComponentData<LocalTransform>(entity).Scale : 0f;

            Debug.Log("[GodgameBuildingPostProbe] " + label +
                      " key=" + key +
                      " count=" + count +
                      " entity=" + entity +
                      " semantic=" + semanticValue +
                      " variant=" + variantValue +
                      " meshPresenter=" + hasMeshPresenter +
                      " meshPresenterEnabled=" + meshPresenterEnabled +
                      " meshDefIndex=" + meshDefIndex +
                      " materialMeshInfo=" + hasMaterialMesh +
                      " material=" + material +
                      " mesh=" + mesh +
                      " subMesh=" + subMesh +
                      " hasMaterialMeshIndexRange=" + hasMaterialMeshIndexRange +
                      " materialMeshIndexRange=" + materialMeshIndexRange +
                      " materialIndex=" + materialIndex +
                      " meshIndex=" + meshIndex +
                      " scale=" + scale);
        }
    }
}
#endif
