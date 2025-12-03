using System;
using Godgame.Villages;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Authoring
{
    /// <summary>
    /// ScriptableObject defining initiative bands (Slow, Measured, Bold, Reckless) with tick budgets and stress breakpoints.
    /// Baked into blob asset for runtime PureDOTS systems.
    /// </summary>
    [CreateAssetMenu(fileName = "InitiativeBandTable", menuName = "Godgame/Initiative Band Table")]
    public sealed class InitiativeBandTable : ScriptableObject
    {
        [Serializable]
        public struct InitiativeBandDefinition
        {
            [Tooltip("Band identifier")]
            public string BandId;

            [Tooltip("Deterministic tick budget between major actions")]
            public uint TickBudget;

            [Tooltip("Recovery half-life in ticks (falloff toward baseline)")]
            public uint RecoveryHalfLife;

            [Tooltip("Stress breakpoints: panic, rally, frenzy thresholds")]
            public StressBreakpoints StressBreakpoints;
        }

        [Serializable]
        public struct StressBreakpoints
        {
            [Tooltip("Panic threshold (initiative drops below this)")]
            [Range(-100, 0)]
            public int Panic;

            [Tooltip("Rally threshold (initiative rises above this)")]
            [Range(0, 100)]
            public int Rally;

            [Tooltip("Frenzy threshold (initiative exceeds this, optional)")]
            [Range(0, 100)]
            public int Frenzy;
        }

        [Tooltip("Initiative band definitions")]
        public InitiativeBandDefinition[] Bands = Array.Empty<InitiativeBandDefinition>();

    }

}
