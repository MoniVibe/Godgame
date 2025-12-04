using Godgame.Villages;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Godgame.Villages
{
    /// <summary>
    /// Authoring component for village entities.
    /// Add to village center prefabs to enable village AI and presentation.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class VillageAuthoring : MonoBehaviour
    {
        [Header("Identity")]
        public string villageId = "village_001";

        [Header("Territory")]
        [Range(5f, 100f)] public float influenceRadius = 20f;

        [Header("Initial State")]
        public VillagePhase startingPhase = VillagePhase.Forming;

        private sealed class VillageBaker : Baker<VillageAuthoring>
        {
            public override void Bake(VillageAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var position = authoring.transform.position;

                // Add Village component
                AddComponent(entity, new Village
                {
                    VillageId = new FixedString64Bytes(authoring.villageId),
                    Phase = authoring.startingPhase,
                    CenterPosition = new float3(position.x, position.y, position.z),
                    InfluenceRadius = authoring.influenceRadius,
                    MemberCount = 0,
                    LastUpdateTick = 0
                });

                // Add AI decision component
                AddComponent(entity, new VillageAIDecision
                {
                    CurrentPriority = 0,
                    DecisionType = 0,
                    TargetEntity = Entity.Null,
                    TargetPosition = new float3(position.x, position.y, position.z),
                    DecisionTick = 0,
                    DecisionDuration = 10f
                });

                // Add presentation component
                AddComponent(entity, new VillagePresentation
                {
                    EffectId = new FixedString64Bytes("village.influence"),
                    VisualPosition = new float3(position.x, position.y, position.z),
                    VisualRadius = authoring.influenceRadius,
                    VisualIntensity = 50,
                    LastVisualUpdateTick = 0
                });

                // Add buffers
                AddBuffer<VillageResource>(entity);
                AddBuffer<VillageMember>(entity);
                AddBuffer<VillageExpansionRequest>(entity);
            }
        }
    }
}

