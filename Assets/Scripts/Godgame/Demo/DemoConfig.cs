using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Demo
{
    /// <summary>
    /// ScriptableObject configuration for demo spawning parameters.
    /// Create an asset and assign it to DemoConfigAuthoring to configure demo content.
    /// </summary>
    [CreateAssetMenu(fileName = "DemoConfig", menuName = "Godgame/Demo Config", order = 1)]
    public class DemoConfig : ScriptableObject
    {
        [Header("Scenario Mode")]
        [Tooltip("Which scenario mode to use")]
        public DemoScenarioMode Mode = DemoScenarioMode.Demo01;

        [Header("Village Settings")]
        [Tooltip("Number of villages to spawn")]
        public int VillageCount = 3;

        [Tooltip("Minimum villagers per village")]
        public int VillagersPerVillageMin = 20;

        [Tooltip("Maximum villagers per village")]
        public int VillagersPerVillageMax = 40;

        [Tooltip("Spacing between villages")]
        public float VillageSpacing = 80f;

        [Tooltip("Influence radius for each village")]
        public float VillageInfluenceRadius = 40f;

        [Header("Resource Settings")]
        [Tooltip("Number of resource nodes to spawn")]
        public int ResourceNodeCount = 30;

        [Tooltip("Number of resource chunks to spawn")]
        public int ResourceChunkCount = 20;

        [Header("Random Seed")]
        [Tooltip("Random seed for deterministic spawning")]
        public uint RandomSeed = 12345;
    }

    /// <summary>
    /// Blob data structure for demo config (baked from ScriptableObject).
    /// </summary>
    public struct DemoConfigBlob
    {
        public DemoScenarioMode Mode;
        public int VillageCount;
        public int VillagersPerVillageMin;
        public int VillagersPerVillageMax;
        public float VillageSpacing;
        public float VillageInfluenceRadius;
        public int ResourceNodeCount;
        public int ResourceChunkCount;
        public uint RandomSeed;
    }

    /// <summary>
    /// Component storing reference to demo config blob.
    /// </summary>
    public struct DemoConfigBlobReference : IComponentData
    {
        public BlobAssetReference<DemoConfigBlob> Config;
    }
}

