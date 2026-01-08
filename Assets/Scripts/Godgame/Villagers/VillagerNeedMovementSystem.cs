using Godgame.Scenario;
using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Physics;
using PureDOTS.Runtime.Villagers;
using Godgame.Physics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Godgame.Villagers
{
    /// <summary>
    /// Moves villagers toward non-work goals (eat, rest, pray, socialize, shelter, flee).
    /// </summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(VillagerJobSystem))]
    public partial struct VillagerNeedMovementSystem : ISystem
    {
        private ComponentLookup<LocalTransform> _transformLookup;
        private ComponentLookup<VillagerSocialFocus> _socialFocusLookup;
        private ComponentLookup<VillagerWorkCooldown> _cooldownLookup;
        private ComponentLookup<VillagerOutlook> _outlookLookup;
        private ComponentLookup<VillagerArchetypeResolved> _archetypeLookup;
        private ComponentLookup<VillagerBehavior> _behaviorLookup;
        private ComponentLookup<VillagerLeisureState> _leisureLookup;
        private BufferLookup<VillagerCooldownOutlookRule> _cooldownOutlookLookup;
        private BufferLookup<VillagerCooldownArchetypeModifier> _cooldownArchetypeLookup;
        private ComponentLookup<PhysicsCollider> _physicsColliderLookup;
        private ComponentLookup<GodgamePhysicsBody> _physicsBodyLookup;
        private ComponentLookup<PhysicsColliderSpec> _colliderSpecLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<VillagerGoalState>();
            state.RequireForUpdate<VillagerScheduleConfig>();
            state.RequireForUpdate<VillagerNeedMovementState>();
            _transformLookup = state.GetComponentLookup<LocalTransform>(true);
            _socialFocusLookup = state.GetComponentLookup<VillagerSocialFocus>(true);
            _cooldownLookup = state.GetComponentLookup<VillagerWorkCooldown>(true);
            _outlookLookup = state.GetComponentLookup<VillagerOutlook>(true);
            _archetypeLookup = state.GetComponentLookup<VillagerArchetypeResolved>(true);
            _behaviorLookup = state.GetComponentLookup<VillagerBehavior>(true);
            _leisureLookup = state.GetComponentLookup<VillagerLeisureState>(true);
            _cooldownOutlookLookup = state.GetBufferLookup<VillagerCooldownOutlookRule>(true);
            _cooldownArchetypeLookup = state.GetBufferLookup<VillagerCooldownArchetypeModifier>(true);
            _physicsColliderLookup = state.GetComponentLookup<PhysicsCollider>(true);
            _physicsBodyLookup = state.GetComponentLookup<GodgamePhysicsBody>(true);
            _colliderSpecLookup = state.GetComponentLookup<PhysicsColliderSpec>(true);
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
            _cooldownLookup.Update(ref state);
            _outlookLookup.Update(ref state);
            _archetypeLookup.Update(ref state);
            _behaviorLookup.Update(ref state);
            _leisureLookup.Update(ref state);
            _cooldownOutlookLookup.Update(ref state);
            _cooldownArchetypeLookup.Update(ref state);
            _physicsColliderLookup.Update(ref state);
            _physicsBodyLookup.Update(ref state);
            _colliderSpecLookup.Update(ref state);
            var tick = timeState.Tick;
            var schedule = SystemAPI.GetSingleton<VillagerScheduleConfig>();
            var movementTuning = SystemAPI.TryGetSingleton<VillagerMovementTuning>(out var tuningValue)
                ? tuningValue
                : VillagerMovementTuning.Default;
            var cooldownProfile = SystemAPI.TryGetSingleton<VillagerCooldownProfile>(out var cooldownValue)
                ? cooldownValue
                : VillagerCooldownProfile.Default;
            var moveSpeedBase = math.max(0.1f, schedule.NeedMoveSpeed);
            var wanderRadius = math.max(0f, schedule.NeedWanderRadius);
            var socialRadius = math.max(wanderRadius, schedule.NeedSocialRadius);
            var lingerMin = math.max(0f, schedule.NeedLingerMinSeconds);
            var lingerMax = math.max(lingerMin, schedule.NeedLingerMaxSeconds);
            var repathMin = math.max(0f, schedule.NeedRepathMinSeconds);
            var repathMax = math.max(repathMin, schedule.NeedRepathMaxSeconds);
            var leisureMoveSpeedMultiplier = math.max(0f, cooldownProfile.LeisureMoveSpeedMultiplier);
            var leisureWanderMultiplier = math.max(0f, cooldownProfile.LeisureWanderRadiusMultiplier);
            var leisureSocialMultiplier = math.max(0f, cooldownProfile.LeisureSocialRadiusMultiplier);
            var leisureLingerMinMultiplier = math.max(0f, cooldownProfile.LeisureLingerMinMultiplier);
            var leisureLingerMaxMultiplier = math.max(0f, cooldownProfile.LeisureLingerMaxMultiplier);
            var leisureRepathMinMultiplier = math.max(0f, cooldownProfile.LeisureRepathMinMultiplier);
            var leisureRepathMaxMultiplier = math.max(0f, cooldownProfile.LeisureRepathMaxMultiplier);
            var baseCrowdingNeighborCap = math.max(0f, cooldownProfile.LeisureCrowdingNeighborCap);
            var baseMoveMultiplier = math.max(0.1f, movementTuning.BaseMoveSpeedMultiplier);
            var varianceAmplitude = math.max(0f, movementTuning.SpeedVarianceAmplitude);
            var variancePeriod = math.max(0f, movementTuning.SpeedVariancePeriodSeconds);
            var separationRadius = math.max(0f, movementTuning.SeparationRadius);
            var separationWeight = math.max(0f, movementTuning.SeparationWeight);
            var separationMaxPush = math.max(0f, movementTuning.SeparationMaxPush);
            var separationCellSize = math.max(0.1f, movementTuning.SeparationCellSize);
            var arriveSlowdownRadius = math.max(0f, movementTuning.ArriveSlowdownRadius);
            var arriveStopRadius = math.max(0f, movementTuning.ArriveStopRadius);
            var arriveMinMultiplier = math.clamp(movementTuning.ArriveMinSpeedMultiplier, 0.1f, 1f);
            var accelMultiplier = math.max(0.1f, movementTuning.AccelerationMultiplier);
            var decelMultiplier = math.max(0.1f, movementTuning.DecelerationMultiplier);
            var turnBlendSpeed = math.max(0.1f, movementTuning.TurnBlendSpeed);

            var hasSettlement = TryGetSettlementRuntime(ref state, out var runtime);
            var deltaTime = SystemAPI.Time.DeltaTime;
            var cooldownProfileEntity = SystemAPI.HasSingleton<VillagerCooldownProfile>()
                ? SystemAPI.GetSingletonEntity<VillagerCooldownProfile>()
                : Entity.Null;
            var hasOutlookRules = cooldownProfileEntity != Entity.Null && _cooldownOutlookLookup.HasBuffer(cooldownProfileEntity);
            var hasArchetypeRules = cooldownProfileEntity != Entity.Null && _cooldownArchetypeLookup.HasBuffer(cooldownProfileEntity);
            var outlookRules = hasOutlookRules ? _cooldownOutlookLookup[cooldownProfileEntity] : default;
            var archetypeRules = hasArchetypeRules ? _cooldownArchetypeLookup[cooldownProfileEntity] : default;
            var hasPhysicsWorld = SystemAPI.TryGetSingleton<PhysicsWorldSingleton>(out var physicsWorld);
            var sweepSkin = 0.02f;

            var villagerQuery = SystemAPI.QueryBuilder()
                .WithAll<VillagerGoalState, LocalTransform>()
                .WithNone<HandHeldTag, Godgame.Runtime.HandQueuedTag>()
                .Build();
            var villagerEntities = villagerQuery.ToEntityArray(state.WorldUpdateAllocator);
            var villagerTransforms = villagerQuery.ToComponentDataArray<LocalTransform>(state.WorldUpdateAllocator);
            var separationMap = new NativeParallelMultiHashMap<int, int>(math.max(1, villagerEntities.Length * 2), Allocator.Temp);
            BuildSeparationMap(villagerTransforms, separationCellSize, separationMap);

            foreach (var (goal, nav, transform, movementState, moveIntent, movePlan, crowding, entity) in SystemAPI
                         .Query<RefRO<VillagerGoalState>, RefRW<Navigation>, RefRW<LocalTransform>, RefRW<VillagerNeedMovementState>, RefRW<MoveIntent>, RefRW<MovePlan>, RefRW<VillagerCrowdingState>>()
                         .WithAll<VillagerBehavior, VillagerLeisureState>()
                         .WithNone<HandHeldTag, Godgame.Runtime.HandQueuedTag>()
                         .WithEntityAccess())
            {
                var behavior = _behaviorLookup[entity];
                var currentGoal = goal.ValueRO.CurrentGoal;
                if (currentGoal == VillagerGoal.Work)
                {
                    continue;
                }

                if (movementState.ValueRW.LingerSeconds > 0f)
                {
                    movementState.ValueRW.LingerSeconds = math.max(0f, movementState.ValueRW.LingerSeconds - deltaTime);
                    nav.ValueRW.Velocity = float3.zero;
                    movePlan.ValueRW = new MovePlan
                    {
                        Mode = MovePlanMode.Arrive,
                        DesiredVelocity = float3.zero,
                        MaxAccel = 0f,
                        EtaSeconds = 0f
                    };
                    continue;
                }

                var isLeisure = false;
                if (_cooldownLookup.HasComponent(entity))
                {
                    var cooldown = _cooldownLookup[entity];
                    isLeisure = cooldown.EndTick > timeState.Tick &&
                                (currentGoal == VillagerGoal.Idle || currentGoal == VillagerGoal.Socialize);
                }

                var activeMoveSpeedBase = moveSpeedBase;
                var activeWanderRadius = wanderRadius;
                var activeSocialRadius = socialRadius;
                var activeLingerMin = lingerMin;
                var activeLingerMax = lingerMax;
                var activeRepathMin = repathMin;
                var activeRepathMax = repathMax;

                if (isLeisure)
                {
                    activeMoveSpeedBase *= leisureMoveSpeedMultiplier;
                    activeWanderRadius *= leisureWanderMultiplier;
                    activeSocialRadius *= leisureSocialMultiplier;
                    activeLingerMin *= leisureLingerMinMultiplier;
                    activeLingerMax *= leisureLingerMaxMultiplier;
                    activeRepathMin *= leisureRepathMinMultiplier;
                    activeRepathMax *= leisureRepathMaxMultiplier;
                }

                activeSocialRadius = math.max(activeSocialRadius, activeWanderRadius);

                var targetPosition = transform.ValueRO.Position;
                Entity targetEntity = Entity.Null;
                if (currentGoal == VillagerGoal.Flee)
                {
                    targetPosition = nav.ValueRO.Destination;
                    targetEntity = Entity.Null;
                    movementState.ValueRW.AnchorOffset = float3.zero;
                    movementState.ValueRW.NextRepathTick = timeState.Tick;
                }
                else
                {
                    var resolvedTarget = false;
                    if (isLeisure && currentGoal == VillagerGoal.Idle && _leisureLookup.HasComponent(entity))
                    {
                        var leisure = _leisureLookup[entity];
                        targetEntity = leisure.ActionTarget;
                        resolvedTarget = TryGetTargetPosition(targetEntity, ref state, out targetPosition);
                        if (!resolvedTarget)
                        {
                            targetEntity = Entity.Null;
                        }
                    }

                    if (!resolvedTarget && !TryResolveNeedTarget(ref state, currentGoal, hasSettlement, runtime, transform.ValueRO.Position,
                            entity, out targetEntity, out targetPosition))
                    {
                        nav.ValueRW.Velocity = float3.zero;
                        movePlan.ValueRW = new MovePlan
                        {
                            Mode = MovePlanMode.Arrive,
                            DesiredVelocity = float3.zero,
                            MaxAccel = 0f,
                            EtaSeconds = 0f
                        };
                        continue;
                    }
                }

                var patience01 = math.saturate((behavior.PatienceScore + 100f) * 0.005f);
                var arrivalDistance = 0.6f;
                var useSmoothing = (nav.ValueRO.FeatureFlags & NavigationFeatureFlags.LocomotionSmoothing) != 0;
                var radius = currentGoal == VillagerGoal.Socialize ? activeSocialRadius : activeWanderRadius;
                if (currentGoal != VillagerGoal.Flee
                    && (timeState.Tick >= movementState.ValueRO.NextRepathTick
                        || math.lengthsq(movementState.ValueRO.AnchorOffset) <= 1e-4f))
                {
                    movementState.ValueRW.AnchorOffset = BuildWanderOffset(entity, timeState.Tick, radius);
                    movementState.ValueRW.NextRepathTick = timeState.Tick + ResolveRepathTicks(activeRepathMin, activeRepathMax, patience01, timeState.FixedDeltaTime, entity);
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
                var moveSpeed = math.max(0.05f, activeMoveSpeedBase * speedScale * baseMoveMultiplier);
                moveSpeed = ApplySpeedVariance(entity, moveSpeed, timeState.WorldSeconds, varianceAmplitude, variancePeriod);
                nav.ValueRW.Speed = moveSpeed;

                var toTarget = destination - transform.ValueRO.Position;
                toTarget.y = 0f;
                var distance = math.length(toTarget);
                if (distance <= arrivalDistance)
                {
                    if (currentGoal != VillagerGoal.Flee)
                    {
                        movementState.ValueRW.LingerSeconds = ResolveLingerSeconds(activeLingerMin, activeLingerMax, patience01, currentGoal, entity);
                    }
                    nav.ValueRW.Velocity = float3.zero;
                    movePlan.ValueRW = new MovePlan
                    {
                        Mode = MovePlanMode.Arrive,
                        DesiredVelocity = float3.zero,
                        MaxAccel = 0f,
                        EtaSeconds = 0f
                    };
                    continue;
                }

                var dir = toTarget / math.max(distance, 1e-4f);
                var separation = ResolveSeparation(entity, transform.ValueRO.Position, villagerEntities, villagerTransforms,
                    separationMap, separationCellSize, separationRadius, separationWeight, separationMaxPush, out var neighborCount);
                var crowdingNeighborCap = baseCrowdingNeighborCap
                                          * ResolveOutlookCrowdingNeighborCapScale(entity, hasOutlookRules, outlookRules, _outlookLookup)
                                          * ResolveArchetypeCrowdingNeighborCapScale(entity, hasArchetypeRules, archetypeRules, _archetypeLookup);
                crowding.ValueRW.Pressure = ResolveCrowdingPressure(neighborCount, crowdingNeighborCap);
                crowding.ValueRW.LastSampleTick = tick;
                dir = math.normalizesafe(dir + separation);
                var currentVelocity = nav.ValueRO.Velocity;
                currentVelocity.y = 0f;
                if (math.lengthsq(currentVelocity) > 1e-4f)
                {
                    var currentDir = math.normalizesafe(currentVelocity);
                    var turnLerp = math.saturate(deltaTime * turnBlendSpeed);
                    dir = math.normalizesafe(math.lerp(currentDir, dir, turnLerp), dir);
                }
                var arriveSpeed = ApplyArriveSlowdown(moveSpeed, distance, arrivalDistance, arriveSlowdownRadius,
                    arriveStopRadius, arriveMinMultiplier, useSmoothing);
                var desiredVelocity = dir * arriveSpeed;
                var currentSpeed = math.length(currentVelocity);
                var acceleration = math.max(0.1f, moveSpeed * accelMultiplier);
                var deceleration = math.max(0.1f, moveSpeed * decelMultiplier);
                var accelLimit = math.length(desiredVelocity) > currentSpeed ? acceleration : deceleration;
                var maxDelta = accelLimit * deltaTime;
                var deltaV = desiredVelocity - currentVelocity;
                var deltaSq = math.lengthsq(deltaV);
                if (maxDelta > 0f && deltaSq > maxDelta * maxDelta)
                {
                    deltaV = math.normalizesafe(deltaV) * maxDelta;
                }
                currentVelocity += deltaV;
                nav.ValueRW.Velocity = currentVelocity;
                var desiredDelta = currentVelocity * deltaTime;
                var resolvedDelta = desiredDelta;
                if (hasPhysicsWorld && _physicsColliderLookup.HasComponent(entity))
                {
                    var collider = _physicsColliderLookup[entity];
                    if (KinematicSweepUtility.TryResolveSweep(
                        physicsWorld,
                        collider,
                        entity,
                        transform.ValueRO.Position,
                        transform.ValueRO.Rotation,
                        desiredDelta,
                        sweepSkin,
                        true,
                        true,
                        out var sweepResult))
                    {
                        var hitEntity = sweepResult.HitEntity;
                        if (sweepResult.HasHit != 0 && IsNonBlockingHit(hitEntity))
                        {
                            resolvedDelta = desiredDelta;
                        }
                        else
                        {
                            resolvedDelta = sweepResult.ResolvedDelta;
                        }
                    }
                }
                transform.ValueRW.Position += resolvedDelta;
                if (deltaTime > 1e-4f)
                {
                    nav.ValueRW.Velocity = resolvedDelta / deltaTime;
                }
                movePlan.ValueRW = new MovePlan
                {
                    Mode = MovePlanMode.Approach,
                    DesiredVelocity = desiredVelocity,
                    MaxAccel = accelLimit,
                    EtaSeconds = arriveSpeed > 0f ? distance / arriveSpeed : 0f
                };
            }

            separationMap.Dispose();
        }

        private bool IsNonBlockingHit(Entity hitEntity)
        {
            if (_colliderSpecLookup.HasComponent(hitEntity))
            {
                var spec = _colliderSpecLookup[hitEntity];
                if (spec.IsTrigger != 0)
                {
                    return true;
                }
            }

            if (_physicsBodyLookup.HasComponent(hitEntity))
            {
                var body = _physicsBodyLookup[hitEntity];
                if ((body.Flags & GodgamePhysicsFlags.IsTrigger) != 0 ||
                    (body.Flags & GodgamePhysicsFlags.SoftAvoidance) != 0)
                {
                    return true;
                }

                if (GodgamePhysicsLayers.IsTriggerLayer(body.Layer) ||
                    GodgamePhysicsLayers.UsesSoftAvoidance(body.Layer))
                {
                    return true;
                }
            }

            return false;
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

        private static float ApplyArriveSlowdown(float speed, float distance, float arrivalDistance, float slowRadius,
            float stopRadius, float minMultiplier, bool useSmoothing)
        {
            if (slowRadius <= 0f || speed <= 0f)
            {
                return speed;
            }

            if (!useSmoothing)
            {
                var t = math.saturate(distance / slowRadius);
                var scale = math.lerp(minMultiplier, 1f, t);
                return speed * scale;
            }

            var clampedStop = math.max(0f, stopRadius);
            if (clampedStop <= 0f)
            {
                clampedStop = math.max(0f, arrivalDistance);
            }
            if (arrivalDistance > 0f)
            {
                clampedStop = math.min(clampedStop, arrivalDistance);
            }
            slowRadius = math.max(slowRadius, clampedStop);

            if (distance <= clampedStop)
            {
                return 0f;
            }

            var t = math.saturate((distance - clampedStop) / math.max(1e-4f, slowRadius - clampedStop));
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
            NativeParallelMultiHashMap<int, int> map, float cellSize, float radius, float weight, float maxPush, out int neighborCount)
        {
            neighborCount = 0;
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
                        neighborCount++;
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

        private static float ResolveCrowdingPressure(int neighborCount, float neighborCap)
        {
            if (neighborCap <= 0f || neighborCount <= 0)
            {
                return 0f;
            }

            return math.saturate(neighborCount / math.max(1f, neighborCap));
        }

        private static float ResolveOutlookCrowdingNeighborCapScale(Entity entity, bool hasRules,
            DynamicBuffer<VillagerCooldownOutlookRule> rules, ComponentLookup<VillagerOutlook> outlookLookup)
        {
            if (!hasRules || !outlookLookup.HasComponent(entity))
            {
                return 1f;
            }

            var outlook = outlookLookup[entity];
            var slotCount = math.min(outlook.OutlookTypes.Length, outlook.OutlookValues.Length);
            var scale = 1f;
            for (var i = 0; i < slotCount; i++)
            {
                var typeId = outlook.OutlookTypes[i];
                if (!TryGetOutlookRule(typeId, rules, out var rule))
                {
                    continue;
                }

                var value01 = math.abs(outlook.OutlookValues[i]) / 100f;
                var target = rule.CrowdingNeighborCapScale <= 0f ? 1f : rule.CrowdingNeighborCapScale;
                scale *= math.lerp(1f, target, value01);
            }

            return math.max(0.1f, scale);
        }

        private static float ResolveArchetypeCrowdingNeighborCapScale(Entity entity, bool hasRules,
            DynamicBuffer<VillagerCooldownArchetypeModifier> modifiers, ComponentLookup<VillagerArchetypeResolved> archetypeLookup)
        {
            if (!hasRules || !archetypeLookup.HasComponent(entity))
            {
                return 1f;
            }

            var data = archetypeLookup[entity].Data;
            var archetypeName = data.ArchetypeName;
            if (archetypeName.IsEmpty)
            {
                return 1f;
            }

            if (!TryGetArchetypeModifier(archetypeName, modifiers, out var modifier))
            {
                return 1f;
            }

            return modifier.CrowdingNeighborCapScale <= 0f ? 1f : modifier.CrowdingNeighborCapScale;
        }

        private static bool TryGetOutlookRule(byte outlookType, in DynamicBuffer<VillagerCooldownOutlookRule> rules,
            out VillagerCooldownOutlookRule rule)
        {
            for (var i = 0; i < rules.Length; i++)
            {
                if (rules[i].OutlookType == outlookType)
                {
                    rule = rules[i];
                    return true;
                }
            }

            rule = default;
            return false;
        }

        private static bool TryGetArchetypeModifier(in FixedString64Bytes archetypeName,
            in DynamicBuffer<VillagerCooldownArchetypeModifier> modifiers, out VillagerCooldownArchetypeModifier modifier)
        {
            for (var i = 0; i < modifiers.Length; i++)
            {
                if (modifiers[i].ArchetypeName.Equals(archetypeName))
                {
                    modifier = modifiers[i];
                    return true;
                }
            }

            modifier = default;
            return false;
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
                        targetEntity = runtime.VillageCenterInstance != Entity.Null
                            ? runtime.VillageCenterInstance
                            : runtime.StorehouseInstance;
                        return TryGetTargetPosition(targetEntity, ref state, out targetPosition)
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

                        targetEntity = runtime.VillageCenterInstance != Entity.Null
                            ? runtime.VillageCenterInstance
                            : runtime.StorehouseInstance;
                        return TryGetTargetPosition(targetEntity, ref state, out targetPosition)
                               || UseFallbackAnchor(origin, out targetPosition);
                }
            }

            if (goal == VillagerGoal.Idle)
            {
                return UseFallbackAnchor(origin, out targetPosition);
            }

            return false;
        }

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
