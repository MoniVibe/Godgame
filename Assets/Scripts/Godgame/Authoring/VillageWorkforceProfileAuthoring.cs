using Godgame.Villages;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Authoring
{
    /// <summary>
    /// Authoring profile that defines workforce ratios for a village.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class VillageWorkforceProfileAuthoring : MonoBehaviour
    {
        [Header("Workforce Ratios")]
        [Range(0f, 1f)] public float foresterRatio = 0.25f;
        [Range(0f, 1f)] public float minerRatio = 0.2f;
        [Range(0f, 1f)] public float farmerRatio = 0.2f;
        [Range(0f, 1f)] public float builderRatio = 0.15f;
        [Range(0f, 1f)] public float breederRatio = 0.1f;
        [Range(0f, 1f)] public float haulerRatio = 0.1f;

        [Header("Reassignment")]
        [Range(0.1f, 10f)] public float reassignmentCooldownDays = 1f;

        private sealed class Baker : Baker<VillageWorkforceProfileAuthoring>
        {
            public override void Bake(VillageWorkforceProfileAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new VillageWorkforceProfile
                {
                    ForesterRatio = Mathf.Max(0f, authoring.foresterRatio),
                    MinerRatio = Mathf.Max(0f, authoring.minerRatio),
                    FarmerRatio = Mathf.Max(0f, authoring.farmerRatio),
                    BuilderRatio = Mathf.Max(0f, authoring.builderRatio),
                    BreederRatio = Mathf.Max(0f, authoring.breederRatio),
                    HaulerRatio = Mathf.Max(0f, authoring.haulerRatio),
                    ReassignmentCooldownDays = Mathf.Max(0.1f, authoring.reassignmentCooldownDays),
                    LastAssignmentTick = 0u
                });
            }
        }
    }
}
