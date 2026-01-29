using PureDOTS.Runtime;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Economy.Production;
using PureDOTS.Runtime.Economy.Resources;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Scenario
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct GodgameScenarioActionSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
            state.RequireForUpdate<GodgameScenarioRuntime>();
            state.RequireForUpdate<GodgameScenarioAction>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var time = SystemAPI.GetSingleton<TimeState>();
            if (time.IsPaused)
            {
                return;
            }

            var rewind = SystemAPI.GetSingleton<RewindState>();
            if (rewind.Mode != RewindMode.Record)
            {
                return;
            }

            var tick = time.Tick;

            foreach (var actions in SystemAPI.Query<DynamicBuffer<GodgameScenarioAction>>())
            {
                var actionBuffer = actions;
                for (int i = 0; i < actionBuffer.Length; i++)
                {
                    var action = actionBuffer[i];
                    if (action.Executed != 0 || action.ExecuteTick > tick)
                    {
                        continue;
                    }

                    switch (action.Kind)
                    {
                        case GodgameScenarioActionKind.EconomyEnable:
                            ProcessEconomyEnable(ref state);
                            break;
                        case GodgameScenarioActionKind.ProdCreateBusiness:
                            ProcessProdCreateBusiness(ref state, action, tick);
                            break;
                        case GodgameScenarioActionKind.ProdAddItem:
                            ProcessProdAddItem(ref state, action, tick);
                            break;
                        case GodgameScenarioActionKind.ProdRequest:
                            ProcessProdRequest(ref state, action);
                            break;
                    }

                    action.Executed = 1;
                    actionBuffer[i] = action;
                }
            }
        }

        private void ProcessEconomyEnable(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingletonEntity<ScenarioState>(out var scenarioEntity))
            {
                return;
            }

            var scenario = state.EntityManager.GetComponentData<ScenarioState>(scenarioEntity);
            if (!scenario.EnableEconomy)
            {
                scenario.EnableEconomy = true;
                state.EntityManager.SetComponentData(scenarioEntity, scenario);
                Debug.Log("[GodgameScenario] Economy enabled for production loop v0.");
            }
        }

        private void ProcessProdCreateBusiness(ref SystemState state, in GodgameScenarioAction action, uint tick)
        {
            if (action.BusinessId.IsEmpty)
            {
                return;
            }

            EnsureEconomyEnabled(ref state);

            if (TryResolveBusiness(ref state, action.BusinessId, out _))
            {
                return;
            }

            var capacity = action.Capacity > 0f ? action.Capacity : 1000f;

            var business = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentData(business, new GodgameScenarioBusinessId
            {
                Value = action.BusinessId
            });
            state.EntityManager.AddComponentData(business, new BusinessProduction
            {
                Type = (BusinessType)action.BusinessType,
                Capacity = capacity,
                Throughput = 0f,
                LastUpdateTick = tick
            });

            var inventoryEntity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentData(inventoryEntity, new Inventory
            {
                MaxMass = capacity,
                MaxVolume = 0f,
                CurrentMass = 0f,
                CurrentVolume = 0f,
                LastUpdateTick = tick
            });
            state.EntityManager.AddBuffer<InventoryItem>(inventoryEntity);

            state.EntityManager.AddComponentData(business, new BusinessInventory
            {
                InventoryEntity = inventoryEntity
            });
            state.EntityManager.AddBuffer<ProductionJob>(business);

            Debug.Log($"[GodgameScenario] Created production business id={action.BusinessId} type={(BusinessType)action.BusinessType} capacity={capacity}.");
        }

        private void ProcessProdAddItem(ref SystemState state, in GodgameScenarioAction action, uint tick)
        {
            if (action.BusinessId.IsEmpty || action.ItemId.IsEmpty || action.Quantity <= 0f)
            {
                return;
            }

            EnsureEconomyEnabled(ref state);

            if (!TryResolveBusiness(ref state, action.BusinessId, out var business))
            {
                return;
            }

            if (!state.EntityManager.HasComponent<BusinessInventory>(business))
            {
                return;
            }

            var inventoryEntity = state.EntityManager.GetComponentData<BusinessInventory>(business).InventoryEntity;
            if (inventoryEntity == Entity.Null || !state.EntityManager.HasBuffer<InventoryItem>(inventoryEntity))
            {
                return;
            }

            var items = state.EntityManager.GetBuffer<InventoryItem>(inventoryEntity);
            for (int i = 0; i < items.Length; i++)
            {
                if (!items[i].ItemId.Equals(action.ItemId))
                {
                    continue;
                }

                var item = items[i];
                item.Quantity += action.Quantity;
                items[i] = item;
                Debug.Log($"[GodgameScenario] Added item id={action.ItemId} qty={action.Quantity} to business={action.BusinessId}.");
                return;
            }

            items.Add(new InventoryItem
            {
                ItemId = action.ItemId,
                Quantity = action.Quantity,
                Quality = 1f,
                Durability = 1f,
                CreatedTick = tick
            });
            Debug.Log($"[GodgameScenario] Added item id={action.ItemId} qty={action.Quantity} to business={action.BusinessId}.");
        }

        private void ProcessProdRequest(ref SystemState state, in GodgameScenarioAction action)
        {
            if (action.BusinessId.IsEmpty || action.RecipeId.IsEmpty)
            {
                return;
            }

            EnsureEconomyEnabled(ref state);

            if (!TryResolveBusiness(ref state, action.BusinessId, out var business))
            {
                return;
            }

            var request = new ProductionJobRequest
            {
                RecipeId = action.RecipeId,
                Worker = Entity.Null
            };

            if (state.EntityManager.HasComponent<ProductionJobRequest>(business))
            {
                state.EntityManager.SetComponentData(business, request);
            }
            else
            {
                state.EntityManager.AddComponentData(business, request);
            }

            Debug.Log($"[GodgameScenario] Requested production recipe id={action.RecipeId} for business={action.BusinessId}.");
        }

        private void EnsureEconomyEnabled(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingletonEntity<ScenarioState>(out var scenarioEntity))
            {
                return;
            }

            var scenario = state.EntityManager.GetComponentData<ScenarioState>(scenarioEntity);
            if (!scenario.EnableEconomy)
            {
                scenario.EnableEconomy = true;
                state.EntityManager.SetComponentData(scenarioEntity, scenario);
            }
        }

        private bool TryResolveBusiness(ref SystemState state, FixedString64Bytes businessId, out Entity entity)
        {
            foreach (var (id, businessEntity) in SystemAPI.Query<RefRO<GodgameScenarioBusinessId>>().WithEntityAccess())
            {
                if (id.ValueRO.Value.Equals(businessId))
                {
                    entity = businessEntity;
                    return true;
                }
            }

            entity = Entity.Null;
            return false;
        }
    }
}
