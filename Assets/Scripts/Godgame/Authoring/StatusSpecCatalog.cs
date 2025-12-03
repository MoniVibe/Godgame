using System;
using Godgame.Abilities;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Authoring
{
    /// <summary>
    /// ScriptableObject catalog for status effect specifications.
    /// Baked into blob asset for runtime PureDOTS systems.
    /// </summary>
    [CreateAssetMenu(fileName = "StatusSpecCatalog", menuName = "Godgame/Status Spec Catalog")]
    public sealed class StatusSpecCatalog : ScriptableObject
    {
        [Serializable]
        public struct StatusSpecDefinition
        {
            [Tooltip("Unique status identifier")]
            public string Id;

            [Tooltip("Maximum stack count (1 = no stacking)")]
            public byte MaxStacks;

            [Tooltip("Can this status be dispelled?")]
            public bool Dispellable;

            [Tooltip("Dispel tags")]
            public DispelTags DispelTags;

            [Tooltip("Duration in seconds (0 = permanent)")]
            public float Duration;

            [Tooltip("Periodic tick interval in seconds (0 = no periodic)")]
            public float Period;
        }

        [Tooltip("Status definitions")]
        public StatusSpecDefinition[] Statuses = Array.Empty<StatusSpecDefinition>();

    }
}

