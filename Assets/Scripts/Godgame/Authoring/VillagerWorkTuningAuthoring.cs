using Godgame.Villagers;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Authoring
{
    /// <summary>
    /// Authoring tunables for villager work roles and hauling behavior.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class VillagerWorkTuningAuthoring : MonoBehaviour
    {
        [Header("Resource Mapping")]
        public string foresterInputId = "wood";
        public string foresterOutputId = "lumber";
        public string minerOutputId = "ore";
        public string farmerOutputId = "grain";

        [Header("Hauling")]
        [Range(0f, 1f)] public float haulChance = 0.2f;
        [Range(0.1f, 60f)] public float haulCooldownSeconds = 8f;
        [Range(0.1f, 50f)] public float pileDropMinUnits = 6f;
        [Range(1f, 200f)] public float pileDropMaxUnits = 30f;
        [Range(0.1f, 50f)] public float pilePickupMinUnits = 3f;
        [Range(5f, 250f)] public float pileSearchRadius = 60f;

        private sealed class Baker : Baker<VillagerWorkTuningAuthoring>
        {
            public override void Bake(VillagerWorkTuningAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new VillagerWorkTuning
                {
                    ForesterInputId = new FixedString64Bytes(authoring.foresterInputId ?? string.Empty),
                    ForesterOutputId = new FixedString64Bytes(authoring.foresterOutputId ?? string.Empty),
                    MinerOutputId = new FixedString64Bytes(authoring.minerOutputId ?? string.Empty),
                    FarmerOutputId = new FixedString64Bytes(authoring.farmerOutputId ?? string.Empty),
                    ForesterInputIndex = ushort.MaxValue,
                    ForesterOutputIndex = ushort.MaxValue,
                    MinerOutputIndex = ushort.MaxValue,
                    FarmerOutputIndex = ushort.MaxValue,
                    HaulChance = Mathf.Clamp01(authoring.haulChance),
                    HaulCooldownSeconds = Mathf.Max(0.1f, authoring.haulCooldownSeconds),
                    PileDropMinUnits = Mathf.Max(0.1f, authoring.pileDropMinUnits),
                    PileDropMaxUnits = Mathf.Max(authoring.pileDropMinUnits, authoring.pileDropMaxUnits),
                    PilePickupMinUnits = Mathf.Max(0.1f, authoring.pilePickupMinUnits),
                    PileSearchRadius = Mathf.Max(1f, authoring.pileSearchRadius),
                    LastResolvedTick = 0u
                });
            }
        }
    }
}
