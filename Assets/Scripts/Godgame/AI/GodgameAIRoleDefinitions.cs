using Godgame.Rendering;
using PureDOTS.Runtime.AI;

namespace Godgame.AI
{
    /// <summary>
    /// Maps render/job roles to shared AI role/doctrine/profile identifiers so telemetry can reason about cohorts headlessly.
    /// </summary>
    public static class GodgameAIRoleDefinitions
    {
        public const ushort RoleCivilian = 1;
        public const ushort RolePeacekeeper = 2;
        public const ushort RoleArmy = 3;
        public const ushort RoleBand = 4;

        public const ushort DoctrineCivilian = 10;
        public const ushort DoctrinePeacekeeper = 11;
        public const ushort DoctrineArmy = 12;
        public const ushort DoctrineBand = 13;

        public const ushort ProfileCivilian = 100;
        public const ushort ProfilePeacekeeper = 110;
        public const ushort ProfileArmy = 120;
        public const ushort ProfileBand = 130;

        public const byte SourceScenario = 1;

        public static RoleProfileAssignment ResolveForVillager(VillagerRenderRoleId role)
        {
            return role switch
            {
                VillagerRenderRoleId.Peacekeeper => new RoleProfileAssignment(RolePeacekeeper, DoctrinePeacekeeper, ProfilePeacekeeper, ProfilePeacekeeperHash),
                VillagerRenderRoleId.Combatant => new RoleProfileAssignment(RoleArmy, DoctrineArmy, ProfileArmy, ProfileArmyHash),
                _ => new RoleProfileAssignment(RoleCivilian, DoctrineCivilian, ProfileCivilian, ProfileCivilianHash)
            };
        }

        public static RoleProfileAssignment ResolveForBand()
        {
            return new RoleProfileAssignment(RoleBand, DoctrineBand, ProfileBand, ProfileBandHash);
        }

        private const uint ProfileCivilianHash = 0x4A0C3F57;
        private const uint ProfilePeacekeeperHash = 0x67C4A2F1;
        private const uint ProfileArmyHash = 0x8DBB12C3;
        private const uint ProfileBandHash = 0x5F2189AD;

        public readonly struct RoleProfileAssignment
        {
            public RoleProfileAssignment(ushort roleId, ushort doctrineId, ushort profileId, uint profileHash)
            {
                RoleId = roleId;
                DoctrineId = doctrineId;
                ProfileId = profileId;
                ProfileHash = profileHash;
            }

            public ushort RoleId { get; }
            public ushort DoctrineId { get; }
            public ushort ProfileId { get; }
            public uint ProfileHash { get; }
        }
    }
}
