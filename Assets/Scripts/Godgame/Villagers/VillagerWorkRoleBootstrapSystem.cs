using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Adds default work role and hauling preference components to villagers.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct VillagerWorkRoleBootstrapSystem : ISystem
    {
        private EntityQuery _missingRole;
        private EntityQuery _missingHaul;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VillagerId>();
            _missingRole = SystemAPI.QueryBuilder()
                .WithAll<VillagerId>()
                .WithNone<VillagerWorkRole>()
                .Build();
            _missingHaul = SystemAPI.QueryBuilder()
                .WithAll<VillagerId>()
                .WithNone<VillagerHaulPreference>()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (_missingRole.IsEmptyIgnoreFilter && _missingHaul.IsEmptyIgnoreFilter)
            {
                state.Enabled = false;
                return;
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            if (!_missingRole.IsEmptyIgnoreFilter)
            {
                ecb.AddComponent(_missingRole, new VillagerWorkRole
                {
                    Value = VillagerWorkRoleKind.None
                });
            }

            if (!_missingHaul.IsEmptyIgnoreFilter)
            {
                ecb.AddComponent(_missingHaul, new VillagerHaulPreference
                {
                    HaulChance = 0f,
                    HaulCooldownSeconds = 0f,
                    NextHaulAllowedTick = 0u,
                    ForceHaul = 0
                });
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
