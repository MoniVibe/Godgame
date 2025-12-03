using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Transport;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Godgame.Authoring
{
    /// <summary>
    /// Authoring component for logistics request entities. Bakes PureDOTS components required for logistics systems.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LogisticsAuthoring : MonoBehaviour
    {
        [SerializeField]
        private ushort resourceTypeIndex = 0;

        [SerializeField]
        private int amount = 10;

        [SerializeField]
        private float3 sourcePosition = float3.zero;

        [SerializeField]
        private float3 destinationPosition = float3.zero;

        [SerializeField]
        private LogisticsRequestPriority priority = LogisticsRequestPriority.Normal;

        [SerializeField]
        private LogisticsRequestFlags flags = LogisticsRequestFlags.None;

        private sealed class Baker : Unity.Entities.Baker<LogisticsAuthoring>
        {
            public override void Bake(LogisticsAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                // Add PureDOTS logistics components
                AddComponent(entity, new LogisticsRequest
                {
                    ResourceTypeIndex = authoring.resourceTypeIndex,
                    RequestedUnits = math.max(1, authoring.amount),
                    SourcePosition = authoring.sourcePosition,
                    DestinationPosition = authoring.destinationPosition,
                    Priority = authoring.priority,
                    Flags = authoring.flags,
                    CreatedTick = 0,
                    LastUpdateTick = 0
                });

                AddComponent(entity, new LogisticsRequestProgress());
            }
        }
    }
}

