using System;
using Godgame.Scenario;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Core;
using PureDOTS.Runtime.Time;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using SystemEnv = System.Environment;

namespace Godgame.Headless
{
    /// <summary>
    /// Headless-only lightweight gather/deliver loop for settlement villagers so proofs can validate movement and storage.
    /// Runs only when the villager proof is enabled.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(GodgameHeadlessVillagerProofSystem))]
    public partial struct GodgameHeadlessSettlementVillagerLoopSystem : ISystem
    {
        private const string EnabledEnv = "GODGAME_HEADLESS_VILLAGER_PROOF";

        private const float DefaultArrivalDistance = 2.5f;
        private const float DefaultMoveSpeed = 35f;
        private const float DefaultHarvestSeconds = 0.35f;
        private const float DefaultRestMinSeconds = 0.25f;
        private const float DefaultRestMaxSeconds = 0.75f;
        private const float DefaultDepositAmount = 1.0f;

        private byte _enabled;
        private byte _tickInitialized;
        private uint _lastTick;
        private float _arrivalDistanceSq;
        private float _moveSpeed;
        private float _harvestSeconds;
        private float _restMinSeconds;
        private float _restMaxSeconds;
        private float _depositAmount;

        private ComponentLookup<SettlementRuntime> _settlementRuntimeLookup;
        private ComponentLookup<LocalTransform> _transformLookup;
        private BufferLookup<SettlementResource> _resourceLookup;
        private ComponentLookup<StorehouseInventory> _storehouseInventoryLookup;

        public void OnCreate(ref SystemState state)
        {
            var enabled = SystemEnv.GetEnvironmentVariable(EnabledEnv);
            if (string.Equals(enabled, "0", StringComparison.OrdinalIgnoreCase))
            {
                state.Enabled = false;
                return;
            }

            if (!RuntimeMode.IsHeadless && !string.Equals(enabled, "1", StringComparison.OrdinalIgnoreCase))
            {
                state.Enabled = false;
                return;
            }

            _enabled = 1;
            _tickInitialized = 0;
            _lastTick = 0;

            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<SettlementVillagerState>();
            state.RequireForUpdate<SettlementRuntime>();
            state.RequireForUpdate<LocalTransform>();

            _moveSpeed = math.max(0.1f, ReadEnvFloat("GODGAME_HEADLESS_VILLAGER_MOVE_SPEED", DefaultMoveSpeed));
            var arrival = math.max(0.25f, ReadEnvFloat("GODGAME_HEADLESS_VILLAGER_ARRIVE_DIST", DefaultArrivalDistance));
            _arrivalDistanceSq = arrival * arrival;
            _harvestSeconds = math.max(0.01f, ReadEnvFloat("GODGAME_HEADLESS_VILLAGER_HARVEST_S", DefaultHarvestSeconds));
            _restMinSeconds = math.max(0f, ReadEnvFloat("GODGAME_HEADLESS_VILLAGER_REST_MIN_S", DefaultRestMinSeconds));
            _restMaxSeconds = math.max(_restMinSeconds, ReadEnvFloat("GODGAME_HEADLESS_VILLAGER_REST_MAX_S", DefaultRestMaxSeconds));
            _depositAmount = math.max(0.01f, ReadEnvFloat("GODGAME_HEADLESS_VILLAGER_DEPOSIT", DefaultDepositAmount));

            _settlementRuntimeLookup = state.GetComponentLookup<SettlementRuntime>(true);
            _transformLookup = state.GetComponentLookup<LocalTransform>(true);
            _resourceLookup = state.GetBufferLookup<SettlementResource>(true);
            _storehouseInventoryLookup = state.GetComponentLookup<StorehouseInventory>(false);
        }

        public void OnUpdate(ref SystemState state)
        {
            if (_enabled == 0)
            {
                return;
            }

            if (SystemAPI.TryGetSingleton<RewindState>(out var rewindState) && rewindState.Mode != RewindMode.Record)
            {
                return;
            }

            var timeState = SystemAPI.GetSingleton<TimeState>();
            var deltaTime = ResolveDeltaTime(timeState, out var deltaTicks, out var fixedDt);
            if (deltaTicks == 0u || deltaTime <= 0f)
            {
                return;
            }

            state.EntityManager.CompleteDependencyBeforeRW<SettlementVillagerState>();
            state.EntityManager.CompleteDependencyBeforeRW<LocalTransform>();
            state.EntityManager.CompleteDependencyBeforeRW<StorehouseInventory>();

            _settlementRuntimeLookup.Update(ref state);
            _transformLookup.Update(ref state);
            _resourceLookup.Update(ref state);
            _storehouseInventoryLookup.Update(ref state);

            foreach (var (villagerState, transform, entity) in SystemAPI
                         .Query<RefRW<SettlementVillagerState>, RefRW<LocalTransform>>()
                         .WithEntityAccess())
            {
                var stateData = villagerState.ValueRO;
                var settlement = stateData.Settlement;
                var hasRuntime = settlement != Entity.Null && settlement.Index >= 0 && _settlementRuntimeLookup.HasComponent(settlement);
                var runtime = hasRuntime ? _settlementRuntimeLookup[settlement] : default;
                var random = new Unity.Mathematics.Random(math.max(1u, stateData.RandomState));
                var updated = stateData;
                var position = transform.ValueRO.Position;

                switch (updated.Phase)
                {
                    case SettlementVillagerPhase.Idle:
                    {
                        updated.CurrentDepot = ResolveDepot(ref state, runtime, position);
                        if (updated.CurrentDepot == Entity.Null)
                        {
                            updated.PhaseTimer = 0f;
                            break;
                        }
                        updated.CurrentResourceNode = ResolveResourceNode(settlement, runtime, updated.CurrentDepot, position);
                        updated.Phase = SettlementVillagerPhase.ToResource;
                        updated.PhaseTimer = math.max(0.25f, 6f * fixedDt);
                        break;
                    }
                    case SettlementVillagerPhase.ToResource:
                    {
                        updated.PhaseTimer -= deltaTime;
                        if (!TryGetPosition(updated.CurrentResourceNode, out var targetPos) || HasReached(position, targetPos) || updated.PhaseTimer <= 0f)
                        {
                            updated.Phase = SettlementVillagerPhase.Harvest;
                            updated.PhaseTimer = _harvestSeconds;
                            break;
                        }

                        position = MoveTowards(position, targetPos, _moveSpeed * deltaTime);
                        break;
                    }
                    case SettlementVillagerPhase.Harvest:
                    {
                        updated.PhaseTimer -= deltaTime;
                        if (updated.PhaseTimer <= 0f)
                        {
                            updated.CurrentDepot = ResolveDepot(ref state, runtime, position);
                            updated.Phase = SettlementVillagerPhase.ToDepot;
                            updated.PhaseTimer = math.max(0.25f, 6f * fixedDt);
                        }
                        break;
                    }
                    case SettlementVillagerPhase.ToDepot:
                    {
                        updated.PhaseTimer -= deltaTime;
                        if (!TryGetPosition(updated.CurrentDepot, out var depotPos) || HasReached(position, depotPos) || updated.PhaseTimer <= 0f)
                        {
                            TryDeposit(updated.CurrentDepot, timeState.Tick);
                            updated.Phase = SettlementVillagerPhase.Resting;
                            updated.PhaseTimer = random.NextFloat(_restMinSeconds, _restMaxSeconds);
                            break;
                        }

                        position = MoveTowards(position, depotPos, _moveSpeed * deltaTime);
                        break;
                    }
                    case SettlementVillagerPhase.Resting:
                    default:
                    {
                        updated.PhaseTimer -= deltaTime;
                        if (updated.PhaseTimer <= 0f)
                        {
                            updated.Phase = SettlementVillagerPhase.Idle;
                            updated.PhaseTimer = 0f;
                        }
                        break;
                    }
                }

                updated.RandomState = random.state;
                villagerState.ValueRW = updated;

                position.y = transform.ValueRO.Position.y;
                transform.ValueRW.Position = position;
            }
        }

        private float ResolveDeltaTime(in TimeState timeState, out uint deltaTicks, out float fixedDt)
        {
            var tick = timeState.Tick;
            if (_tickInitialized == 0)
            {
                _tickInitialized = 1;
                _lastTick = tick;
                deltaTicks = 0u;
                fixedDt = math.max(timeState.FixedDeltaTime, 1e-4f);
                return 0f;
            }

            deltaTicks = tick >= _lastTick ? tick - _lastTick : 0u;
            _lastTick = tick;
            fixedDt = math.max(timeState.FixedDeltaTime, 1e-4f);
            return deltaTicks == 0u ? 0f : fixedDt * deltaTicks;
        }

        private Entity ResolveDepot(ref SystemState state, in SettlementRuntime runtime, in float3 villagerPosition)
        {
            var candidate = ResolveDepotFromRuntime(runtime);
            if (candidate != Entity.Null && candidate.Index >= 0 && _storehouseInventoryLookup.HasComponent(candidate))
            {
                return candidate;
            }

            var best = Entity.Null;
            var bestDistSq = float.MaxValue;

            foreach (var (transform, entity) in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<StorehouseInventory>().WithEntityAccess())
            {
                var pos = transform.ValueRO.Position;
                var distSq = math.distancesq(villagerPosition.xz, pos.xz);
                if (distSq < bestDistSq || (math.abs(distSq - bestDistSq) <= 1e-4f && entity.Index < best.Index))
                {
                    bestDistSq = distSq;
                    best = entity;
                }
            }

            return best;
        }

        private Entity ResolveResourceNode(Entity settlement, in SettlementRuntime runtime, Entity depot, in float3 villagerPosition)
        {
            if (settlement != Entity.Null && settlement.Index >= 0 && _resourceLookup.HasBuffer(settlement))
            {
                var resources = _resourceLookup[settlement];
                if (resources.Length > 0)
                {
                    var best = Entity.Null;
                    var bestDistSq = float.MaxValue;
                    for (int i = 0; i < resources.Length; i++)
                    {
                        var candidate = resources[i].Node;
                        if (!TryGetPosition(candidate, out var candidatePos))
                        {
                            continue;
                        }

                        var distSq = math.distancesq(villagerPosition.xz, candidatePos.xz);
                        if (distSq < bestDistSq)
                        {
                            bestDistSq = distSq;
                            best = candidate;
                        }
                    }

                    if (best != Entity.Null)
                    {
                        return best;
                    }
                }
            }

            var fallback = runtime.VillageCenterInstance != Entity.Null && runtime.VillageCenterInstance != depot
                ? runtime.VillageCenterInstance
                : runtime.HousingInstance != Entity.Null && runtime.HousingInstance != depot
                    ? runtime.HousingInstance
                    : runtime.WorshipInstance != Entity.Null && runtime.WorshipInstance != depot
                        ? runtime.WorshipInstance
                        : depot;

            if (fallback != Entity.Null)
            {
                return fallback;
            }

            // Last resort: keep the villager moving via a deterministic dummy.
            return depot;
        }

        private static Entity ResolveDepotFromRuntime(in SettlementRuntime runtime)
        {
            return runtime.StorehouseInstance != Entity.Null
                ? runtime.StorehouseInstance
                : runtime.VillageCenterInstance != Entity.Null
                    ? runtime.VillageCenterInstance
                    : runtime.HousingInstance != Entity.Null
                        ? runtime.HousingInstance
                        : runtime.WorshipInstance;
        }

        private bool TryGetPosition(Entity candidate, out float3 position)
        {
            if (candidate != Entity.Null && candidate.Index >= 0 && _transformLookup.HasComponent(candidate))
            {
                position = _transformLookup[candidate].Position;
                return true;
            }

            position = float3.zero;
            return false;
        }

        private bool HasReached(in float3 current, in float3 target)
        {
            return math.distancesq(current.xz, target.xz) <= _arrivalDistanceSq;
        }

        private static float3 MoveTowards(in float3 current, in float3 target, float maxMove)
        {
            var delta = target - current;
            delta.y = 0f;
            var distance = math.length(delta);
            if (distance <= 1e-4f)
            {
                return current;
            }

            var move = math.min(distance, math.max(0f, maxMove));
            var dir = delta / distance;
            return current + dir * move;
        }

        private void TryDeposit(Entity depot, uint tick)
        {
            if (depot == Entity.Null || depot.Index < 0 || !_storehouseInventoryLookup.HasComponent(depot))
            {
                return;
            }

            var inventory = _storehouseInventoryLookup[depot];
            var capacity = math.max(0f, inventory.TotalCapacity);
            var stored = math.max(0f, inventory.TotalStored);
            stored = capacity > 0f ? math.min(capacity, stored + _depositAmount) : stored + _depositAmount;
            inventory.TotalStored = stored;
            inventory.LastUpdateTick = tick;
            _storehouseInventoryLookup[depot] = inventory;
        }

        private static float ReadEnvFloat(string key, float defaultValue)
        {
            var raw = SystemEnv.GetEnvironmentVariable(key);
            if (string.IsNullOrWhiteSpace(raw))
            {
                return defaultValue;
            }

            return float.TryParse(raw, out var parsed) ? parsed : defaultValue;
        }
    }
}
