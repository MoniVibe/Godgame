using Unity.Collections;
using Unity.Entities;

namespace Godgame.Relations
{
    /// <summary>
    /// Relation tier thresholds.
    /// See Docs/Concepts/Villagers/Entity_Relations_And_Interactions.md
    /// </summary>
    public enum RelationTier : byte
    {
        /// <summary>-100 to -80: Seek death/destruction</summary>
        MortalEnemy = 0,
        /// <summary>-79 to -50: Active opposition</summary>
        Hostile = 1,
        /// <summary>-49 to -25: Distrust, avoid cooperation</summary>
        Unfriendly = 2,
        /// <summary>-24 to +24: No strong feelings</summary>
        Neutral = 3,
        /// <summary>+25 to +49: Trust, cooperate willingly</summary>
        Friendly = 4,
        /// <summary>+50 to +79: Mutual support, sacrifice</summary>
        CloseFriend = 5,
        /// <summary>+80 to +100: Unwavering loyalty</summary>
        Devoted = 6
    }

    /// <summary>
    /// Context in which two entities first met.
    /// Affects initial relation calculation.
    /// </summary>
    public enum MeetingContext : byte
    {
        /// <summary>Same village, offset: 0</summary>
        VillageNeighbor = 0,
        /// <summary>Same workplace, offset: +10</summary>
        Workplace = 1,
        /// <summary>Family introduction, offset: +20</summary>
        FamilyIntroduction = 2,
        /// <summary>Festival/social event, offset: +5</summary>
        FestivalSocial = 3,
        /// <summary>Combat on same side, offset: +15</summary>
        CombatSameSide = 4,
        /// <summary>Combat on opposing sides, offset: -30</summary>
        CombatOpposing = 5,
        /// <summary>Adventuring/guild, offset: +10</summary>
        Adventuring = 6,
        /// <summary>Military conscription, offset: +5</summary>
        Conscription = 7,
        /// <summary>Diplomatic meeting, offset: 0</summary>
        Diplomatic = 8,
        /// <summary>Crime victim, offset: -50</summary>
        CrimeVictim = 9,
        /// <summary>Rescue/salvation, offset: +40</summary>
        RescueSalvation = 10
    }

    /// <summary>
    /// Kinship type for familial relations.
    /// </summary>
    public enum KinshipType : byte
    {
        None = 0,
        ParentChild = 1,    // +80 base
        Sibling = 2,        // +60 base
        Spouse = 3,         // +70 base
        Grandparent = 4,    // +50 base
        UncleAunt = 5,      // +40 base
        Cousin = 6,         // +30 base
        Dynasty = 7,        // +20 base
        Disowned = 8        // -40 base
    }

    /// <summary>
    /// Relation between this entity and another.
    /// Buffer element for sparse storage (only store met entities).
    /// </summary>
    [InternalBufferCapacity(8)]
    public struct EntityRelation : IBufferElementData
    {
        /// <summary>
        /// The entity this relation is with.
        /// </summary>
        public Entity OtherEntity;

        /// <summary>
        /// Relation value from -100 (mortal enemy) to +100 (devoted).
        /// </summary>
        public sbyte RelationValue;

        /// <summary>
        /// Computed tier for quick threshold checks.
        /// </summary>
        public RelationTier Tier;

        /// <summary>
        /// Game tick when entities first met.
        /// </summary>
        public uint FirstMetTick;

        /// <summary>
        /// How they first met.
        /// </summary>
        public MeetingContext Context;

        /// <summary>
        /// Game tick of last interaction.
        /// </summary>
        public uint LastInteractionTick;

        /// <summary>
        /// Count of positive interactions.
        /// </summary>
        public ushort PositiveInteractions;

        /// <summary>
        /// Count of negative interactions.
        /// </summary>
        public ushort NegativeInteractions;

        /// <summary>
        /// Shared experiences (battles, festivals, work).
        /// </summary>
        public ushort SharedExperiences;

        /// <summary>
        /// Kinship type if family relation.
        /// </summary>
        public KinshipType Kinship;

        /// <summary>
        /// Whether this is a romantic relation.
        /// </summary>
        public bool IsRomantic;

        /// <summary>
        /// Whether this is a professional/work relation.
        /// </summary>
        public bool IsProfessional;

        /// <summary>
        /// Modify relation value (clamped to -100..+100).
        /// </summary>
        public void ModifyRelation(int delta, uint tick)
        {
            int newValue = RelationValue + delta;
            RelationValue = (sbyte)Unity.Mathematics.math.clamp(newValue, -100, 100);
            LastInteractionTick = tick;
            UpdateTier();

            if (delta > 0)
                PositiveInteractions++;
            else if (delta < 0)
                NegativeInteractions++;
        }

        /// <summary>
        /// Update tier based on current value.
        /// </summary>
        public void UpdateTier()
        {
            Tier = GetTier(RelationValue);
        }

        /// <summary>
        /// Get tier for a relation value.
        /// </summary>
        public static RelationTier GetTier(sbyte value)
        {
            if (value <= -80) return RelationTier.MortalEnemy;
            if (value <= -50) return RelationTier.Hostile;
            if (value <= -25) return RelationTier.Unfriendly;
            if (value <= 24) return RelationTier.Neutral;
            if (value <= 49) return RelationTier.Friendly;
            if (value <= 79) return RelationTier.CloseFriend;
            return RelationTier.Devoted;
        }

        /// <summary>
        /// Get meeting context offset for initial relation.
        /// </summary>
        public static sbyte GetContextOffset(MeetingContext context)
        {
            return context switch
            {
                MeetingContext.VillageNeighbor => 0,
                MeetingContext.Workplace => 10,
                MeetingContext.FamilyIntroduction => 20,
                MeetingContext.FestivalSocial => 5,
                MeetingContext.CombatSameSide => 15,
                MeetingContext.CombatOpposing => -30,
                MeetingContext.Adventuring => 10,
                MeetingContext.Conscription => 5,
                MeetingContext.Diplomatic => 0,
                MeetingContext.CrimeVictim => -50,
                MeetingContext.RescueSalvation => 40,
                _ => 0
            };
        }

        /// <summary>
        /// Get kinship bonus for familial relations.
        /// </summary>
        public static sbyte GetKinshipBonus(KinshipType kinship)
        {
            return kinship switch
            {
                KinshipType.ParentChild => 80,
                KinshipType.Sibling => 60,
                KinshipType.Spouse => 70,
                KinshipType.Grandparent => 50,
                KinshipType.UncleAunt => 40,
                KinshipType.Cousin => 30,
                KinshipType.Dynasty => 20,
                KinshipType.Disowned => -40,
                _ => 0
            };
        }

        /// <summary>
        /// Create a new relation from first meeting.
        /// </summary>
        public static EntityRelation Create(
            Entity other,
            sbyte initialValue,
            MeetingContext context,
            uint tick,
            KinshipType kinship = KinshipType.None)
        {
            var relation = new EntityRelation
            {
                OtherEntity = other,
                RelationValue = initialValue,
                Context = context,
                FirstMetTick = tick,
                LastInteractionTick = tick,
                PositiveInteractions = 0,
                NegativeInteractions = 0,
                SharedExperiences = 0,
                Kinship = kinship,
                IsRomantic = false,
                IsProfessional = context == MeetingContext.Workplace
            };
            relation.UpdateTier();
            return relation;
        }
    }

    /// <summary>
    /// Component tracking entity's relation to the player god.
    /// Every entity has feelings toward the deity.
    /// </summary>
    public struct GodRelation : IComponentData
    {
        /// <summary>
        /// Relation to god from -100 (hostile) to +100 (devoted).
        /// </summary>
        public sbyte RelationToGod;

        /// <summary>
        /// Prayer frequency (0 = never, 100 = constant).
        /// </summary>
        public byte PrayerFrequency;

        /// <summary>
        /// Obedience level (0 = rebel, 100 = devout).
        /// </summary>
        public byte Obedience;

        /// <summary>
        /// Last tick when witnessed a miracle.
        /// </summary>
        public uint LastMiracleWitnessedTick;

        /// <summary>
        /// Inherited influence from parents.
        /// </summary>
        public sbyte ParentalInfluence;

        /// <summary>
        /// Modify god relation (clamped).
        /// </summary>
        public void ModifyRelation(int delta)
        {
            int newValue = RelationToGod + delta;
            RelationToGod = (sbyte)Unity.Mathematics.math.clamp(newValue, -100, 100);
        }

        /// <summary>
        /// Default god relation (neutral).
        /// </summary>
        public static GodRelation Default => new GodRelation
        {
            RelationToGod = 0,
            PrayerFrequency = 50,
            Obedience = 50,
            LastMiracleWitnessedTick = 0,
            ParentalInfluence = 0
        };
    }

    /// <summary>
    /// Tag for entities that track relations.
    /// </summary>
    public struct HasRelationsTag : IComponentData { }

    /// <summary>
    /// Request to create a relation between two entities.
    /// </summary>
    public struct CreateRelationRequest : IComponentData
    {
        public Entity Entity1;
        public Entity Entity2;
        public MeetingContext Context;
        public KinshipType Kinship;
    }

    /// <summary>
    /// Request to modify an existing relation.
    /// </summary>
    public struct ModifyRelationRequest : IComponentData
    {
        public Entity SourceEntity;
        public Entity TargetEntity;
        public sbyte Delta;
    }

    /// <summary>
    /// Event log for significant relation changes.
    /// </summary>
    [InternalBufferCapacity(4)]
    public struct RelationEvent : IBufferElementData
    {
        public Entity OtherEntity;
        public uint EventTick;
        public FixedString32Bytes EventType;
        public sbyte RelationChange;
        public sbyte RelationAfter;
    }

    /// <summary>
    /// Configuration for relation system.
    /// </summary>
    public struct RelationSystemConfig : IComponentData
    {
        /// <summary>
        /// Distance threshold for meeting detection.
        /// </summary>
        public float MeetingDistance;

        /// <summary>
        /// Whether to track relation events.
        /// </summary>
        public bool TrackEvents;

        /// <summary>
        /// Maximum events to keep per entity.
        /// </summary>
        public int MaxEventsPerEntity;

        /// <summary>
        /// Default configuration.
        /// </summary>
        public static RelationSystemConfig Default => new RelationSystemConfig
        {
            MeetingDistance = 10f,
            TrackEvents = true,
            MaxEventsPerEntity = 10
        };
    }
}

