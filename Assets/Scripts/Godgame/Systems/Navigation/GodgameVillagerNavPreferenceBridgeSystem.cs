using Godgame.Villagers;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Core;
using PureDOTS.Runtime.Identity;
using PureDOTS.Runtime.Navigation;
using PureDOTS.Systems;
using PureDOTS.Systems.Navigation;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Systems.Navigation
{
    /// <summary>
    /// Applies villager alignment/outlook to navigation preference when path requests are made.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(WarmPathSystemGroup))]
    [UpdateBefore(typeof(NavPreferenceSystem))]
    public partial struct GodgameVillagerNavPreferenceBridgeSystem : ISystem
    {
        private ComponentLookup<VillagerOutlook> _outlookLookup;
        private ComponentLookup<UpdateCadence> _cadenceLookup;
        private ComponentLookup<NavPreference> _preferenceLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
            state.RequireForUpdate<PathRequest>();

            _outlookLookup = state.GetComponentLookup<VillagerOutlook>(true);
            _cadenceLookup = state.GetComponentLookup<UpdateCadence>(true);
            _preferenceLookup = state.GetComponentLookup<NavPreference>(false);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<RewindState>(out var rewindState) || rewindState.Mode != RewindMode.Record)
            {
                return;
            }

            var timeState = SystemAPI.GetSingleton<TimeState>();
            if (timeState.IsPaused)
            {
                return;
            }

            _outlookLookup.Update(ref state);
            _cadenceLookup.Update(ref state);
            _preferenceLookup.Update(ref state);

            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);

            foreach (var (alignment, entity) in SystemAPI.Query<RefRO<VillagerAlignment>>()
                .WithAll<VillagerId, PathRequest>()
                .WithEntityAccess())
            {
                var hasPreference = _preferenceLookup.HasComponent(entity);
                var hasOutlook = TryGetOutlook(entity, out var outlook);
                if (hasPreference)
                {
                    if (_cadenceLookup.HasComponent(entity))
                    {
                        var cadence = _cadenceLookup[entity];
                        if (!UpdateCadenceHelpers.ShouldUpdate(timeState.Tick, cadence))
                        {
                            continue;
                        }
                    }

                    var preference = BuildPreference(alignment.ValueRO, hasOutlook, outlook, _preferenceLookup[entity]);
                    _preferenceLookup[entity] = preference;
                }
                else
                {
                    var preference = BuildPreference(alignment.ValueRO, hasOutlook, outlook, NavPreference.CreateDefault());
                    ecb.AddComponent(entity, preference);
                }
            }

            ecb.Playback(state.EntityManager);
        }

        private bool TryGetOutlook(Entity entity, out VillagerOutlook outlook)
        {
            if (_outlookLookup.HasComponent(entity))
            {
                outlook = _outlookLookup[entity];
                return true;
            }

            outlook = default;
            return false;
        }

        private static NavPreference BuildPreference(in VillagerAlignment alignment, bool hasOutlook, in VillagerOutlook outlook, in NavPreference basePreference)
        {
            var preference = basePreference;
            var order01 = math.clamp(alignment.OrderAxis / 100f, -1f, 1f);

            var peaceful01 = hasOutlook ? ResolveOutlookValue01(outlook, OutlookType.Peaceful) : 0f;
            var warlike01 = hasOutlook ? ResolveOutlookValue01(outlook, OutlookType.Warlike) : 0f;
            var outlookBias = math.clamp(peaceful01 - warlike01, -1f, 1f);

            const float axisWeight = 0.25f;
            const float outlookWeight = 0.25f;

            var riskWeight = 0.5f + order01 * axisWeight + outlookBias * outlookWeight;
            riskWeight = math.clamp(riskWeight, 0f, 1f);
            preference.RiskWeight = riskWeight;
            preference.TimeWeight = math.clamp(1f - riskWeight, 0f, 1f);

            return preference;
        }

        private static float ResolveOutlookValue01(in VillagerOutlook outlook, OutlookType type)
        {
            var slotCount = math.min(outlook.OutlookTypes.Length, outlook.OutlookValues.Length);
            for (int i = 0; i < slotCount; i++)
            {
                if (outlook.OutlookTypes[i] != (byte)type)
                {
                    continue;
                }

                var raw = outlook.OutlookValues[i];
                return math.clamp(raw / 100f, -1f, 1f);
            }

            return 0f;
        }
    }
}
