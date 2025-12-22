using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Scenario
{
    /// <summary>
    /// ScriptableObject configuration for scenario spawning parameters.
    /// Create an asset and assign it to GodgameScenarioConfigAuthoring to configure scenario content.
    /// </summary>
    [CreateAssetMenu(fileName = "GodgameScenarioConfig", menuName = "Godgame/Scenario Config", order = 1)]
    public class GodgameScenarioConfig : ScriptableObject
    {
        [Header("Scenario Mode")]
        [Tooltip("Which scenario mode to use")]
        public GodgameScenarioMode Mode = GodgameScenarioMode.Scenario01;

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
    /// Blob data structure for scenario config (baked from ScriptableObject).
    /// </summary>
    public struct GodgameScenarioConfigBlob
    {
        public GodgameScenarioMode Mode;
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
    /// Component storing reference to scenario config blob.
    /// </summary>
    public struct GodgameScenarioConfigBlobReference : IComponentData
    {
        public BlobAssetReference<GodgameScenarioConfigBlob> Config;
    }
}
