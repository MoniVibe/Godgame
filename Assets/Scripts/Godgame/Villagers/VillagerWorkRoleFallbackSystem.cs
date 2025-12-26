using Godgame.Rendering;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Maps render role to work role when no explicit assignment is present.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct VillagerWorkRoleFallbackSystem : ISystem
    {
        private ComponentLookup<VillagerWorkRoleOverride> _overrideLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VillagerWorkRole>();
            _overrideLookup = state.GetComponentLookup<VillagerWorkRoleOverride>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.TryGetSingleton<RewindState>(out var rewind) && rewind.Mode != RewindMode.Record)
            {
                return;
            }

            _overrideLookup.Update(ref state);

            foreach (var (role, renderRole, entity) in SystemAPI
                         .Query<RefRW<VillagerWorkRole>, RefRO<VillagerRenderRole>>()
                         .WithEntityAccess())
            {
                if (_overrideLookup.HasComponent(entity))
                {
                    continue;
                }

                if (role.ValueRO.Value != VillagerWorkRoleKind.None)
                {
                    continue;
                }

                role.ValueRW = new VillagerWorkRole
                {
                    Value = MapRenderRole(renderRole.ValueRO.Value)
                };
            }
        }

        private static VillagerWorkRoleKind MapRenderRole(VillagerRenderRoleId renderRole)
        {
            return renderRole switch
            {
                VillagerRenderRoleId.Miner => VillagerWorkRoleKind.Miner,
                VillagerRenderRoleId.Farmer => VillagerWorkRoleKind.Farmer,
                VillagerRenderRoleId.Forester => VillagerWorkRoleKind.Forester,
                VillagerRenderRoleId.Breeder => VillagerWorkRoleKind.Breeder,
                _ => VillagerWorkRoleKind.None
            };
        }
    }
}
