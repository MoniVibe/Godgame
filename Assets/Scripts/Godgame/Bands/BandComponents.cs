using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Godgame.Scenario;

namespace Godgame.Bands
{
    /// <summary>
    /// Status flags for band state.
    /// </summary>
    [Flags]
    public enum BandStatus : byte
    {
        None = 0,
        Forming = 1 << 0,
        Idle = 1 << 1,
        Moving = 1 << 2,
        Engaged = 1 << 3,
        Routing = 1 << 4,
        Resting = 1 << 5
    }

    /// <summary>
    /// Formation types for band tactical arrangements.
    /// </summary>
    public enum BandFormationType : byte
    {
        Column = 0,
        Line = 1,
        Wedge = 2,
        Circle = 3,
        Skirmish = 4,
        ShieldWall = 5
    }

    /// <summary>
    /// Purpose/type of band - determines behavior and skill requirements.
    /// Bands are NOT purely military; they form for any collective goal.
    /// See Docs/Concepts/Villagers/Band_Formation_And_Dynamics.md
    /// </summary>
    public enum BandPurpose : byte
    {
        // Military Bands
        Military_Warband = 0,
        Military_Defense = 1,
        Military_Mercenary = 2,

        // Logistics Bands
        Logistics_Caravan = 10,
        Logistics_Construction = 11,
        Logistics_Repair = 12,

        // Civilian Bands
        Civilian_Merchant = 20,
        Civilian_Entertainer = 21,
        Civilian_Artisan = 22,
        Civilian_Missionary = 23,
        Civilian_Adventuring = 24,

        // Work Groups
        Work_Hunting = 30,
        Work_Mining = 31,
        Work_Logging = 32,

        Custom = 255
    }

    /// <summary>
    /// Types of goals bands can pursue.
    /// </summary>
    public enum BandGoalType : byte
    {
        None = 0,
        Travel_To_Location = 1,
        Hunt_Creature = 2,
        Escort_Entity = 3,
        Defend_Location = 4,
        Trade_Route = 5,
        Build_Structure = 6,
        Explore_Region = 7,
        Complete_Quest = 8,
        Find_Resources = 9,
        Raid_Target = 10,
        Settle_Location = 11
    }

    /// <summary>
    /// Types of shared experiences that build bonds between members.
    /// </summary>
    public enum SharedExperienceType : byte
    {
        CombatVictory = 0,
        CombatDefeat = 1,
        MemberDeath = 2,
        ResourceCrisis = 3,
        QuestCompleted = 4,
        LongJourney = 5,
        Betrayal = 6,
        Rescue = 7,
        NearDeath = 8,
        GreatFeast = 9,
        DivineMiracle = 10
    }

    /// <summary>
    /// Roles within a band beyond simple membership.
    /// </summary>
    public enum BandRole : byte
    {
        Regular = 0,
        Leader = 1,
        StandardBearer = 2,
        Healer = 3,
        Scout = 4,
        Quartermaster = 5,
        Specialist = 6
    }

    /// <summary>
    /// Godgame-specific band component representing a group of villagers.
    /// Bridges to PureDOTS BandId and BandStats components.
    /// </summary>
    public struct Band : IComponentData
    {
        public int Id;
        public int FactionId;
        public Entity Leader;
        public float Morale;
        public float Cohesion;
        public float Fatigue;
        public BandStatus Status;
        public uint LastUpdateTick;

        // Godgame-specific fields
        public byte MinSize;           // Minimum members required to be effective
        public byte MaxSize;            // Maximum members allowed
        public Entity RecruitmentBuilding; // Armory/barracks where band was formed
        public float Experience;        // Band experience/veterancy level

        // New fields from concept doc
        public BandPurpose Purpose;     // What type of band this is
        public uint FormationTick;      // When the band was formed
        public FixedString64Bytes BandName; // Optional display name
    }

    /// <summary>
    /// Band's current objective/goal.
    /// </summary>
    public struct BandGoal : IComponentData
    {
        public BandGoalType Type;
        public float3 TargetLocation;   // If location-based goal
        public Entity TargetEntity;     // If entity-based goal (hunt creature, escort, etc.)
        public uint DeadlineTick;       // Optional deadline (0 = no deadline)
        public float Progress;          // 0-1 progress toward goal
        public bool IsCompleted;
    }

    /// <summary>
    /// Band evolution/transformation state tracking.
    /// </summary>
    public struct BandEvolutionState : IComponentData
    {
        public bool HasFamilies;           // Families present increases roaming village chance
        public bool OriginalGoalCompleted; // Has the founding goal been achieved
        public uint TicksAsRoamingVillage; // Time spent in roaming village mode
        public bool HasSettlementPlans;    // Members want to settle down
        public bool HasGuildBacking;       // Band has formed/joined a guild
        public Entity BackingGuildEntity;  // If guild formed
        public byte DissolutionRisk;       // 0-100 risk of disbanding
    }

    /// <summary>
    /// Band desperation level affects recruitment standards.
    /// </summary>
    public struct BandDesperation : IComponentData
    {
        /// <summary>
        /// 0-100 desperation level. Higher = lower recruitment standards.
        /// </summary>
        public byte Level;

        /// <summary>
        /// If true, band is below minimum functional size.
        /// </summary>
        public bool IsBelowMinSize;

        /// <summary>
        /// If true, band lacks a critical specialist role.
        /// </summary>
        public bool LacksCriticalRole;

        /// <summary>
        /// Tick when desperation was last updated.
        /// </summary>
        public uint LastUpdateTick;
    }

    /// <summary>
    /// Formation configuration for band tactical positioning.
    /// Bridges to PureDOTS BandFormation component.
    /// </summary>
    public struct BandFormation : IComponentData
    {
        public BandFormationType Formation;
        public float Spacing;          // Distance between members
        public float Width;            // Formation width
        public float Depth;            // Formation depth
        public float3 Facing;          // Direction the formation faces
        public float3 Anchor;          // Formation anchor point
        public float Stability;        // How well-formed the formation is (0-1)
        public uint LastSolveTick;     // Last time formation was recalculated
    }

    /// <summary>
    /// Buffer element tracking band members.
    /// </summary>
    [InternalBufferCapacity(20)]
    public struct BandMember : IBufferElementData
    {
        public Entity Villager;
        public BandRole Role;
        public uint JoinedTick;         // When member joined
        public bool IsDoubleAgent;      // Spy from another faction
        public byte LoyaltyScore;       // 0-100 loyalty to the band
    }

    /// <summary>
    /// Buffer tracking shared experiences between band members.
    /// Shared hardships build friendship and combat bonuses.
    /// </summary>
    [InternalBufferCapacity(10)]
    public struct SharedExperience : IBufferElementData
    {
        public SharedExperienceType Type;
        public uint OccurredTick;
        public Entity WitnessedWith;    // Other entity who shared this experience
        public float ImpactScore;       // How significant this experience was
    }

    /// <summary>
    /// Attached to individual entities who are band members.
    /// Reference back to their band for quick lookups.
    /// </summary>
    public struct BandMembership : IComponentData
    {
        public Entity BandEntity;
        public uint JoinedTick;
        public BandRole Role;
        public byte SharedExperienceCount; // Quick reference to bonding level
    }

    /// <summary>
    /// Band formation candidate - tracks entities considering forming a band.
    /// Created when 2+ entities with aligned goals meet.
    /// </summary>
    public struct BandFormationCandidate : IComponentData
    {
        public Entity InitiatorEntity;
        public BandPurpose ProposedPurpose;
        public uint ProposedTick;
        public byte ProspectiveMemberCount;
        public bool AllProspectsAccepted;
    }

    /// <summary>
    /// Buffer of entities being considered for band formation.
    /// </summary>
    [InternalBufferCapacity(10)]
    public struct BandFormationProspect : IBufferElementData
    {
        public Entity ProspectEntity;
        public bool HasAccepted;
        public float CompatibilityScore; // How well they fit with the proposed band
    }

    /// <summary>
    /// Join request sent from band leader to potential member.
    /// </summary>
    public struct BandJoinRequest : IComponentData
    {
        public Entity BandEntity;
        public Entity LeaderEntity;
        public Entity TargetEntity;
        public BandRole OfferedRole;
        public uint RequestTick;
        public uint ExpirationTick;
        public bool WasAccepted;
        public bool WasRejected;
    }

    /// <summary>
    /// Aggregate combat bonus from shared history.
    /// Recalculated when SharedExperience buffer changes.
    /// </summary>
    public struct BandCombatBonus : IComponentData
    {
        /// <summary>
        /// Bonus combat effectiveness from shared victories.
        /// Percentage modifier (e.g., 15 = +15%).
        /// </summary>
        public byte SharedVictoryBonus;

        /// <summary>
        /// Bonus from having friends in the band.
        /// Percentage modifier per friend.
        /// </summary>
        public byte FriendshipBonus;

        /// <summary>
        /// Vengeance bonus against a specific faction (if member died to them).
        /// </summary>
        public byte VengeanceBonus;

        /// <summary>
        /// Faction ID that killed a member (for vengeance targeting).
        /// </summary>
        public int VengeanceTargetFaction;
    }
}
