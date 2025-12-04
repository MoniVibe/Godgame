using PureDOTS.Runtime.Identity;

namespace Godgame.Identity
{
    /// <summary>
    /// Godgame-specific identity extensions and interpretations.
    /// </summary>
    public static class GodgameIdentityExtensions
    {
        /// <summary>
        /// Godgame purity interpretation: Pure altruist ↔ Corrupt selfish/demonic
        /// </summary>
        public static string GetPurityDescription(float purity)
        {
            if (purity > 50f)
                return "Pure Altruist";
            if (purity > 20f)
                return "Altruistic";
            if (purity > -20f)
                return "Neutral";
            if (purity > -50f)
                return "Selfish";
            return "Corrupt/Demonic";
        }

        /// <summary>
        /// Godgame outlook tags: Warlike, Peaceful, Spiritual, Materialistic, Scholarly
        /// (Uses base OutlookType enum, no extensions needed)
        /// </summary>

        /// <summary>
        /// Godgame might/magic interpretation: Warriors/siege engines ↔ Mages/clerics/shamans
        /// </summary>
        public static string GetMightMagicDescription(float axis)
        {
            if (axis < -30f)
                return "Might-focused (Warriors, Siege Engines, Physical Training)";
            if (axis > 30f)
                return "Magic-focused (Mages, Clerics, Shamans, Divine Miracles)";
            return "Hybrid (Uses both steel and spell)";
        }
    }
}

