using PureDOTS.Runtime.Groups;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Godgame.Authoring.Groups
{
    /// <summary>
    /// Authoring component for village groups.
    /// Bakes GroupObjective and GroupMetrics for villages.
    /// </summary>
    public class VillageGroupAuthoring : MonoBehaviour
    {
        [Tooltip("Initial objective type")]
        public GroupObjectiveType InitialObjective = GroupObjectiveType.Idle;

        [Tooltip("Initial objective priority")]
        [Range(0, 255)]
        public byte InitialPriority = 50;

        [Tooltip("Resource budget (food)")]
        public float MaxFood = 1000f;

        [Tooltip("Resource budget (materials)")]
        public float MaxMaterials = 500f;
    }

    /// <summary>
    /// Baker for VillageGroupAuthoring.
    /// </summary>
    public class VillageGroupBaker : Baker<VillageGroupAuthoring>
    {
        public override void Bake(VillageGroupAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            // Add GroupObjective
            AddComponent(entity, new GroupObjective
            {
                ObjectiveType = authoring.InitialObjective,
                TargetEntity = Entity.Null,
                TargetPosition = float3.zero,
                Priority = authoring.InitialPriority,
                SetTick = 0,
                ExpirationTick = 0,
                IsActive = 1
            });

            // Add GroupMetrics
            AddComponent<GroupMetrics>(entity);

            // Add GroupResourceBudget
            var budget = new GroupResourceBudget { IsEnforced = 1 };
            budget.MaxResource0 = authoring.MaxFood; // Food/Supplies
            budget.MaxResource1 = authoring.MaxMaterials; // Materials
            AddComponent(entity, budget);
        }
    }
}

