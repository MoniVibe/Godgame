using Godgame.Scenario;
using Godgame.Villages;
using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Villagers
{
    /// <summary>
    /// Lets work urgency cool down when villagers are off-duty.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(VillagerNeedSatisfactionSystem))]
    public partial struct VillagerWorkRecoverySystem : ISystem
    {
        private ComponentLookup<VillagerScheduleProfile> _villagerScheduleLookup;
        private ComponentLookup<VillagerAlignment> _alignmentLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<VillagerNeedState>();
            state.RequireForUpdate<VillagerGoalState>();
            state.RequireForUpdate<VillagerScheduleConfig>();
            _villagerScheduleLookup = state.GetComponentLookup<VillagerScheduleProfile>(true);
            _alignmentLookup = state.GetComponentLookup<VillagerAlignment>(true);
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

            var baseSchedule = SystemAPI.GetSingleton<VillagerScheduleConfig>();

            var secondsPerDay = 480f;
            if (SystemAPI.HasSingleton<VillagerLifecycleTuning>())
            {
                var tuning = SystemAPI.GetSingleton<VillagerLifecycleTuning>();
                if (tuning.SecondsPerDay > 1f)
                {
                    secondsPerDay = tuning.SecondsPerDay;
                }
            }

            var time01 = math.frac(timeState.WorldSeconds / math.max(1f, secondsPerDay));
            var dayIndex = (uint)math.floor(timeState.WorldSeconds / math.max(1f, secondsPerDay));
            var deltaTime = SystemAPI.Time.DeltaTime;
            _villagerScheduleLookup.Update(ref state);
            _alignmentLookup.Update(ref state);
            var villageScheduleMap = BuildVillageScheduleMap(ref state);

            foreach (var (needs, goal, behavior, entity) in SystemAPI
                         .Query<RefRW<VillagerNeedState>, RefRO<VillagerGoalState>, RefRO<VillagerBehavior>>()
                         .WithEntityAccess())
            {
                if (goal.ValueRO.CurrentGoal == VillagerGoal.Work)
                {
                    continue;
                }

                var schedule = ResolveScheduleConfig(entity, baseSchedule, villageScheduleMap, _villagerScheduleLookup);
                if (schedule.WorkRecoveryPerSecond <= 0f)
                {
                    continue;
                }

                var patience01 = math.saturate((behavior.ValueRO.PatienceScore + 100f) * 0.005f);
                var order01 = ResolveOrder01(entity);
                var offsetTime01 = schedule.ScheduleOffsetMax01 > 0f || schedule.ScheduleChaosJitterMax01 > 0f
                    ? math.frac(time01 + ResolveScheduleOffset01(entity, schedule, patience01, order01, dayIndex))
                    : time01;

                var recovery = schedule.WorkRecoveryPerSecond * deltaTime;
                if (!IsWithinWindow(offsetTime01, schedule.WorkStart01, schedule.WorkEnd01))
                {
                    recovery *= math.max(1f, schedule.WorkRecoveryOffHoursMultiplier);
                }

                if (recovery <= 0f)
                {
                    continue;
                }

                var updated = needs.ValueRO;
                updated.WorkUrgency = math.max(0f, updated.WorkUrgency - recovery);
                needs.ValueRW = updated;
            }

            villageScheduleMap.Dispose();
        }

        private static bool IsWithinWindow(float time01, float start01, float end01)
        {
            if (start01 <= end01)
            {
                return time01 >= start01 && time01 < end01;
            }

            return time01 >= start01 || time01 < end01;
        }

        private static float ResolveScheduleOffset01(Entity entity, in VillagerScheduleConfig schedule, float patience01, float order01, uint dayIndex)
        {
            var maxOffset = math.max(0f, schedule.ScheduleOffsetMax01);
            if (maxOffset <= 0f && schedule.ScheduleChaosJitterMax01 <= 0f)
            {
                return 0f;
            }

            var chaos01 = math.saturate(1f - order01);
            var patienceScale = math.lerp(1f, 1f - patience01, math.clamp(schedule.ScheduleOffsetPatienceWeight, 0f, 1f));
            var orderScale = math.lerp(1f, 1f + chaos01, math.clamp(schedule.ScheduleOffsetOrderWeight, 0f, 1f));
            var scaledMax = maxOffset * patienceScale * orderScale;

            var seed = math.hash(new uint2((uint)(entity.Index + 1), 0x7f4au));
            var unit = (seed & 0xffffu) / 65535f;
            var baseOffset = (unit * 2f - 1f) * scaledMax;

            var jitterMax = math.max(0f, schedule.ScheduleChaosJitterMax01) * chaos01;
            if (jitterMax <= 0f)
            {
                return baseOffset;
            }

            var jitterSeed = math.hash(new uint3((uint)(entity.Index + 11), dayIndex + 31u, 0x2ea1u));
            var jitterUnit = (jitterSeed & 0xffffu) / 65535f;
            var jitter = (jitterUnit * 2f - 1f) * jitterMax;
            return baseOffset + jitter;
        }

        private static VillagerScheduleConfig ResolveScheduleConfig(Entity entity, in VillagerScheduleConfig baseSchedule,
            NativeParallelHashMap<Entity, VillageScheduleProfile> villageScheduleMap,
            ComponentLookup<VillagerScheduleProfile> villagerScheduleLookup)
        {
            var schedule = baseSchedule;

            if (villageScheduleMap.TryGetValue(entity, out var villageProfile))
            {
                schedule = BlendSchedules(schedule, villageProfile.Schedule, math.saturate(villageProfile.Weight));
            }

            if (villagerScheduleLookup.HasComponent(entity))
            {
                var profile = villagerScheduleLookup[entity];
                schedule = BlendSchedules(schedule, profile.Schedule, math.saturate(profile.Weight));
            }

            return schedule;
        }

        private NativeParallelHashMap<Entity, VillageScheduleProfile> BuildVillageScheduleMap(ref SystemState state)
        {
            var estimated = 8;
            foreach (var members in SystemAPI.Query<DynamicBuffer<VillageMember>>())
            {
                estimated += members.Length;
            }

            var map = new NativeParallelHashMap<Entity, VillageScheduleProfile>(math.max(estimated, 8), Allocator.Temp);

            foreach (var (profile, members) in SystemAPI.Query<RefRO<VillageScheduleProfile>, DynamicBuffer<VillageMember>>())
            {
                var scheduleProfile = profile.ValueRO;
                for (int i = 0; i < members.Length; i++)
                {
                    map.TryAdd(members[i].VillagerEntity, scheduleProfile);
                }
            }

            return map;
        }

        private float ResolveOrder01(Entity entity)
        {
            if (!_alignmentLookup.HasComponent(entity))
            {
                return 0.5f;
            }

            var orderAxis = _alignmentLookup[entity].OrderAxis;
            return math.saturate((orderAxis + 100f) / 200f);
        }

        private static VillagerScheduleConfig BlendSchedules(in VillagerScheduleConfig a, in VillagerScheduleConfig b, float weight)
        {
            var w = math.saturate(weight);
            if (w <= 0f)
            {
                return a;
            }
            if (w >= 1f)
            {
                return b;
            }

            return new VillagerScheduleConfig
            {
                WorkStart01 = math.lerp(a.WorkStart01, b.WorkStart01, w),
                WorkEnd01 = math.lerp(a.WorkEnd01, b.WorkEnd01, w),
                SocialStart01 = math.lerp(a.SocialStart01, b.SocialStart01, w),
                SocialEnd01 = math.lerp(a.SocialEnd01, b.SocialEnd01, w),
                RestStart01 = math.lerp(a.RestStart01, b.RestStart01, w),
                RestEnd01 = math.lerp(a.RestEnd01, b.RestEnd01, w),
                WorkBias = math.lerp(a.WorkBias, b.WorkBias, w),
                SocialBias = math.lerp(a.SocialBias, b.SocialBias, w),
                RestBias = math.lerp(a.RestBias, b.RestBias, w),
                FaithBias = math.lerp(a.FaithBias, b.FaithBias, w),
                AdherenceMin = math.lerp(a.AdherenceMin, b.AdherenceMin, w),
                AdherenceMax = math.lerp(a.AdherenceMax, b.AdherenceMax, w),
                ScheduleOffsetMax01 = math.lerp(a.ScheduleOffsetMax01, b.ScheduleOffsetMax01, w),
                ScheduleOffsetPatienceWeight = math.lerp(a.ScheduleOffsetPatienceWeight, b.ScheduleOffsetPatienceWeight, w),
                ScheduleOffsetOrderWeight = math.lerp(a.ScheduleOffsetOrderWeight, b.ScheduleOffsetOrderWeight, w),
                ScheduleChaosJitterMax01 = math.lerp(a.ScheduleChaosJitterMax01, b.ScheduleChaosJitterMax01, w),
                NeedMoveSpeed = math.lerp(a.NeedMoveSpeed, b.NeedMoveSpeed, w),
                NeedSatisfyRate = math.lerp(a.NeedSatisfyRate, b.NeedSatisfyRate, w),
                WorkSatisfactionPerDelivery = math.lerp(a.WorkSatisfactionPerDelivery, b.WorkSatisfactionPerDelivery, w),
                WorkRecoveryPerSecond = math.lerp(a.WorkRecoveryPerSecond, b.WorkRecoveryPerSecond, w),
                WorkRecoveryOffHoursMultiplier = math.lerp(a.WorkRecoveryOffHoursMultiplier, b.WorkRecoveryOffHoursMultiplier, w),
                DeliberationMinSeconds = math.lerp(a.DeliberationMinSeconds, b.DeliberationMinSeconds, w),
                DeliberationMaxSeconds = math.lerp(a.DeliberationMaxSeconds, b.DeliberationMaxSeconds, w),
                PatienceCadenceBonusMax = math.lerp(a.PatienceCadenceBonusMax, b.PatienceCadenceBonusMax, w),
                NeedWanderRadius = math.lerp(a.NeedWanderRadius, b.NeedWanderRadius, w),
                NeedSocialRadius = math.lerp(a.NeedSocialRadius, b.NeedSocialRadius, w),
                NeedLingerMinSeconds = math.lerp(a.NeedLingerMinSeconds, b.NeedLingerMinSeconds, w),
                NeedLingerMaxSeconds = math.lerp(a.NeedLingerMaxSeconds, b.NeedLingerMaxSeconds, w),
                NeedRepathMinSeconds = math.lerp(a.NeedRepathMinSeconds, b.NeedRepathMinSeconds, w),
                NeedRepathMaxSeconds = math.lerp(a.NeedRepathMaxSeconds, b.NeedRepathMaxSeconds, w),
                SocialPickMinSeconds = math.lerp(a.SocialPickMinSeconds, b.SocialPickMinSeconds, w),
                SocialPickMaxSeconds = math.lerp(a.SocialPickMaxSeconds, b.SocialPickMaxSeconds, w),
                SocialPreferRelationWeight = math.lerp(a.SocialPreferRelationWeight, b.SocialPreferRelationWeight, w),
                SocialPreferGoalBonus = math.lerp(a.SocialPreferGoalBonus, b.SocialPreferGoalBonus, w)
            };
        }
    }
}
