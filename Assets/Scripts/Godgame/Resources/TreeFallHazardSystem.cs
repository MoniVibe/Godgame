using Godgame.Villagers;
using Godgame.Villages;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Spatial;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using GVillagerId = Godgame.Villagers.VillagerId;
using GVillagerCombatStats = Godgame.Villagers.VillagerCombatStats;
using GVillagerNeeds = Godgame.Villagers.VillagerNeeds;

namespace Godgame.Resources
{
    /// <summary>
    /// Resolves tree fall hazards, applies damage, and updates safety learning.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(Godgame.Villagers.VillagerJobSystem))]
    public partial struct TreeFallHazardSystem : ISystem
    {
        private ComponentLookup<LocalTransform> _transformLookup;
        private ComponentLookup<GVillagerId> _villagerIdLookup;
        private ComponentLookup<GVillagerCombatStats> _combatLookup;
        private ComponentLookup<GVillagerNeeds> _needsLookup;
        private ComponentLookup<VillagerTreeSafetyMemory> _memoryLookup;
        private ComponentLookup<Village> _villageLookup;
        private ComponentLookup<VillageTreeSafetyMemory> _villageMemoryLookup;
        private ComponentLookup<LocalTransform> _villageTransformLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<TreeFallEventBuffer>();
            _transformLookup = state.GetComponentLookup<LocalTransform>(true);
            _villagerIdLookup = state.GetComponentLookup<GVillagerId>(true);
            _combatLookup = state.GetComponentLookup<GVillagerCombatStats>(false);
            _needsLookup = state.GetComponentLookup<GVillagerNeeds>(false);
            _memoryLookup = state.GetComponentLookup<VillagerTreeSafetyMemory>(false);
            _villageLookup = state.GetComponentLookup<Village>(true);
            _villageMemoryLookup = state.GetComponentLookup<VillageTreeSafetyMemory>(false);
            _villageTransformLookup = state.GetComponentLookup<LocalTransform>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.TryGetSingleton<RewindState>(out var rewindState) && rewindState.Mode != RewindMode.Record)
            {
                return;
            }

            if (!SystemAPI.TryGetSingletonEntity<TreeFallEventBuffer>(out var eventEntity))
            {
                return;
            }

            var events = SystemAPI.GetBuffer<TreeFallEvent>(eventEntity);
            if (events.Length == 0)
            {
                return;
            }

            var timeState = SystemAPI.GetSingleton<TimeState>();
            var tuning = SystemAPI.TryGetSingleton<TreeFellingTuning>(out var tuningValue)
                ? tuningValue
                : TreeFellingTuning.Default;
            var secondsPerTick = math.max(timeState.FixedDeltaTime, 1e-4f);
            var cooldownTicks = tuning.IncidentCooldownSeconds > 0f
                ? (uint)math.ceil(tuning.IncidentCooldownSeconds / secondsPerTick)
                : 0u;

            _transformLookup.Update(ref state);
            _villagerIdLookup.Update(ref state);
            _combatLookup.Update(ref state);
            _needsLookup.Update(ref state);
            _memoryLookup.Update(ref state);
            _villageLookup.Update(ref state);
            _villageMemoryLookup.Update(ref state);
            _villageTransformLookup.Update(ref state);

            var hasSpatial = SystemAPI.TryGetSingleton(out SpatialGridConfig spatialConfig) &&
                             SystemAPI.TryGetSingleton(out SpatialGridState _);
            DynamicBuffer<SpatialGridCellRange> cellRanges = default;
            DynamicBuffer<SpatialGridEntry> gridEntries = default;
            if (hasSpatial)
            {
                var gridEntity = SystemAPI.GetSingletonEntity<SpatialGridConfig>();
                cellRanges = SystemAPI.GetBuffer<SpatialGridCellRange>(gridEntity);
                gridEntries = SystemAPI.GetBuffer<SpatialGridEntry>(gridEntity);
            }

            foreach (var fallEvent in events)
            {
                var baseAwareness = math.max(0.1f, math.max(fallEvent.AwarenessRadius, fallEvent.FallLength));
                var nearMissRadius = baseAwareness * math.max(0.1f, tuning.NearMissRadiusMultiplier);
                var queryRadius = math.max(fallEvent.FallLength, nearMissRadius);
                var maxSeverity = 0f;

                if (hasSpatial)
                {
                    var position = fallEvent.Position;
                    var nearby = new NativeList<Entity>(32, Allocator.Temp);
                    SpatialQueryHelper.GetEntitiesWithinRadius(
                        ref position,
                        queryRadius,
                        spatialConfig,
                        cellRanges,
                        gridEntries,
                        ref nearby);

                    for (int i = 0; i < nearby.Length; i++)
                    {
                        var entity = nearby[i];
                        if (!_villagerIdLookup.HasComponent(entity) || !_transformLookup.HasComponent(entity))
                        {
                            continue;
                        }

                        var pos = _transformLookup[entity].Position;
                        ProcessEntity(entity, pos, fallEvent, nearMissRadius, cooldownTicks, tuning, timeState.Tick, ref maxSeverity);
                    }

                    nearby.Dispose();
                }
                else
                {
                    foreach (var (transform, entity) in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<GVillagerId>().WithEntityAccess())
                    {
                        var pos = transform.ValueRO.Position;
                        if (math.distancesq(pos, fallEvent.Position) > queryRadius * queryRadius)
                        {
                            continue;
                        }

                        ProcessEntity(entity, pos, fallEvent, nearMissRadius, cooldownTicks, tuning, timeState.Tick, ref maxSeverity);
                    }
                }

                if (maxSeverity > 0f)
                {
                    UpdateVillageMemory(fallEvent.Position, maxSeverity, tuning, timeState.Tick, ref state);
                }
            }

            events.Clear();
        }

        private void ProcessEntity(
            Entity entity,
            float3 position,
            in TreeFallEvent fallEvent,
            float nearMissRadius,
            uint cooldownTicks,
            in TreeFellingTuning tuning,
            uint currentTick,
            ref float maxSeverity)
        {
            var toEntity = position - fallEvent.Position;
            toEntity.y = 0f;
            if (math.lengthsq(toEntity) > nearMissRadius * nearMissRadius)
            {
                return;
            }

            if (TryComputeFallDamage(toEntity, fallEvent, out var damage, out var severity))
            {
                ApplyDamage(entity, damage);
                ApplyMemory(entity, severity, true, currentTick, cooldownTicks, tuning);
                if (severity > maxSeverity)
                {
                    maxSeverity = severity;
                }
            }
            else
            {
                var missSeverity = math.saturate(1f - math.length(toEntity) / math.max(0.1f, nearMissRadius));
                ApplyMemory(entity, missSeverity, false, currentTick, cooldownTicks, tuning);
                if (missSeverity > maxSeverity)
                {
                    maxSeverity = missSeverity;
                }
            }
        }

        private static bool TryComputeFallDamage(float3 toEntity, in TreeFallEvent fallEvent, out float damage, out float severity)
        {
            damage = 0f;
            severity = 0f;

            if (fallEvent.BaseDamage <= 0f || fallEvent.FallLength <= 0f || fallEvent.FallWidth <= 0f)
            {
                return false;
            }

            var direction = fallEvent.FallDirection;
            direction.y = 0f;
            direction = math.normalizesafe(direction, new float3(0f, 0f, 1f));

            var longitudinal = math.dot(toEntity, direction);
            if (longitudinal < 0f || longitudinal > fallEvent.FallLength)
            {
                return false;
            }

            var lateral = math.length(toEntity - (direction * longitudinal));
            if (lateral > fallEvent.FallWidth)
            {
                return false;
            }

            var lengthFactor = 1f - (longitudinal / math.max(0.1f, fallEvent.FallLength));
            var widthFactor = 1f - (lateral / math.max(0.05f, fallEvent.FallWidth));
            var impactFactor = math.saturate(lengthFactor * widthFactor);

            damage = fallEvent.BaseDamage * impactFactor;
            severity = math.saturate(impactFactor);
            return damage > 0f;
        }

        private void ApplyDamage(Entity entity, float damage)
        {
            if (damage <= 0f)
            {
                return;
            }

            if (_combatLookup.HasComponent(entity))
            {
                var combat = _combatLookup[entity];
                combat.CurrentHealth = math.max(0f, combat.CurrentHealth - damage);
                _combatLookup[entity] = combat;
            }

            if (_needsLookup.HasComponent(entity))
            {
                var needs = _needsLookup[entity];
                needs.Health = math.max(0f, needs.Health - damage);
                var general = (int)needs.GeneralHealth - (int)math.round(damage);
                needs.GeneralHealth = (byte)math.clamp(general, 0, 100);
                _needsLookup[entity] = needs;
            }
        }

        private void ApplyMemory(
            Entity entity,
            float severity,
            bool wasHit,
            uint currentTick,
            uint cooldownTicks,
            in TreeFellingTuning tuning)
        {
            if (!_memoryLookup.HasComponent(entity))
            {
                return;
            }

            var memory = _memoryLookup[entity];
            if (currentTick < memory.NextIncidentAllowedTick)
            {
                return;
            }

            var gain = wasHit ? tuning.MemoryGainOnHit : tuning.MemoryGainOnNearMiss;
            memory.CautionBias = math.saturate(memory.CautionBias + severity * gain);
            memory.RecentSeverity = math.max(memory.RecentSeverity, severity);
            memory.LastIncidentTick = currentTick;
            if (cooldownTicks > 0u)
            {
                memory.NextIncidentAllowedTick = currentTick + cooldownTicks;
            }

            if (wasHit)
            {
                memory.IncidentCount = (byte)math.min(byte.MaxValue, memory.IncidentCount + 1);
            }
            else
            {
                memory.NearMissCount = (byte)math.min(byte.MaxValue, memory.NearMissCount + 1);
            }

            _memoryLookup[entity] = memory;
        }

        private void UpdateVillageMemory(float3 position, float severity, in TreeFellingTuning tuning, uint currentTick, ref SystemState state)
        {
            var bestVillage = Entity.Null;
            var bestDistSq = float.MaxValue;

            foreach (var (village, entity) in SystemAPI.Query<RefRO<Village>>().WithEntityAccess())
            {
                var center = village.ValueRO.CenterPosition;
                if (_villageTransformLookup.HasComponent(entity))
                {
                    center = _villageTransformLookup[entity].Position;
                }

                var radius = math.max(0.1f, village.ValueRO.InfluenceRadius);
                var distSq = math.distancesq(center, position);
                if (distSq > radius * radius)
                {
                    continue;
                }

                if (distSq < bestDistSq)
                {
                    bestDistSq = distSq;
                    bestVillage = entity;
                }
            }

            if (bestVillage == Entity.Null || !_villageMemoryLookup.HasComponent(bestVillage))
            {
                return;
            }

            var memory = _villageMemoryLookup[bestVillage];
            memory.CautionBias = math.saturate(memory.CautionBias + severity * tuning.VillageMemoryGain);
            memory.RecentSeverity = math.max(memory.RecentSeverity, severity);
            memory.LastIncidentTick = currentTick;
            memory.IncidentCount += 1u;
            _villageMemoryLookup[bestVillage] = memory;
        }
    }
}
