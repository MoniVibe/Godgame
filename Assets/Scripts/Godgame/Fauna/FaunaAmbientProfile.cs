using System;
using System.Collections.Generic;
using Godgame.Environment;
using UnityEngine;

namespace Godgame.Fauna
{
    /// <summary>
    /// Designer-authored catalogue describing which ambient creatures to spawn per biome/time-of-day.
    /// </summary>
    [CreateAssetMenu(menuName = "Godgame/Fauna/Ambient Profile", fileName = "FaunaAmbientProfile")]
    public sealed class FaunaAmbientProfile : ScriptableObject
    {
        [Serializable]
        public sealed class SpawnRule
        {
            [Tooltip("Friendly identifier for debugging.")]
            public string id = "critters";

            [Tooltip("Descriptor key consumed by the presentation service (e.g., godgame.fauna.wolf).")]
            public string descriptorKey = "godgame.fauna.generic";

            [Tooltip("Optional descriptor key for miracle/VFX bindings when this rule spawns miracle-linked ambience.")]
            public string miracleDescriptorKey = "godgame.miracle.fauna.glow";

            [Tooltip("Optional fallback prefab if no descriptor variant has been assigned yet.")]
            public GameObject fallbackPrefab;

            [Tooltip("Optional AudioClip to fire via the ambient sound system.")]
            public AudioClip ambientClip;

            [Min(0f)]
            [Tooltip("Radius (in meters) around the volume center used when selecting spawn points for this rule.")]
            public float spawnRadius = 6f;

            [Tooltip("Idle/patrol wander radius per agent.")]
            [Min(1f)]
            public float wanderRadius = 4f;

            [Tooltip("Seconds the creature idles before pacing again.")]
            [Range(0.5f, 30f)]
            public float baseIdleDuration = 4f;

            [Tooltip("Movement speed in meters/second for DOTS-driven loops.")]
            [Range(0.5f, 8f)]
            public float moveSpeed = 2f;

            [Tooltip("Minimum/maximum simultaneous spawns when this rule fires.")]
            public Vector2Int spawnCount = new(1, 2);

            [Tooltip("Should we rely on third-party controllers such as Emerald AI for behaviour?")]
            public bool useEmeraldAi;

            [Tooltip("Limit to specific biomes.")]
            public BiomeMask allowedBiomes = BiomeMask.All;

            [Tooltip("Time-of-day window determining when this spawn rule is active.")]
            public TimeOfDayWindow activityWindow = TimeOfDayWindow.Daytime;

            [Tooltip("Set true for nocturnal creatures. Convenience flag for queries/UI.")]
            public bool nocturnal;

            [Tooltip("Seconds between automatic ambient audio cues. 0 disables.")]
            [Min(0f)]
            public float ambientSoundInterval = 12f;
        }

        [SerializeField] private List<SpawnRule> m_Rules = new();

        public IReadOnlyList<SpawnRule> Rules => m_Rules;
    }
}
