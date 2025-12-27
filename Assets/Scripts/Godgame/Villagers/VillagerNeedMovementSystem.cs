using Godgame.Scenario;
using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Villagers
{
    /// <summary>
    /// Moves villagers toward non-work goals (eat, rest, pray, socialize, shelter, flee).
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(VillagerJobSystem))]
    public partial struct VillagerNeedMovementSystem : ISystem
    {
        private ComponentLookup<LocalTransform> _transformLookup;
        private ComponentLookup<VillagerSocialFocus> _socialFocusLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<VillagerGoalState>();
            state.RequireForUpdate<VillagerScheduleConfig>();
            state.RequireForUpdate<VillagerNeedMovementState>();
            _transformLookup = state.GetComponentLookup<LocalTransform>(true);
            _socialFocusLookup = state.GetComponentLookup<VillagerSocialFocus>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<TimeState>(out var timeState) || timeState.IsPaused)
            {
                return;
            }

            if (SystemAPI.TryGetSingleton<RewindState>(out var rewindState) && rewindState.Mode != RewindMode.Record)
            {
                return;
            }

            _transformLookup.Update(ref state);
            _socialFocusLookup.Update(ref state);
            var schedule = SystemAPI.GetSingleton<VillagerScheduleConfig>();
            var movementTuning = SystemAPI.TryGetSingleton<VillagerMovementTuning>(out var tuningValue)
                ? tuningValue
                : VillagerMovementTuning.Default;
            var moveSpeedBase = math.max(0.1f, schedule.NeedMoveSpeed);
            var wanderRadius = math.max(0f, schedule.NeedWanderRadius);
            var socialRadius = math.max(wanderRadius, schedule.NeedSocialRadius);
            var lingerMin = math.max(0f, schedule.NeedLingerMinSeconds);
            var lingerMax = math.max(lingerMin, schedule.NeedLingerMaxSeconds);
            var repathMin = math.max(0f, schedule.NeedRepathMinSeconds);
            var repathMax = math.max(repathMin, schedule.NeedRepathMaxSeconds);
            var baseMoveMultiplier = math.max(0.1f, movementTuning.BaseMoveSpeedMultiplier);
            var varianceAmplitude = math.max(0f, movementTuning.SpeedVarianceAmplitude);
            var variancePeriod = math.max(0f, movementTuning.SpeedVariancePeriodSeconds);
            var separationRadius = math.max(0f, movementTuning.SeparationRadius);
            var separationWeight = math.max(0f, movementTuning.SeparationWeight);
            var separationMaxPush = math.max(0f, movementTuning.SeparationMaxPush);
            var separationCellSize = math.max(0.1f, movementTuning.SeparationCellSize);
            var arriveSlowdownRadius = math.max(0f, movementTuning.ArriveSlowdownRadius);
            var arriveMinMultiplier = math.clamp(movementTuning.ArriveMinSpeedMultiplier, 0.1f, 1f);

            var hasSettlement = TryGetSettlementRuntime(ref state, out var runtime);
            var deltaTime = SystemAPI.Time.DeltaTime;

            var villagerQuery = SystemAPI.QueryBuilder()
                .WithAll<VillagerGoalState, LocalTransform>()
                .WithNone<HandHeldTag, Godgame.Runtime.HandQueuedTag>()
                .Build();
            var villagerEntities = villagerQuery.ToEntityArray(state.WorldUpdateAllocator);
            var villagerTransforms = villagerQuery.ToComponentDataArray<LocalTransform>(state.WorldUpdateAllocator);
            var separationMap = new NativeParallelMultiHashMap<int, int>(math.max(1, villagerEntities.Length * 2), Allocator.Temp);
            BuildSeparationMap(villagerTransforms, separationCellSize, separationMap);

            foreach (var (goal, behavior, nav, transform, movementState, moveIntent, movePlan, entity) in SystemAPI
                         .Query<RefRO<VillagerGoalState>, RefRO<VillagerBehavior>, RefRW<Navigation>, RefRW<LocalTransform>, RefRW<VillagerNeedMovementState>, RefRW<MoveIntent>, RefRW<MovePlan>>()
                         .WithNone<HandHeldTag, Godgame.Runtime.HandQueuedTag>()
                         .WithEntityAccess())
            {
                var currentGoal = goal.ValueRO.CurrentGoal;
                if (currentGoal == VillagerGoal.Work)
                {
                    continue;
                }

                if (movementState.ValueRW.LingerSeconds > 0f)
                {
                    movementState.ValueRW.LingerSeconds = math.max(0f, movementState.ValueRW.LingerSeconds - deltaTime);
                    continue;
                }

                float3 targetPosition;
                Entity targetEntity = Entity.Null;
                if (currentGoal == VillagerGoal.Flee)
                {
                    targetPosition = nav.ValueRO.Destination;
                    targetEntity = Entity.Null;
                    movementState.ValueRW.AnchorOffset = float3.zero;
                    movementState.ValueRW.NextRepathTick = timeState.Tick;
                }
                else if (!TryResolveNeedTarget(ref state, currentGoal, hasSettlement, runtime, transform.ValueRO.Position, entity, out targetEntity, out targetPosition))
                {
                    continue;
                }

                var patience01 = math.saturate((behavior.ValueRO.PatienceScore + 100f) * 0.005f);
                var arrivalDistance = 0.6f;
                var radius = currentGoal == VillagerGoal.Socialize ? socialRadius : wanderRadius;
                if (currentGoal != VillagerGoal.Flee
                    && (timeState.Tick >= movementState.ValueRO.NextRepathTick
                        || math.lengthsq(movementState.ValueRO.AnchorOffset) <= 1e-4f))
                {
                    movementState.ValueRW.AnchorOffset = BuildWanderOffset(entity, timeState.Tick, radius);
                    movementState.ValueRW.NextRepathTick = timeState.Tick + ResolveRepathTicks(repathMin, repathMax, patience01, timeState.FixedDeltaTime, entity);
                }

                var destination = targetPosition + movementState.ValueRO.AnchorOffset;
                if (currentGoal == VillagerGoal.Socialize && targetEntity != Entity.Null)
                {
                    destination += ResolveSocialOrbitOffset(entity, timeState.WorldSeconds, radius);
                }
                nav.ValueRW.Destination = destination;
                moveIntent.ValueRW = new MoveIntent
                {
                    TargetEntity = targetEntity,
                    TargetPosition = destination,
                    IntentType = currentGoal == VillagerGoal.Flee
                        ? MoveIntentType.Flee
                        : currentGoal == VillagerGoal.Idle
                            ? MoveIntentType.Idle
                            : MoveIntentType.Need
                };

                var speedScale = math.lerp(1.15f, 0.65f, patience01);
                var moveSpeed = math.max(0.05f, moveSpeedBase * speedScale * baseMoveMultiplier);
                moveSpeed = ApplySpeedVariance(entity, moveSpeed, timeState.WorldSeconds, varianceAmplitude, variancePeriod);
                nav.ValueRW.Speed = moveSpeed;

                var toTarget = destination - transform.ValueRO.Position;
                toTarget.y = 0f;
                var distance = math.length(toTarget);
                if (distance <= arrivalDistance)
                {
                    if (currentGoal != VillagerGoal.Flee)
                    {
                        movementState.ValueRW.LingerSeconds = ResolveLingerSeconds(lingerMin, lingerMax, patience01, currentGoal, entity);
                    }
                    movePlan.ValueRW = new MovePlan
                    {
                        Mode = MovePlanMode.Arrive,
                        DesiredVelocity = float3.zero,
                        MaxAccel = moveSpeed / math.max(DeltaTimeEpsilon, deltaTime),
                        EtaSeconds = 0f
                    };
                    continue;
                }

                var move = math.min(distance, moveSpeed * deltaTime);
                var dir = toTarget / math.max(distance, 1e-4f);
                var separation = ResolveSeparation(entity, transform.ValueRO.Position, villagerEntities, villagerTransforms,
                    separationMap, separationCellSize, separationRadius, separationWeight, separationMaxPush);
                dir = math.normalizesafe(dir + separation);
                var arriveSpeed = ApplyArriveSlowdown(moveSpeed, distance, arriveSlowdownRadius, arriveMinMultiplier);
                move = math.min(distance, arriveSpeed * deltaTime);
                transform.ValueRW.Position += dir * move;
                movePlan.ValueRW = new MovePlan
                {
                    Mode = MovePlanMode.Approach,
                    DesiredVelocity = dir * arriveSpeed,
                    MaxAccel = moveSpeed / math.max(DeltaTimeEpsilon, deltaTime),
                    EtaSeconds = arriveSpeed > 0f ? distance / arriveSpeed : 0f
                };
            }

            separationMap.Dispose();
        }

        private static float3 BuildWanderOffset(Entity entity, uint tick, float radius)
        {
            if (radius <= 0f)
            {
                return float3.zero;
            }

            var seed = math.hash(new uint3((uint)(entity.Index + 1), tick + 31u, 9049u));
            var random = Unity.Mathematics.Random.CreateFromIndex(seed == 0u ? 1u : seed);
            var angle = random.NextFloat(0f, math.PI * 2f);
            var distance = random.NextFloat(0f, radius);
            return new float3(math.cos(angle) * distance, 0f, math.sin(angle) * distance);
        }

        private static uint ResolveRepathTicks(float minSeconds, float maxSeconds, float patience01, float fixedDeltaTime, Entity entity)
        {
            if (maxSeconds <= 0f)
            {
                return 0;
            }

            var min = math.max(0f, minSeconds);
            var max = math.max(min, maxSeconds);
            var seed = math.hash(new uint2((uint)(entity.Index + 7), 517u));
            var random = Unity.Mathematics.Random.CreateFromIndex(seed == 0u ? 1u : seed);
            var sample = random.NextFloat(min, max);
            var seconds = math.lerp(min, sample, math.lerp(0.6f, 1.1f, patience01));
            return (uint)math.max(1f, math.ceil(seconds / math.max(1e-4f, fixedDeltaTime)));
        }

        private static float ResolveLingerSeconds(float minSeconds, float maxSeconds, float patience01, VillagerGoal goal, Entity entity)
        {
            if (maxSeconds <= 0f)
            {
                return 0f;
            }

            var min = math.max(0f, minSeconds);
            var max = math.max(min, maxSeconds);
            var seed = math.hash(new uint3((uint)(entity.Index + 11), (uint)goal + 3u, 1229u));
            var random = Unity.Mathematics.Random.CreateFromIndex(seed == 0u ? 1u : seed);
            var sample = random.NextFloat(min, max);
            var goalBoost = goal == VillagerGoal.Socialize ? 1.2f : 1f;
            return math.lerp(min, sample, math.lerp(0.6f, 1.2f, patience01)) * goalBoost;
        }

        private static float ApplySpeedVariance(Entity entity, float baseSpeed, float worldSeconds, float amplitude, float periodSeconds)
        {
            if (amplitude <= 0f || periodSeconds <= 0f || baseSpeed <= 0f)
            {
                return baseSpeed;
            }

            var seed = math.hash(new uint2((uint)(entity.Index + 1), 0x5f21u));
            var phase01 = (seed & 0xffffu) / 65535f;
            var angle = (worldSeconds / periodSeconds + phase01) * math.PI * 2f;
            var variance = 1f + math.sin(angle) * amplitude;
            return baseSpeed * math.max(0.1f, variance);
        }

        private static float ApplyArriveSlowdown(float speed, float distance, float radius, float minMultiplier)
        {
            if (radius <= 0f || speed <= 0f)
            {
                return speed;
            }

            var t = math.saturate(distance / radius);
            var scale = math.lerp(minMultiplier, 1f, t);
            return speed * scale;
        }

        private static float3 ResolveSocialOrbitOffset(Entity entity, float worldSeconds, float radius)
        {
            if (radius <= 0f)
            {
                return float3.zero;
            }

            var orbitRadius = math.min(radius * 0.3f, 2.5f);
            var seed = math.hash(new uint2((uint)(entity.Index + 13), 0x41b3u));
            var phase01 = (seed & 0xffffu) / 65535f;
            var angle = (worldSeconds * 0.35f + phase01) * math.PI * 2f;
            return new float3(math.cos(angle) * orbitRadius, 0f, math.sin(angle) * orbitRadius);
        }

        private static void BuildSeparationMap(NativeArray<LocalTransform> transforms, float cellSize, NativeParallelMultiHashMap<int, int> map)
        {
            map.Clear();
            if (cellSize <= 0f)
            {
                return;
            }

            for (int i = 0; i < transforms.Length; i++)
            {
                var key = HashCell(transforms[i].Position, cellSize);
                map.Add(key, i);
            }
        }

        private static float3 ResolveSeparation(Entity self, float3 position, NativeArray<Entity> entities, NativeArray<LocalTransform> transforms,
            NativeParallelMultiHashMap<int, int> map, float cellSize, float radius, float weight, float maxPush)
        {
            if (radius <= 0f || weight <= 0f || entities.Length == 0)
            {
                return float3.zero;
            }

            var radiusSq = radius * radius;
            var baseCell = CellCoord(position, cellSize);
            float3 separation = float3.zero;

            for (int x = -1; x <= 1; x++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    var neighborCell = baseCell + new int2(x, z);
                    var key = HashCell(neighborCell);
                    if (!map.TryGetFirstValue(key, out var index, out var iterator))
                    {
                        continue;
                    }

                    do
                    {
                        if (entities[index] == self)
                        {
                            continue;
                        }

                        var other = transforms[index].Position;
                        var diff = position - other;
                        diff.y = 0f;
                        var distSq = math.lengthsq(diff);
                        if (distSq <= 1e-4f || distSq >= radiusSq)
                        {
                            continue;
                        }

                        var dist = math.sqrt(distSq);
                        var push = (radius - dist) / radius;
                        separation += diff / math.max(dist, 1e-4f) * push;
                    } while (map.TryGetNextValue(out index, ref iterator));
                }
            }

            var magnitude = math.length(separation);
            if (magnitude <= 1e-4f)
            {
                return float3.zero;
            }

            var scaled = math.min(maxPush, magnitude * weight);
            return separation / magnitude * scaled;
        }

        private static int HashCell(float3 position, float cellSize)
        {
            return HashCell(CellCoord(position, cellSize));
        }

        private static int2 CellCoord(float3 position, float cellSize)
        {
            if (cellSize <= 0f)
            {
                return int2.zero;
            }

            return (int2)math.floor(position.xz / cellSize);
        }

        private static int HashCell(int2 cell)
        {
            return (int)math.hash(cell);
        }

        private bool TryResolveNeedTarget(ref SystemState state, VillagerGoal goal, bool hasSettlement, in SettlementRuntime runtime, float3 origin, Entity entity, out Entity targetEntity, out float3 targetPosition)
        {
            targetPosition = float3.zero;
            targetEntity = Entity.Null;

            if (hasSettlement)
            {
                switch (goal)
                {
                    case VillagerGoal.Idle:
                        targetEntity = runtime.VillageCenterInstance;
                        return TryGetTargetPosition(runtime.VillageCenterInstance, ref state, out targetPosition)
                               || UseFallbackAnchor(origin, out targetPosition);
                    case VillagerGoal.Eat:
                        targetEntity = runtime.StorehouseInstance;
                        return TryGetTargetPosition(runtime.StorehouseInstance, ref state, out targetPosition);
                    case VillagerGoal.Sleep:
                    case VillagerGoal.SeekShelter:
                        targetEntity = runtime.HousingInstance;
                        return TryGetTargetPosition(runtime.HousingInstance, ref state, out targetPosition);
                    case VillagerGoal.Pray:
                        targetEntity = runtime.WorshipInstance;
                        return TryGetTargetPosition(runtime.WorshipInstance, ref state, out targetPosition);
                    case VillagerGoal.Socialize:
                        if (_socialFocusLookup.HasComponent(entity))
                        {
                            var focus = _socialFocusLookup[entity];
                            if (focus.Target != Entity.Null && TryGetTargetPosition(focus.Target, ref state, out targetPosition))
                            {
                                targetEntity = focus.Target;
                                return true;
                            }
                        }

                        targetEntity = runtime.VillageCenterInstance;
                        return TryGetTargetPosition(runtime.VillageCenterInstance, ref state, out targetPosition);
                }
            }

            if (goal == VillagerGoal.Idle)
            {
                return UseFallbackAnchor(origin, out targetPosition);
            }

            return false;
        }

        private const float DeltaTimeEpsilon = 0.0001f;

        private static bool UseFallbackAnchor(float3 origin, out float3 targetPosition)
        {
            targetPosition = origin;
            return true;
        }

        private static bool TryGetTargetPosition(Entity target, ref SystemState state, out float3 position)
        {
            position = float3.zero;
            if (target == Entity.Null || !state.EntityManager.Exists(target))
            {
                return false;
            }

            if (!state.EntityManager.HasComponent<LocalTransform>(target))
            {
                return false;
            }

            position = state.EntityManager.GetComponentData<LocalTransform>(target).Position;
            return true;
        }

        private bool TryGetSettlementRuntime(ref SystemState state, out SettlementRuntime runtime)
        {
            runtime = default;
            var bestScore = int.MinValue;
            var found = false;

            foreach (var runtimeRef in SystemAPI.Query<RefRO<SettlementRuntime>>())
            {
                var candidate = runtimeRef.ValueRO;
                var score = ScoreSettlementRuntime(ref state, candidate);
                if (score > bestScore)
                {
                    bestScore = score;
                    runtime = candidate;
                    found = true;
                }
            }

            return found && bestScore > int.MinValue;
        }

        private int ScoreSettlementRuntime(ref SystemState state, in SettlementRuntime runtime)
        {
            var score = 0;

            if (TryGetTargetPosition(runtime.StorehouseInstance, ref state, out _))
            {
                score += 200;
            }

            if (TryGetTargetPosition(runtime.VillageCenterInstance, ref state, out _))
            {
                score += 100;
            }

            if (TryGetTargetPosition(runtime.HousingInstance, ref state, out _))
            {
                score += 60;
            }

            if (TryGetTargetPosition(runtime.WorshipInstance, ref state, out _))
            {
                score += 40;
            }

            return score;
        }
    }
}
