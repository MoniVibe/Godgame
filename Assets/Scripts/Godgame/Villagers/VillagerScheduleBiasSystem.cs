using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

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
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<VillagerScheduleConfig>();
            state.RequireForUpdate<VillagerNeedState>();
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

            var schedule = SystemAPI.GetSingleton<VillagerScheduleConfig>();
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

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (behavior, entity) in SystemAPI
                         .Query<RefRO<VillagerBehavior>>()
                         .WithEntityAccess())
            {
                var patience01 = math.saturate((behavior.ValueRO.PatienceScore + 100f) * 0.005f);
                var adherence = math.lerp(schedule.AdherenceMin, schedule.AdherenceMax, patience01);

                var workBias = IsWithinWindow(time01, schedule.WorkStart01, schedule.WorkEnd01) ? schedule.WorkBias : 1f;
                var socialBias = IsWithinWindow(time01, schedule.SocialStart01, schedule.SocialEnd01) ? schedule.SocialBias : 1f;
                var restBias = IsWithinWindow(time01, schedule.RestStart01, schedule.RestEnd01) ? schedule.RestBias : 1f;

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
