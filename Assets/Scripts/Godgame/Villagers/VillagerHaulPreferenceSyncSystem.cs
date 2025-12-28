using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Applies global hauling defaults to villagers unless explicitly overridden.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(VillagerWorkRoleResourceSyncSystem))]
    [UpdateBefore(typeof(VillagerJobSystem))]
    public partial struct VillagerHaulPreferenceSyncSystem : ISystem
    {
        private ComponentLookup<VillagerHaulPreferenceOverride> _overrideLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VillagerHaulPreference>();
            state.RequireForUpdate<VillagerWorkRole>();
            _overrideLookup = state.GetComponentLookup<VillagerHaulPreferenceOverride>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.TryGetSingleton<RewindState>(out var rewind) && rewind.Mode != RewindMode.Record)
            {
                return;
            }

            var tuning = SystemAPI.HasSingleton<VillagerWorkTuning>()
                ? SystemAPI.GetSingleton<VillagerWorkTuning>()
                : new VillagerWorkTuning
                {
                    HaulChance = 0.2f,
                    HaulCooldownSeconds = 8f
                };

            _overrideLookup.Update(ref state);

            foreach (var (haul, role, entity) in SystemAPI.Query<RefRW<VillagerHaulPreference>, RefRO<VillagerWorkRole>>().WithEntityAccess())
            {
                if (_overrideLookup.HasComponent(entity))
                {
                    continue;
                }

                haul.ValueRW.HaulChance = tuning.HaulChance;
                haul.ValueRW.HaulCooldownSeconds = tuning.HaulCooldownSeconds;
                haul.ValueRW.ForceHaul = (byte)(role.ValueRO.Value == VillagerWorkRoleKind.Hauler ? 1 : 0);
            }
        }
    }
}
