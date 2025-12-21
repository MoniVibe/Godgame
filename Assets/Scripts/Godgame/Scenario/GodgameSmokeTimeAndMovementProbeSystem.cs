#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using PureDOTS.Runtime.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityTime = UnityEngine.Time;

namespace Godgame.Scenario
{
    /// <summary>
    /// Dev-only smoke probe that proves:
    /// - time is ticking (TimeState/TickTimeState advance),
    /// - villagers are moving (sample position changes),
    /// without requiring any presentation correctness.
    /// Logs exactly twice (Initial + Final after ~5s real time), then disables itself.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct GodgameSmokeTimeAndMovementProbeSystem : ISystem
    {
        private bool _loggedInitial;
        private bool _loggedFinal;
        private double _initialRealTimeSeconds;
        private Entity _trackedVillager;
        private float3 _trackedVillagerInitialPos;

        private EntityQuery _villagerQuery;
        private EntityQuery _timeStateQuery;
        private EntityQuery _tickTimeStateQuery;
        private EntityQuery _rewindStateQuery;

        public void OnCreate(ref SystemState state)
        {
            _villagerQuery = state.GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<SettlementVillagerState>(),
                    ComponentType.ReadOnly<LocalTransform>()
                }
            });

            _timeStateQuery = state.GetEntityQuery(ComponentType.ReadOnly<TimeState>());
            _tickTimeStateQuery = state.GetEntityQuery(ComponentType.ReadOnly<TickTimeState>());
            _rewindStateQuery = state.GetEntityQuery(ComponentType.ReadOnly<RewindState>());
        }

        public void OnUpdate(ref SystemState state)
        {
            if (Application.isBatchMode)
            {
                state.Enabled = false;
                return;
            }

            var worldName = state.WorldUnmanaged.Name.ToString();
            if (!string.Equals(worldName, "Game World", StringComparison.Ordinal))
            {
                state.Enabled = false;
                return;
            }

            var villagerCount = _villagerQuery.CalculateEntityCount();
            if (!_loggedInitial)
            {
                // Wait until the scenario has actually spawned villagers so we can track a stable sample entity.
                if (villagerCount == 0)
                {
                    return;
                }

                _trackedVillager = PickTrackedVillager(ref state);
                _trackedVillagerInitialPos = ReadTrackedVillagerPosition(ref state, _trackedVillager);

                _loggedInitial = true;
                _initialRealTimeSeconds = UnityTime.realtimeSinceStartupAsDouble;
                LogPhase(ref state, "Initial", villagerCount);
                return;
            }

            if (_loggedFinal)
            {
                state.Enabled = false;
                return;
            }

            if (UnityTime.realtimeSinceStartupAsDouble >= _initialRealTimeSeconds + 5.0)
            {
                _loggedFinal = true;
                LogPhase(ref state, "Final", villagerCount);
                state.Enabled = false;
            }
        }

        private Entity PickTrackedVillager(ref SystemState state)
        {
            using var villagers = _villagerQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
            return villagers.Length > 0 ? villagers[0] : Entity.Null;
        }

        private static float3 ReadTrackedVillagerPosition(ref SystemState state, Entity villager)
        {
            if (villager == Entity.Null || !state.EntityManager.Exists(villager) || !state.EntityManager.HasComponent<LocalTransform>(villager))
            {
                return float3.zero;
            }

            return state.EntityManager.GetComponentData<LocalTransform>(villager).Position;
        }

        private void LogPhase(ref SystemState state, string phase, int villagerCount)
        {
            var timeCount = _timeStateQuery.CalculateEntityCount();
            var tickCount = _tickTimeStateQuery.CalculateEntityCount();
            var rewindCount = _rewindStateQuery.CalculateEntityCount();

            var info = $" TimeStateCount={timeCount} TickTimeStateCount={tickCount} RewindStateCount={rewindCount}";

            if (timeCount == 1 && _timeStateQuery.TryGetSingleton(out TimeState time))
            {
                info += $" TimeTick={time.Tick} TimeWorldSeconds={time.WorldSeconds} TimePaused={time.IsPaused}";
            }
            else
            {
                info += timeCount == 0 ? " TimeState=missing" : " TimeState=non-singleton";
            }

            if (tickCount == 1 && _tickTimeStateQuery.TryGetSingleton(out TickTimeState tick))
            {
                info += $" Tick={tick.Tick} TargetTick={tick.TargetTick} WorldSeconds={tick.WorldSeconds} IsPlaying={tick.IsPlaying} IsPaused={tick.IsPaused} Speed={tick.CurrentSpeedMultiplier}";
            }
            else
            {
                info += tickCount == 0 ? " TickTimeState=missing" : " TickTimeState=non-singleton";
            }

            if (rewindCount == 1 && _rewindStateQuery.TryGetSingleton(out RewindState rewind))
            {
                info += $" RewindMode={rewind.Mode}";
            }
            else
            {
                info += rewindCount == 0 ? " RewindState=missing" : " RewindState=non-singleton";
            }

            Debug.Log($"[GodgameSmokeTimeProbe] Phase={phase} World='Game World' Villagers={villagerCount}{info}");

            var em = state.EntityManager;
            var sampleEntity = _trackedVillager;
            if (sampleEntity == Entity.Null || !em.Exists(sampleEntity) || !em.HasComponent<LocalTransform>(sampleEntity))
            {
                sampleEntity = PickTrackedVillager(ref state);
            }

            if (sampleEntity == Entity.Null || !em.Exists(sampleEntity) || !em.HasComponent<LocalTransform>(sampleEntity))
            {
                Debug.Log("[GodgameSmokeTimeProbe] Sample='Villager' Entity=none");
                return;
            }

            var transform = em.GetComponentData<LocalTransform>(sampleEntity);
            var settlementState = em.GetComponentData<SettlementVillagerState>(sampleEntity);

            if (phase == "Final" && _trackedVillager != Entity.Null && em.Exists(_trackedVillager) && em.HasComponent<LocalTransform>(_trackedVillager))
            {
                var delta = transform.Position - _trackedVillagerInitialPos;
                delta.y = 0f;
                var deltaMeters = math.length(delta);
                Debug.Log($"[GodgameSmokeTimeProbe] Sample='Villager' Entity={sampleEntity} Pos={transform.Position} Phase={settlementState.Phase} InitialPos={_trackedVillagerInitialPos} DeltaXZ={deltaMeters:0.###}");
            }
            else
            {
                Debug.Log($"[GodgameSmokeTimeProbe] Sample='Villager' Entity={sampleEntity} Pos={transform.Position} Phase={settlementState.Phase}");
            }
        }
    }
}
#endif
