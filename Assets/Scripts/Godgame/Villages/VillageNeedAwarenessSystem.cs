using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Villages
{
    /// <summary>
    /// Aggregates villager need pressures into a village-level awareness signal used by macro decisions.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(PureDOTS.Runtime.AI.VillagerMindSystemGroup))]
    [UpdateBefore(typeof(VillageAIDecisionSystem))]
    public partial struct VillageNeedAwarenessSystem : ISystem
    {
        private const uint SampleIntervalTicks = 30; // ~0.5s at 60hz
        private const float AwarenessSmoothing = 0.35f;
        private const float InfluenceWeight = 0.35f;

        private ComponentLookup<VillagerNeedState> _needLookup;
        private ComponentLookup<LocalTransform> _transformLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
            state.RequireForUpdate<VillageNeedAwareness>();

            _needLookup = state.GetComponentLookup<VillagerNeedState>(true);
            _transformLookup = state.GetComponentLookup<LocalTransform>(true);
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

            _needLookup.Update(ref state);
            _transformLookup.Update(ref state);

            foreach (var (village, awareness, members, entity) in SystemAPI
                         .Query<RefRW<Village>, RefRW<VillageNeedAwareness>, DynamicBuffer<VillageMember>>()
                         .WithEntityAccess())
            {
                var awarenessValue = awareness.ValueRO;
                if (awarenessValue.LastSampleTick != 0u && timeState.Tick - awarenessValue.LastSampleTick < SampleIntervalTicks)
                {
                    continue;
                }

                var center = village.ValueRO.CenterPosition;
                if (_transformLookup.HasComponent(entity))
                {
                    center = _transformLookup[entity].Position;
                }

                var radius = math.max(0.1f, village.ValueRO.InfluenceRadius);
                var radiusSq = radius * radius;

                float hungerSum = 0f;
                float restSum = 0f;
                float faithSum = 0f;
                float safetySum = 0f;
                float socialSum = 0f;
                float workSum = 0f;
                int sampleCount = 0;

                var leaderEntity = Entity.Null;
                var leaderDistSq = float.MaxValue;
                var leaderNeeds = default(VillagerNeedState);
                var leaderValid = false;

                if (awarenessValue.DerivedMembers != 0 || members.Length == 0)
                {
                    members.Clear();
                    foreach (var (need, transform, villagerEntity) in SystemAPI
                                 .Query<RefRO<VillagerNeedState>, RefRO<LocalTransform>>()
                                 .WithEntityAccess())
                    {
                        var pos = transform.ValueRO.Position;
                        var distSq = math.distancesq(pos.xz, center.xz);
                        if (distSq > radiusSq)
                        {
                            continue;
                        }

                        members.Add(new VillageMember { VillagerEntity = villagerEntity });

                        var needValue = need.ValueRO;
                        hungerSum += needValue.HungerUrgency;
                        restSum += needValue.RestUrgency;
                        faithSum += needValue.FaithUrgency;
                        safetySum += needValue.SafetyUrgency;
                        socialSum += needValue.SocialUrgency;
                        workSum += needValue.WorkUrgency;
                        sampleCount++;

                        if (distSq < leaderDistSq)
                        {
                            leaderDistSq = distSq;
                            leaderEntity = villagerEntity;
                            leaderNeeds = needValue;
                            leaderValid = true;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < members.Length; i++)
                    {
                        var villagerEntity = members[i].VillagerEntity;
                        if (!_needLookup.HasComponent(villagerEntity) || !_transformLookup.HasComponent(villagerEntity))
                        {
                            continue;
                        }

                        var needValue = _needLookup[villagerEntity];
                        hungerSum += needValue.HungerUrgency;
                        restSum += needValue.RestUrgency;
                        faithSum += needValue.FaithUrgency;
                        safetySum += needValue.SafetyUrgency;
                        socialSum += needValue.SocialUrgency;
                        workSum += needValue.WorkUrgency;
                        sampleCount++;

                        var distSq = math.distancesq(_transformLookup[villagerEntity].Position.xz, center.xz);
                        if (distSq < leaderDistSq)
                        {
                            leaderDistSq = distSq;
                            leaderEntity = villagerEntity;
                            leaderNeeds = needValue;
                            leaderValid = true;
                        }
                    }
                }

                if (sampleCount <= 0)
                {
                    awareness.ValueRW.SampleCount = 0;
                    awareness.ValueRW.LastSampleTick = timeState.Tick;
                    village.ValueRW.MemberCount = 0;
                    continue;
                }

                var invCount = 1f / sampleCount;
                var avgHunger = hungerSum * invCount;
                var avgRest = restSum * invCount;
                var avgFaith = faithSum * invCount;
                var avgSafety = safetySum * invCount;
                var avgSocial = socialSum * invCount;
                var avgWork = workSum * invCount;

                var influence = leaderValid ? math.saturate(InfluenceWeight) : 0f;
                var influencedHunger = leaderValid ? math.lerp(avgHunger, leaderNeeds.HungerUrgency, influence) : avgHunger;
                var influencedRest = leaderValid ? math.lerp(avgRest, leaderNeeds.RestUrgency, influence) : avgRest;
                var influencedFaith = leaderValid ? math.lerp(avgFaith, leaderNeeds.FaithUrgency, influence) : avgFaith;
                var influencedSafety = leaderValid ? math.lerp(avgSafety, leaderNeeds.SafetyUrgency, influence) : avgSafety;
                var influencedSocial = leaderValid ? math.lerp(avgSocial, leaderNeeds.SocialUrgency, influence) : avgSocial;
                var influencedWork = leaderValid ? math.lerp(avgWork, leaderNeeds.WorkUrgency, influence) : avgWork;

                awareness.ValueRW.Hunger = math.lerp(awarenessValue.Hunger, influencedHunger, AwarenessSmoothing);
                awareness.ValueRW.Rest = math.lerp(awarenessValue.Rest, influencedRest, AwarenessSmoothing);
                awareness.ValueRW.Faith = math.lerp(awarenessValue.Faith, influencedFaith, AwarenessSmoothing);
                awareness.ValueRW.Safety = math.lerp(awarenessValue.Safety, influencedSafety, AwarenessSmoothing);
                awareness.ValueRW.Social = math.lerp(awarenessValue.Social, influencedSocial, AwarenessSmoothing);
                awareness.ValueRW.Work = math.lerp(awarenessValue.Work, influencedWork, AwarenessSmoothing);
                awareness.ValueRW.SampleCount = sampleCount;
                awareness.ValueRW.InfluenceEntity = leaderEntity;
                awareness.ValueRW.LastSampleTick = timeState.Tick;

                var max = awareness.ValueRW.Hunger;
                var dominant = VillageNeedChannel.Hunger;
                Consider(awareness.ValueRW.Rest, VillageNeedChannel.Rest, ref max, ref dominant);
                Consider(awareness.ValueRW.Faith, VillageNeedChannel.Faith, ref max, ref dominant);
                Consider(awareness.ValueRW.Safety, VillageNeedChannel.Safety, ref max, ref dominant);
                Consider(awareness.ValueRW.Social, VillageNeedChannel.Social, ref max, ref dominant);
                Consider(awareness.ValueRW.Work, VillageNeedChannel.Work, ref max, ref dominant);

                awareness.ValueRW.MaxNeed = math.clamp(max, 0f, 1f);
                awareness.ValueRW.DominantNeed = dominant;
                village.ValueRW.MemberCount = sampleCount;
            }
        }

        private static void Consider(float value, VillageNeedChannel channel, ref float max, ref VillageNeedChannel dominant)
        {
            if (value <= max)
            {
                return;
            }

            max = value;
            dominant = channel;
        }

    }
}
