using Unity.Entities;
using LocalVillagerJob = Godgame.Villagers.VillagerJob;
using DotsVillagerJob = PureDOTS.Runtime.Components.VillagerJob;

namespace Godgame.Rendering
{
    /// <summary>
    /// Shared helpers that convert villager job/role data into render catalog keys.
    /// </summary>
    public static class VillagerRenderKeyUtility
    {
        private static readonly VillagerRenderRoleId[] DefaultRoleCycle =
        {
            VillagerRenderRoleId.Miner,
            VillagerRenderRoleId.Farmer,
            VillagerRenderRoleId.Forester,
            VillagerRenderRoleId.Breeder,
            VillagerRenderRoleId.Worshipper,
            VillagerRenderRoleId.Refiner,
            VillagerRenderRoleId.Peacekeeper,
            VillagerRenderRoleId.Combatant
        };

        public const int RoleCycleLength = 8;

        public static VillagerRenderRoleId GetDefaultRoleForIndex(int index)
        {
            if (index < 0)
            {
                index = 0;
            }

            return DefaultRoleCycle[index % DefaultRoleCycle.Length];
        }

        /// <summary>
        /// Provides a best-effort PureDOTS job to assign when spawning demo villagers for a role.
        /// </summary>
        public static DotsVillagerJob.JobType GetDefaultPureDotsJobForRole(VillagerRenderRoleId role) =>
            role switch
            {
                VillagerRenderRoleId.Miner => DotsVillagerJob.JobType.Gatherer,
                VillagerRenderRoleId.Farmer => DotsVillagerJob.JobType.Farmer,
                VillagerRenderRoleId.Forester => DotsVillagerJob.JobType.Builder,
                VillagerRenderRoleId.Breeder => DotsVillagerJob.JobType.Merchant,
                VillagerRenderRoleId.Worshipper => DotsVillagerJob.JobType.Priest,
                VillagerRenderRoleId.Refiner => DotsVillagerJob.JobType.Crafter,
                VillagerRenderRoleId.Peacekeeper => DotsVillagerJob.JobType.Guard,
                VillagerRenderRoleId.Combatant => DotsVillagerJob.JobType.Hunter,
                _ => DotsVillagerJob.JobType.Gatherer
            };

        public static ushort GetRenderKeyForRole(VillagerRenderRoleId role) =>
            role switch
            {
                VillagerRenderRoleId.Miner => GodgameRenderKeys.VillagerMiner,
                VillagerRenderRoleId.Farmer => GodgameRenderKeys.VillagerFarmer,
                VillagerRenderRoleId.Forester => GodgameRenderKeys.VillagerForester,
                VillagerRenderRoleId.Breeder => GodgameRenderKeys.VillagerBreeder,
                VillagerRenderRoleId.Worshipper => GodgameRenderKeys.VillagerWorshipper,
                VillagerRenderRoleId.Refiner => GodgameRenderKeys.VillagerRefiner,
                VillagerRenderRoleId.Peacekeeper => GodgameRenderKeys.VillagerPeacekeeper,
                VillagerRenderRoleId.Combatant => GodgameRenderKeys.VillagerCombatant,
                _ => GodgameRenderKeys.Villager
            };

        public static ushort ResolveVillagerRenderKey(EntityManager entityManager, Entity villager)
        {
            if (entityManager.HasComponent<VillagerRenderRole>(villager))
            {
                var role = entityManager.GetComponentData<VillagerRenderRole>(villager);
                if (TryGetKeyForRole(role.Value, out var keyForRole))
                {
                    return keyForRole;
                }
            }

            if (entityManager.HasComponent<LocalVillagerJob>(villager))
            {
                var localJob = entityManager.GetComponentData<LocalVillagerJob>(villager);
                if (TryMapLocalJob(localJob.Type, out var keyForLocal))
                {
                    return keyForLocal;
                }
            }

            if (entityManager.HasComponent<DotsVillagerJob>(villager))
            {
                var dotsJob = entityManager.GetComponentData<DotsVillagerJob>(villager);
                if (TryMapDotsJob(dotsJob.Type, out var keyForDots))
                {
                    return keyForDots;
                }
            }

            return GodgameRenderKeys.Villager;
        }

        public static ushort ResolveVillagerRenderKey(
            VillagerRenderRoleId? preferredRole,
            LocalVillagerJob.JobType? localJob = null,
            DotsVillagerJob.JobType? dotsJob = null)
        {
            if (preferredRole.HasValue && TryGetKeyForRole(preferredRole.Value, out var key))
            {
                return key;
            }

            if (localJob.HasValue && TryMapLocalJob(localJob.Value, out key))
            {
                return key;
            }

            if (dotsJob.HasValue && TryMapDotsJob(dotsJob.Value, out key))
            {
                return key;
            }

            return GodgameRenderKeys.Villager;
        }

        public static bool TryGetKeyForRole(VillagerRenderRoleId role, out ushort key)
        {
            key = GetRenderKeyForRole(role);
            return true;
        }

        private static bool TryMapLocalJob(LocalVillagerJob.JobType jobType, out ushort key)
        {
            switch (jobType)
            {
                case LocalVillagerJob.JobType.Gatherer:
                    key = GodgameRenderKeys.VillagerMiner;
                    return true;
                default:
                    key = GodgameRenderKeys.Villager;
                    return false;
            }
        }

        private static bool TryMapDotsJob(DotsVillagerJob.JobType jobType, out ushort key)
        {
            key = jobType switch
            {
                DotsVillagerJob.JobType.Farmer => GodgameRenderKeys.VillagerFarmer,
                DotsVillagerJob.JobType.Builder => GodgameRenderKeys.VillagerForester,
                DotsVillagerJob.JobType.Gatherer => GodgameRenderKeys.VillagerMiner,
                DotsVillagerJob.JobType.Hunter => GodgameRenderKeys.VillagerCombatant,
                DotsVillagerJob.JobType.Guard => GodgameRenderKeys.VillagerPeacekeeper,
                DotsVillagerJob.JobType.Priest => GodgameRenderKeys.VillagerWorshipper,
                DotsVillagerJob.JobType.Merchant => GodgameRenderKeys.VillagerBreeder,
                DotsVillagerJob.JobType.Crafter => GodgameRenderKeys.VillagerRefiner,
                _ => GodgameRenderKeys.Villager
            };

            return jobType != DotsVillagerJob.JobType.None;
        }
    }
}
