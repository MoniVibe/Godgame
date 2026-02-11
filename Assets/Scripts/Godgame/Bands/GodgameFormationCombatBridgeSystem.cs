using Godgame.Bands;
using PureDOTS.Runtime.Bands;
using PureDOTS.Runtime.Combat;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Formation;
using PureDOTS.Runtime.Time;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using GodgameVillagerCombatStats = Godgame.Villagers.VillagerCombatStats;

namespace Godgame.Bands
{
    /// <summary>
    /// Bridges Godgame BandFormation to PureDOTS FormationState/FormationIntegrity/FormationBonus.
    /// Follows projection pattern: if entity has PureDOTS FormationState, leave alone.
    /// If entity has BandFormation but not FormationState, project/add PureDOTS components.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(PureDOTS.Systems.Combat.FormationCombatSystem))]
    public partial struct GodgameFormationCombatBridgeSystem : ISystem
    {
        private ComponentLookup<LocalTransform> _transformLookup;
        private ComponentLookup<BandCombatStats> _bandCombatLookup;
        private ComponentLookup<GodgameVillagerCombatStats> _villagerCombatLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            _transformLookup = state.GetComponentLookup<LocalTransform>(true);
            _bandCombatLookup = state.GetComponentLookup<BandCombatStats>(false);
            _villagerCombatLookup = state.GetComponentLookup<GodgameVillagerCombatStats>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            var currentTick = timeState.Tick;

            var ecb = SystemAPI
                .GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            _transformLookup.Update(ref state);
            _bandCombatLookup.Update(ref state);
            _villagerCombatLookup.Update(ref state);

            SyncBandCombatStats(ref state);

            // Phase A: Add components and buffers via ECB
            // 1. PROJECT: Query bands with BandFormation but no FormationState
            foreach (var (bandFormation, transform, entity) in SystemAPI
                     .Query<RefRO<BandFormation>, RefRO<LocalTransform>>()
                     .WithNone<FormationState>()
                     .WithEntityAccess())
            {
                if (bandFormation.ValueRO.Formation == BandFormationType.Column ||
                    bandFormation.ValueRO.Formation == BandFormationType.Line ||
                    bandFormation.ValueRO.Formation == BandFormationType.Wedge ||
                    bandFormation.ValueRO.Formation == BandFormationType.Circle ||
                    bandFormation.ValueRO.Formation == BandFormationType.Skirmish ||
                    bandFormation.ValueRO.Formation == BandFormationType.ShieldWall)
                {
                    var hasBandCombat = _bandCombatLookup.HasComponent(entity);
                    var bandCombat = hasBandCombat ? _bandCombatLookup[entity] : default;
                    CreateFormationState(entity, bandFormation.ValueRO, transform.ValueRO, currentTick, hasBandCombat, bandCombat, ref state, ref ecb);
                }
            }

            // 2. UPDATE: Sync FormationState when BandFormation changes
            foreach (var (bandFormation, formationState, transform, entity) in SystemAPI
                     .Query<RefRO<BandFormation>, RefRW<FormationState>, RefRO<LocalTransform>>()
                     .WithChangeFilter<BandFormation>()
                     .WithEntityAccess())
            {
                FormationType mappedType = MapFormationType(bandFormation.ValueRO.Formation);
                if (formationState.ValueRO.Type != mappedType)
                {
                    formationState.ValueRW.Type = mappedType;
                    formationState.ValueRW.Spacing = bandFormation.ValueRO.Spacing;
                    formationState.ValueRW.AnchorPosition = bandFormation.ValueRO.Anchor;
                    formationState.ValueRW.AnchorRotation = quaternion.LookRotationSafe(bandFormation.ValueRO.Facing, math.up());
                    formationState.ValueRW.LastUpdateTick = currentTick;
                }
            }

            // Ensure FormationSlot buffers exist for entities that need them
            foreach (var (bandMembers, entity) in SystemAPI
                     .Query<DynamicBuffer<BandMember>>()
                     .WithAll<FormationState>()
                     .WithEntityAccess())
            {
                if (!state.EntityManager.HasBuffer<FormationSlot>(entity))
                {
                    ecb.AddBuffer<FormationSlot>(entity);
                }
            }

            // Phase B: Populate buffers via EntityManager (after playback)
            // 3. UPDATE: Sync FormationSlot buffer when BandMember buffer changes
            foreach (var (bandMembers, entity) in SystemAPI
                     .Query<DynamicBuffer<BandMember>>()
                     .WithAll<FormationState>()
                     .WithChangeFilter<BandMember>()
                     .WithEntityAccess())
            {
                if (state.EntityManager.HasBuffer<FormationSlot>(entity))
                {
                    var slots = state.EntityManager.GetBuffer<FormationSlot>(entity);
                    UpdateFormationSlots(slots, bandMembers, entity, ref state);
                }
            }

            // 4. PROJECT: Populate FormationSlot buffer if it exists but is empty
            foreach (var (bandMembers, entity) in SystemAPI
                     .Query<DynamicBuffer<BandMember>>()
                     .WithAll<FormationState>()
                     .WithEntityAccess())
            {
                if (state.EntityManager.HasBuffer<FormationSlot>(entity))
                {
                    var slots = state.EntityManager.GetBuffer<FormationSlot>(entity);
                    if (slots.Length == 0 && bandMembers.Length > 0)
                    {
                        UpdateFormationSlots(slots, bandMembers, entity, ref state);
                    }
                }
            }

            // 5. UPDATE: Sync CombatStats from BandCombatStats changes
            foreach (var (bandCombat, combatStats) in SystemAPI
                     .Query<RefRO<BandCombatStats>, RefRW<CombatStats>>()
                     .WithChangeFilter<BandCombatStats>())
            {
                var updated = combatStats.ValueRW;
                ApplyBandCombatStats(ref updated, bandCombat.ValueRO);
                combatStats.ValueRW = updated;
            }

            // 6. UPDATE: Sync morale from BandStats changes
            foreach (var (bandStats, combatStats) in SystemAPI
                     .Query<RefRO<BandStats>, RefRW<CombatStats>>()
                     .WithChangeFilter<BandStats>())
            {
                var updated = combatStats.ValueRW;
                updated.Morale = ClampStatByte(bandStats.ValueRO.Morale * 100f, updated.Morale);
                combatStats.ValueRW = updated;
            }
        }

        private void SyncBandCombatStats(ref SystemState state)
        {
            using var missing = new NativeList<PendingBandCombatStat>(Allocator.Temp);
            foreach (var (bandMembers, entity) in SystemAPI.Query<DynamicBuffer<BandMember>>().WithEntityAccess())
            {
                var next = ComputeBandCombatStats(bandMembers);
                if (_bandCombatLookup.HasComponent(entity))
                {
                    var current = _bandCombatLookup[entity];
                    if (!ApproximatelyEqual(current, next))
                    {
                        _bandCombatLookup[entity] = next;
                    }
                }
                else
                {
                    missing.Add(new PendingBandCombatStat
                    {
                        Entity = entity,
                        Stats = next
                    });
                }
            }

            for (var i = 0; i < missing.Length; i++)
            {
                state.EntityManager.AddComponentData(missing[i].Entity, missing[i].Stats);
            }
        }

        private BandCombatStats ComputeBandCombatStats(DynamicBuffer<BandMember> members)
        {
            var sumAttackDamage = 0f;
            var sumAttackSpeed = 0f;
            var sampleCount = 0;

            for (var i = 0; i < members.Length; i++)
            {
                var villager = members[i].Villager;
                if (!_villagerCombatLookup.HasComponent(villager))
                {
                    continue;
                }

                var villagerCombat = _villagerCombatLookup[villager];
                if (villagerCombat.AttackDamage <= 0f && villagerCombat.AttackSpeed <= 0f)
                {
                    continue;
                }

                sumAttackDamage += math.max(0f, villagerCombat.AttackDamage);
                sumAttackSpeed += math.max(0f, villagerCombat.AttackSpeed);
                sampleCount++;
            }

            if (sampleCount <= 0)
            {
                return default;
            }

            return new BandCombatStats
            {
                AverageAttackDamage = sumAttackDamage / sampleCount,
                AverageAttackSpeed = sumAttackSpeed / sampleCount,
                SampleCount = (byte)math.min(sampleCount, 255)
            };
        }

        private static bool ApproximatelyEqual(in BandCombatStats left, in BandCombatStats right)
        {
            if (left.SampleCount != right.SampleCount)
            {
                return false;
            }

            return math.abs(left.AverageAttackDamage - right.AverageAttackDamage) < 0.001f
                && math.abs(left.AverageAttackSpeed - right.AverageAttackSpeed) < 0.001f;
        }

        private struct PendingBandCombatStat
        {
            public Entity Entity;
            public BandCombatStats Stats;
        }

        private static void CreateFormationState(
            Entity entity,
            BandFormation bandFormation,
            LocalTransform transform,
            uint currentTick,
            bool hasBandCombat,
            BandCombatStats bandCombatStats,
            ref SystemState state,
            ref EntityCommandBuffer ecb)
        {
            FormationType mappedType = MapFormationType(bandFormation.Formation);
            
            ecb.AddComponent(entity, new FormationState
            {
                Type = mappedType,
                AnchorPosition = bandFormation.Anchor,
                AnchorRotation = quaternion.LookRotationSafe(bandFormation.Facing, math.up()),
                Spacing = bandFormation.Spacing,
                Scale = 1f,
                MaxSlots = 20,
                FilledSlots = 0,
                IsMoving = false,
                LastUpdateTick = currentTick
            });

            ecb.AddComponent(entity, new FormationIntegrity
            {
                IntegrityPercent = 1f,
                LastCalculatedTick = currentTick,
                MembersInPosition = 0,
                TotalMembers = 0
            });

            // Ensure SquadCohesion component exists
            if (!state.EntityManager.HasComponent<SquadCohesion>(entity))
            {
                ecb.AddComponent(entity, new SquadCohesion
                {
                    CohesionLevel = 0.7f,
                    Threshold = CohesionThreshold.Cohesive,
                    LastUpdatedTick = currentTick,
                    DegradationRate = 0.1f,
                    RegenRate = 0.05f
                });
            }

            // Ensure CombatStats component exists (for morale wave application)
            if (!state.EntityManager.HasComponent<CombatStats>(entity))
            {
                // Derive Morale from BandStats if available, otherwise use default
                byte morale = 50; // Default placeholder
                if (state.EntityManager.HasComponent<BandStats>(entity))
                {
                    var bandStats = state.EntityManager.GetComponentData<BandStats>(entity);
                    morale = (byte)math.clamp((int)(bandStats.Morale * 100f), 0, 100);
                }

                byte attackDamage = 10;
                byte attackSpeed = 50;
                if (hasBandCombat && bandCombatStats.SampleCount > 0)
                {
                    attackDamage = ClampStatByte(bandCombatStats.AverageAttackDamage, attackDamage);
                    attackSpeed = ClampStatByte(bandCombatStats.AverageAttackSpeed, attackSpeed);
                }

                ecb.AddComponent(entity, new CombatStats
                {
                    Attack = 50, // Placeholder - TODO: Aggregate from band members
                    Defense = 50, // Placeholder - TODO: Aggregate from band members
                    Morale = morale, // Derived from BandStats.Morale if available
                    AttackSpeed = attackSpeed,
                    AttackDamage = attackDamage,
                    Accuracy = 50, // Placeholder - TODO: Aggregate from band members
                    CriticalChance = 5, // Placeholder - TODO: Aggregate from band members
                    Health = 100, // Placeholder - TODO: Aggregate from band members
                    CurrentHealth = 100, // Placeholder
                    Stamina = 10, // Placeholder
                    CurrentStamina = 10, // Placeholder
                    SpellPower = 0, // Placeholder
                    ManaPool = 0, // Placeholder
                    CurrentMana = 0, // Placeholder
                    EquippedWeapon = Entity.Null,
                    EquippedArmor = Entity.Null,
                    EquippedShield = Entity.Null,
                    CombatExperience = 0,
                    IsInCombat = false,
                    CurrentOpponent = Entity.Null
                });
            }

            // FormationSlot buffer will be populated in Phase B after ECB playback
        }

        private static void ApplyBandCombatStats(ref CombatStats combatStats, BandCombatStats bandCombatStats)
        {
            if (bandCombatStats.SampleCount <= 0)
            {
                return;
            }

            combatStats.AttackDamage = ClampStatByte(bandCombatStats.AverageAttackDamage, combatStats.AttackDamage);
            combatStats.AttackSpeed = ClampStatByte(bandCombatStats.AverageAttackSpeed, combatStats.AttackSpeed);
        }

        private static byte ClampStatByte(float value, byte fallback)
        {
            if (value <= 0f)
            {
                return fallback;
            }

            var rounded = (int)math.round(value);
            return (byte)math.clamp(rounded, 0, 100);
        }

        private static void UpdateFormationSlots(
            DynamicBuffer<FormationSlot> slots,
            DynamicBuffer<BandMember> bandMembers,
            Entity entity,
            ref SystemState state)
        {
            slots.Clear();

            // Defensive check: verify entity still exists
            if (!state.EntityManager.Exists(entity))
            {
                return;
            }

            // Defensive check: handle empty band members
            if (bandMembers.Length == 0)
            {
                // Update FormationState filled slots count to 0
                if (state.EntityManager.HasComponent<FormationState>(entity))
                {
                    var formationState = state.EntityManager.GetComponentData<FormationState>(entity);
                    formationState.FilledSlots = 0;
                    state.EntityManager.SetComponentData(entity, formationState);
                }
                return;
            }

            // Bounds check: max 20 slots per formation
            int maxSlots = math.min(bandMembers.Length, 20);
            for (int i = 0; i < maxSlots; i++)
            {
                var member = bandMembers[i];
                
                // Defensive check: verify member entity still exists
                if (!state.EntityManager.Exists(member.Villager))
                {
                    continue;
                }

                FormationSlotRole role = MapBandRoleToFormationRole(member.Role);
                
                slots.Add(new FormationSlot
                {
                    SlotIndex = (byte)i,
                    LocalOffset = float3.zero, // Will be calculated by FormationPositionSystem
                    Role = role,
                    AssignedEntity = member.Villager,
                    Priority = (byte)i,
                    IsRequired = (member.Role == BandRole.Leader)
                });
            }

            // Update FormationState filled slots count
            if (state.EntityManager.Exists(entity) && state.EntityManager.HasComponent<FormationState>(entity))
            {
                var formationState = state.EntityManager.GetComponentData<FormationState>(entity);
                formationState.FilledSlots = (byte)maxSlots;
                state.EntityManager.SetComponentData(entity, formationState);
            }
        }

        private static FormationType MapFormationType(BandFormationType formation)
        {
            return formation switch
            {
                BandFormationType.Column => FormationType.Column,
                BandFormationType.Line => FormationType.Line,
                BandFormationType.Wedge => FormationType.Wedge,
                BandFormationType.Circle => FormationType.Circle,
                BandFormationType.Skirmish => FormationType.Skirmish,
                BandFormationType.ShieldWall => FormationType.Phalanx,
                _ => FormationType.None
            };
        }

        private static FormationSlotRole MapBandRoleToFormationRole(BandRole role)
        {
            return role switch
            {
                BandRole.Leader => FormationSlotRole.Leader,
                BandRole.Regular => FormationSlotRole.Front,
                BandRole.StandardBearer => FormationSlotRole.Front,
                BandRole.Scout => FormationSlotRole.Scout,
                BandRole.Healer => FormationSlotRole.Support,
                BandRole.Quartermaster => FormationSlotRole.Support,
                BandRole.Specialist => FormationSlotRole.Support,
                _ => FormationSlotRole.Any
            };
        }
    }
}
