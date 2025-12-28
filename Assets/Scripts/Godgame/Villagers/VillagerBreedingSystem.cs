using Godgame.Relations;
using Godgame.Villages;
using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Villagers
{
    /// <summary>
    /// Handles villager conception checks and pregnancy starts.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(VillagerLifecycleSystem))]
    public partial struct VillagerBreedingSystem : ISystem
    {
        private ComponentLookup<VillagerReproductionState> _reproductionLookup;
        private ComponentLookup<VillagerLifecycleState> _lifecycleLookup;
        private ComponentLookup<LocalTransform> _transformLookup;
        private BufferLookup<EntityRelation> _relationLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<VillagerReproductionState>();
            state.RequireForUpdate<VillageMember>();
            _reproductionLookup = state.GetComponentLookup<VillagerReproductionState>(false);
            _lifecycleLookup = state.GetComponentLookup<VillagerLifecycleState>(true);
            _transformLookup = state.GetComponentLookup<LocalTransform>(true);
            _relationLookup = state.GetBufferLookup<EntityRelation>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            if (timeState.IsPaused)
            {
                return;
            }

            if (SystemAPI.TryGetSingleton<RewindState>(out var rewind) && rewind.Mode != RewindMode.Record)
            {
                return;
            }

            var tuning = SystemAPI.HasSingleton<VillagerLifecycleTuning>()
                ? SystemAPI.GetSingleton<VillagerLifecycleTuning>()
                : new VillagerLifecycleTuning
                {
                    SecondsPerDay = 480f,
                    FertilityStartDays = 18f,
                    FertilityEndDays = 70f,
                    BreedingCooldownDays = 1f,
                    BreedingChancePerDay = 0.15f,
                    BreedingDistance = 3f,
                    BreedingCadenceTicks = 30,
                    MinRelationTier = RelationTier.Friendly
                };

            var cadenceTicks = math.max(1, tuning.BreedingCadenceTicks);
            if (!CadenceGate.ShouldRun(timeState.Tick, cadenceTicks))
            {
                return;
            }

            var secondsPerDay = math.max(1f, tuning.SecondsPerDay);
            var secondsPerTick = math.max(1e-4f, timeState.FixedDeltaTime);
            var daysPerUpdate = (cadenceTicks * secondsPerTick) / secondsPerDay;
            var baseChance = math.clamp(tuning.BreedingChancePerDay, 0f, 1f);
            var chancePerUpdate = 1f - math.pow(1f - baseChance, math.max(0f, daysPerUpdate));

            var ticksPerDay = math.max(1u, (uint)math.ceil(secondsPerDay / secondsPerTick));
            var cooldownTicks = (uint)math.ceil(math.max(0f, tuning.BreedingCooldownDays) * ticksPerDay);
            var distanceSq = math.max(0.1f, tuning.BreedingDistance);
            distanceSq *= distanceSq;

            _reproductionLookup.Update(ref state);
            _lifecycleLookup.Update(ref state);
            _transformLookup.Update(ref state);
            _relationLookup.Update(ref state);

            foreach (var (members, villageEntity) in SystemAPI.Query<DynamicBuffer<VillageMember>>().WithEntityAccess())
            {
                var memberCount = members.Length;
                if (memberCount < 2)
                {
                    continue;
                }

                var seed = math.hash(new uint2((uint)(villageEntity.Index + 1), timeState.Tick + 13u));
                var random = Unity.Mathematics.Random.CreateFromIndex(seed == 0u ? 1u : seed);

                for (int i = 0; i < memberCount; i++)
                {
                    var villager = members[i].VillagerEntity;
                    if (villager == Entity.Null)
                    {
                        continue;
                    }

                    if (!_reproductionLookup.HasComponent(villager) ||
                        !_lifecycleLookup.HasComponent(villager) ||
                        !_transformLookup.HasComponent(villager))
                    {
                        continue;
                    }

                    var reproduction = _reproductionLookup[villager];
                    if (reproduction.IsPregnant != 0 || reproduction.Sex == VillagerSex.None)
                    {
                        continue;
                    }

                    if (timeState.Tick < reproduction.NextConceptionTick)
                    {
                        continue;
                    }

                    var lifecycle = _lifecycleLookup[villager];
                    if (!IsFertile(in lifecycle, in reproduction, in tuning))
                    {
                        continue;
                    }

                    var attempts = math.min(4, memberCount);
                    var position = _transformLookup[villager].Position;

                    for (int attempt = 0; attempt < attempts; attempt++)
                    {
                        var partner = members[random.NextInt(0, memberCount)].VillagerEntity;
                        if (partner == Entity.Null || partner == villager)
                        {
                            continue;
                        }

                        if (!_reproductionLookup.HasComponent(partner) ||
                            !_lifecycleLookup.HasComponent(partner) ||
                            !_transformLookup.HasComponent(partner))
                        {
                            continue;
                        }

                        var partnerReproduction = _reproductionLookup[partner];
                        if (partnerReproduction.IsPregnant != 0 || partnerReproduction.Sex == VillagerSex.None)
                        {
                            continue;
                        }

                        if (timeState.Tick < partnerReproduction.NextConceptionTick)
                        {
                            continue;
                        }

                        var partnerLifecycle = _lifecycleLookup[partner];
                        if (!IsFertile(in partnerLifecycle, in partnerReproduction, in tuning))
                        {
                            continue;
                        }

                        if (!TryResolveMother(reproduction.Sex, partnerReproduction.Sex, out var selfIsMother))
                        {
                            continue;
                        }

                        var partnerPos = _transformLookup[partner].Position;
                        if (math.distancesq(position, partnerPos) > distanceSq)
                        {
                            continue;
                        }

                        if (!IsRelationAllowed(villager, partner, tuning.MinRelationTier, in _relationLookup))
                        {
                            continue;
                        }

                        var fertilityFactor = math.clamp(reproduction.Fertility, 0f, 1f) * math.clamp(partnerReproduction.Fertility, 0f, 1f);
                        var chance = chancePerUpdate * fertilityFactor;
                        if (chance <= 0f || random.NextFloat() > chance)
                        {
                            continue;
                        }

                        if (selfIsMother)
                        {
                            reproduction.IsPregnant = 1;
                            reproduction.PregnancyDays = 0f;
                            reproduction.ConceptionTick = timeState.Tick;
                            reproduction.Partner = partner;
                            reproduction.NextConceptionTick = timeState.Tick + cooldownTicks;
                            partnerReproduction.NextConceptionTick = timeState.Tick + cooldownTicks;
                        }
                        else
                        {
                            partnerReproduction.IsPregnant = 1;
                            partnerReproduction.PregnancyDays = 0f;
                            partnerReproduction.ConceptionTick = timeState.Tick;
                            partnerReproduction.Partner = villager;
                            partnerReproduction.NextConceptionTick = timeState.Tick + cooldownTicks;
                            reproduction.NextConceptionTick = timeState.Tick + cooldownTicks;
                        }

                        _reproductionLookup[villager] = reproduction;
                        _reproductionLookup[partner] = partnerReproduction;
                        break;
                    }
                }
            }
        }

        private static bool IsFertile(in VillagerLifecycleState lifecycle, in VillagerReproductionState reproduction, in VillagerLifecycleTuning tuning)
        {
            if (lifecycle.Stage < VillagerLifeStage.Adult)
            {
                return false;
            }

            if (reproduction.Fertility <= 0f)
            {
                return false;
            }

            if (lifecycle.AgeDays < tuning.FertilityStartDays || lifecycle.AgeDays > tuning.FertilityEndDays)
            {
                return false;
            }

            return true;
        }

        private static bool TryResolveMother(VillagerSex self, VillagerSex other, out bool selfIsMother)
        {
            if (self == VillagerSex.Female && other == VillagerSex.Male)
            {
                selfIsMother = true;
                return true;
            }

            if (self == VillagerSex.Male && other == VillagerSex.Female)
            {
                selfIsMother = false;
                return true;
            }

            selfIsMother = false;
            return false;
        }

        private static bool IsRelationAllowed(Entity self, Entity other, RelationTier minTier, in BufferLookup<EntityRelation> relationLookup)
        {
            if (minTier <= RelationTier.Neutral)
            {
                return true;
            }

            RelationTier currentTier = RelationTier.Neutral;
            if (relationLookup.HasBuffer(self))
            {
                var relations = relationLookup[self];
                for (int i = 0; i < relations.Length; i++)
                {
                    if (relations[i].OtherEntity == other)
                    {
                        currentTier = relations[i].Tier;
                        break;
                    }
                }
            }

            return currentTier >= minTier;
        }
    }
}
