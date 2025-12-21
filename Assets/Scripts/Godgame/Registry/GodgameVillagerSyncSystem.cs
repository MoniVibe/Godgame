using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Registry;
using PureDOTS.Runtime.Spatial;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Registry
{
    /// <summary>
    /// Syncs Godgame villager entities to the shared PureDOTS villager registry so telemetry & HUD flows
    /// can discover them while we exercise the gathering loop.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(GodgameRegistryCoordinatorSystem))]
    public partial struct GodgameVillagerSyncSystem : ISystem
    {
        private EntityQuery _villagerQuery;
        private ComponentLookup<VillagerAvailability> _availabilityLookup;
        private ComponentLookup<VillagerJobTicket> _ticketLookup;
        private ComponentLookup<VillagerNeeds> _needsLookup;
        private ComponentLookup<VillagerAIState> _aiLookup;
        private ComponentLookup<VillagerCombatStats> _combatLookup;
        private ComponentLookup<SpatialGridResidency> _residencyLookup;

        public void OnCreate(ref SystemState state)
        {
            _villagerQuery = SystemAPI.QueryBuilder()
                .WithAll<VillagerId, VillagerJob, LocalTransform>()
                .WithNone<PlaybackGuardTag>()
                .Build();

            _availabilityLookup = state.GetComponentLookup<VillagerAvailability>(true);
            _ticketLookup = state.GetComponentLookup<VillagerJobTicket>(true);
            _needsLookup = state.GetComponentLookup<VillagerNeeds>(true);
            _aiLookup = state.GetComponentLookup<VillagerAIState>(true);
            _combatLookup = state.GetComponentLookup<VillagerCombatStats>(true);
            _residencyLookup = state.GetComponentLookup<SpatialGridResidency>(true);

            state.RequireForUpdate<VillagerRegistry>();
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            var rewindState = SystemAPI.GetSingleton<RewindState>();
            if (timeState.IsPaused || rewindState.Mode != RewindMode.Record)
            {
                return;
            }

            var registryEntity = SystemAPI.GetSingletonEntity<VillagerRegistry>();

            if (_villagerQuery.IsEmptyIgnoreFilter)
            {
                ClearRegistry(ref state, registryEntity, timeState.Tick);
                return;
            }

            state.EntityManager.CompleteDependencyBeforeRO<VillagerAIState>();
            _availabilityLookup.Update(ref state);
            _ticketLookup.Update(ref state);
            _needsLookup.Update(ref state);
            _aiLookup.Update(ref state);
            _combatLookup.Update(ref state);
            _residencyLookup.Update(ref state);

            var registry = SystemAPI.GetComponentRW<VillagerRegistry>(registryEntity);
            var entries = state.EntityManager.GetBuffer<VillagerRegistryEntry>(registryEntity);
            ref var registryMetadata = ref SystemAPI.GetComponentRW<RegistryMetadata>(registryEntity).ValueRW;

            var hasSpatialConfig = SystemAPI.TryGetSingleton(out SpatialGridConfig gridConfig);
            var hasSpatialState = SystemAPI.TryGetSingleton(out SpatialGridState gridState);
            var hasSpatial = hasSpatialConfig
                             && hasSpatialState
                             && gridConfig.CellCount > 0
                             && gridConfig.CellSize > 0f;
            var hasSyncState = SystemAPI.TryGetSingleton(out RegistrySpatialSyncState syncState);

            var requireSpatialSync = registryMetadata.SupportsSpatialQueries && hasSyncState && syncState.HasSpatialData;
            var spatialVersionSource = hasSpatial
                ? gridState.Version
                : (requireSpatialSync ? syncState.SpatialVersion : 0u);

            var expectedCount = math.max(32, _villagerQuery.CalculateEntityCount());
            using var builder = new DeterministicRegistryBuilder<VillagerRegistryEntry>(expectedCount, Allocator.Temp);

            var totalVillagers = 0;
            var availableCount = 0;
            var idleCount = 0;
            var reservedCount = 0;
            var combatReadyCount = 0;
            var healthAccumulator = 0f;
            var energyAccumulator = 0f;
            var moraleAccumulator = 0f;
            var healthSamples = 0;
            var energySamples = 0;
            var moraleSamples = 0;
            var resolvedCount = 0;
            var fallbackCount = 0;
            var unmappedCount = 0;

            foreach (var (villagerId, job, transform, entity) in SystemAPI.Query<RefRO<VillagerId>, RefRO<VillagerJob>, RefRO<LocalTransform>>()
                         .WithNone<PlaybackGuardTag>()
                         .WithEntityAccess())
            {
                totalVillagers++;

                var availabilityFlags = (byte)0;
                if (_availabilityLookup.HasComponent(entity))
                {
                    var availability = _availabilityLookup[entity];
                    availabilityFlags = VillagerAvailabilityFlags.FromAvailability(availability);
                    if ((availabilityFlags & VillagerAvailabilityFlags.Available) != 0)
                    {
                        availableCount++;
                    }

                    if (availability.IsReserved != 0)
                    {
                        reservedCount++;
                    }
                }

                if (job.ValueRO.Phase == VillagerJob.JobPhase.Idle)
                {
                    idleCount++;
                }

                if (_combatLookup.HasComponent(entity) && _combatLookup[entity].AttackDamage > 0f)
                {
                    combatReadyCount++;
                }

                byte healthPercent = 0;
                byte energyPercent = 0;
                byte moralePercent = 0;
                if (_needsLookup.HasComponent(entity))
                {
                    var needs = _needsLookup[entity];
                    if (needs.MaxHealth > 0f)
                    {
                        var percent = math.saturate(needs.Health / math.max(1e-3f, needs.MaxHealth)) * 100f;
                        healthPercent = (byte)math.round(math.clamp(percent, 0f, 100f));
                        healthAccumulator += percent;
                        healthSamples++;
                    }

                    var energy = math.clamp(needs.Energy * 0.1f, 0f, 100f);
                    energyPercent = (byte)math.round(energy);
                    energyAccumulator += energy;
                    energySamples++;

                    var morale = math.clamp(needs.Morale * 0.1f, 0f, 100f);
                    moralePercent = (byte)math.round(morale);
                    moraleAccumulator += morale;
                    moraleSamples++;
                }

                ushort resourceTypeIndex = ushort.MaxValue;
                var currentTarget = Entity.Null;
                if (_ticketLookup.HasComponent(entity))
                {
                    var ticket = _ticketLookup[entity];
                    resourceTypeIndex = ticket.ResourceTypeIndex;
                    currentTarget = ticket.ResourceEntity;
                }

                byte aiStateValue = 0;
                byte aiGoalValue = 0;
                if (_aiLookup.HasComponent(entity))
                {
                    var aiState = _aiLookup[entity];
                    aiStateValue = (byte)aiState.CurrentState;
                    aiGoalValue = (byte)aiState.CurrentGoal;
                    if (aiState.TargetEntity != Entity.Null)
                    {
                        currentTarget = aiState.TargetEntity;
                    }
                }

                var position = transform.ValueRO.Position;
                var cellId = -1;
                var entrySpatialVersion = spatialVersionSource;

                if (hasSpatial)
                {
                    var resolved = false;
                    var fallback = false;

                    if (_residencyLookup.HasComponent(entity))
                    {
                        var residency = _residencyLookup[entity];
                        if ((uint)residency.CellId < (uint)gridConfig.CellCount && residency.Version == gridState.Version)
                        {
                            cellId = residency.CellId;
                            entrySpatialVersion = residency.Version;
                            resolved = true;
                        }
                    }

                    if (!resolved)
                    {
                        SpatialHash.Quantize(position, gridConfig, out var coords);
                        var fallbackCell = SpatialHash.Flatten(in coords, in gridConfig);
                        if ((uint)fallbackCell < (uint)gridConfig.CellCount)
                        {
                            cellId = fallbackCell;
                            entrySpatialVersion = gridState.Version;
                            fallback = true;
                        }
                        else
                        {
                            cellId = -1;
                            entrySpatialVersion = 0;
                            unmappedCount++;
                        }
                    }

                    if (resolved)
                    {
                        resolvedCount++;
                    }
                    else if (fallback)
                    {
                        fallbackCount++;
                    }
                }

                var entry = new VillagerRegistryEntry
                {
                    VillagerEntity = entity,
                    VillagerId = villagerId.ValueRO.Value,
                    FactionId = villagerId.ValueRO.FactionId,
                    Position = position,
                    CellId = cellId,
                    SpatialVersion = entrySpatialVersion,
                    JobType = job.ValueRO.Type,
                    JobPhase = job.ValueRO.Phase,
                    ActiveTicketId = job.ValueRO.ActiveTicketId,
                    AvailabilityFlags = availabilityFlags,
                    CurrentResourceTypeIndex = resourceTypeIndex,
                    HealthPercent = healthPercent,
                    MoralePercent = moralePercent,
                    EnergyPercent = energyPercent,
                    AIState = aiStateValue,
                    AIGoal = aiGoalValue,
                    CurrentTarget = currentTarget,
                    Productivity = job.ValueRO.Productivity
                };

                builder.Add(in entry);
            }

            var continuity = registryMetadata.SupportsSpatialQueries && (resolvedCount + fallbackCount + unmappedCount > 0)
                ? RegistryContinuitySnapshot.WithSpatialData(spatialVersionSource, resolvedCount, fallbackCount, unmappedCount, requireSpatialSync)
                : registryMetadata.SupportsSpatialQueries
                    ? RegistryContinuitySnapshot.WithoutSpatialData(requireSpatialSync)
                    : default;

            builder.ApplyTo(ref entries, ref registryMetadata, timeState.Tick, continuity);

            var registryValue = registry.ValueRW;
            registryValue.TotalVillagers = totalVillagers;
            registryValue.AvailableVillagers = availableCount;
            registryValue.IdleVillagers = idleCount;
            registryValue.ReservedVillagers = reservedCount;
            registryValue.CombatReadyVillagers = combatReadyCount;
            registryValue.AverageHealthPercent = healthSamples > 0 ? healthAccumulator / healthSamples : 0f;
            registryValue.AverageEnergyPercent = energySamples > 0 ? energyAccumulator / energySamples : 0f;
            registryValue.AverageMoralePercent = moraleSamples > 0 ? moraleAccumulator / moraleSamples : 0f;
            registryValue.LastUpdateTick = timeState.Tick;
            registryValue.LastSpatialVersion = spatialVersionSource;
            registryValue.SpatialResolvedCount = resolvedCount;
            registryValue.SpatialFallbackCount = fallbackCount;
            registryValue.SpatialUnmappedCount = unmappedCount;
            registry.ValueRW = registryValue;
        }

        private void ClearRegistry(ref SystemState state, Entity registryEntity, uint tick)
        {
            var entityManager = state.EntityManager;
            var entries = entityManager.GetBuffer<VillagerRegistryEntry>(registryEntity);
            entries.Clear();

            var metadata = entityManager.GetComponentData<RegistryMetadata>(registryEntity);
            metadata.MarkUpdated(0, tick);
            entityManager.SetComponentData(registryEntity, metadata);

            var registryValue = entityManager.GetComponentData<VillagerRegistry>(registryEntity);
            registryValue = default;
            registryValue.LastUpdateTick = tick;
            entityManager.SetComponentData(registryEntity, registryValue);
        }
    }
}
