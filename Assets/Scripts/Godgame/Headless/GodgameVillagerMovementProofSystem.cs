using System;
using Godgame.Scenario;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Core;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityDebug = UnityEngine.Debug;
using SystemEnv = System.Environment;

namespace Godgame.Headless
{
    /// <summary>
    /// Headless proof that at least one villager changes position within a timeout window.
    /// Logs exactly one PASS/FAIL line and can request exit when configured.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    public partial struct GodgameVillagerMovementProofSystem : ISystem
    {
        private const string EnabledEnv = "GODGAME_HEADLESS_MOVEMENT_PROOF";
        private const string ExitOnResultEnv = "GODGAME_HEADLESS_MOVEMENT_PROOF_EXIT";
        private const uint DefaultTimeoutTicks = 600; // ~10 seconds at 60hz
        private const float MovementEpsilon = 0.25f;
        private const int MaxTracked = 16;

        private byte _enabled;
        private byte _done;
        private uint _startTick;
        private uint _timeoutTick;
        private NativeList<Entity> _trackedEntities;
        private NativeList<float3> _trackedPositions;
        private EntityQuery _villagerQuery;

        public void OnCreate(ref SystemState state)
        {
            if (!RuntimeMode.IsHeadless || !Application.isBatchMode)
            {
                state.Enabled = false;
                return;
            }

            var enabled = SystemEnv.GetEnvironmentVariable(EnabledEnv);
            if (string.Equals(enabled, "0", StringComparison.OrdinalIgnoreCase))
            {
                state.Enabled = false;
                return;
            }

            _enabled = 1;
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<SettlementVillagerState>();
            state.RequireForUpdate<LocalTransform>();

            _villagerQuery = SystemAPI.QueryBuilder()
                .WithAll<SettlementVillagerState, LocalTransform>()
                .Build();

            _trackedEntities = new NativeList<Entity>(MaxTracked, Allocator.Persistent);
            _trackedPositions = new NativeList<float3>(MaxTracked, Allocator.Persistent);
        }

        public void OnDestroy(ref SystemState state)
        {
            if (_trackedEntities.IsCreated)
            {
                _trackedEntities.Dispose();
            }

            if (_trackedPositions.IsCreated)
            {
                _trackedPositions.Dispose();
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            if (_enabled == 0 || _done != 0)
            {
                return;
            }

            if (SystemAPI.TryGetSingleton<RewindState>(out var rewind) && rewind.Mode != RewindMode.Record)
            {
                return;
            }

            var timeState = SystemAPI.GetSingleton<TimeState>();
            if (timeState.IsPaused)
            {
                return;
            }

            if (_timeoutTick == 0)
            {
                _startTick = timeState.Tick;
                _timeoutTick = _startTick + DefaultTimeoutTicks;
            }

            EnsureTrackedSample(ref state);

            var moved = TryDetectMovement(ref state, out var maxDistance, out var validTracked);
            var villagerCount = _villagerQuery.CalculateEntityCount();

            if (moved)
            {
                _done = 1;
                UnityDebug.Log($"[GodgameVillagerMovementProof] PASS tick={timeState.Tick} moved={maxDistance:F2} tracked={validTracked} villagers={villagerCount} timeoutTicks={DefaultTimeoutTicks}");
                ExitIfRequested(ref state, timeState.Tick, 0);
                return;
            }

            if (timeState.Tick >= _timeoutTick)
            {
                _done = 1;
                UnityDebug.LogError($"[GodgameVillagerMovementProof] FAIL tick={timeState.Tick} moved={maxDistance:F2} tracked={validTracked} villagers={villagerCount} timeoutTicks={DefaultTimeoutTicks}");
                ExitIfRequested(ref state, timeState.Tick, 4);
            }
        }

        private void EnsureTrackedSample(ref SystemState state)
        {
            if (_trackedEntities.Length > 0)
            {
                return;
            }

            foreach (var (transform, entity) in SystemAPI
                         .Query<RefRO<LocalTransform>>()
                         .WithAll<SettlementVillagerState>()
                         .WithEntityAccess())
            {
                _trackedEntities.Add(entity);
                _trackedPositions.Add(transform.ValueRO.Position);
                if (_trackedEntities.Length >= MaxTracked)
                {
                    break;
                }
            }
        }

        private bool TryDetectMovement(ref SystemState state, out float maxDistance, out int validTracked)
        {
            maxDistance = 0f;
            validTracked = 0;

            if (_trackedEntities.Length == 0)
            {
                return false;
            }

            var entityManager = state.EntityManager;
            for (int i = 0; i < _trackedEntities.Length; i++)
            {
                var entity = _trackedEntities[i];
                if (!entityManager.Exists(entity) || !entityManager.HasComponent<LocalTransform>(entity))
                {
                    continue;
                }

                validTracked++;
                var position = entityManager.GetComponentData<LocalTransform>(entity).Position;
                var distance = math.distance(position, _trackedPositions[i]);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                }

                if (distance > MovementEpsilon)
                {
                    return true;
                }
            }

            if (validTracked == 0)
            {
                _trackedEntities.Clear();
                _trackedPositions.Clear();
            }

            return false;
        }

        private static void ExitIfRequested(ref SystemState state, uint tick, int exitCode)
        {
            if (!string.Equals(SystemEnv.GetEnvironmentVariable(ExitOnResultEnv), "1", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            GodgameHeadlessExitSystem.Request(ref state, tick, exitCode);
        }
    }
}
