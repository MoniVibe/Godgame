using Unity.Entities;

namespace Godgame.AI
{
    /// <summary>
    /// Per-villager patriotism/loyalty tracking.
    /// Measures attachment to current settlement/band.
    /// From Docs/Concepts/Core/Sandbox_Autonomous_Villages.md
    /// </summary>
    public struct VillagerPatriotism : IComponentData
    {
        /// <summary>
        /// Current patriotism score (0-100).
        /// 100 = absolute loyalty (willing to die for aggregate).
        /// 0 = no loyalty (may desert/migrate at any moment).
        /// </summary>
        public byte Score;

        /// <summary>
        /// Entity of the village this villager is patriotic toward.
        /// </summary>
        public Entity VillageEntity;

        /// <summary>
        /// Entity of the band this villager is patriotic toward (if any).
        /// </summary>
        public Entity BandEntity;

        /// <summary>
        /// Ticks spent in current aggregate (contributes to patriotism).
        /// </summary>
        public uint TicksInAggregate;

        /// <summary>
        /// Number of family members in same aggregate (contributes to patriotism).
        /// </summary>
        public byte FamilyMembersInAggregate;

        /// <summary>
        /// Value of personal assets in aggregate (contributes to patriotism).
        /// </summary>
        public ushort PersonalAssetsValue;

        /// <summary>
        /// Whether villager's alignment matches aggregate's alignment.
        /// </summary>
        public bool AlignmentMatches;

        /// <summary>
        /// Whether villager's outlook matches aggregate's outlook.
        /// </summary>
        public bool OutlookMatches;

        /// <summary>
        /// Last tick when patriotism was recalculated.
        /// </summary>
        public uint LastUpdateTick;

        /// <summary>
        /// Creates default patriotism state.
        /// </summary>
        public static VillagerPatriotism Default => new VillagerPatriotism
        {
            Score = 50,
            VillageEntity = Entity.Null,
            BandEntity = Entity.Null,
            TicksInAggregate = 0,
            FamilyMembersInAggregate = 0,
            PersonalAssetsValue = 0,
            AlignmentMatches = true,
            OutlookMatches = true,
            LastUpdateTick = 0
        };

        /// <summary>
        /// Recalculates patriotism score from contributing factors.
        /// </summary>
        public void RecalculateScore()
        {
            float score = 30f; // Base score

            // Time contribution (up to +20 for long service)
            float timeBonus = Unity.Mathematics.math.min(20f, TicksInAggregate / 10000f);
            score += timeBonus;

            // Family contribution (up to +25 for 5+ family members)
            float familyBonus = Unity.Mathematics.math.min(25f, FamilyMembersInAggregate * 5f);
            score += familyBonus;

            // Assets contribution (up to +15 for valuable holdings)
            float assetsBonus = Unity.Mathematics.math.min(15f, PersonalAssetsValue / 100f);
            score += assetsBonus;

            // Alignment match (+5)
            if (AlignmentMatches) score += 5f;

            // Outlook match (+5)
            if (OutlookMatches) score += 5f;

            Score = (byte)Unity.Mathematics.math.clamp(score, 0f, 100f);
        }
    }

    /// <summary>
    /// Tag indicating villager is considering migration/desertion.
    /// Added when patriotism drops below threshold.
    /// </summary>
    public struct ConsideringMigrationTag : IComponentData
    {
        /// <summary>
        /// Tick when migration consideration started.
        /// </summary>
        public uint StartedTick;

        /// <summary>
        /// Target entity for potential migration (village or band).
        /// </summary>
        public Entity TargetEntity;
    }

    /// <summary>
    /// Tag indicating villager has deserted their aggregate.
    /// </summary>
    public struct DesertedTag : IComponentData
    {
        /// <summary>
        /// Entity villager deserted from.
        /// </summary>
        public Entity DesertedFrom;

        /// <summary>
        /// Tick when desertion occurred.
        /// </summary>
        public uint DesertedTick;
    }

    /// <summary>
    /// Global patriotism configuration singleton.
    /// </summary>
    public struct PatriotismConfig : IComponentData
    {
        /// <summary>
        /// Patriotism threshold below which migration is considered.
        /// </summary>
        public byte MigrationThreshold;

        /// <summary>
        /// Patriotism threshold below which desertion is possible.
        /// </summary>
        public byte DesertionThreshold;

        /// <summary>
        /// Ticks of consideration before actual migration.
        /// </summary>
        public uint MigrationConsiderationPeriod;

        /// <summary>
        /// Patriotism boost from shared victory.
        /// </summary>
        public byte SharedVictoryBoost;

        /// <summary>
        /// Patriotism penalty from shared defeat.
        /// </summary>
        public byte SharedDefeatPenalty;

        /// <summary>
        /// Patriotism boost from supportive leadership.
        /// </summary>
        public byte SupportiveLeadershipBoost;

        /// <summary>
        /// Patriotism penalty from abusive leadership.
        /// </summary>
        public byte AbusiveLeadershipPenalty;

        /// <summary>
        /// Creates default configuration.
        /// </summary>
        public static PatriotismConfig Default => new PatriotismConfig
        {
            MigrationThreshold = 30,
            DesertionThreshold = 15,
            MigrationConsiderationPeriod = 500,
            SharedVictoryBoost = 10,
            SharedDefeatPenalty = 5,
            SupportiveLeadershipBoost = 5,
            AbusiveLeadershipPenalty = 10
        };
    }
}

