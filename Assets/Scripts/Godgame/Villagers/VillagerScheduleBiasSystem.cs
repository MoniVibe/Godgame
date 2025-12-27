using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Godgame.Villages;

namespace Godgame.Villagers
{
    /// <summary>
    /// Applies time-of-day schedule bias to villager need weights.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(VillagerMindSystemGroup))]
    [UpdateBefore(typeof(VillagerGoalSelectionSystem))]
    public partial struct VillagerScheduleBiasSystem : ISystem
    {
        private ComponentLookup<VillagerScheduleProfile> _villagerScheduleLookup;
        private ComponentLookup<VillagerAlignment> _alignmentLookup;
        private ComponentLookup<VillagerDirectiveBias> _directiveBiasLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<VillagerScheduleConfig>();
            state.RequireForUpdate<VillagerNeedState>();
            _villagerScheduleLookup = state.GetComponentLookup<VillagerScheduleProfile>(true);
            _alignmentLookup = state.GetComponentLookup<VillagerAlignment>(true);
            _directiveBiasLookup = state.GetComponentLookup<VillagerDirectiveBias>(true);
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

            _villagerScheduleLookup.Update(ref state);
            _alignmentLookup.Update(ref state);
            _directiveBiasLookup.Update(ref state);

            var villageScheduleMap = BuildVillageScheduleMap(ref state);

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (behavior, entity) in SystemAPI
                         .Query<RefRO<VillagerBehavior>>()
                         .WithEntityAccess())
            {
                var schedule = ResolveScheduleConfig(entity, baseSchedule, villageScheduleMap, _villagerScheduleLookup);
                var patience01 = math.saturate((behavior.ValueRO.PatienceScore + 100f) * 0.005f);
                var adherence = math.lerp(schedule.AdherenceMin, schedule.AdherenceMax, patience01);
                var order01 = ResolveOrder01(entity);

                var offsetTime01 = schedule.ScheduleOffsetMax01 > 0f || schedule.ScheduleChaosJitterMax01 > 0f
                    ? math.frac(time01 + ResolveScheduleOffset01(entity, schedule, patience01, order01, dayIndex))
                    : time01;

                var workBias = IsWithinWindow(offsetTime01, schedule.WorkStart01, schedule.WorkEnd01) ? schedule.WorkBias : 1f;
                var socialBias = IsWithinWindow(offsetTime01, schedule.SocialStart01, schedule.SocialEnd01) ? schedule.SocialBias : 1f;
                var restBias = IsWithinWindow(offsetTime01, schedule.RestStart01, schedule.RestEnd01) ? schedule.RestBias : 1f;

                workBias = math.lerp(1f, workBias, adherence);
                socialBias = math.lerp(1f, socialBias, adherence);
                restBias = math.lerp(1f, restBias, adherence);

                var bias = new VillagerNeedBias
                {
                    HungerWeight = 1f,
                    RestWeight = restBias,
                    FaithWeight = schedule.FaithBias,
                    SafetyWeight = 1f,
                    SocialWeight = socialBias,
                    WorkWeight = workBias
                };

                if (_directiveBiasLookup.HasComponent(entity))
                {
                    var directiveBias = _directiveBiasLookup[entity];
                    bias.HungerWeight *= math.max(0.1f, directiveBias.HungerWeight);
                    bias.RestWeight *= math.max(0.1f, directiveBias.RestWeight);
                    bias.FaithWeight *= math.max(0.1f, directiveBias.FaithWeight);
                    bias.SafetyWeight *= math.max(0.1f, directiveBias.SafetyWeight);
                    bias.SocialWeight *= math.max(0.1f, directiveBias.SocialWeight);
                    bias.WorkWeight *= math.max(0.1f, directiveBias.WorkWeight);
                }

                if (SystemAPI.HasComponent<VillagerNeedBias>(entity))
                {
                    SystemAPI.SetComponent(entity, bias);
                }
                else
                {
                    ecb.AddComponent(entity, bias);
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
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
