using Godgame.Registry;
using PureDOTS.Runtime.Bands;
using PureDOTS.Runtime.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Godgame.Authoring
{
    /// <summary>
    /// Authoring component for band entities. Bakes PureDOTS components required for band gameplay systems.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BandAuthoring : MonoBehaviour
    {
        [SerializeField]
        private int bandId = 1;

        [SerializeField]
        private int factionId = 0;

        [SerializeField]
        private int memberCount = 5;

        [SerializeField]
        private float morale = 0.5f;

        [SerializeField]
        private float cohesion = 0.5f;

        [SerializeField]
        private float averageDiscipline = 0.5f;

        [SerializeField]
        private BandFormationType formation = BandFormationType.Column;

        [SerializeField]
        private float formationSpacing = 2f;

        [SerializeField]
        private float3 spawnPosition = float3.zero;

        private sealed class Baker : Unity.Entities.Baker<BandAuthoring>
        {
            public override void Bake(BandAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic | TransformUsageFlags.Renderable);

                // Add PureDOTS band components
                AddComponent(entity, new BandId
                {
                    Value = authoring.bandId,
                    FactionId = authoring.factionId,
                    Leader = Entity.Null
                });

                AddComponent(entity, new BandStats
                {
                    MemberCount = authoring.memberCount,
                    AverageDiscipline = math.clamp(authoring.averageDiscipline, 0f, 1f),
                    Morale = math.clamp(authoring.morale, 0f, 1f),
                    Cohesion = math.clamp(authoring.cohesion, 0f, 1f),
                    Fatigue = 0f,
                    Flags = BandStatusFlags.None,
                    LastUpdateTick = 0
                });

                AddComponent(entity, new BandFormation
                {
                    Formation = authoring.formation,
                    Spacing = math.max(0.1f, authoring.formationSpacing),
                    Width = 0f,
                    Depth = 0f,
                    Facing = new float3(0f, 0f, 1f),
                    Anchor = float3.zero,
                    Stability = 0f,
                    LastSolveTick = 0
                });

                // Add Godgame mirror component for registry bridge
                AddComponent(entity, new GodgameBand
                {
                    DisplayName = new Unity.Collections.FixedString64Bytes($"Band_{authoring.bandId}"),
                    BandId = authoring.bandId,
                    FactionId = authoring.factionId,
                    Leader = Entity.Null,
                    MemberCount = authoring.memberCount,
                    Morale = math.clamp(authoring.morale, 0f, 1f),
                    Cohesion = math.clamp(authoring.cohesion, 0f, 1f),
                    AverageDiscipline = math.clamp(authoring.averageDiscipline, 0f, 1f),
                    Fatigue = 0f,
                    StatusFlags = BandStatusFlags.None,
                    Formation = authoring.formation,
                    FormationSpacing = math.max(0.1f, authoring.formationSpacing),
                    FormationWidth = 0f,
                    FormationDepth = 0f,
                    Anchor = float3.zero,
                    Facing = new float3(0f, 0f, 1f)
                });

                // Set position if specified
                if (math.any(authoring.spawnPosition != float3.zero))
                {
                    var transform = GetComponent<Transform>();
                    if (transform != null)
                    {
                        transform.position = authoring.spawnPosition;
                    }
                }
            }
        }
    }
}

