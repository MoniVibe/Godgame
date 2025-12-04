using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Behavioral personality traits for villagers.
    /// These axes determine HOW villagers respond emotionally, not WHAT they value morally.
    /// See Docs/Concepts/Villagers/Villager_Behavioral_Personality.md for full spec.
    /// </summary>
    public struct VillagerBehavior : IComponentData
    {
        /// <summary>
        /// Response to harm, betrayal, and loss.
        /// -100 (forgiving: lets go quickly, seeks reconciliation)
        /// +100 (vengeful: holds grudges, seeks retribution)
        /// </summary>
        public float VengefulScore;

        /// <summary>
        /// Response to danger and risk tolerance.
        /// -100 (craven: flees first, avoids risk, defensive)
        /// +100 (bold: seeks challenges, charges forward, offensive)
        /// </summary>
        public float BoldScore;

        /// <summary>
        /// Computed modifier applied to base initiative for autonomous decisions.
        /// Higher values = more frequent life-changing decisions.
        /// </summary>
        public float InitiativeModifier;

        /// <summary>
        /// Quick reference count of active unresolved grudges.
        /// Grudges boost initiative while seeking revenge.
        /// </summary>
        public byte ActiveGrudgeCount;

        /// <summary>
        /// Tick when villager last made a major autonomous decision
        /// (family formation, career change, business venture, etc.)
        /// </summary>
        public uint LastMajorActionTick;

        /// <summary>
        /// Creates a neutral behavior profile.
        /// </summary>
        public static VillagerBehavior Neutral => new VillagerBehavior
        {
            VengefulScore = 0f,
            BoldScore = 0f,
            InitiativeModifier = 0f,
            ActiveGrudgeCount = 0,
            LastMajorActionTick = 0
        };

        /// <summary>
        /// Creates a randomized behavior profile within the given range.
        /// </summary>
        public static VillagerBehavior Random(ref Unity.Mathematics.Random random, float range = 60f)
        {
            return new VillagerBehavior
            {
                VengefulScore = random.NextFloat(-range, range),
                BoldScore = random.NextFloat(-range, range),
                InitiativeModifier = 0f,
                ActiveGrudgeCount = 0,
                LastMajorActionTick = 0
            };
        }

        /// <summary>
        /// Recalculates the initiative modifier based on current traits.
        /// Bold villagers act more frequently; craven less so.
        /// </summary>
        public void RecalculateInitiative()
        {
            // Bold adds up to +0.12 initiative, craven subtracts up to -0.10
            InitiativeModifier = BoldScore * 0.002f;
        }

        /// <summary>
        /// Gets the grudge intensity boost to initiative.
        /// Active grudges increase action frequency while seeking revenge.
        /// </summary>
        public float GetGrudgeInitiativeBoost(float totalGrudgeIntensity)
        {
            return totalGrudgeIntensity * 0.002f;
        }

        /// <summary>
        /// Gets the grudge decay rate per day based on vengeful score.
        /// Forgiving villagers forget faster; vengeful hold grudges longer.
        /// </summary>
        public float GetGrudgeDecayRatePerDay()
        {
            // Formula: 0.01 × (100 + VengefulScore)
            // Forgiving (-100): 0.01 × 0 = 0 decay (instant forgiveness handled separately)
            // Neutral (0): 0.01 × 100 = 1.0 decay per day
            // Vengeful (+100): 0.01 × 200 = 2.0 decay per day (but higher starting intensity)
            // Note: Forgiving villagers start with dampened intensity, so they still forget faster overall
            return 0.01f * (100f + VengefulScore);
        }

        /// <summary>
        /// Calculates grudge intensity dampening for forgiving villagers.
        /// Forgiving villagers feel wrongs less intensely.
        /// </summary>
        public float GetGrudgeIntensityMultiplier()
        {
            if (VengefulScore > 0f)
            {
                // Vengeful: full intensity
                return 1f;
            }
            // Forgiving: dampened to 30% at max forgiveness
            return 1f - ((-VengefulScore / 100f) * 0.7f);
        }
    }

    /// <summary>
    /// Combat behavior modifiers derived from Bold/Craven axis.
    /// Applied by combat systems when processing attacks.
    /// </summary>
    public struct VillagerCombatBehavior : IComponentData
    {
        /// <summary>
        /// Multiplier to engage range. Bold: +50%, Craven: -50%
        /// </summary>
        public float EngageRangeModifier;

        /// <summary>
        /// HP threshold at which villager considers retreating.
        /// Bold: 30 HP, Craven: 60 HP
        /// </summary>
        public byte RetreatThreshold;

        /// <summary>
        /// Dodge chance modifier. Bold: -10%, Craven: +20%
        /// </summary>
        public float DodgeChanceModifier;

        /// <summary>
        /// Damage output modifier. Bold: +15%, Craven: -10%
        /// </summary>
        public float DamageModifier;

        /// <summary>
        /// Morale aura effect on nearby allies. Bold: +5, Craven: -5
        /// </summary>
        public sbyte MoraleAura;

        /// <summary>
        /// Creates combat modifiers from behavior traits.
        /// </summary>
        public static VillagerCombatBehavior FromBehavior(in VillagerBehavior behavior)
        {
            var boldNormalized = behavior.BoldScore / 100f; // -1 to +1

            return new VillagerCombatBehavior
            {
                EngageRangeModifier = 1f + (boldNormalized * 0.5f),
                RetreatThreshold = (byte)(45 - (boldNormalized * 15)), // 30-60
                DodgeChanceModifier = -boldNormalized * 0.15f, // Bold: -15%, Craven: +15%
                DamageModifier = 1f + (boldNormalized * 0.125f), // Bold: +12.5%, Craven: -12.5%
                MoraleAura = (sbyte)(boldNormalized * 5)
            };
        }
    }
}

