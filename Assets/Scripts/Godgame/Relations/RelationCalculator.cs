using Godgame.Villagers;
using Unity.Mathematics;

namespace Godgame.Relations
{
    /// <summary>
    /// Calculator for initial relation values based on alignment, personality, and context.
    /// Implements formulas from Entity_Relations_And_Interactions.md
    /// </summary>
    public static class RelationCalculator
    {
        /// <summary>
        /// Calculate initial relation between two entities.
        /// </summary>
        public static sbyte CalculateInitialRelation(
            sbyte moralAxis1, sbyte orderAxis1, sbyte purityAxis1,
            sbyte moralAxis2, sbyte orderAxis2, sbyte purityAxis2,
            float vengeful1, float bold1,
            float vengeful2, float bold2,
            MeetingContext context,
            KinshipType kinship,
            uint randomSeed)
        {
            float total = 0f;

            // 1. Context offset
            total += EntityRelation.GetContextOffset(context);

            // 2. Kinship bonus (overrides most other factors)
            if (kinship != KinshipType.None)
            {
                total += EntityRelation.GetKinshipBonus(kinship);
            }

            // 3. Alignment compatibility
            total += CalculateMoralAxisDelta(moralAxis1, moralAxis2);
            total += CalculateOrderAxisDelta(orderAxis1, orderAxis2, randomSeed);
            total += CalculatePurityAxisDelta(purityAxis1, purityAxis2);

            // 4. Behavioral modifiers
            total += CalculateBehavioralModifier(vengeful1, bold1, vengeful2, bold2);

            // 5. Random factor
            var rng = new Random(randomSeed);
            bool eitherChaotic = orderAxis1 < -30 || orderAxis2 < -30;
            float randomRange = eitherChaotic ? 20f : 10f;
            total += rng.NextFloat(-randomRange, randomRange);

            // Clamp to valid range
            return (sbyte)math.clamp(math.round(total), -100, 100);
        }

        /// <summary>
        /// Calculate moral axis compatibility (Good/Evil).
        /// </summary>
        public static float CalculateMoralAxisDelta(sbyte moral1, sbyte moral2)
        {
            int delta = math.abs(moral1 - moral2);

            bool bothGood = moral1 > 30 && moral2 > 30;
            bool bothEvil = moral1 < -30 && moral2 < -30;
            bool opposite = (moral1 > 30 && moral2 < -30) || (moral1 < -30 && moral2 > 30);

            if (bothGood)
                return 20f - (delta * 0.2f);
            if (bothEvil)
                return 15f - (delta * 0.15f);
            if (opposite)
                return -(delta * 0.3f);

            return 0f;
        }

        /// <summary>
        /// Calculate order axis compatibility (Lawful/Chaotic).
        /// </summary>
        public static float CalculateOrderAxisDelta(sbyte order1, sbyte order2, uint seed)
        {
            int delta = math.abs(order1 - order2);

            bool bothLawful = order1 > 30 && order2 > 30;
            bool bothChaotic = order1 < -30 && order2 < -30;
            bool opposite = (order1 > 30 && order2 < -30) || (order1 < -30 && order2 > 30);

            if (bothLawful)
                return 15f - (delta * 0.1f);

            if (bothChaotic)
            {
                // Chaotic entities are unpredictable
                var rng = new Random(seed + 12345);
                return rng.NextFloat(-10f, 10f);
            }

            if (opposite)
                return -(delta * 0.2f);

            return 0f;
        }

        /// <summary>
        /// Calculate purity axis compatibility (Pure/Corrupt).
        /// </summary>
        public static float CalculatePurityAxisDelta(sbyte purity1, sbyte purity2)
        {
            int delta = math.abs(purity1 - purity2);

            bool bothPure = purity1 > 30 && purity2 > 30;
            bool bothCorrupt = purity1 < -30 && purity2 < -30;
            bool opposite = (purity1 > 30 && purity2 < -30) || (purity1 < -30 && purity2 > 30);

            if (bothPure)
                return 10f - (delta * 0.05f);

            if (bothCorrupt)
                return -(delta * 0.1f); // Corrupt entities compete

            if (opposite)
            {
                // Pure sees corrupt negatively, corrupt doesn't care as much
                if (purity1 > 30)
                    return -(delta * 0.15f);
                else
                    return -(delta * 0.05f);
            }

            return 0f;
        }

        /// <summary>
        /// Calculate behavioral personality modifier.
        /// </summary>
        public static float CalculateBehavioralModifier(
            float vengeful1, float bold1,
            float vengeful2, float bold2)
        {
            float modifier = 0f;

            // Forgiving entities give benefit of doubt
            if (vengeful1 < -60)
                modifier += 10f;
            if (vengeful2 < -60)
                modifier += 5f;

            // Vengeful entities are suspicious
            if (vengeful1 > 60)
                modifier -= 5f;
            if (vengeful2 > 60)
                modifier -= 2.5f;

            // Bold + Bold = mutual respect
            if (bold1 > 60 && bold2 > 60)
                modifier += 8f;

            // Craven + Craven = mutual understanding
            if (bold1 < -60 && bold2 < -60)
                modifier += 5f;

            // Bold + Craven = poor match
            if ((bold1 > 60 && bold2 < -60) || (bold1 < -60 && bold2 > 60))
                modifier -= 6f;

            return modifier;
        }

        /// <summary>
        /// Calculate relation change from shared combat experience.
        /// </summary>
        public static sbyte GetCombatBondingDelta(bool sameTeam, bool oneProtectedOther)
        {
            if (!sameTeam)
                return -10;

            sbyte delta = 5; // Base combat bonding

            if (oneProtectedOther)
                delta += 10; // Saved their life

            return delta;
        }

        /// <summary>
        /// Calculate miracle impact on god relation.
        /// </summary>
        public static sbyte GetMiracleImpact(bool beneficial, bool directTarget, bool witnessed)
        {
            if (directTarget)
            {
                return beneficial ? (sbyte)20 : (sbyte)-40;
            }

            if (witnessed)
            {
                return beneficial ? (sbyte)5 : (sbyte)-15;
            }

            return 0;
        }

        /// <summary>
        /// Get obedience probability based on god relation.
        /// </summary>
        public static float GetObedienceProbability(sbyte godRelation)
        {
            if (godRelation >= 80) return 0.95f;
            if (godRelation >= 50) return 0.70f;
            if (godRelation >= 20) return 0.40f;
            if (godRelation >= 0) return 0.10f;
            if (godRelation >= -30) return 0f;
            return 0f; // Hostile entities actively resist
        }
    }
}

