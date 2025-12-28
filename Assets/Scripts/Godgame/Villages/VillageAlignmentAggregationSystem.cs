using Godgame.Villagers;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Villages
{
    /// <summary>
    /// Aggregates member alignment/outlook into a village-level alignment state.
    /// Micro → macro: members (and especially leadership) bias the village profile.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(VillageNeedAwarenessSystem))]
    [UpdateBefore(typeof(VillageInitiativeSystem))]
    public partial struct VillageAlignmentAggregationSystem : ISystem
    {
        private const float LeadershipInfluenceWeight = 0.35f;

        private ComponentLookup<VillagerAlignment> _alignmentLookup;
        private ComponentLookup<VillagerOutlook> _outlookLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
            state.RequireForUpdate<VillageNeedAwareness>();
            state.RequireForUpdate<VillageAlignmentState>();

            _alignmentLookup = state.GetComponentLookup<VillagerAlignment>(true);
            _outlookLookup = state.GetComponentLookup<VillagerOutlook>(true);
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

            _alignmentLookup.Update(ref state);
            _outlookLookup.Update(ref state);

            foreach (var (awareness, members, alignmentState) in SystemAPI.Query<
                         RefRO<VillageNeedAwareness>,
                         DynamicBuffer<VillageMember>,
                         RefRW<VillageAlignmentState>>())
            {
                var leader = awareness.ValueRO.InfluenceEntity;

                var count = 0;
                var moralSum = 0;
                var orderSum = 0;
                var puritySum = 0;

                for (int i = 0; i < members.Length; i++)
                {
                    var villager = members[i].VillagerEntity;
                    if (villager == Entity.Null || !_alignmentLookup.HasComponent(villager))
                    {
                        continue;
                    }

                    var alignment = _alignmentLookup[villager];
                    moralSum += alignment.MoralAxis;
                    orderSum += alignment.OrderAxis;
                    puritySum += alignment.PurityAxis;
                    count++;
                }

                var averageMoral = count > 0 ? moralSum / (float)count : 0f;
                var averageOrder = count > 0 ? orderSum / (float)count : 0f;
                var averagePurity = count > 0 ? puritySum / (float)count : 0f;

                if (leader != Entity.Null && _alignmentLookup.HasComponent(leader))
                {
                    var leaderAlignment = _alignmentLookup[leader];
                    averageMoral = math.lerp(averageMoral, leaderAlignment.MoralAxis, LeadershipInfluenceWeight);
                    averageOrder = math.lerp(averageOrder, leaderAlignment.OrderAxis, LeadershipInfluenceWeight);
                    averagePurity = math.lerp(averagePurity, leaderAlignment.PurityAxis, LeadershipInfluenceWeight);
                }

                var dominantOutlook = ResolveDominantOutlook(leader, members, ref _outlookLookup);

                alignmentState.ValueRW = new VillageAlignmentState
                {
                    MoralAxis = (sbyte)math.clamp(math.round(averageMoral), -100f, 100f),
                    OrderAxis = (sbyte)math.clamp(math.round(averageOrder), -100f, 100f),
                    PurityAxis = (sbyte)math.clamp(math.round(averagePurity), -100f, 100f),
                    DominantOutlookId = dominantOutlook
                };
            }
        }

        private static byte ResolveDominantOutlook(
            Entity leader,
            DynamicBuffer<VillageMember> members,
            ref ComponentLookup<VillagerOutlook> outlookLookup)
        {
            if (members.Length == 0)
            {
                return 0;
            }

            var hasLeader = leader != Entity.Null;
            var scores = new NativeHashMap<byte, int>(16, Allocator.Temp);

            for (int i = 0; i < members.Length; i++)
            {
                var villager = members[i].VillagerEntity;
                if (villager == Entity.Null || !outlookLookup.HasComponent(villager))
                {
                    continue;
                }

                var outlook = outlookLookup[villager];
                var typeCount = outlook.OutlookTypes.Length;
                var valueCount = outlook.OutlookValues.Length;
                var slots = math.min(typeCount, valueCount);
                if (slots <= 0)
                {
                    continue;
                }

                var isLeader = hasLeader && villager == leader;

                for (int slot = 0; slot < slots; slot++)
                {
                    var typeId = outlook.OutlookTypes[slot];
                    if (typeId == 0)
                    {
                        continue;
                    }

                    var rawValue = outlook.OutlookValues[slot];
                    var contribution = math.max(0, (int)rawValue);
                    if (contribution <= 0)
                    {
                        continue;
                    }

                    if (isLeader)
                    {
                        contribution += (int)math.round(contribution * LeadershipInfluenceWeight);
                    }

                    if (!scores.TryGetValue(typeId, out var existing))
                    {
                        scores.TryAdd(typeId, contribution);
                    }
                    else
                    {
                        scores[typeId] = existing + contribution;
                    }
                }
            }

            byte bestType = 0;
            var bestScore = 0;

            foreach (var kvp in scores)
            {
                if (kvp.Value > bestScore || (kvp.Value == bestScore && kvp.Key < bestType))
                {
                    bestType = kvp.Key;
                    bestScore = kvp.Value;
                }
            }

            scores.Dispose();
            return bestType;
        }
    }
}
