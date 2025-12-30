using System;
using Godgame.Rendering;
using PureDOTS.Rendering;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Core;
using PureDOTS.Runtime.Physics;
using PureDOTS.Runtime.Scenarios;
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
    /// Headless proof that villagers never penetrate buildings below summed radii.
    /// Logs exactly one PASS/FAIL line and can request exit when configured.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    public partial struct GodgameCollisionProofSystem : ISystem
    {
        private const string EnabledEnv = "GODGAME_HEADLESS_COLLISION_PROOF";
        private const string ExitOnResultEnv = "GODGAME_HEADLESS_COLLISION_PROOF_EXIT";
        private const string ScenarioPathEnv = "GODGAME_SCENARIO_PATH";
        private const string CollisionScenarioFile = "godgame_collision_micro.json";
        private const uint DefaultTimeoutTicks = 600;
        private const float PenetrationEpsilon = 0.05f;
        private static readonly FixedString64Bytes CollisionTestId = new FixedString64Bytes("G0.GODGAME_COLLISION_MICRO");

        private struct BuildingProbe
        {
            public float3 Position;
            public float3 HalfExtents;
            public float Radius;
            public byte IsBox;
        }

        private byte _enabled;
        private byte _done;
        private byte _bankResolved;
        private byte _bankLogged;
        private FixedString64Bytes _bankTestId;
        private uint _startTick;
        private uint _timeoutTick;
        private byte _profileReady;
        private float _villagerRadius;
        private float3 _villageCenterHalfExtents;
        private float3 _storehouseHalfExtents;
        private float3 _housingHalfExtents;
        private float _villageCenterRadius;
        private float _storehouseRadius;
        private float _housingRadius;

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
            state.RequireForUpdate<RenderSemanticKey>();
            state.RequireForUpdate<LocalTransform>();
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

            if (ResolveBankTestId() == false)
            {
                return;
            }

            if (_timeoutTick == 0)
            {
                _startTick = timeState.Tick;
                _timeoutTick = _startTick + DefaultTimeoutTicks;
            }

            EnsureProfileRadii(ref state);

            var hasVillager = false;
            var hasBuilding = false;

            using var buildings = new NativeList<BuildingProbe>(Allocator.Temp);
            foreach (var (semantic, transform) in SystemAPI.Query<RefRO<RenderSemanticKey>, RefRO<LocalTransform>>())
            {
                var key = semantic.ValueRO.Value;
                if (!TryResolveBuildingInfo(key, out var halfExtents, out var radius, out var isBox))
                {
                    continue;
                }

                hasBuilding = true;
                buildings.Add(new BuildingProbe
                {
                    Position = transform.ValueRO.Position,
                    HalfExtents = halfExtents,
                    Radius = radius,
                    IsBox = isBox
                });
            }

            foreach (var (semantic, transform) in SystemAPI.Query<RefRO<RenderSemanticKey>, RefRO<LocalTransform>>())
            {
                if (semantic.ValueRO.Value != GodgameSemanticKeys.Villager)
                {
                    continue;
                }

                hasVillager = true;
                var villagerPos = transform.ValueRO.Position;
                for (int i = 0; i < buildings.Length; i++)
                {
                    if (IsPenetrating(villagerPos, buildings[i]))
                    {
                        Fail(ref state, timeState.Tick, "penetration", villagerPos, buildings[i].Position);
                        return;
                    }
                }
            }

            if (timeState.Tick < _timeoutTick)
            {
                return;
            }

            if (_profileReady == 0 || !hasVillager || !hasBuilding)
            {
                Fail(ref state, timeState.Tick, "missing", float3.zero, float3.zero);
                return;
            }

            Pass(ref state, timeState.Tick, hasVillager, hasBuilding);
        }

        private void EnsureProfileRadii(ref SystemState state)
        {
            if (_profileReady != 0)
            {
                return;
            }

            if (!SystemAPI.TryGetSingleton<PhysicsColliderProfileComponent>(out var profileComponent) ||
                !profileComponent.Profile.IsCreated)
            {
                return;
            }

            ref var profile = ref profileComponent.Profile.Value;
            ref var entries = ref profile.Entries;
            if (!PhysicsColliderProfileHelpers.TryGetSpec(ref entries, GodgameSemanticKeys.Villager, out var villagerSpec))
            {
                return;
            }

            if (!PhysicsColliderProfileHelpers.TryGetSpec(ref entries, GodgameSemanticKeys.VillageCenter, out var centerSpec))
            {
                return;
            }

            if (!PhysicsColliderProfileHelpers.TryGetSpec(ref entries, GodgameSemanticKeys.Storehouse, out var storehouseSpec))
            {
                return;
            }

            if (!PhysicsColliderProfileHelpers.TryGetSpec(ref entries, GodgameSemanticKeys.Housing, out var housingSpec))
            {
                return;
            }

            _villagerRadius = ResolveRadius(villagerSpec);
            _villageCenterRadius = ResolveRadius(centerSpec);
            _storehouseRadius = ResolveRadius(storehouseSpec);
            _housingRadius = ResolveRadius(housingSpec);
            _villageCenterHalfExtents = ResolveHalfExtents(centerSpec);
            _storehouseHalfExtents = ResolveHalfExtents(storehouseSpec);
            _housingHalfExtents = ResolveHalfExtents(housingSpec);

            if (_villagerRadius <= 0f || _villageCenterRadius <= 0f || _storehouseRadius <= 0f || _housingRadius <= 0f)
            {
                return;
            }

            _profileReady = 1;
        }

        private bool TryResolveBuildingInfo(ushort semanticKey, out float3 halfExtents, out float radius, out byte isBox)
        {
            halfExtents = float3.zero;
            radius = 0f;
            isBox = 0;
            if (_profileReady == 0)
            {
                return false;
            }

            if (semanticKey == GodgameSemanticKeys.VillageCenter)
            {
                halfExtents = _villageCenterHalfExtents;
                radius = _villageCenterRadius;
                isBox = 1;
                return true;
            }

            if (semanticKey == GodgameSemanticKeys.Storehouse)
            {
                halfExtents = _storehouseHalfExtents;
                radius = _storehouseRadius;
                isBox = 1;
                return true;
            }

            if (semanticKey == GodgameSemanticKeys.Housing)
            {
                halfExtents = _housingHalfExtents;
                radius = _housingRadius;
                isBox = 1;
                return true;
            }

            return false;
        }

        private static float ResolveRadius(in PhysicsColliderSpec spec)
        {
            return spec.Shape switch
            {
                PhysicsColliderShape.Box => math.cmax(spec.Dimensions) * 0.5f,
                PhysicsColliderShape.Capsule => spec.Dimensions.x,
                _ => spec.Dimensions.x
            };
        }

        private static float3 ResolveHalfExtents(in PhysicsColliderSpec spec)
        {
            if (spec.Shape == PhysicsColliderShape.Box)
            {
                return math.max(spec.Dimensions * 0.5f, new float3(0.01f));
            }

            var radius = math.max(spec.Dimensions.x, 0.01f);
            return new float3(radius, radius, radius);
        }

        private void Pass(ref SystemState state, uint tick, bool hasVillager, bool hasBuilding)
        {
            _done = 1;
            UnityDebug.Log($"[GodgameCollisionProof] PASS tick={tick} villagers={(hasVillager ? 1 : 0)} buildings={(hasBuilding ? 1 : 0)} eps={PenetrationEpsilon:F2} boxMode=aabb");
            LogBankResult(ref state, true, "pass", tick);
            ExitIfRequested(ref state, tick, 0);
        }

        private void Fail(ref SystemState state, uint tick, string reason, float3 villagerPos, float3 buildingPos)
        {
            _done = 1;
            UnityDebug.LogError($"[GodgameCollisionProof] FAIL tick={tick} reason={reason} villager={villagerPos} building={buildingPos} eps={PenetrationEpsilon:F2} boxMode=aabb");
            LogBankResult(ref state, false, reason, tick);
            ExitIfRequested(ref state, tick, 4);
        }

        private static void ExitIfRequested(ref SystemState state, uint tick, int exitCode)
        {
            if (!string.Equals(SystemEnv.GetEnvironmentVariable(ExitOnResultEnv), "1", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            GodgameHeadlessExitSystem.Request(ref state, tick, exitCode);
        }

        private bool ResolveBankTestId()
        {
            if (_bankResolved != 0)
            {
                return !_bankTestId.IsEmpty;
            }

            _bankResolved = 1;
            var scenarioPath = SystemEnv.GetEnvironmentVariable(ScenarioPathEnv);
            if (string.IsNullOrWhiteSpace(scenarioPath))
            {
                return false;
            }

            if (scenarioPath.EndsWith(CollisionScenarioFile, StringComparison.OrdinalIgnoreCase))
            {
                _bankTestId = CollisionTestId;
                return true;
            }

            _enabled = 0;
            return false;
        }

        private void LogBankResult(ref SystemState state, bool pass, string reason, uint tick)
        {
            if (_bankLogged != 0 || _bankTestId.IsEmpty)
            {
                return;
            }

            ResolveTickInfo(ref state, tick, out var tickTime, out var scenarioTick);
            var delta = (int)tickTime - (int)scenarioTick;
            _bankLogged = 1;

            if (pass)
            {
                UnityDebug.Log($"BANK:{_bankTestId}:PASS tickTime={tickTime} scenarioTick={scenarioTick} delta={delta}");
                return;
            }

            UnityDebug.Log($"BANK:{_bankTestId}:FAIL reason={reason} tickTime={tickTime} scenarioTick={scenarioTick} delta={delta}");
        }

        private void ResolveTickInfo(ref SystemState state, uint tick, out uint tickTime, out uint scenarioTick)
        {
            tickTime = tick;
            if (SystemAPI.TryGetSingleton<TickTimeState>(out var tickTimeState))
            {
                tickTime = tickTimeState.Tick;
            }

            scenarioTick = SystemAPI.TryGetSingleton<ScenarioRunnerTick>(out var scenario)
                ? scenario.Tick
                : 0u;
        }

        private bool IsPenetrating(float3 villagerPos, in BuildingProbe building)
        {
            if (_villagerRadius <= 0f)
            {
                return false;
            }

            var toCenter = villagerPos - building.Position;
            var halfExtents = building.IsBox != 0 ? building.HalfExtents : new float3(building.Radius);
            var clamped = math.clamp(toCenter, -halfExtents, halfExtents);
            var closest = building.Position + clamped;
            var distance = math.distance(villagerPos, closest);
            return distance < math.max(0f, _villagerRadius - PenetrationEpsilon);
        }
    }
}
