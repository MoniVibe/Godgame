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
    /// Chooses a routine directive per villager and emits directive bias for goal selection.
    /// </summary>
    [UpdateInGroup(typeof(VillagerMindSystemGroup))]
    [UpdateBefore(typeof(VillagerScheduleBiasSystem))]
    public partial struct VillagerDirectiveSystem : ISystem
    {
        private ComponentLookup<VillagerDirectiveProfile> _villagerProfileLookup;
        private ComponentLookup<VillagerAlignment> _alignmentLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<VillagerGoalState>();
            _villagerProfileLookup = state.GetComponentLookup<VillagerDirectiveProfile>(true);
            _alignmentLookup = state.GetComponentLookup<VillagerAlignment>(true);
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

            _villagerProfileLookup.Update(ref state);
            _alignmentLookup.Update(ref state);

            var baseConfig = SystemAPI.TryGetSingleton<VillagerDirectiveConfig>(out var cfg)
                ? cfg
                : VillagerDirectiveConfig.Default;
            var schedule = SystemAPI.TryGetSingleton<VillagerScheduleConfig>(out var scheduleCfg)
                ? scheduleCfg
                : VillagerScheduleConfig.Default;

            var secondsPerDay = 480f;
            if (SystemAPI.TryGetSingleton<VillagerLifecycleTuning>(out var lifecycle) && lifecycle.SecondsPerDay > 1f)
            {
                secondsPerDay = lifecycle.SecondsPerDay;
            }

            var time01 = math.frac(timeState.WorldSeconds / math.max(1f, secondsPerDay));
            var dayIndex = (uint)math.floor(timeState.WorldSeconds / math.max(1f, secondsPerDay));

            var villageProfileMap = BuildVillageDirectiveMap(ref state);

            foreach (var (behavior, directiveState, directiveBias, entity) in SystemAPI
                         .Query<RefRO<VillagerBehavior>, RefRW<VillagerDirectiveState>, RefRW<VillagerDirectiveBias>>()
                         .WithEntityAccess())
            {
                var config = baseConfig;
                if (timeState.Tick < directiveState.ValueRO.NextDecisionTick && directiveState.ValueRO.Current != VillagerDirective.None)
                {
                    directiveBias.ValueRW = ResolveDirectiveBias(config, directiveState.ValueRO.Current);
                    continue;
                }

                var patience01 = math.saturate((behavior.ValueRO.PatienceScore + 100f) * 0.005f);
                var order01 = ResolveOrder01(entity);

                var weights = ResolveWeights(config, schedule, time01);
                var minDuration = config.MinDurationSeconds;
                var maxDuration = config.MaxDurationSeconds;
                if (villageProfileMap.TryGetValue(entity, out var villageProfile))
                {
                    weights = BlendWeights(weights, villageProfile.Weights, math.saturate(villageProfile.Weight));
                    ApplyDurationOverrides(ref minDuration, ref maxDuration, villageProfile.MinDurationSeconds, villageProfile.MaxDurationSeconds);
                }

                if (_villagerProfileLookup.HasComponent(entity))
                {
                    var profile = _villagerProfileLookup[entity];
                    weights = BlendWeights(weights, profile.Weights, math.saturate(profile.Weight));
                    ApplyDurationOverrides(ref minDuration, ref maxDuration, profile.MinDurationSeconds, profile.MaxDurationSeconds);
                }

                weights = ApplyChaosJitter(weights, entity, order01, config, dayIndex);

                var nextDirective = PickDirective(weights, entity, timeState.Tick);
                if (nextDirective != directiveState.ValueRO.Current)
                {
                    directiveState.ValueRW.Previous = directiveState.ValueRO.Current;
                    directiveState.ValueRW.Current = nextDirective;
                    directiveState.ValueRW.LastChangeTick = timeState.Tick;
                }

                var durationTicks = ResolveDirectiveDurationTicks(minDuration, maxDuration, config, patience01, order01, entity, timeState.FixedDeltaTime, dayIndex);
                directiveState.ValueRW.NextDecisionTick = timeState.Tick + durationTicks;
                directiveBias.ValueRW = ResolveDirectiveBias(config, directiveState.ValueRO.Current);
            }

            villageProfileMap.Dispose();
        }

        private NativeParallelHashMap<Entity, VillageDirectiveProfile> BuildVillageDirectiveMap(ref SystemState state)
        {
            var estimated = 8;
            foreach (var members in SystemAPI.Query<DynamicBuffer<VillageMember>>())
            {
                estimated += members.Length;
            }

            var map = new NativeParallelHashMap<Entity, VillageDirectiveProfile>(math.max(estimated, 8), Allocator.Temp);

            foreach (var (profile, members) in SystemAPI.Query<RefRO<VillageDirectiveProfile>, DynamicBuffer<VillageMember>>())
            {
                var value = profile.ValueRO;
                for (int i = 0; i < members.Length; i++)
                {
                    map.TryAdd(members[i].VillagerEntity, value);
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

        private static VillagerDirectiveWeights ResolveWeights(in VillagerDirectiveConfig config, in VillagerScheduleConfig schedule, float time01)
        {
            var weights = config.BaseWeights;
            if (IsWithinWindow(time01, schedule.WorkStart01, schedule.WorkEnd01))
            {
                weights.Work *= math.max(0.1f, schedule.WorkBias);
            }
            if (IsWithinWindow(time01, schedule.RestStart01, schedule.RestEnd01))
            {
                weights.Rest *= math.max(0.1f, schedule.RestBias);
            }
            if (IsWithinWindow(time01, schedule.SocialStart01, schedule.SocialEnd01))
            {
                weights.Social *= math.max(0.1f, schedule.SocialBias);
            }

            weights.Faith *= math.max(0.1f, schedule.FaithBias);
            return weights;
        }

        private static VillagerDirectiveWeights BlendWeights(in VillagerDirectiveWeights a, in VillagerDirectiveWeights b, float weight)
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

            return new VillagerDirectiveWeights
            {
                Work = math.lerp(a.Work, b.Work, w),
                Rest = math.lerp(a.Rest, b.Rest, w),
                Social = math.lerp(a.Social, b.Social, w),
                Faith = math.lerp(a.Faith, b.Faith, w),
                Roam = math.lerp(a.Roam, b.Roam, w)
            };
        }

        private static void ApplyDurationOverrides(ref float minDuration, ref float maxDuration, float minOverride, float maxOverride)
        {
            var min = math.max(0f, minOverride);
            var max = math.max(0f, maxOverride);
            if (min <= 0f && max <= 0f)
            {
                return;
            }

            if (max > 0f)
            {
                maxDuration = math.max(minDuration, max);
            }

            if (min > 0f)
            {
                minDuration = min;
            }
        }

        private static VillagerDirectiveWeights ApplyChaosJitter(in VillagerDirectiveWeights weights, Entity entity, float order01, VillagerDirectiveConfig config, uint dayIndex)
        {
            var chaos01 = math.saturate(1f - order01);
            if (chaos01 <= 0f || config.ChaosWeight <= 0f)
            {
                return weights;
            }

            var seed = math.hash(new uint3((uint)(entity.Index + 7), dayIndex + 13u, 0x53a1u));
            var random = Unity.Mathematics.Random.CreateFromIndex(seed == 0u ? 1u : seed);
            var jitter = random.NextFloat(0.85f, 1.15f);
            var scale = math.lerp(1f, jitter, chaos01 * config.ChaosWeight);

            return new VillagerDirectiveWeights
            {
                Work = weights.Work * scale,
                Rest = weights.Rest * scale,
                Social = weights.Social * scale,
                Faith = weights.Faith * scale,
                Roam = weights.Roam * scale
            };
        }

        private static VillagerDirective PickDirective(in VillagerDirectiveWeights weights, Entity entity, uint tick)
        {
            var total = math.max(0f, weights.Work)
                        + math.max(0f, weights.Rest)
                        + math.max(0f, weights.Social)
                        + math.max(0f, weights.Faith)
                        + math.max(0f, weights.Roam);
            if (total <= 0f)
            {
                return VillagerDirective.None;
            }

            var seed = math.hash(new uint2((uint)(entity.Index + 1), tick + 37u));
            var random = Unity.Mathematics.Random.CreateFromIndex(seed == 0u ? 1u : seed);
            var roll = random.NextFloat(0f, total);

            if ((roll -= math.max(0f, weights.Work)) <= 0f) return VillagerDirective.Work;
            if ((roll -= math.max(0f, weights.Rest)) <= 0f) return VillagerDirective.Rest;
            if ((roll -= math.max(0f, weights.Social)) <= 0f) return VillagerDirective.Socialize;
            if ((roll -= math.max(0f, weights.Faith)) <= 0f) return VillagerDirective.Pray;
            return VillagerDirective.Roam;
        }

        private static uint ResolveDirectiveDurationTicks(float minDuration, float maxDuration, in VillagerDirectiveConfig config, float patience01, float order01, Entity entity, float fixedDelta, uint dayIndex)
        {
            var min = math.max(1f, minDuration);
            var max = math.max(min, maxDuration);
            var seed = math.hash(new uint3((uint)(entity.Index + 13), dayIndex + 19u, 0x2b17u));
            var random = Unity.Mathematics.Random.CreateFromIndex(seed == 0u ? 1u : seed);
            var sample = random.NextFloat(min, max);

            var chaos01 = math.saturate(1f - order01);
            var patienceScale = math.lerp(0.85f, 1.25f, patience01);
            var orderScale = math.lerp(1f, 0.85f, chaos01 * config.OrderWeight);
            var seconds = sample * patienceScale * orderScale;
            return (uint)math.max(1f, math.ceil(seconds / math.max(1e-4f, fixedDelta)));
        }

        private static VillagerDirectiveBias ResolveDirectiveBias(in VillagerDirectiveConfig config, VillagerDirective directive)
        {
            return directive switch
            {
                VillagerDirective.Work => config.WorkBias,
                VillagerDirective.Rest => config.RestBias,
                VillagerDirective.Socialize => config.SocialBias,
                VillagerDirective.Pray => config.FaithBias,
                VillagerDirective.Roam => config.RoamBias,
                _ => config.IdleBias
            };
        }

        private static bool IsWithinWindow(float time01, float start01, float end01)
        {
            if (start01 <= end01)
            {
                return time01 >= start01 && time01 < end01;
            }

            return time01 >= start01 || time01 < end01;
        }
    }
}
